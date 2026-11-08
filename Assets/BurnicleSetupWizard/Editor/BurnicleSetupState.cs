using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 初始化记录。用一个纯文本文件保存（不依赖 ScriptableObject 的 GUID），
/// 既作为「是否已初始化」的标记，也保存上次应用的值，供再次运行时作为旧值。
/// </summary>
internal sealed class BurnicleSetupData
{
    public string ProductName = "Burnicle Template";
    public string CompanyName = "DefaultCompany";
    public string BundleIdentifier = "com.defaultcompany.template";
    public string NamespaceRoot = "Template";
    public string LaunchSceneName = "Launcher";
    public string AppliedAt = "";
}

internal static class BurnicleSetupState
{
    private const string MarkerPath = "ProjectSettings/BurnicleProjectSetup.txt";

    public static bool Completed => File.Exists(MarkerPath);

    public static BurnicleSetupData Load()
    {
        var data = new BurnicleSetupData();
        if (!File.Exists(MarkerPath))
        {
            return data;
        }

        foreach (var line in File.ReadAllLines(MarkerPath))
        {
            var separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            var key = line.Substring(0, separator).Trim();
            var value = line.Substring(separator + 1).Trim();
            switch (key)
            {
                case "productName":
                    data.ProductName = value;
                    break;
                case "companyName":
                    data.CompanyName = value;
                    break;
                case "bundleIdentifier":
                    data.BundleIdentifier = value;
                    break;
                case "namespaceRoot":
                    data.NamespaceRoot = value;
                    break;
                case "launchSceneName":
                    data.LaunchSceneName = value;
                    break;
                case "appliedAt":
                    data.AppliedAt = value;
                    break;
            }
        }

        return data;
    }

    public static void Save(BurnicleSetupData data)
    {
        var lines = new List<string>
        {
            $"productName={data.ProductName}",
            $"companyName={data.CompanyName}",
            $"bundleIdentifier={data.BundleIdentifier}",
            $"namespaceRoot={data.NamespaceRoot}",
            $"launchSceneName={data.LaunchSceneName}",
            $"appliedAt={DateTime.Now:yyyy-MM-dd HH:mm:ss}",
        };

        File.WriteAllLines(MarkerPath, lines);
    }
}
