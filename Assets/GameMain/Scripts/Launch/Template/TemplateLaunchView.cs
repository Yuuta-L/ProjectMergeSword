using System;
using System.Collections.Generic;
using BurnicleFramework.Runtime;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MergeSword
{
    /// <summary>
    /// 模板启动界面（内建图形 + 纯色块 + 文本，列表在运行时生成）。
    /// <para>
    /// 与启动流程只通过 <see cref="ILaunchView"/> 交互：本组件删掉后流程仍能正常工作，
    /// 项目可以自己实现 ILaunchView 顶替它。
    /// </para>
    /// </summary>
    public class TemplateLaunchView : MonoBehaviour, ILaunchView
    {
        [Header("基础显示")]
        public Image background;
        public TextMeshProUGUI versionText;
        public Image title;
        public Slider progress;
        public TextMeshProUGUI tipText;
        public Button closeButton;
        public GameObject launchCamera;
        public GameObject orna;

        [Header("场景入口")]
        [Tooltip("场景入口列表的容器（列表内容在运行时生成）")]
        public RectTransform sceneEntryRoot;
        [Tooltip("单行按钮用的九宫格图片，留空则为纯色块")]
        public Sprite entryRowSprite;

        private const float RowHeight = 96f;
        private const float RowGap = 12f;

        private static readonly Color RowColor = new Color(0.19f, 0.22f, 0.30f);
        private static readonly Color RowLabelColor = new Color(0.92f, 0.94f, 0.99f);
        private static readonly Color HintColor = new Color(0.60f, 0.64f, 0.72f);

        private readonly List<GameObject> _rows = new List<GameObject>();

        private bool _finished;
        private Tweener _progressTweener;
        private ScrollRect _scroll;
        private RectTransform _content;
        private TextMeshProUGUI _hint;

        public event Action<string> SceneRequested;
        public event Action Closed;

        private void Awake()
        {
            Launch.Register(this);
            _finished = false;
            if (versionText != null)
            {
                versionText.text = $"v{RuntimeCrossPlatformSettings.Load().Version}";
            }

            AdaptBackground();
            SetProgressVisible(false);
            if (sceneEntryRoot != null)
            {
                sceneEntryRoot.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            _progressTweener?.Kill();
            Launch.Unregister(this);
        }

        private void Start()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClick);
            }
        }

        private void AdaptBackground()
        {
            if (background == null)
            {
                return;
            }

            // cover 模式：取宽高比最大值，确保背景在任何屏幕比例下都能完全覆盖不留空白
            var designRatio = 1920f / 1080f;
            var screenRatio = ScreenUtility.ScreenHeight() / ScreenUtility.ScreenWidth();
            var scale = Mathf.Max(1f, screenRatio / designRatio);
            background.transform.localScale = Vector3.one * scale;
        }

        // ---------------------------------------------------------------- ILaunchView

        public void ShowProgress(float progressValue, string tip)
        {
            if (_finished)
            {
                return;
            }

            if (progress != null && progressValue >= 0f)
            {
                SetProgressVisible(true);
                progressValue = Mathf.Clamp01(progressValue);
                _progressTweener?.Kill();
                if (progressValue <= progress.value)
                {
                    progress.value = progressValue;
                }
                else
                {
                    _progressTweener = DOTween.To(
                        () => progress.value,
                        x => progress.value = x,
                        progressValue,
                        0.15f
                    ).SetEase(Ease.OutCubic);
                }
            }

            SetTip(tip);
        }

        public void SetTip(string text)
        {
            if (_finished || tipText == null)
            {
                return;
            }

            tipText.text = text;
        }

        public void ShowSceneEntry(IReadOnlyList<LaunchScene> scenes)
        {
            _finished = true;
            _progressTweener?.Kill();
            SetProgressVisible(false);
            if (sceneEntryRoot == null)
            {
                return;
            }

            sceneEntryRoot.gameObject.SetActive(true);
            BuildListIfNeeded();
            FillList(scenes);

            if (tipText != null)
            {
                tipText.text = string.Empty;
            }
        }

        public void Close()
        {
            Closed?.Invoke();
            Closed = null;
            SceneRequested = null;

            var canvas = GetComponentInParent<Canvas>();
            Destroy(canvas != null ? canvas.gameObject : gameObject);
        }

        // ---------------------------------------------------------------- 场景入口列表

        private void BuildListIfNeeded()
        {
            if (_scroll != null)
            {
                return;
            }

            // 滚动区域：固定高度 + 竖向滚动，场景数量多了也只占同一块位置
            var scrollRect = CreateRect(sceneEntryRoot, "Scroll");
            Stretch(scrollRect);
            var scrollImage = scrollRect.gameObject.AddComponent<Image>(); // 只用于接收滚轮/拖拽事件
            scrollImage.color = new Color(0f, 0f, 0f, 0f);
            _scroll = scrollRect.gameObject.AddComponent<ScrollRect>();
            _scroll.horizontal = false;
            _scroll.vertical = true;
            _scroll.movementType = ScrollRect.MovementType.Clamped;
            _scroll.scrollSensitivity = 30f;

            var viewport = CreateRect(scrollRect, "Viewport");
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();

            _content = CreateRect(viewport, "Content");
            _content.anchorMin = new Vector2(0f, 1f);
            _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.anchoredPosition = Vector2.zero;
            _content.sizeDelta = Vector2.zero;

            var layout = _content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = RowGap;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = _content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _scroll.viewport = viewport;
            _scroll.content = _content;
        }

        private void FillList(IReadOnlyList<LaunchScene> scenes)
        {
            for (var i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] != null)
                {
                    Destroy(_rows[i]);
                }
            }

            _rows.Clear();

            var count = scenes?.Count ?? 0;
            if (count == 0)
            {
                _scroll.gameObject.SetActive(false);
                ShowHint("No scene to enter.\nAdd scenes to Build Settings and they will show up here.");
                return;
            }

            HideHint();
            _scroll.gameObject.SetActive(true);
            for (var i = 0; i < count; i++)
            {
                CreateRow(scenes[i]);
            }

            _scroll.verticalNormalizedPosition = 1f;
        }

        private void CreateRow(LaunchScene scene)
        {
            var rowRect = CreateRect(_content, scene.DisplayName);
            var image = rowRect.gameObject.AddComponent<Image>();
            image.color = RowColor;
            image.raycastTarget = true;
            if (entryRowSprite != null)
            {
                image.sprite = entryRowSprite;
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = 1f;
            }

            var layoutElement = rowRect.gameObject.AddComponent<LayoutElement>();
            layoutElement.minHeight = RowHeight;
            layoutElement.preferredHeight = RowHeight;

            var button = rowRect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var path = scene.Path;
            button.onClick.AddListener(() => OnSceneRowClicked(path));

            var labelRect = CreateRect(rowRect, "Label");
            Stretch(labelRect);
            var label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = ResolveFont();
            label.text = scene.DisplayName;
            label.fontSize = 34f;
            label.color = RowLabelColor;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = false;
            label.raycastTarget = false;

            _rows.Add(rowRect.gameObject);
        }

        private void OnSceneRowClicked(string scenePath)
        {
            SceneRequested?.Invoke(scenePath);
        }

        private void ShowHint(string text)
        {
            if (_hint == null)
            {
                var hintRect = CreateRect(sceneEntryRoot, "Hint");
                Stretch(hintRect);
                _hint = hintRect.gameObject.AddComponent<TextMeshProUGUI>();
                _hint.font = ResolveFont();
                _hint.fontSize = 28f;
                _hint.color = HintColor;
                _hint.alignment = TextAlignmentOptions.Center;
                _hint.enableWordWrapping = true;
                _hint.raycastTarget = false;
            }

            _hint.text = text;
            _hint.gameObject.SetActive(true);
        }

        private void HideHint()
        {
            if (_hint != null)
            {
                _hint.gameObject.SetActive(false);
            }
        }

        private TMP_FontAsset ResolveFont()
        {
            if (tipText != null && tipText.font != null)
            {
                return tipText.font;
            }

            return TMP_Settings.defaultFontAsset;
        }

        // ---------------------------------------------------------------- 工具

        private void SetProgressVisible(bool visible)
        {
            if (orna != null)
            {
                orna.SetActive(visible);
            }

            if (progress != null)
            {
                progress.gameObject.SetActive(visible);
            }
        }

        private static RectTransform CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rectTransform = (RectTransform)go.transform;
            rectTransform.SetParent(parent, false);
            return rectTransform;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private void OnCloseButtonClick()
        {
            if (!_finished)
            {
                return;
            }

            Close();
        }
    }
}
