using BurnicleFramework.Runtime;
using GameFramework;
using UnityEngine;

namespace MergeSword
{
    public class BuiltinDataComponent : BurnicleComponent
    {
        [SerializeField]
        private TextAsset m_VersionInfoTextAsset = null;

        [SerializeField]
        private TextAsset m_DefaultDictionaryTextAsset = null;

        private VersionInfo m_VersionInfo = null;

        public VersionInfo VersionInfo
        {
            get
            {
                return m_VersionInfo;
            }
        }

        public void InitVersionInfo()
        {
            if (m_VersionInfoTextAsset == null || string.IsNullOrEmpty(m_VersionInfoTextAsset.text))
            {
                Log.Info("Build info can not be found or empty.");
                return;
            }

            m_VersionInfo = Utility.Json.ToObject<VersionInfo>(m_VersionInfoTextAsset.text);
            if (m_VersionInfo == null)
            {
                Log.Warning("Parse build info failure.");
                return;
            }
        }

        public void InitDefaultDictionary()
        {
            if (m_DefaultDictionaryTextAsset == null || string.IsNullOrEmpty(m_DefaultDictionaryTextAsset.text))
            {
                Log.Info("Default dictionary can not be found or empty.");
                return;
            }

            if (!Burnicle.Localization.ParseData(m_DefaultDictionaryTextAsset.text))
            {
                Log.Warning("Parse default dictionary failure.");
                return;
            }
        }
    }
}
