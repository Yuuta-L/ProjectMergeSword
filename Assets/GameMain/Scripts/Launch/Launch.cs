namespace MergeSword
{
    /// <summary>
    /// 启动界面的注册点：流程只读 <see cref="View"/>，界面实现自己在 Awake/OnDestroy 里注册与注销。
    /// 没有任何界面注册时流程照常工作（<see cref="View"/> 为 null）。
    /// </summary>
    public static class Launch
    {
        /// <summary>当前的启动界面，可能为 null。</summary>
        public static ILaunchView View { get; private set; }

        public static void Register(ILaunchView view)
        {
            View = view;
        }

        public static void Unregister(ILaunchView view)
        {
            if (ReferenceEquals(View, view))
            {
                View = null;
            }
        }
    }
}
