using GameFramework;
using GameFramework.Event;
using System.Collections.Generic;
using BurnicleFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace MergeSword
{
    public class ProcedureUpdateResources : ProcedureBase
    {
        private bool m_UpdateResourcesComplete = false;
        private int m_UpdateCount = 0;
        private long m_UpdateTotalCompressedLength = 0L;
        private int m_UpdateSuccessCount = 0;
        private List<UpdateLengthData> m_UpdateLengthData = new List<UpdateLengthData>();
        private Dictionary<string, long> m_DownloadProgress = new Dictionary<string, long>();
        private Dictionary<string, string> m_ResourceDownloadPaths = new Dictionary<string, string>();
        private long m_TotalDownloadedBytes = 0L;
        private string m_VersionListCleanupPath = null;
        
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_UpdateResourcesComplete = false;
            m_UpdateCount = procedureOwner.GetData<int>("UpdateResourceCount");
            procedureOwner.RemoveData("UpdateResourceCount");
            m_UpdateTotalCompressedLength = procedureOwner.GetData<long>("UpdateResourceTotalCompressedLength");
            procedureOwner.RemoveData("UpdateResourceTotalCompressedLength");
            m_UpdateSuccessCount = 0;
            m_UpdateLengthData.Clear();
            m_DownloadProgress.Clear();
            m_ResourceDownloadPaths.Clear();
            m_TotalDownloadedBytes = 0L;
            m_VersionListCleanupPath = procedureOwner.GetData<string>("VersionListCleanupPath");
            if (m_VersionListCleanupPath != null)
            {
                procedureOwner.RemoveData("VersionListCleanupPath");
            }

            Burnicle.Event.Subscribe(ResourceUpdateStartEventArgs.EventId, OnResourceUpdateStart);
            Burnicle.Event.Subscribe(ResourceUpdateSuccessEventArgs.EventId, OnResourceUpdateSuccess);
            Burnicle.Event.Subscribe(ResourceUpdateFailureEventArgs.EventId, OnResourceUpdateFailure);
            Burnicle.Event.Subscribe(DownloadUpdateEventArgs.EventId, OnDownloadUpdate);

            StartUpdateResources(null);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            Burnicle.Event.Unsubscribe(ResourceUpdateStartEventArgs.EventId, OnResourceUpdateStart);
            Burnicle.Event.Unsubscribe(ResourceUpdateSuccessEventArgs.EventId, OnResourceUpdateSuccess);
            Burnicle.Event.Unsubscribe(ResourceUpdateFailureEventArgs.EventId, OnResourceUpdateFailure);
            Burnicle.Event.Unsubscribe(DownloadUpdateEventArgs.EventId, OnDownloadUpdate);

            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_UpdateResourcesComplete)
            {
                RefreshProgress();
                return;
            }

            ChangeState<ProcedureMain>(procedureOwner);
        }

        private void StartUpdateResources(object userData)
        {
            Log.Info("Start update resources...");
            Burnicle.Resource.UpdateResources(OnUpdateResourcesComplete);
        }

        private void RefreshProgress()
        {
            float progressTotal = (float)m_TotalDownloadedBytes / m_UpdateTotalCompressedLength;
            string descriptionText = Burnicle.Localization.GetString("UpdateResource.Tips", m_UpdateSuccessCount.ToString(), m_UpdateCount.ToString(), GetByteLengthString(m_TotalDownloadedBytes), GetByteLengthString(m_UpdateTotalCompressedLength));

            // 更新启动界面进度
            if (Launch.View != null)
            {
                Launch.View.ShowProgress(progressTotal, descriptionText);
            }
        }

        private string GetByteLengthString(long byteLength)
        {
            if (byteLength < 1024L) // 2 ^ 10
            {
                return Utility.Text.Format("{0} Bytes", byteLength);
            }

            if (byteLength < 1048576L) // 2 ^ 20
            {
                return Utility.Text.Format("{0:F2} KB", byteLength / 1024f);
            }

            if (byteLength < 1073741824L) // 2 ^ 30
            {
                return Utility.Text.Format("{0:F2} MB", byteLength / 1048576f);
            }

            if (byteLength < 1099511627776L) // 2 ^ 40
            {
                return Utility.Text.Format("{0:F2} GB", byteLength / 1073741824f);
            }

            if (byteLength < 1125899906842624L) // 2 ^ 50
            {
                return Utility.Text.Format("{0:F2} TB", byteLength / 1099511627776f);
            }

            if (byteLength < 1152921504606846976L) // 2 ^ 60
            {
                return Utility.Text.Format("{0:F2} PB", byteLength / 1125899906842624f);
            }

            return Utility.Text.Format("{0:F2} EB", byteLength / 1152921504606846976f);
        }

        private void OnUpdateResourcesComplete(GameFramework.Resource.IResourceGroup resourceGroup, bool result)
        {
            if (result)
            {
                m_UpdateResourcesComplete = true;
                if (Launch.View != null)
                {
                    Launch.View.ShowProgress(1f, Burnicle.Localization.GetString("Launch.Tip.UpdateComplete"));
                }
                Log.Info("[Cache] 资源更新完成: 共需更新 {0} 个，成功 {1} 个（新下载资源已写入读写区缓存）", m_UpdateCount, m_UpdateSuccessCount);

                CleanupOrphanedResources();
            }
            else
            {
                Log.Error("[Cache] 资源更新完成但存在错误: 共需更新 {0} 个，成功 {1} 个", m_UpdateCount, m_UpdateSuccessCount);
            }
        }

        private void CleanupOrphanedResources()
        {
            if (string.IsNullOrEmpty(m_VersionListCleanupPath) || !System.IO.File.Exists(m_VersionListCleanupPath))
            {
                Log.Info("[Cleanup] 无旧版本清单备份，跳过清理（首次安装或无需清理）");
                return;
            }

            try
            {
                string currentVersionListPath = Utility.Path.GetRegularPath(
                    System.IO.Path.Combine(Burnicle.Resource.ReadWritePath, "GameFrameworkVersion.dat"));
                if (!System.IO.File.Exists(currentVersionListPath))
                {
                    Log.Warning("[Cleanup] 当前版本清单不存在，跳过清理");
                    return;
                }

                // 反序列化旧版本清单
                GameFramework.Resource.UpdatableVersionList oldList;
                using (var fs = new System.IO.FileStream(m_VersionListCleanupPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    oldList = Burnicle.Resource.UpdatableVersionListSerializer.Deserialize(fs);
                }

                if (!oldList.IsValid)
                {
                    Log.Warning("[Cleanup] 旧版本清单反序列化失败，跳过清理");
                    return;
                }

                // 反序列化新版本清单
                GameFramework.Resource.UpdatableVersionList newList;
                using (var fs = new System.IO.FileStream(currentVersionListPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    newList = Burnicle.Resource.UpdatableVersionListSerializer.Deserialize(fs);
                }

                if (!newList.IsValid)
                {
                    Log.Warning("[Cleanup] 新版本清单反序列化失败，跳过清理");
                    return;
                }

                // 构建新清单的资源 FullName 集合
                var newResourceNames = new HashSet<string>();
                GameFramework.Resource.UpdatableVersionList.Resource[] newResources = newList.GetResources();
                for (int i = 0; i < newResources.Length; i++)
                {
                    string fullName = BuildResourceFullName(newResources[i]);
                    newResourceNames.Add(fullName);
                }

                // 遍历旧清单，删除不在新清单中的资源文件
                int deleteCount = 0;
                string readWritePath = Burnicle.Resource.ReadWritePath;
                GameFramework.Resource.UpdatableVersionList.Resource[] oldResources = oldList.GetResources();
                for (int i = 0; i < oldResources.Length; i++)
                {
                    string fullName = BuildResourceFullName(oldResources[i]);
                    if (!newResourceNames.Contains(fullName))
                    {
                        string filePath = Utility.Path.GetRegularPath(System.IO.Path.Combine(readWritePath, fullName));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                            deleteCount++;
                            Log.Info("[Cleanup] 删除旧版本残留资源文件: '{0}'", filePath);
                        }
                    }
                }

                Log.Info("[Cleanup] 清理完成: 共删除 {0} 个旧版本残留资源文件", deleteCount);
            }
            catch (System.Exception ex)
            {
                Log.Error("[Cleanup] 清理残留资源时出现异常: {0}", ex);
            }
            finally
            {
                // 删除备份文件
                try
                {
                    if (System.IO.File.Exists(m_VersionListCleanupPath))
                    {
                        System.IO.File.Delete(m_VersionListCleanupPath);
                        Log.Info("[Cleanup] 已删除旧版本清单备份: '{0}'", m_VersionListCleanupPath);
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Warning("[Cleanup] 删除备份文件失败: {0}", ex);
                }
            }
        }

        private static string BuildResourceFullName(GameFramework.Resource.UpdatableVersionList.Resource resource)
        {
            if (!string.IsNullOrEmpty(resource.Variant))
            {
                return Utility.Text.Format("{0}.{1}.{2}", resource.Name, resource.Variant, resource.Extension);
            }
            return Utility.Text.Format("{0}.{1}", resource.Name, resource.Extension);
        }

        private void OnResourceUpdateStart(object sender, GameEventArgs e)
        {
            ResourceUpdateStartEventArgs ne = (ResourceUpdateStartEventArgs)e;

            m_ResourceDownloadPaths[ne.Name] = ne.DownloadPath;

            for (int i = 0; i < m_UpdateLengthData.Count; i++)
            {
                if (m_UpdateLengthData[i].Name == ne.Name)
                {
                    Log.Warning("Update resource '{0}' is invalid.", ne.Name);
                    return;
                }
            }

            m_UpdateLengthData.Add(new UpdateLengthData(ne.Name));
        }

        private void OnDownloadUpdate(object sender, GameEventArgs e)
        {
            BurnicleFramework.Runtime.DownloadUpdateEventArgs ne = (BurnicleFramework.Runtime.DownloadUpdateEventArgs)e;

            string path = ne.DownloadPath;
            m_DownloadProgress.TryGetValue(path, out long lastLength);
            long delta = ne.CurrentLength - lastLength;
            if (delta > 0)
            {
                m_TotalDownloadedBytes += delta;
            }
            else if (delta < 0)
            {
                // 重试场景：新下载任务从0开始，需扣除上次失败的部分
                m_TotalDownloadedBytes += delta;
            }
            m_DownloadProgress[path] = ne.CurrentLength;
        }

        private void OnResourceUpdateSuccess(object sender, GameEventArgs e)
        {
            ResourceUpdateSuccessEventArgs ne = (ResourceUpdateSuccessEventArgs)e;
            Log.Info("[Cache] 资源 '{0}' 下载成功（已写入读写区缓存）", ne.Name);

            // 确保该资源的完整压缩大小已计入总下载量
            if (m_DownloadProgress.TryGetValue(ne.DownloadPath, out long lastLength))
            {
                long remaining = ne.CompressedLength - lastLength;
                if (remaining > 0)
                {
                    m_TotalDownloadedBytes += remaining;
                }
                m_DownloadProgress.Remove(ne.DownloadPath);
            }
            m_ResourceDownloadPaths.Remove(ne.Name);

            for (int i = 0; i < m_UpdateLengthData.Count; i++)
            {
                if (m_UpdateLengthData[i].Name == ne.Name)
                {
                    m_UpdateSuccessCount++;
                    return;
                }
            }

            Log.Warning("Update resource '{0}' is invalid.", ne.Name);
        }

        private void OnResourceUpdateFailure(object sender, GameEventArgs e)
        {
            ResourceUpdateFailureEventArgs ne = (ResourceUpdateFailureEventArgs)e;

            // 清理该资源的下载进度数据，避免重试后重复计数
            if (m_ResourceDownloadPaths.TryGetValue(ne.Name, out string downloadPath))
            {
                if (m_DownloadProgress.TryGetValue(downloadPath, out long failedLength))
                {
                    m_TotalDownloadedBytes -= failedLength;
                    m_DownloadProgress.Remove(downloadPath);
                }
                m_ResourceDownloadPaths.Remove(ne.Name);
            }

            if (ne.RetryCount >= ne.TotalRetryCount)
            {
                Log.Error("[Cache] 资源 '{0}' 下载失败（重试耗尽 {1}/{2}）: {3}", ne.Name, ne.RetryCount, ne.TotalRetryCount, ne.ErrorMessage);
                if (Launch.View != null)
                {
                    Launch.View.SetTip(Burnicle.Localization.GetString("Launch.Tip.DownloadFailed"));
                }
                return;
            }
            else
            {
                Log.Warning("[Cache] 资源 '{0}' 下载失败（重试 {1}/{2}）: {3}", ne.Name, ne.RetryCount, ne.TotalRetryCount, ne.ErrorMessage);
                if (Launch.View != null)
                {
                    Launch.View.SetTip(Burnicle.Localization.GetString("Launch.Tip.DownloadFailedRetry"));
                }
            }

            for (int i = 0; i < m_UpdateLengthData.Count; i++)
            {
                if (m_UpdateLengthData[i].Name == ne.Name)
                {
                    m_UpdateLengthData.Remove(m_UpdateLengthData[i]);
                    return;
                }
            }

            Log.Warning("Update resource '{0}' is invalid.", ne.Name);
        }

        private class UpdateLengthData
        {
            private readonly string m_Name;

            public UpdateLengthData(string name)
            {
                m_Name = name;
            }

            public string Name
            {
                get
                {
                    return m_Name;
                }
            }

            public int Length
            {
                get;
                set;
            }
        }
    }
}
