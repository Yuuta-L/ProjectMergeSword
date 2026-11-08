using System;
using System.Collections.Generic;
using GameFramework.Event;
using UnityEngine;
using BurnicleFramework.Runtime;
using BurnicleFramework.Generated;
using GameFramework;

namespace MergeSword
{
    /// <summary>
    /// 多界面序列管理
    /// </summary>
    public sealed partial class UISequence : MonoBehaviour
    {
        private readonly GameFrameworkLinkedList<Cell> _sequenceItems = new GameFrameworkLinkedList<Cell>();
        private int? _currentUIFormId;
        private Cell _currentCell;
        private bool _isActive;
        private bool _wasIdle = true;

        // ---- 中断模式（Interrupt）支持 ----

        private int _pauseCount;
        private readonly HashSet<int> _interruptSerialIds = new HashSet<int>();

        /// <summary>序列进入空闲状态时触发（无待处理界面，无正在显示的界面）。</summary>
        public event Action OnSequenceIdle;

        public void Init()
        {
            Burnicle.Event.Subscribe(CloseUIFormCompleteEventArgs.EventId,OnCloseUIFormComplete);
            Burnicle.Event.Subscribe(OpenUIFormFailureEventArgs.EventId,OnOpenUIFormFailure);
        }

        /// <summary>暂停序列推进（中断表单打开时调用）。</summary>
        public void Pause()
        {
            _pauseCount++;
        }

        /// <summary>恢复序列推进（中断表单关闭时调用）。</summary>
        public void Resume()
        {
            if (_pauseCount > 0)
            {
                _pauseCount--;
                if (_pauseCount == 0)
                {
                    _wasIdle = false;
                }
            }
        }

        /// <summary>序列是否处于暂停状态。</summary>
        public bool IsPaused => _pauseCount > 0;

        /// <summary>注册一个中断表单的 SerialId（关闭时自动 Resume）。</summary>
        public void RegisterInterrupt(int serialId)
        {
            if (serialId > 0)
                _interruptSerialIds.Add(serialId);
        }
        
        /// <summary>序列中是否有等待打开的界面。</summary>
        public bool HasPendingItems => _sequenceItems.Count > 0;

        /// <summary>当前是否有界面正在展示（由序列打开的）。</summary>
        public bool IsDisplaying => _currentCell != null;

        private void Update()
        {
            if (!_isActive)
            {
                return;
            }

            bool isIdleNow = _sequenceItems.Count == 0 && _currentCell == null;
            if (isIdleNow && !_wasIdle)
            {
                _wasIdle = true;
                OnSequenceIdle?.Invoke();
            }

            if (_sequenceItems.Count > 0 && _currentCell == null && !Burnicle.UI.HasUIForm(EnumUIForm.LoadingForm) && _pauseCount == 0 && !PopupPriorityGate.IsLocked)
            {
                _wasIdle = false;
                DecideNextUIForm();
            }
        }
        
        private void DecideNextUIForm()
        {
            if (_currentCell != null)
            {
                ReferencePool.Release(_currentCell);
            }
            _currentCell = _sequenceItems.First.Value;
            _sequenceItems.RemoveFirst();
            OpenCurrentUIForm();
        }
        
        public void AppendUIForm(int uiFormId,UIOpenParams openParams,int priority = 0)
        {
            var item = Cell.Create(uiFormId, openParams, priority);
            var current = _sequenceItems.First;
            while (current != null)
            {
                if (priority > current.Value.Priority)
                {
                    break;
                }
                current = current.Next;
            }

            if (current == null)
            {
                _sequenceItems.AddLast(item);
            }
            else
            {
                _sequenceItems.AddBefore(current, item);
            }
        }

        public void StartSequence()
        {
            _isActive = true;
        }

        /// <summary>
        /// 强制推进：当前有界面正在展示时，立即打开序列中下一个待展示界面（叠加在当前界面之上），
        /// 不等待当前界面关闭。用于需要立即响应的特殊场景（如宝石共鸣在宝石界面内解锁时）。
        /// 无待展示界面或处于空闲/全局门控状态时无操作。
        /// </summary>
        public void ForceAdvance()
        {
            if (_sequenceItems.Count == 0 || _currentCell == null
                || Burnicle.UI.HasUIForm(EnumUIForm.LoadingForm) || _pauseCount > 0 || PopupPriorityGate.IsLocked)
            {
                return;
            }

            var item = _sequenceItems.First.Value;
            _sequenceItems.RemoveFirst();
            Burnicle.UI.Open(item.UIFormId, item.OpenParams);
            ReferencePool.Release(item);
        }
        
        public void StopSequence()
        {
            _isActive = false;
        }
        
        private void OpenCurrentUIForm()
        {
            _currentUIFormId = Burnicle.UI.Open(_currentCell.UIFormId, _currentCell.OpenParams);
        }
        
        private void OnCloseUIFormComplete(object sender,GameEventArgs e)
        {
            if (e is CloseUIFormCompleteEventArgs ne)
            {
                // 中断表单关闭 → 自动恢复序列
                if (_interruptSerialIds.Remove(ne.SerialId))
                {
                    Resume();
                    return;
                }

                // 当前序列表单关闭
                if (ne.SerialId == _currentUIFormId)
                {
                    _currentUIFormId = null;
                    _currentCell = null;
                }
            }
        }
        
        private void OnOpenUIFormFailure(object sender,GameEventArgs e)
        {
            if (e is OpenUIFormFailureEventArgs ne && ne.SerialId == _currentUIFormId)
            {
                _currentUIFormId = null;
                _currentCell = null;
            }
        }
    }
}