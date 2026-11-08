using System.Collections.Generic;
using BurnicleFramework.Runtime;
using GameFramework.Fsm;
using GameFramework.Procedure;
using Obfuz;
using UnityEngine.SceneManagement;

namespace MergeSword
{
    /// <summary>
    /// 模板主流程：把 Build Settings 里的场景交给启动界面选择，玩家选中后直接加载该场景。
    /// <para>
    /// 与界面解耦：长什么样子由 <see cref="ILaunchView"/> 的实现决定，没有界面时自动进默认场景。
    /// 场景一律 additive 加载，Launcher 场景作为框架宿主必须保留。
    /// </para>
    /// </summary>
    [ObfuzIgnore]
    public class ProcedureMain : ProcedureBase
    {
        private readonly List<LaunchScene> _scenes = new List<LaunchScene>();

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            _scenes.Clear();
            _scenes.AddRange(LaunchSceneCatalog.Collect(SceneManager.GetActiveScene().path));

            var view = Launch.View;
            if (view != null)
            {
                // 界面存在就交给玩家选；没有可进入的场景时界面自己给出提示
                view.SceneRequested -= OnSceneRequested;
                view.SceneRequested += OnSceneRequested;
                view.ShowSceneEntry(_scenes);
                return;
            }

            // 没有启动界面（项目删掉了模板界面）：直接进默认场景
            if (_scenes.Count > 0)
            {
                EnterScene(_scenes[0].Path);
            }
            else
            {
                Log.Warning("[Launch] 没有启动界面，Build Settings 里也没有可进入的场景。");
            }
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            var view = Launch.View;
            if (view != null)
            {
                view.SceneRequested -= OnSceneRequested;
            }
        }

        private void OnSceneRequested(string scenePath)
        {
            var view = Launch.View;
            if (view != null)
            {
                view.SceneRequested -= OnSceneRequested;
                view.Close();
            }

            EnterScene(scenePath);
        }

        private void EnterScene(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                return;
            }

            Log.Info("[Launch] 进入场景 '{0}'", scenePath);
            Burnicle.Scene.LoadScene(scenePath);
        }
    }
}
