using GameFramework;
using GameFramework.Event;
using BurnicleFramework.Runtime;
using UnityEngine;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace MergeSword
{
    public class ProcedureVerifyResources : ProcedureBase
    {
        private bool m_VerifyResourcesComplete = false;
        private int m_VerifyTotalCount = 0;
        private int m_VerifySuccessCount = 0;
        private int m_VerifyFailureCount = 0;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            Burnicle.Event.Subscribe(ResourceVerifyStartEventArgs.EventId, OnResourceVerifyStart);
            Burnicle.Event.Subscribe(ResourceVerifySuccessEventArgs.EventId, OnResourceVerifySuccess);
            Burnicle.Event.Subscribe(ResourceVerifyFailureEventArgs.EventId, OnResourceVerifyFailure);

            m_VerifyResourcesComplete = false;
            m_VerifyTotalCount = 0;
            m_VerifySuccessCount = 0;
            m_VerifyFailureCount = 0;
            Burnicle.Resource.VerifyResources(OnVerifyResourcesComplete);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            Burnicle.Event.Unsubscribe(ResourceVerifyStartEventArgs.EventId, OnResourceVerifyStart);
            Burnicle.Event.Unsubscribe(ResourceVerifySuccessEventArgs.EventId, OnResourceVerifySuccess);
            Burnicle.Event.Unsubscribe(ResourceVerifyFailureEventArgs.EventId, OnResourceVerifyFailure);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_VerifyResourcesComplete)
            {
                UpdateProgress();
                return;
            }

            ChangeState<ProcedureCheckResources>(procedureOwner);
        }

        private void OnVerifyResourcesComplete(bool result)
        {
            m_VerifyResourcesComplete = true;
            if (result)
            {
                Log.Info("[Cache] 资源校验完成: 共 {0} 个，校验成功 {1} 个，校验失败 {2} 个。结果: 全部通过（缓存有效）",
                    m_VerifyTotalCount, m_VerifySuccessCount, m_VerifyFailureCount);
            }
            else
            {
                Log.Warning("[Cache] 资源校验完成: 共 {0} 个，校验成功 {1} 个，校验失败 {2} 个。结果: 存在失败（部分资源需重新下载）",
                    m_VerifyTotalCount, m_VerifySuccessCount, m_VerifyFailureCount);
            }
            if (Launch.View != null)
            {
                Launch.View.ShowProgress(1f, Burnicle.Localization.GetString("Launch.Tip.VerificationComplete"));
            }
        }

        private void OnResourceVerifyStart(object sender, GameEventArgs e)
        {
            ResourceVerifyStartEventArgs ne = (ResourceVerifyStartEventArgs)e;
            m_VerifyTotalCount = ne.Count;
            if (Launch.View != null)
            {
                Launch.View.ShowProgress(0f, Burnicle.Localization.GetString("Launch.Tip.VerifyingResources"));
            }
            Log.Info("[Cache] 开始资源校验: 共 {0} 个资源，总大小 {1} 字节", ne.Count, ne.TotalLength);
        }

        private void OnResourceVerifySuccess(object sender, GameEventArgs e)
        {
            ResourceVerifySuccessEventArgs ne = (ResourceVerifySuccessEventArgs)e;
            m_VerifySuccessCount++;
            UpdateProgress();
        }

        private void OnResourceVerifyFailure(object sender, GameEventArgs e)
        {
            ResourceVerifyFailureEventArgs ne = (ResourceVerifyFailureEventArgs)e;
            m_VerifyFailureCount++;
            UpdateProgress();
            Log.Warning("[Cache] 资源 '{0}' 校验失败（缓存失效，需重新下载）", ne.Name);
        }

        private void UpdateProgress()
        {
            if (m_VerifyTotalCount > 0 && Launch.View != null)
            {
                float p = (float)(m_VerifySuccessCount + m_VerifyFailureCount) / m_VerifyTotalCount;
                Launch.View.ShowProgress(Mathf.Clamp01(p), Burnicle.Localization.GetString("Launch.Tip.VerifyingResources"));
            }
        }
    }
}
