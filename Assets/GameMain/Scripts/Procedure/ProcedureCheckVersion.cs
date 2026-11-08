using GameFramework;
using GameFramework.Resource;
using UnityEngine;
using BurnicleFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace MergeSword
{
    public class ProcedureCheckVersion : ProcedureBase
    {
        private bool m_CheckVersionComplete = false;
        private bool m_NeedUpdateVersion = false;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_CheckVersionComplete = false;
            m_NeedUpdateVersion = false;

            var versionInfo = Burnicle.BuiltinData.VersionInfo;
            if (versionInfo == null)
            {
                Log.Error("BuiltinData.VersionInfo is null, cannot check version.");
                m_CheckVersionComplete = true;
                m_NeedUpdateVersion = false;
                return;
            }

            // 从本地 VersionInfo 读取资源更新地址和版本号
            if (!string.IsNullOrEmpty(versionInfo.UpdatePrefixUri))
            {
                Burnicle.Resource.UpdatePrefixUri = Utility.Path.GetRegularPath(versionInfo.UpdatePrefixUri);
            }

            int resourceVersion = versionInfo.ResourceVersion;

            var checkResult = Burnicle.Resource.CheckVersionList(resourceVersion);
            m_NeedUpdateVersion = checkResult == CheckVersionListResult.NeedUpdate;

            Log.Info("[Cache] CheckVersionList result: {0}, internal resource version: {1}", checkResult, resourceVersion);

            if (m_NeedUpdateVersion)
            {
                Log.Info("[Cache] 版本列表缓存未命中（CDN 上的版本比本地新），将下载: UpdatePrefixUri='{0}'", versionInfo.UpdatePrefixUri);

                // 备份旧版本清单，用于后续清理残留资源
                string versionListPath = Utility.Path.GetRegularPath(System.IO.Path.Combine(Burnicle.Resource.ReadWritePath, "GameFrameworkVersion.dat"));
                string backupPath = Utility.Path.GetRegularPath(System.IO.Path.Combine(Burnicle.Resource.ReadWritePath, "GameFrameworkVersion.cleanup.dat"));
                if (System.IO.File.Exists(versionListPath))
                {
                    System.IO.File.Copy(versionListPath, backupPath, true);
                    Log.Info("[Cleanup] 已备份旧版本清单: '{0}' -> '{1}'", versionListPath, backupPath);
                    procedureOwner.SetData("VersionListCleanupPath", backupPath);
                }
                else
                {
                    Log.Info("[Cleanup] 读写区无旧版本清单，无需备份（首次安装）");
                }

                procedureOwner.SetData("VersionListLength", versionInfo.VersionListLength);
                procedureOwner.SetData("VersionListHashCode", versionInfo.VersionListHashCode);
                procedureOwner.SetData("VersionListCompressedLength", versionInfo.VersionListCompressedLength);
                procedureOwner.SetData("VersionListCompressedHashCode", versionInfo.VersionListCompressedHashCode);
            }
            else
            {
                Log.Info("[Cache] 版本列表缓存命中（读写区已有最新版本列表）");
            }

            m_CheckVersionComplete = true;
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_CheckVersionComplete)
            {
                return;
            }

            if (m_NeedUpdateVersion)
            {
                ChangeState<ProcedureUpdateVersion>(procedureOwner);
            }
            else
            {
                ChangeState<ProcedureVerifyResources>(procedureOwner);
            }
        }
    }
}
