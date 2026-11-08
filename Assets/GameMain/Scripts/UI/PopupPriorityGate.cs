using System;
using BurnicleFramework.Runtime;

namespace MergeSword
{
    /// <summary>
    /// 弹出界面优先级门控系统。
    /// 高优先级内容（如教程、强制引导）启动前调用 Lock 阻塞 UISequence 等自动弹窗系统，
    /// 完成后调用 Unlock 恢复。
    ///
    /// Reserve / CancelReserve 用于"预约"即将启动的高优先级操作，
    /// 其他系统（如每日签到）在入队前检查 IsReserved，若已被预约则不加入队列，
    /// 避免将低优先级内容加入 UISequence 后被门控阻塞。
    /// </summary>
    public static class PopupPriorityGate
    {
        /// <summary>当前是否有高优先级内容锁定。</summary>
        public static bool IsLocked { get; private set; }

        /// <summary>是否已预约高优先级内容（即将锁定或已锁定）。</summary>
        public static bool IsReserved { get; private set; }

        /// <summary>当前锁的持有者标识。</summary>
        public static string CurrentLocker { get; private set; }

        /// <summary>锁状态变化时触发。</summary>
        public static event Action<bool> OnLockStateChanged;

        /// <summary>
        /// 预约高优先级操作。在真正 Lock 之前调用，告知其他系统不要插入低优先级内容。
        /// </summary>
        public static void Reserve()
        {
            IsReserved = true;
        }

        /// <summary>
        /// 取消预约（条件不满足时调用）。
        /// </summary>
        public static void CancelReserve()
        {
            IsReserved = false;
        }

        /// <summary>
        /// 申请高优先级锁，自动清除预约状态。
        /// </summary>
        /// <param name="locker">锁持有者标识（如 "Tutorial"）。</param>
        public static void Lock(string locker)
        {
            if (IsLocked && CurrentLocker != locker)
            {
                Log.Warning($"[PopupPriorityGate] 已被 '{CurrentLocker}' 锁定，'{locker}' 无法获取锁");
                return;
            }

            IsReserved = false;
            IsLocked = true;
            CurrentLocker = locker;
            Log.Info($"[PopupPriorityGate] Locked by '{locker}'");
            OnLockStateChanged?.Invoke(true);
        }

        /// <summary>
        /// 释放高优先级锁。
        /// </summary>
        /// <param name="locker">锁持有者标识，必须与 Lock 时一致。</param>
        public static void Unlock(string locker)
        {
            if (!IsLocked)
                return;

            if (CurrentLocker != locker)
            {
                Log.Warning($"[PopupPriorityGate] '{locker}' 尝试解锁但当前持有者为 '{CurrentLocker}'");
                return;
            }

            IsLocked = false;
            CurrentLocker = null;
            Log.Info($"[PopupPriorityGate] Unlocked by '{locker}'");
            OnLockStateChanged?.Invoke(false);
        }
    }
}
