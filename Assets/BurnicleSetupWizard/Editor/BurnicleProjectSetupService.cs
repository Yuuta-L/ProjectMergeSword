using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 向导的执行逻辑：产品名 / 公司名 / 包名 / 命名空间根 / 启动场景名。
/// 命名空间根改动会重写 Assets/GameMain/Scripts 下的源码，因此必须最后执行。
/// </summary>
public static class BurnicleProjectSetupService
{
    public const string ScriptsRoot = "Assets/GameMain/Scripts";
    public const string ScenesFolder = "Assets/GameMain/Scenes";

    private const string DefaultNamespaceRoot = "Template";
    private const string DefaultLaunchSceneName = "Launcher";

    private static readonly Regex NamespaceRootPattern =
        new Regex(@"^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)*$");

    public static bool IsValidNamespaceRoot(string root)
    {
        return !string.IsNullOrEmpty(root) && NamespaceRootPattern.IsMatch(root);
    }

    /// <summary>
    /// 统计命名空间根改名会改写的源文件数（只统计，不改动）。
    /// </summary>
    public static int CountFilesWithNamespaceRoot(string root)
    {
        if (string.IsNullOrEmpty(root) || !Directory.Exists(ScriptsRoot))
        {
            return 0;
        }

        var pattern = new Regex($@"\b{Regex.Escape(root)}\b");
        var count = 0;
        foreach (var path in Directory.GetFiles(ScriptsRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (pattern.IsMatch(File.ReadAllText(path)))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// 应用设置，返回逐条改动说明。
    /// </summary>
    public static List<string> Apply(string productName, string companyName, string bundleIdentifier,
        string namespaceRoot, string launchSceneName)
    {
        var report = new List<string>();
        if (!IsValidNamespaceRoot(namespaceRoot))
        {
            report.Add($"命名空间根 '{namespaceRoot}' 不是合法的 C# 命名空间，已中止。");
            return report;
        }

        if (string.IsNullOrWhiteSpace(launchSceneName))
        {
            report.Add("启动场景名不能为空，已中止。");
            return report;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            report.Add("已取消：当前场景有未保存修改。");
            return report;
        }

        var current = BurnicleSetupState.Load();
        var oldRoot = string.IsNullOrEmpty(current.NamespaceRoot) ? DefaultNamespaceRoot : current.NamespaceRoot;
        var oldSceneName = string.IsNullOrEmpty(current.LaunchSceneName) ? DefaultLaunchSceneName : current.LaunchSceneName;

        var oldScenePath = $"{ScenesFolder}/{oldSceneName}.unity";
        var newScenePath = $"{ScenesFolder}/{launchSceneName}.unity";
        var scenePath = RenameLaunchScene(oldScenePath, newScenePath, report);

        ApplyPlayerSettings(productName, companyName, bundleIdentifier, report);

        if (scenePath != null && !string.Equals(oldRoot, namespaceRoot, StringComparison.Ordinal))
        {
            UpdateSerializedTypeNames(scenePath, oldRoot, namespaceRoot, report);
            RenameNamespaceRoot(oldRoot, namespaceRoot, report);
        }

        BurnicleSetupState.Save(new BurnicleSetupData
        {
            ProductName = productName,
            CompanyName = companyName,
            BundleIdentifier = bundleIdentifier,
            NamespaceRoot = namespaceRoot,
            LaunchSceneName = launchSceneName,
        });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return report;
    }

    private static string RenameLaunchScene(string oldScenePath, string newScenePath, List<string> report)
    {
        if (string.Equals(oldScenePath, newScenePath, StringComparison.Ordinal) || !File.Exists(oldScenePath))
        {
            return oldScenePath;
        }

        if (File.Exists(newScenePath))
        {
            report.Add($"场景已存在，未改名：{newScenePath}");
            return oldScenePath;
        }

        var error = AssetDatabase.MoveAsset(oldScenePath, newScenePath);
        if (!string.IsNullOrEmpty(error))
        {
            report.Add($"场景改名失败：{error}");
            return oldScenePath;
        }

        ReplaceBuildScene(oldScenePath, newScenePath);
        report.Add($"启动场景改名：{oldScenePath} → {newScenePath}");
        return newScenePath;
    }

    private static void ReplaceBuildScene(string oldScenePath, string newScenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var scene in scenes)
        {
            if (string.Equals(scene.path, oldScenePath, StringComparison.Ordinal))
            {
                scene.path = newScenePath;
            }
        }

        EditorBuildSettings.scenes = scenes;
    }

    private static void ApplyPlayerSettings(string productName, string companyName, string bundleIdentifier,
        List<string> report)
    {
        if (!string.IsNullOrEmpty(productName) && PlayerSettings.productName != productName)
        {
            PlayerSettings.productName = productName;
            report.Add($"产品名：{productName}");
        }

        if (!string.IsNullOrEmpty(companyName) && PlayerSettings.companyName != companyName)
        {
            PlayerSettings.companyName = companyName;
            report.Add($"公司名：{companyName}");
        }

        if (string.IsNullOrEmpty(bundleIdentifier))
        {
            return;
        }

        var groups = new[]
        {
            BuildTargetGroup.Android, BuildTargetGroup.iOS, BuildTargetGroup.Standalone, BuildTargetGroup.WebGL
        };
        foreach (var group in groups.Where(group =>
                     PlayerSettings.GetApplicationIdentifier(group) != bundleIdentifier))
        {
            PlayerSettings.SetApplicationIdentifier(group, bundleIdentifier);
            report.Add($"包名（{group}）：{bundleIdentifier}");
        }
    }

    /// <summary>
    /// 把场景里以旧命名空间开头的字符串（各种 Helper、流程的类型全名）换成新命名空间。
    /// </summary>
    private static void UpdateSerializedTypeNames(string scenePath, string oldRoot, string newRoot,
        List<string> report)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var changed = false;

        foreach (var component in scene.GetRootGameObjects()
                     .SelectMany(root => root.GetComponentsInChildren<Component>(true)))
        {
            if (component == null)
            {
                continue;
            }

            var serializedObject = new SerializedObject(component);
            var propertyPaths = new List<string>();
            var iterator = serializedObject.GetIterator();
            while (iterator.NextVisible(true))
            {
                if (iterator.propertyType != SerializedPropertyType.String ||
                    string.IsNullOrEmpty(iterator.stringValue) ||
                    !iterator.stringValue.StartsWith(oldRoot + ".", StringComparison.Ordinal))
                {
                    continue;
                }

                propertyPaths.Add(iterator.propertyPath);
            }

            foreach (var propertyPath in propertyPaths)
            {
                var property = serializedObject.FindProperty(propertyPath);
                property.stringValue = newRoot + property.stringValue.Substring(oldRoot.Length);
                serializedObject.ApplyModifiedProperties();
                changed = true;
                report.Add($"类型名：{component.GetType().Name}.{property.name} → {property.stringValue}");
            }
        }

        if (changed)
        {
            EditorSceneManager.SaveScene(scene);
        }
    }

    /// <summary>
    /// 重写项目源码的命名空间根。会触发重新编译，所以放在最后执行。
    /// </summary>
    private static void RenameNamespaceRoot(string oldRoot, string newRoot, List<string> report)
    {
        var pattern = new Regex($@"\b{Regex.Escape(oldRoot)}\b");
        var changedFiles = 0;

        foreach (var path in Directory.GetFiles(ScriptsRoot, "*.cs", SearchOption.AllDirectories))
        {
            var bytes = File.ReadAllBytes(path);
            var hasBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
            var text = (hasBom ? Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3) : Encoding.UTF8.GetString(bytes))
                .Replace("\r\n", "\n").Replace("\r", "\n");

            if (!pattern.IsMatch(text))
            {
                continue;
            }

            var replaced = pattern.Replace(text, newRoot);
            var content = Encoding.UTF8.GetBytes(replaced.Replace("\n", "\r\n"));
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                stream.Write(new byte[] { 0xEF, 0xBB, 0xBF }, 0, 3);
                stream.Write(content, 0, content.Length);
            }

            changedFiles++;
        }

        report.Add(changedFiles == 0
            ? $"命名空间根：没有找到 '{oldRoot}'，未改动源码"
            : $"命名空间根：{oldRoot} → {newRoot}，改写 {changedFiles} 个源文件");
    }
}
