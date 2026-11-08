using GameFramework;
using GameFramework.Event;
using GameFramework.Resource;
using UnityEngine;
using BurnicleFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace MergeSword
{
    public class ProcedureUpdateVersion : ProcedureBase
    {
        private bool m_UpdateVersionComplete = false;
        private int m_VersionListCompressedLength = 0;
        private UpdateVersionListCallbacks m_UpdateVersionListCallbacks = null;

        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);

            m_UpdateVersionListCallbacks = new UpdateVersionListCallbacks(OnUpdateVersionListSuccess, OnUpdateVersionListFailure);
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_UpdateVersionComplete = false;

            int versionListLength = procedureOwner.GetData<int>("VersionListLength");
            int versionListHashCode = procedureOwner.GetData<int>("VersionListHashCode");
            m_VersionListCompressedLength = procedureOwner.GetData<int>("VersionListCompressedLength");
            int versionListCompressedHashCode = procedureOwner.GetData<int>("VersionListCompressedHashCode");

            procedureOwner.RemoveData("VersionListLength");
            procedureOwner.RemoveData("VersionListHashCode");
            procedureOwner.RemoveData("VersionListCompressedLength");
            procedureOwner.RemoveData("VersionListCompressedHashCode");

            Burnicle.Event.Subscribe(DownloadUpdateEventArgs.EventId, OnDownloadUpdate);
            Burnicle.Event.Subscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
            Burnicle.Event.Subscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);

            if (Launch.View != null)
            {
                Launch.View.ShowProgress(0f, Burnicle.Localization.GetString("Launch.Tip.FetchingVersion"));
            }

            Log.Info("[Cache] 开始从 CDN 下载版本列表: 期望长度={0}, 期望压缩长度={1}, 压缩Hash=0x{2:x8}",
                versionListLength, m_VersionListCompressedLength, versionListCompressedHashCode);
            Burnicle.Resource.UpdateVersionList(versionListLength, versionListHashCode, m_VersionListCompressedLength, versionListCompressedHashCode, m_UpdateVersionListCallbacks);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            Burnicle.Event.Unsubscribe(DownloadUpdateEventArgs.EventId, OnDownloadUpdate);
            Burnicle.Event.Unsubscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
            Burnicle.Event.Unsubscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);

            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_UpdateVersionComplete)
            {
                return;
            }

            ChangeState<ProcedureVerifyResources>(procedureOwner);
        }

        private void OnUpdateVersionListSuccess(string downloadPath, string downloadUri)
        {
            m_UpdateVersionComplete = true;
            if (Launch.View != null)
            {
                Launch.View.ShowProgress(1f, Burnicle.Localization.GetString("Launch.Tip.VersionFetched"));
            }
            Log.Info("[Cache] 版本列表下载成功: '{0}' -> 已写入读写区缓存 '{1}'", downloadUri, downloadPath);
        }

        private void OnUpdateVersionListFailure(string downloadUri, string errorMessage)
        {
            Log.Warning("[Cache] 版本列表下载失败: '{0}', 错误: {1}", downloadUri, errorMessage);
            if (Launch.View != null)
            {
                Launch.View.SetTip(Burnicle.Localization.GetString("Launch.Tip.FetchVersionFailedRetry"));
            }
        }

        private void OnDownloadUpdate(object sender, GameEventArgs e)
        {
            DownloadUpdateEventArgs ne = (DownloadUpdateEventArgs)e;
            if (m_VersionListCompressedLength > 0 && Launch.View != null)
            {
                float progress = (float)ne.CurrentLength / m_VersionListCompressedLength;
                Launch.View.ShowProgress(Mathf.Clamp01(progress), Burnicle.Localization.GetString("Launch.Tip.FetchingVersion"));
            }
        }

        private void OnDownloadSuccess(object sender, GameEventArgs e)
        {
            DownloadSuccessEventArgs ne = (DownloadSuccessEventArgs)e;
            Log.Info("[Cache] 版本列表下载段完成: 路径 '{0}'", ne.DownloadPath);
        }

        private void OnDownloadFailure(object sender, GameEventArgs e)
        {
            DownloadFailureEventArgs ne = (DownloadFailureEventArgs)e;
            Log.Warning("[Cache] 版本列表下载段失败: {0}", ne.ErrorMessage);
        }
    }
}
