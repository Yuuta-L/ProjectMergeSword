using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MergeSword
{
    /// <summary>
    /// 可进入场景的来源：固定名字的目录（<see cref="ScanRoots"/>）下的所有场景，不看 Build Settings。
    /// <para>
    /// 新场景放进 <c>Assets/GameMain</c>（或可选包的 <c>Assets/BurnicleFramework/UtilityBundles/Extracted/&lt;包&gt;/Sample/</c>），
    /// 就会自动出现在启动界面的入口列表里，不需要手工往 Build Settings 拖。
    /// </para>
    /// <para>编辑器里用 AssetDatabase 扫目录；构建出来的包里没有 AssetDatabase，退回读 Build Settings。</para>
    /// </summary>
    public static class LaunchSceneCatalog
    {
        /// <summary>固定扫描根目录：框架目录 + 工程目录。</summary>
        private static readonly string[] ScanRoots = { "Assets/BurnicleFramework", "Assets/GameMain" };

        public static List<LaunchScene> Collect(string excludePath)
        {
#if UNITY_EDITOR
            return CollectFromFolders(excludePath);
#else
            return CollectFromBuildSettings(excludePath);
#endif
        }

#if UNITY_EDITOR
        private static List<LaunchScene> CollectFromFolders(string excludePath)
        {
            var scenes = new List<LaunchScene>();
            for (var i = 0; i < ScanRoots.Length; i++)
            {
                var root = ScanRoots[i];
                if (!UnityEditor.AssetDatabase.IsValidFolder(root))
                {
                    continue;
                }

                var guids = UnityEditor.AssetDatabase.FindAssets("t:Scene", new[] { root });
                for (var j = 0; j < guids.Length; j++)
                {
                    Add(scenes, UnityEditor.AssetDatabase.GUIDToAssetPath(guids[j]), excludePath);
                }
            }

            // 扫出来的顺序不保证稳定，按路径排一下，界面上的顺序才固定
            scenes.Sort((left, right) => string.CompareOrdinal(left.Path, right.Path));
            return scenes;
        }
#endif

        /// <summary>构建包里的退化路径：只认 Build Settings 里登记过的场景。</summary>
        private static List<LaunchScene> CollectFromBuildSettings(string excludePath)
        {
            var scenes = new List<LaunchScene>();
            var count = SceneManager.sceneCountInBuildSettings;
            for (var i = 0; i < count; i++)
            {
                Add(scenes, SceneUtility.GetScenePathByBuildIndex(i), excludePath);
            }

            return scenes;
        }

        private static void Add(List<LaunchScene> scenes, string path, string excludePath)
        {
            if (string.IsNullOrEmpty(path) || path == excludePath ||
                !path.EndsWith(".unity", System.StringComparison.Ordinal))
            {
                return;
            }

            for (var i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].Path == path)
                {
                    return;
                }
            }

            scenes.Add(new LaunchScene(path));
        }
    }
}
