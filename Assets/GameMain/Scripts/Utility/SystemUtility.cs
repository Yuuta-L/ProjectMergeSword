using System;
using UnityEngine;
using UnityEngine.UI;
using BurnicleFramework.Runtime;
using Object = UnityEngine.Object;

namespace MergeSword
{
    public enum QualityLevel { None, Low, Middle, High }

    public static class SystemUtility
    {
        private const string GRAPHIC_QUALITY_KEY = "Graphic_Quality";
        private static QualityLevel _qualityLevel = QualityLevel.None;
        
        public static QualityLevel QualityLevel
        {
            get
            {
                if (_qualityLevel == QualityLevel.None)
                {
                    if (SystemInfo.systemMemorySize <= 4096)
                    {
                        _qualityLevel = QualityLevel.Low;
                    }
                    else if (SystemInfo.systemMemorySize >= 8192)
                    {
                        _qualityLevel = QualityLevel.High;
                    }
                    else
                    {
                        _qualityLevel = QualityLevel.Middle;
                    }
                    Log.Debug($"MemorySize:{SystemInfo.systemMemorySize}  Frequency:{SystemInfo.processorFrequency}  ProcessorCount:{SystemInfo.processorCount}  QualityLevel:{_qualityLevel}", "System");
                }

#if UNITY_EDITOR
                if (UnityEditor.EditorPrefs.HasKey(GRAPHIC_QUALITY_KEY))
                {
                    _qualityLevel = (QualityLevel)Enum.Parse(typeof(QualityLevel), UnityEditor.EditorPrefs.GetString(GRAPHIC_QUALITY_KEY));
                }
#endif

                return _qualityLevel;
            }
        }

        public static void SetQualityLevel(QualityLevel qualityLevel)
        {
            if (qualityLevel == QualityLevel.Low)
            {
                Application.targetFrameRate = 60;
                QualitySettings.SetQualityLevel(0);
            }
            else if (qualityLevel == QualityLevel.Middle)
            {
                Application.targetFrameRate = 60;
                QualitySettings.SetQualityLevel(1);
            }
            else if (qualityLevel == QualityLevel.High)
            {
                Application.targetFrameRate = 60;
                QualitySettings.SetQualityLevel(2);
            }
        }

        public static float DpiConvertPx(float dpi)
        {
            return Screen.dpi / 160f * dpi;
        }
    }
}