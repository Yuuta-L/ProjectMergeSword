using BurnicleFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace MergeSword
{
    public class ProcedureCheckResources : ProcedureBase
    {
        private bool m_CheckResourcesComplete = false;
        private bool m_NeedUpdateResources = false;
        private int m_UpdateResourceCount = 0;
        private long m_UpdateResourceTotalCompressedLength = 0L;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_CheckResourcesComplete = false;
            m_NeedUpdateResources = false;
            m_UpdateResourceCount = 0;
            m_UpdateResourceTotalCompressedLength = 0L;

            if (Launch.View != null)
            {
                Launch.View.ShowProgress(0f, Burnicle.Localization.GetString("Launch.Tip.CheckingResources"));
            }

            Burnicle.Resource.CheckResources(OnCheckResourcesComplete);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_CheckResourcesComplete)
            {
                return;
            }

            if (m_NeedUpdateResources)
            {
                procedureOwner.SetData("UpdateResourceCount", m_UpdateResourceCount);
                procedureOwner.SetData("UpdateResourceTotalCompressedLength", m_UpdateResourceTotalCompressedLength);
                ChangeState<ProcedureUpdateResources>(procedureOwner);
            }
            else
            {
                ChangeState<ProcedureMain>(procedureOwner);
            }
        }

        private void OnCheckResourcesComplete(int movedCount, int removedCount, int updateCount, long updateTotalLength, long updateTotalCompressedLength)
        {
            m_CheckResourcesComplete = true;
            m_NeedUpdateResources = updateCount > 0;
            m_UpdateResourceCount = updateCount;
            m_UpdateResourceTotalCompressedLength = updateTotalCompressedLength;

            if (updateCount > 0)
            {
                Log.Info("[Cache] 资源检查完成: {0} 个资源需更新（压缩后 {1}），{2} 个已移动，{3} 个已移除。缓存有效资源 = 总数 - {0}",
                    updateCount, updateTotalCompressedLength, movedCount, removedCount);
            }
            else
            {
                Log.Info("[Cache] 资源检查完成: 无需更新任何资源（全部缓存命中）");
            }
        }
    }
}
