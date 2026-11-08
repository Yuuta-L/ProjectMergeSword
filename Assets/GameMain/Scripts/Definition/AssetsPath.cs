using BurnicleFramework.Runtime;
using GameFramework;

namespace MergeSword
{
    /// <summary>
    /// 资源路径约定。项目按自己的目录习惯扩展这里，不要让路径散落在业务代码里。
    /// </summary>
    public static class AssetsPath
    {
        public static string GetSpritePath(string assetName)
        {
            return Utility.Text.Format("Assets/GameMain/{0}.png", assetName);
        }

        public static string GetScenePath(string sceneName)
        {
            return Utility.Text.Format("Assets/GameMain/Scenes/{0}.unity", sceneName);
        }

        public static string GetUIFormPath(string uiFormName)
        {
            return Utility.Text.Format("Assets/GameMain/UI/UIForms/{0}.prefab", uiFormName);
        }

        public static string GetFontPath(string fontName)
        {
            return Utility.Text.Format("Assets/GameMain/Fonts/{0}.asset", fontName);
        }

        public static string GetMusicPath(string musicName)
        {
            return Utility.Text.Format("Assets/GameMain/Audio/{0}.mp3", musicName);
        }

        public static string GetSoundPath(string soundName)
        {
            return Utility.Text.Format("Assets/GameMain/Audio/{0}.wav", soundName);
        }

        public static string GetConfigFilePath(string assetName, bool fromBytes)
        {
            return GetABTestFilePath(assetName, fromBytes, _getConfigFilePath);
        }

        private static string _getConfigFilePath(string assetName, bool fromBytes)
        {
            return Utility.Text.Format("Assets/GameMain/DataFiles/Config/{0}.{1}", assetName, fromBytes ? "bytes" : "txt");
        }

        public static string GetDataTableFilePath(string assetName, bool fromBytes)
        {
            return GetABTestFilePath(assetName, fromBytes, _getDataTableFilePath);
        }

        private static string _getDataTableFilePath(string assetName, bool fromBytes)
        {
            return Utility.Text.Format("Assets/GameMain/DataFiles/DataTable/{0}.{1}", assetName, fromBytes ? "bytes" : "txt");
        }

        public static string GetLocalizationFilePath(string assetName, bool fromBytes)
        {
            return GetABTestFilePath(assetName, fromBytes, _getLocalizationFilePath);
        }

        private static string _getLocalizationFilePath(string assetName, bool fromBytes)
        {
            return Utility.Text.Format("Assets/GameMain/DataFiles/Localization/{0}.{1}", assetName, fromBytes ? "bytes" : "json");
        }

        /// <summary>
        /// ABTest 分组下存在同名资源时优先使用分组资源。
        /// </summary>
        private static string GetABTestFilePath(string assetName, bool fromBytes,
            System.Func<string, bool, string> buildPath)
        {
            var abTestGroup = Burnicle.Setting.GetABTestGroup();
            if (string.IsNullOrWhiteSpace(abTestGroup))
            {
                return buildPath(assetName, fromBytes);
            }

            var abTestAssetName = Utility.Text.Format("{0}{1}{2}", assetName, Constant.Setting.AB_TEST_TAG, abTestGroup);
            var abTestPath = buildPath(abTestAssetName, fromBytes);
            return Burnicle.Resource.HasAsset(abTestPath) != GameFramework.Resource.HasAssetResult.NotExist
                ? abTestPath
                : buildPath(assetName, fromBytes);
        }
    }
}
