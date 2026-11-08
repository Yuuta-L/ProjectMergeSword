using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 项目初始化向导窗口。自动弹出见 <see cref="BurnicleSetupWizardBootstrap"/>，手动入口见菜单。
/// 删除整个 Assets/BurnicleSetupWizard 文件夹即可移除向导。
/// </summary>
public class BurnicleSetupWizardWindow : EditorWindow
{
    private const string WindowTitle = "项目初始化向导";
    private const string MenuPath = "Tools/项目初始化向导";

    private static GUIStyle s_TitleStyle;
    private static GUIStyle s_SectionStyle;

    private BurnicleSetupData _data;
    private string _oldNamespaceRoot;
    private string _oldSceneName;
    private int _namespaceFileCount;
    private Vector2 _scroll;

    [MenuItem(MenuPath, false, 560)]
    public static void Open()
    {
        var window = GetWindow<BurnicleSetupWizardWindow>(WindowTitle);
        window.minSize = new Vector2(620f, 560f);
        window.Show();
    }

    private void OnEnable()
    {
        Reload();
    }

    private void Reload()
    {
        var current = BurnicleSetupState.Load();
        _oldNamespaceRoot = current.NamespaceRoot;
        _oldSceneName = current.LaunchSceneName;
        _data = current;
        _namespaceFileCount = BurnicleProjectSetupService.CountFilesWithNamespaceRoot(_oldNamespaceRoot);
    }

    private void OnGUI()
    {
        // GUIStyle 在 OnGUI 里首次创建，避免在 GUI 上下文之外访问 EditorStyles
        if (s_TitleStyle == null)
        {
            s_TitleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 15 };
            s_SectionStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(WindowTitle, s_TitleStyle);
        EditorGUILayout.HelpBox(
            "这个向导把模板改成你自己的项目：改 PlayerSettings、重命名启动场景、替换代码命名空间根。\n" +
            "不需要向导时，删除 Assets/BurnicleSetupWizard 整个文件夹即可。",
            MessageType.Info);

        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));

        DrawProjectSection();
        DrawCodeSection();
        DrawPreviewSection();

        EditorGUILayout.EndScrollView();

        DrawActions();
    }

    private void DrawProjectSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("1. 项目信息", s_SectionStyle);
        _data.ProductName = EditorGUILayout.TextField("产品名", _data.ProductName);
        _data.CompanyName = EditorGUILayout.TextField("公司名", _data.CompanyName);
        _data.BundleIdentifier = EditorGUILayout.TextField("包名", _data.BundleIdentifier);
        EditorGUILayout.HelpBox(
            "包名写进 Unity PlayerSettings（Android / iOS / Standalone / WebGL），" +
            "各平台构建配置里的包名如有独立设置需要自己同步。",
            MessageType.None);
    }

    private void DrawCodeSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("2. 代码与场景", s_SectionStyle);
        _data.NamespaceRoot = EditorGUILayout.TextField("命名空间根", _data.NamespaceRoot);
        _data.LaunchSceneName = EditorGUILayout.TextField("启动场景名", _data.LaunchSceneName);
        EditorGUILayout.HelpBox(
            $"命名空间根会重写 {BurnicleProjectSetupService.ScriptsRoot} 下的源码命名空间，" +
            "场景里各种 Helper / 流程的类型全名也会一起改；场景文件名跟「启动场景名」改，Build Settings 同步更新。",
            MessageType.None);

        if (!BurnicleProjectSetupService.IsValidNamespaceRoot(_data.NamespaceRoot))
        {
            EditorGUILayout.HelpBox("命名空间根必须是合法的 C# 命名空间。", MessageType.Error);
        }

        if (string.IsNullOrWhiteSpace(_data.LaunchSceneName))
        {
            EditorGUILayout.HelpBox("启动场景名不能为空。", MessageType.Error);
        }
    }

    private void DrawPreviewSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("3. 应用预览", s_SectionStyle);

        var changes = BuildPreview();
        if (changes.Count == 0)
        {
            EditorGUILayout.HelpBox("没有检测到需要应用的改动。", MessageType.None);
            return;
        }

        foreach (var change in changes)
        {
            EditorGUILayout.LabelField($"· {change}", EditorStyles.wordWrappedLabel);
        }
    }

    private List<string> BuildPreview()
    {
        var changes = new List<string>();

        if (!string.IsNullOrEmpty(_data.ProductName) && _data.ProductName != PlayerSettings.productName)
        {
            changes.Add($"产品名：{PlayerSettings.productName} → {_data.ProductName}");
        }

        if (!string.IsNullOrEmpty(_data.CompanyName) && _data.CompanyName != PlayerSettings.companyName)
        {
            changes.Add($"公司名：{PlayerSettings.companyName} → {_data.CompanyName}");
        }

        if (!string.IsNullOrEmpty(_data.BundleIdentifier))
        {
            var currentIdentifier = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            if (currentIdentifier != _data.BundleIdentifier)
            {
                changes.Add($"包名：{currentIdentifier} → {_data.BundleIdentifier}");
            }
        }

        var oldScenePath = $"{BurnicleProjectSetupService.ScenesFolder}/{_oldSceneName}.unity";
        var newScenePath = $"{BurnicleProjectSetupService.ScenesFolder}/{_data.LaunchSceneName}.unity";
        if (oldScenePath != newScenePath)
        {
            changes.Add(File.Exists(oldScenePath)
                ? $"启动场景：{oldScenePath} → {newScenePath}"
                : $"启动场景：{_data.LaunchSceneName}（旧场景 {oldScenePath} 不存在，只更新记录）");
        }

        if (_oldNamespaceRoot != _data.NamespaceRoot)
        {
            changes.Add($"命名空间根：{_oldNamespaceRoot} → {_data.NamespaceRoot}" +
                        $"（改写 {_namespaceFileCount} 个源文件）");
        }

        if (BurnicleSetupState.Completed)
        {
            changes.Add("完成标记：已存在（应用后会更新记录）");
        }

        return changes;
    }

    private void DrawActions()
    {
        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("稍后再说", GUILayout.Height(30f)))
            {
                Close();
            }

            var valid = BurnicleProjectSetupService.IsValidNamespaceRoot(_data.NamespaceRoot)
                        && !string.IsNullOrWhiteSpace(_data.LaunchSceneName);
            using (new EditorGUI.DisabledScope(!valid))
            {
                if (GUILayout.Button("应用设置", GUILayout.Height(30f)))
                {
                    Apply();
                }
            }
        }
    }

    private void Apply()
    {
        var report = BurnicleProjectSetupService.Apply(_data.ProductName, _data.CompanyName,
            _data.BundleIdentifier, _data.NamespaceRoot, _data.LaunchSceneName);

        EditorUtility.DisplayDialog(WindowTitle, string.Join("\n", report), "好");

        if (BurnicleSetupState.Completed)
        {
            Close();
        }
        else
        {
            Reload();
        }
    }
}
