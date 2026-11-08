using BurnicleFramework.Runtime;
using GameFramework;

namespace MergeSword
{
    /// <summary>
    /// 业务代码相关拓展
    /// </summary>
    public static class SettingExtension
    {
        /// <summary>
        /// 设置音乐/音效/震动是否被静音
        /// </summary>
        public static void SetMediaMute(this SettingComponent settingComponent, string groupName, bool isOn)
        {
            string key = Utility.Text.Format("Sound.{0}.Mute", groupName);
            var mediaGp = Burnicle.Sound.GetSoundGroup(groupName);
            if (null == mediaGp)
            {
                return;
            }

            mediaGp.Mute = isOn;
            settingComponent.SetBool(key, isOn);
        }

        /// <summary>
        /// 获取音乐/音效/震动是否静音
        /// </summary>
        /// <param name="settingComponent"></param>
        /// <param name="groupName"></param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static bool GetMediaMute(this SettingComponent settingComponent, string groupName, bool defaultValue = true)
        {
            string key = Utility.Text.Format("Sound.{0}.Mute", groupName);
            return settingComponent.GetBool(key, defaultValue);
        }

        /// <summary>
        /// 设置音乐/音效音量
        /// </summary>
        /// <param name="settingComponent"></param>
        /// <param name="groupName"></param>
        /// <param name="volume"></param>
        public static void SetMediaVolume(this SettingComponent settingComponent, string groupName, float volume)
        {
            string key = Utility.Text.Format("Sound.{0}.Volume", groupName);
            var soundGp = Burnicle.Sound.GetSoundGroup(groupName);
            if (null == soundGp)
            {
                return;
            }

            soundGp.Volume = volume;
            settingComponent.SetFloat(key, soundGp.Volume);
        }

        /// <summary>
        /// 获取音乐/音效音量
        /// </summary>
        /// <param name="com"></param>
        /// <param name="groupName"></param>
        /// <param name="defaultVolume">默认音量0-1</param>
        /// <returns></returns>
        public static float GetMediaVolume(this SettingComponent com, string groupName, float defaultVolume = 1f)
        {
            string key = Utility.Text.Format("Sound.{0}.Volume", groupName);

            return com.GetFloat(key, defaultVolume);
        }
        
        /// <summary>
        /// 设置A/B测试组
        /// </summary>
        public static void SetABTestGroup(this SettingComponent settingComponent, string groupName)
        {
            settingComponent.SetString(Constant.Setting.ABTestGroup, groupName);
        }

        /// <summary>
        /// 获取A/B测试组
        /// </summary>
        /// <returns></returns>
        public static string GetABTestGroup(this SettingComponent settingComponent)
        {
            return settingComponent.GetString(Constant.Setting.ABTestGroup, string.Empty);
        }

        /// <summary>
        /// 设置语言
        /// </summary>
        public static void SetLanguage(this SettingComponent com, GameFramework.Localization.Language language, bool saveSetting = true)
        {
            Burnicle.Localization.Language = language;
            com.SetString(Constant.Setting.Language, language.ToString());
        }

        /// <summary>
        /// 获取当前设置的语言
        /// </summary>
        /// <returns></returns>
        public static GameFramework.Localization.Language GetLanguage(this SettingComponent settingComponent)
        {
            string language = settingComponent.GetString(Constant.Setting.Language, string.Empty);
            if (string.IsNullOrEmpty(language))
            {
                return GameFramework.Localization.Language.Unspecified;
            }

            if (!System.Enum.TryParse(language, out GameFramework.Localization.Language result))
            {
                result = GameFramework.Localization.Language.English;
            }

            return result;
        }
    }
}