using UnityEditor;
using UnityEngine;

/// <summary>
/// 打开工程时检查是否初始化过，没有就自动弹向导。
/// 关掉向导不会在同一会话里反复弹；没点「应用」的话下次打开工程还会出现。
/// 需要手动打开时走菜单 Tools > 项目初始化向导。
/// </summary>
[InitializeOnLoad]
internal static class BurnicleSetupWizardBootstrap
{
    private const string SessionKey = "Burnicle.SetupWizard.Opened";

    static BurnicleSetupWizardBootstrap()
    {
        EditorApplication.delayCall += TryOpen;
    }

    private static void TryOpen()
    {
        if (Application.isBatchMode ||
            BuildPipeline.isBuildingPlayer ||
            EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorApplication.isCompiling)
        {
            return;
        }

        if (SessionState.GetBool(SessionKey, false) || BurnicleSetupState.Completed)
        {
            return;
        }

        SessionState.SetBool(SessionKey, true);
        BurnicleSetupWizardWindow.Open();
    }
}
