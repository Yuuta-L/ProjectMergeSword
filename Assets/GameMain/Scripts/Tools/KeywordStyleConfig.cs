using System;
using BurnicleFramework.Runtime;
using UnityEngine;

namespace EndlessAbyss
{
    [CreateAssetMenu(fileName = "KeywordStyleConfig", menuName = "EndlessAbyss/Keyword Style Config")]
    public class KeywordStyleConfig : ScriptableObject
    {
        public KeywordStyle[] keywords;
    }

    [Serializable]
    public class KeywordStyle
    {
        public string keyword;
        public Color color = Color.white;
        public bool hasUnderline;
        [LocalizationScan("Keyword.Des.", ConditionField = "hasUnderline")]
        public string linkId;
    }
}
