using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MergeSword
{
    [RequireComponent(typeof(RectTransform))]
    public class RecyclableScrollList : MonoBehaviour
    {
        public enum Direction
        {
            Vertical,
            Horizontal
        }

        [Header("Layout Settings")]
        public Direction direction = Direction.Vertical;
        [Tooltip(">=2 enables grid layout")]
        public int constraintCount = 1;
        public RectOffset padding = new RectOffset();
        public float spacing = 0f;
        public TextAnchor alignment = TextAnchor.UpperLeft;
        [Tooltip("Reverse arrangement: bottom-to-top (vertical) or right-to-left (horizontal)")]
        public bool reverseArrangement = false;

        [Header("Item Settings")]
        public GameObject itemPrefab;
        [Tooltip("0 = auto detect from prefab RectTransform")]
        public float itemWidth = 0f;
        [Tooltip("0 = auto detect from prefab RectTransform")]
        public float itemHeight = 0f;

        [Header("Advanced")]
        [SerializeField] private int bufferCount = 2;

        // Internal state
        private RectTransform _content;
        private ScrollRect _scrollRect;
        private int _totalCount;

        private float _cachedItemWidth;
        private float _cachedItemHeight;
        private bool _sizeInitialized;

        private readonly List<ItemRecord> _activeItems = new List<ItemRecord>();
        private readonly Stack<UIItemBase> _itemPool = new Stack<UIItemBase>();
        private Action<int, UIItemBase> _updateCallback;
        private bool _dirty;
        private bool _pendingResetPosition;

        private struct ItemRecord
        {
            public int index;
            public UIItemBase item;
        }

        private void Awake()
        {
            _content = (RectTransform)transform;
            _scrollRect = GetComponentInParent<ScrollRect>();

            if (_scrollRect == null)
            {
                Debug.LogError("[RecyclableScrollList] Must be a child of a GameObject with ScrollRect!", this);
                enabled = false;
                return;
            }

            // Configure scroll direction
            if (direction == Direction.Vertical)
            {
                _scrollRect.vertical = true;
                _scrollRect.horizontal = false;
            }
            else
            {
                _scrollRect.vertical = false;
                _scrollRect.horizontal = true;
            }

            // Set content anchor/pivot based on direction and arrangement
            if (direction == Direction.Vertical)
            {
                if (reverseArrangement)
                {
                    // Bottom-stretch: content grows upward from viewport bottom
                    _content.anchorMin = new Vector2(0f, 0f);
                    _content.anchorMax = new Vector2(1f, 0f);
                    _content.pivot = new Vector2(0.5f, 0f);
                }
                else
                {
                    // Top-stretch: content grows downward from viewport top
                    _content.anchorMin = new Vector2(0f, 1f);
                    _content.anchorMax = new Vector2(1f, 1f);
                    _content.pivot = new Vector2(0.5f, 1f);
                }
            }
            else
            {
                if (reverseArrangement)
                {
                    // Right-stretch: content grows leftward from viewport right
                    _content.anchorMin = new Vector2(1f, 0f);
                    _content.anchorMax = new Vector2(1f, 1f);
                    _content.pivot = new Vector2(1f, 0.5f);
                }
                else
                {
                    // Left-stretch: content grows rightward from viewport left
                    _content.anchorMin = new Vector2(0f, 0f);
                    _content.anchorMax = new Vector2(0f, 1f);
                    _content.pivot = new Vector2(0f, 0.5f);
                }
            }
            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        private void Start()
        {
            if (!_sizeInitialized && itemPrefab != null)
            {
                InitItemSize();
                UpdateVisibleItems();
                UpdateContentSize();
                ResetScrollPosition();
            }
        }

        private void OnDestroy()
        {
            Clear();

            while (_itemPool.Count > 0)
            {
                var pooled = _itemPool.Pop();
                if (pooled != null && pooled.gameObject != null)
                    Destroy(pooled.gameObject);
            }
        }
        
        private void LateUpdate()
        {
            if (_dirty)
            {
                _dirty = false;
                UpdateVisibleItems();
            }
        }

        private void OnScrollValueChanged(Vector2 value)
        {
            // Apply deferred scroll reset here — OnScrollValueChanged is called
            // from within ScrollRect's LateUpdate, after it has processed bounds
            // and elastic physics. Resetting the position here takes effect
            // immediately, and ScrollRect's subsequent UpdatePrevData() captures
            // the corrected position as the new baseline for the next frame.
            if (_pendingResetPosition)
            {
                _pendingResetPosition = false;
                _content.anchoredPosition = Vector2.zero;
                _scrollRect.velocity = Vector2.zero;
            }
            _dirty = true;
        }

        // ===================== Public API =====================

        /// <summary>Set or change the item prefab.</summary>
        public void SetItemProvider(GameObject prefab)
        {
            itemPrefab = prefab;
            _sizeInitialized = false;
        }

        /// <summary>Set total data count (0 = empty).</summary>
        public void SetTotalCount(int count)
        {
            _totalCount = Mathf.Max(0, count);
        }

        /// <summary>
        /// Register a typed callback invoked for each visible item.
        /// Signature: (dataIndex, itemInstance) => { ... }
        /// </summary>
        public void SetItemUpdateCallback<T>(Action<int, T> callback) where T : UIItemBase
        {
            _updateCallback = (index, item) => callback(index, (T)item);
        }

        /// <summary>Rebuild the visible item set. Call after SetTotalCount / SetItemProvider / data changes.</summary>
        public void Refresh()
        {
            if (itemPrefab == null)
            {
                Debug.LogError("[RecyclableScrollList] itemPrefab is not set!", this);
                return;
            }

            if (!_sizeInitialized) InitItemSize();
            Clear();
            UpdateVisibleItems();
            UpdateContentSize();
            ResetScrollPosition();
        }

        private void ResetScrollPosition()
        {
            if (_scrollRect == null)
            {
                _content.anchoredPosition = Vector2.zero;
                return;
            }
            
            // Defer the actual position reset to OnScrollValueChanged, which is
            // called from within ScrollRect's LateUpdate after it has processed
            // bounds and elastic physics. By that point the layout is fully settled,
            // so resetting the position won't fight with ScrollRect corrections.
            _pendingResetPosition = true;
        }

        /// <summary>Get the active item at the given data index, or null if it's not currently visible.</summary>
        public T GetActiveItem<T>(int index) where T : UIItemBase
        {
            for (int i = 0; i < _activeItems.Count; i++)
            {
                if (_activeItems[i].index == index)
                    return (T)_activeItems[i].item;
            }
            return null;
        }

        /// <summary>Smooth scroll to make the item at dataIndex visible.</summary>
        public void ScrollToIndex(int index, bool instant = false)
        {
            if (_scrollRect == null || _totalCount <= 0) return;

            index = Mathf.Clamp(index, 0, _totalCount - 1);
            float step = GetItemStep();
            int groupCount = Mathf.CeilToInt((float)_totalCount / constraintCount);
            int targetGroup = index / constraintCount;

            if (direction == Direction.Vertical)
            {
                float contentHeight = _content.rect.height;
                float viewportHeight = _scrollRect.viewport.rect.height;
                float maxScroll = Mathf.Max(0f, contentHeight - viewportHeight);

                float scrollY;
                if (reverseArrangement)
                {
                    // Bottom-anchor: target position from viewport bottom
                    scrollY = Mathf.Clamp(padding.bottom + targetGroup * step, 0f, maxScroll);
                }
                else
                {
                    scrollY = Mathf.Clamp(padding.top + targetGroup * step, 0f, maxScroll);
                }

                float normalized = maxScroll > 0f ? scrollY / maxScroll : 0f;
                _scrollRect.verticalNormalizedPosition = normalized;
            }
            else
            {
                float contentWidth = _content.rect.width;
                float viewportWidth = _scrollRect.viewport.rect.width;
                float maxScroll = Mathf.Max(0f, contentWidth - viewportWidth);

                float scrollX;
                if (reverseArrangement)
                {
                    // Right-anchor: target position from viewport right (negative leftward)
                    scrollX = Mathf.Clamp(padding.right + targetGroup * step, 0f, maxScroll);
                }
                else
                {
                    scrollX = Mathf.Clamp(padding.left + targetGroup * step, 0f, maxScroll);
                }

                float normalized = maxScroll > 0f ? scrollX / maxScroll : 0f;
                _scrollRect.horizontalNormalizedPosition = normalized;
            }
        }

        /// <summary>Smooth scroll to make the item at dataIndex visible over the given duration.</summary>
        public void ScrollToIndexAnimated(int index, float duration)
        {
            if (_scrollRect == null || _totalCount <= 0 || duration <= 0f) return;

            index = Mathf.Clamp(index, 0, _totalCount - 1);
            float target = CalculateNormalizedPosition(index);

            if (direction == Direction.Vertical)
                DOTween.To(() => _scrollRect.verticalNormalizedPosition,
                    v => _scrollRect.verticalNormalizedPosition = v,
                    target, duration).SetEase(Ease.OutCubic);
            else
                DOTween.To(() => _scrollRect.horizontalNormalizedPosition,
                    v => _scrollRect.horizontalNormalizedPosition = v,
                    target, duration).SetEase(Ease.OutCubic);
        }

        /// <summary>Calculate the normalized scroll position (0-1) for a given data index.</summary>
        private float CalculateNormalizedPosition(int index)
        {
            float step = GetItemStep();
            int targetGroup = index / constraintCount;

            float contentSize = direction == Direction.Vertical ? _content.rect.height : _content.rect.width;
            float viewportSize = direction == Direction.Vertical
                ? _scrollRect.viewport.rect.height
                : _scrollRect.viewport.rect.width;
            float maxScroll = Mathf.Max(0f, contentSize - viewportSize);

            float scrollPos;
            if (direction == Direction.Vertical)
            {
                if (reverseArrangement)
                    scrollPos = Mathf.Clamp(padding.bottom + targetGroup * step, 0f, maxScroll);
                else
                    scrollPos = Mathf.Clamp(padding.top + targetGroup * step, 0f, maxScroll);
            }
            else
            {
                if (reverseArrangement)
                    scrollPos = Mathf.Clamp(padding.right + targetGroup * step, 0f, maxScroll);
                else
                    scrollPos = Mathf.Clamp(padding.left + targetGroup * step, 0f, maxScroll);
            }

            return maxScroll > 0f ? scrollPos / maxScroll : 0f;
        }

        /// <summary>Return all active items to the pool.</summary>
        public void Clear()
        {
            for (int i = _activeItems.Count - 1; i >= 0; i--)
            {
                RecycleItem(i);
            }
            _activeItems.Clear();
        }

        /// <summary>Refresh a single visible item by data index. Does nothing if the index is not currently visible.</summary>
        public void RefreshItem(int dataIndex)
        {
            for (int i = 0; i < _activeItems.Count; i++)
            {
                if (_activeItems[i].index == dataIndex)
                {
                    // Recycle current item at this index
                    var oldItem = _activeItems[i].item;
                    oldItem.OnUnspawn();
                    oldItem.transform.SetParent(_content, false);
                    _itemPool.Push(oldItem);
                    _activeItems.RemoveAt(i);

                    // Spawn a new (or pooled) item at the same position
                    var newItem = SpawnItem();
                    if (newItem == null) return;

                    var itemRect = (RectTransform)newItem.transform;
                    newItem.OnSpawn();

                    // Apply the same positioning logic as UpdateVisibleItems
                    if (direction == Direction.Vertical && !reverseArrangement)
                    {
                        itemRect.anchorMin = new Vector2(0f, 1f);
                        itemRect.anchorMax = new Vector2(0f, 1f);
                    }
                    else if (direction == Direction.Horizontal && reverseArrangement)
                    {
                        itemRect.anchorMin = new Vector2(1f, 0f);
                        itemRect.anchorMax = new Vector2(1f, 0f);
                    }
                    else
                    {
                        itemRect.anchorMin = Vector2.zero;
                        itemRect.anchorMax = Vector2.zero;
                    }
                    itemRect.pivot = new Vector2(0.5f, 0.5f);
                    itemRect.sizeDelta = new Vector2(_cachedItemWidth, _cachedItemHeight);
                    itemRect.anchoredPosition = GetItemPosition(dataIndex);

                    _activeItems.Add(new ItemRecord { index = dataIndex, item = newItem });

                    if (_updateCallback != null)
                    {
                        try { _updateCallback(dataIndex, newItem); }
                        catch (Exception e)
                        {
                            Debug.LogError($"[RecyclableScrollList] Update callback error at index {dataIndex}: {e}", this);
                        }
                    }
                    return;
                }
            }
        }

        // ===================== Internal =====================

        private void InitItemSize()
        {
            if (itemPrefab == null) return;

            var rect = itemPrefab.GetComponent<RectTransform>();
            if (rect == null) return;

            _cachedItemWidth = itemWidth > 0f ? itemWidth : rect.rect.width;
            _cachedItemHeight = itemHeight > 0f ? itemHeight : rect.rect.height;
            _sizeInitialized = true;
        }

        private float GetItemStep()
        {
            return (direction == Direction.Vertical ? _cachedItemHeight : _cachedItemWidth) + spacing;
        }

        private void UpdateContentSize()
        {
            if (_totalCount <= 0)
            {
                if (direction == Direction.Vertical)
                    _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);
                else
                    _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
                return;
            }

            int groupCount = Mathf.CeilToInt((float)_totalCount / constraintCount);
            float step = GetItemStep();

            if (direction == Direction.Vertical)
            {
                // Content width = viewport width
                float viewportWidth = _scrollRect.viewport.rect.width;
                _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewportWidth);

                float height = padding.top + groupCount * _cachedItemHeight
                               + (groupCount - 1) * spacing + padding.bottom;
                _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            }
            else
            {
                // Content height = viewport height
                float viewportHeight = _scrollRect.viewport.rect.height;
                _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, viewportHeight);

                float width = padding.left + groupCount * _cachedItemWidth
                              + (groupCount - 1) * spacing + padding.right;
                _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
        }

        private void UpdateVisibleItems()
        {
            if (_totalCount <= 0 || _scrollRect == null || !_sizeInitialized)
                return;

            int firstVisible, lastVisible;
            CalculateVisibleRange(out firstVisible, out lastVisible);

            // Recycle items that scrolled out of view
            for (int i = _activeItems.Count - 1; i >= 0; i--)
            {
                int idx = _activeItems[i].index;
                if (idx < firstVisible || idx > lastVisible)
                {
                    RecycleItem(i);
                    _activeItems.RemoveAt(i);
                }
            }

            // Spawn items that entered the view
            for (int idx = firstVisible; idx <= lastVisible; idx++)
            {
                if (IsIndexActive(idx)) continue;

                var item = SpawnItem();
                if (item == null) continue;

                var itemRect = (RectTransform)item.transform;

                // OnSpawn must be called BEFORE setting anchoredPosition,
                // because UIItemBase.OnSpawn resets transform.localPosition to zero.
                item.OnSpawn();

                // Set item anchor based on direction and arrangement.
                // For vertical non-reverse (top-stretch), use top-left so negative Y goes downward.
                // For vertical reverse (bottom-stretch), use bottom-left.
                // For horizontal non-reverse (left-stretch), use bottom-left.
                // For horizontal reverse (right-stretch), use bottom-right.
                if (direction == Direction.Vertical && !reverseArrangement)
                {
                    itemRect.anchorMin = new Vector2(0f, 1f);
                    itemRect.anchorMax = new Vector2(0f, 1f);
                }
                else if (direction == Direction.Horizontal && reverseArrangement)
                {
                    itemRect.anchorMin = new Vector2(1f, 0f);
                    itemRect.anchorMax = new Vector2(1f, 0f);
                }
                else
                {
                    itemRect.anchorMin = Vector2.zero;
                    itemRect.anchorMax = Vector2.zero;
                }
                itemRect.pivot = new Vector2(0.5f, 0.5f);
                itemRect.sizeDelta = new Vector2(_cachedItemWidth, _cachedItemHeight);
                itemRect.anchoredPosition = GetItemPosition(idx);

                _activeItems.Add(new ItemRecord { index = idx, item = item });

                if (_updateCallback != null)
                {
                    try { _updateCallback(idx, item); }
                    catch (Exception e)
                    {
                        Debug.LogError($"[RecyclableScrollList] Update callback error at index {idx}: {e}", this);
                    }
                }
            }
        }

        private void CalculateVisibleRange(out int firstIdx, out int lastIdx)
        {
            int groupCount = Mathf.CeilToInt((float)_totalCount / constraintCount);
            float itemStep = GetItemStep();

            if (reverseArrangement)
            {
                // Items positioned from the anchor edge (bottom/right) upward/leftward
                float viewportSize;
                float scrollOffset;

                if (direction == Direction.Vertical)
                {
                    viewportSize = _scrollRect.viewport.rect.height;
                    scrollOffset = Mathf.Abs(_content.anchoredPosition.y);
                }
                else
                {
                    viewportSize = _scrollRect.viewport.rect.width;
                    scrollOffset = Mathf.Abs(_content.anchoredPosition.x);
                }

                int firstGroup = Mathf.FloorToInt((scrollOffset - GetItemSize()) / itemStep) - bufferCount;
                int lastGroup = Mathf.CeilToInt((scrollOffset + viewportSize) / itemStep) + bufferCount - 1;

                firstGroup = Mathf.Max(0, firstGroup);
                lastGroup = Mathf.Min(groupCount - 1, lastGroup);

                firstIdx = firstGroup * constraintCount;
                lastIdx = Mathf.Min((lastGroup + 1) * constraintCount - 1, _totalCount - 1);
            }
            else
            {
                // Forward: items from top/left downward/rightward
                float viewportSize;
                float scrollOffset;

                if (direction == Direction.Vertical)
                {
                    viewportSize = _scrollRect.viewport.rect.height;
                    // Use absolute value: normal scroll (anchoredPos.y < 0) gives positive offset;
                    // elastic overscroll (anchoredPos.y > 0) also gives positive offset.
                    scrollOffset = Mathf.Abs(_content.anchoredPosition.y);
                }
                else
                {
                    viewportSize = _scrollRect.viewport.rect.width;
                    scrollOffset = Mathf.Abs(_content.anchoredPosition.x);
                }

                float startOffset = scrollOffset - padding.top;
                float endOffset = scrollOffset + viewportSize - padding.top;

                int firstGroup = Mathf.FloorToInt(startOffset / itemStep) - bufferCount;
                int lastGroup = Mathf.CeilToInt(endOffset / itemStep) + bufferCount - 1;

                firstGroup = Mathf.Max(0, firstGroup);
                lastGroup = Mathf.Min(groupCount - 1, lastGroup);

                firstIdx = firstGroup * constraintCount;
                lastIdx = Mathf.Min((lastGroup + 1) * constraintCount - 1, _totalCount - 1);
            }
        }

        private float GetItemSize()
        {
            return direction == Direction.Vertical ? _cachedItemHeight : _cachedItemWidth;
        }

        private Vector2 GetItemPosition(int index)
        {
            int group = index / constraintCount;    // row (vertical) or col (horizontal)
            int posInGroup = index % constraintCount;

            if (direction == Direction.Vertical)
            {
                float groupWidth = constraintCount * _cachedItemWidth
                                   + (constraintCount - 1) * spacing;
                float contentWidth = _content.rect.width;

                float x;
                if (alignment == TextAnchor.UpperLeft
                    || alignment == TextAnchor.MiddleLeft
                    || alignment == TextAnchor.LowerLeft)
                {
                    x = padding.left + posInGroup * (_cachedItemWidth + spacing) + _cachedItemWidth * 0.5f;
                }
                else if (alignment == TextAnchor.UpperCenter
                         || alignment == TextAnchor.MiddleCenter
                         || alignment == TextAnchor.LowerCenter)
                {
                    x = (contentWidth - groupWidth) * 0.5f
                        + posInGroup * (_cachedItemWidth + spacing)
                        + _cachedItemWidth * 0.5f;
                }
                else // Right
                {
                    x = contentWidth - padding.right - groupWidth
                        + posInGroup * (_cachedItemWidth + spacing)
                        + _cachedItemWidth * 0.5f;
                }

                float y;
                if (reverseArrangement)
                {
                    // Bottom-to-top with bottom-anchor content: y positive upward
                    y = padding.bottom + group * (_cachedItemHeight + spacing) + _cachedItemHeight * 0.5f;
                }
                else
                {
                    // Top-to-bottom with top-anchor content: y negative downward
                    y = -(padding.top + group * (_cachedItemHeight + spacing) + _cachedItemHeight * 0.5f);
                }
                return new Vector2(x, y);
            }
            else
            {
                float groupHeight = constraintCount * _cachedItemHeight
                                    + (constraintCount - 1) * spacing;
                float contentHeight = _content.rect.height;

                float y;
                if (alignment == TextAnchor.UpperLeft
                    || alignment == TextAnchor.UpperCenter
                    || alignment == TextAnchor.UpperRight)
                {
                    y = -(padding.top + posInGroup * (_cachedItemHeight + spacing) + _cachedItemHeight * 0.5f);
                }
                else if (alignment == TextAnchor.MiddleLeft
                         || alignment == TextAnchor.MiddleCenter
                         || alignment == TextAnchor.MiddleRight)
                {
                    y = -((contentHeight - groupHeight) * 0.5f
                          + posInGroup * (_cachedItemHeight + spacing)
                          + _cachedItemHeight * 0.5f);
                }
                else // Lower
                {
                    y = -(contentHeight - padding.bottom - groupHeight
                           + posInGroup * (_cachedItemHeight + spacing)
                           + _cachedItemHeight * 0.5f);
                }

                float x;
                if (reverseArrangement)
                {
                    // Right-to-left with right-anchor: x negative leftward
                    x = -(padding.right + group * (_cachedItemWidth + spacing) + _cachedItemWidth * 0.5f);
                }
                else
                {
                    // Left-to-right with left-anchor: x positive rightward
                    x = padding.left + group * (_cachedItemWidth + spacing) + _cachedItemWidth * 0.5f;
                }
                return new Vector2(x, y);
            }
        }

        private bool IsIndexActive(int index)
        {
            for (int i = 0; i < _activeItems.Count; i++)
            {
                if (_activeItems[i].index == index)
                    return true;
            }
            return false;
        }

        private void RecycleItem(int activeIndex)
        {
            var record = _activeItems[activeIndex];
            if (record.item != null && record.item.gameObject != null)
            {
                record.item.OnUnspawn();
                record.item.transform.SetParent(_content, false);
                _itemPool.Push(record.item);
            }
        }

        private UIItemBase SpawnItem()
        {
            if (_itemPool.Count > 0)
            {
                var item = _itemPool.Pop();
                if (item != null && item.gameObject != null)
                {
                    item.transform.SetParent(_content, false);
                    return item;
                }
            }

            if (itemPrefab == null) return null;

            var go = Instantiate(itemPrefab, _content);
            go.transform.localScale = Vector3.one;
            var itemBase = go.GetComponent<UIItemBase>();
            if (itemBase == null)
            {
                Debug.LogError("[RecyclableScrollList] itemPrefab must have a UIItemBase component!", this);
                Destroy(go);
                return null;
            }
            return itemBase;
        }
    }
}
