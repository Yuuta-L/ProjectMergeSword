using System;
using System.Collections.Generic;

namespace MergeSword
{
    /// <summary>
    /// 启动界面契约。启动流程只依赖它，不认识任何具体实现。
    /// <para>
    /// 项目想换成自己的启动界面：实现本接口，在 Awake 里调用 <see cref="Launch.Register"/>，
    /// 然后把模板的 TemplateLaunchView 删掉即可；流程不需要改。
    /// </para>
    /// </summary>
    public interface ILaunchView
    {
        /// <summary>刷新进度与提示。progress 小于 0 表示隐藏进度条。</summary>
        void ShowProgress(float progress, string tip);

        /// <summary>只刷新提示文本，不动进度条。</summary>
        void SetTip(string tip);

        /// <summary>资源就绪：显示可进入的场景列表，等待玩家选择。</summary>
        void ShowSceneEntry(IReadOnlyList<LaunchScene> scenes);

        /// <summary>关闭并销毁启动界面。</summary>
        void Close();

        /// <summary>玩家选择了某个场景，参数为场景资源路径（Assets/.../Xxx.unity）。</summary>
        event Action<string> SceneRequested;

        /// <summary>界面已经关闭。</summary>
        event Action Closed;
    }

    /// <summary>启动界面上的一个场景入口。</summary>
    public readonly struct LaunchScene
    {
        /// <summary>场景资源路径，可直接用于 Burnicle.Scene.LoadScene。</summary>
        public readonly string Path;

        /// <summary>展示用名称（文件名去掉 .unity）。</summary>
        public readonly string DisplayName;

        public LaunchScene(string path)
        {
            Path = path;
            var slash = path.LastIndexOf('/');
            var name = slash >= 0 ? path.Substring(slash + 1) : path;
            DisplayName = name.EndsWith(".unity", StringComparison.Ordinal)
                ? name.Substring(0, name.Length - ".unity".Length)
                : name;
        }
    }
}
