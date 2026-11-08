using System;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace EndlessAbyss
{
    public static class KeywordProcessor
    {
        private static KeywordStyleConfig _config;
        private static bool _loadAttempted;

        /// <summary>点击关键词时的回调，参数为 linkId 和点击屏幕位置</summary>
        public static event Action<string, Vector2> OnKeywordClicked;

        /// <summary>供外部代码触发链接点击事件（event 只能由声明类内部 Invoke）</summary>
        public static void FireKeywordClicked(string linkId, Vector2 clickPosition)
        {
            OnKeywordClicked?.Invoke(linkId, clickPosition);
        }

        private static void EnsureConfig()
        {
            if (_config != null || _loadAttempted) return;
            _loadAttempted = true;
            _config = Resources.Load<KeywordStyleConfig>("Configs/KeywordStyleConfig");
            if (_config == null)
                Debug.LogWarning("[KeywordProcessor] Configs/KeywordStyleConfig not found in Resources.");
        }

        public static void SetConfig(KeywordStyleConfig config)
        {
            _config = config;
            _loadAttempted = true;
        }

        /// <summary>将纯文本中的关键词替换为 TMP 富文本标签</summary>
        public static string Process(string rawText)
        {
            EnsureConfig();
            if (string.IsNullOrEmpty(rawText) || _config?.keywords == null || _config.keywords.Length == 0)
                return rawText;

            var sorted = _config.keywords
                .Where(k => !string.IsNullOrEmpty(k.keyword))
                .OrderByDescending(k => k.keyword.Length)
                .ToList();

            if (sorted.Count == 0)
                return rawText;

            var pattern = string.Join("|", sorted.Select(k => Regex.Escape(k.keyword)));

            return Regex.Replace(rawText, pattern, match =>
            {
                var matched = match.Value;
                var kw = sorted.First(k => k.keyword == matched);

                var linkId = string.IsNullOrEmpty(kw.linkId) ? $"kw_{matched}" : kw.linkId;
                var hexColor = ColorUtility.ToHtmlStringRGB(kw.color);

                return kw.hasUnderline
                    ? $"<color=#{hexColor}><u><link={linkId}>{matched}</link></u></color>"
                    : $"<color=#{hexColor}>{matched}</color>";
            });
        }

        /// <summary>为 TMP 文本添加链接点击检测</summary>
        /// <param name="forwardNonLinkClickToParent">未命中 link 的点击是否转发给父级</param>
        public static void SetupClickHandler(TMP_Text tmpText, bool forwardNonLinkClickToParent = false)
        {
            var handler = tmpText.GetComponent<TMPLinkClickHandler>();
            if (handler == null)
                handler = tmpText.gameObject.AddComponent<TMPLinkClickHandler>();
            handler.Setup(tmpText, forwardNonLinkClickToParent);
        }
    }
}
