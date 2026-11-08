using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.DataTable;
using GameFramework.UI;
using BurnicleFramework.Generated;
using BurnicleFramework.Runtime;
using Cysharp.Threading.Tasks;
using GameFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MergeSword
{
    public static class UIExtension
    {
        /// <summary>
        /// 打开UI界面
        /// </summary>
        public static int? Open(this UIComponent uiComponent, EnumUIForm uiFormId, UIOpenParams uiOpenParams = null)
        {
            return uiComponent.Open((int)uiFormId, uiOpenParams);
        }
        
        /// <summary>
        /// 打开UI界面
        /// </summary>
        public static int? Open(this UIComponent uiComponent, int uiFormId, UIOpenParams uiOpenParams = null)
        {
            var dtUIForm = Burnicle.DataTable.GetDataTable<DRUIForm>();
            if (!dtUIForm.HasDataRow(uiFormId))
            {
                Log.Error(Utility.Text.Format("UIForm表不存在id:{0}", uiFormId));
                if (uiOpenParams != null) ReferencePool.Release(uiOpenParams);
                return null;
            }

            var drUIForm = dtUIForm.GetDataRow(uiFormId);
            string uiFormAssetName = AssetsPath.GetUIFormPath(drUIForm.AssetName);
            
            if (!drUIForm.AllowMultiInstance)
            {
                if (uiComponent.IsLoadingUIForm(uiFormAssetName))
                {
                    if (uiOpenParams != null) ReferencePool.Release(uiOpenParams);
                    return null;
                }

                if (uiComponent.HasUIForm(uiFormAssetName))
                {
                    if (uiOpenParams != null) ReferencePool.Release(uiOpenParams);
                    return null;
                }
            }

            uiOpenParams ??= UIOpenParams.Create();
            uiOpenParams.AllowEscapeClose ??= drUIForm.AllowEscapeClose;
            
            var serialId = uiComponent.OpenUIForm(uiFormAssetName, drUIForm.UIGroupName, drUIForm.Depth ,Constant.AssetPriority.UIFormAsset, drUIForm.PauseCoveredUIForm, uiOpenParams);
            if (drUIForm.ShowScreenDim && UIScreenDim)
            {
                UIScreenDim.Show(serialId);
            }
            
            return serialId;
        }

        /// <summary>
        /// 打开UI界面
        /// </summary>
        public static async UniTask<T> OpenAwait<T>(this UIComponent uiComponent, EnumUIForm uiFormId, UIOpenParams uiOpenParams = null) where T : UIFormLogic
        {
            return await uiComponent.OpenAwait<T>((int)uiFormId, uiOpenParams);
        }
        
        public static async UniTask<UIFormLogic> OpenAwait(this UIComponent uiComponent, EnumUIForm uiFormId, UIOpenParams uiOpenParams = null)
        {
            return await uiComponent.OpenAwait((int)uiFormId, uiOpenParams);
        }

        public static async UniTask<T> OpenAwait<T>(this UIComponent uiComponent, int uiFormId, UIOpenParams uiOpenParams = null) where T : UIFormLogic
        {
            var dtUIForm = Burnicle.DataTable.GetDataTable<DRUIForm>();
            if (!dtUIForm.HasDataRow(uiFormId))
            {
                Log.Error(Utility.Text.Format("UIForm表不存在id:{0}", uiFormId));
                if (uiOpenParams != null) ReferencePool.Release(uiOpenParams);
                return null;
            }

            var drUIForm = dtUIForm.GetDataRow(uiFormId);
            string uiFormAssetName = AssetsPath.GetUIFormPath(drUIForm.AssetName);
            
            if (!drUIForm.AllowMultiInstance)
            {
                if (uiComponent.IsLoadingUIForm(uiFormAssetName))
                {
                    if (uiOpenParams != null) ReferencePool.Release(uiOpenParams);
                    return null;
                }

                if (uiComponent.HasUIForm(uiFormAssetName))
                {
                    if (uiOpenParams != null) ReferencePool.Release(uiOpenParams);
                    return null;
                }
            }

            uiOpenParams ??= UIOpenParams.Create();
            uiOpenParams.AllowEscapeClose ??= drUIForm.AllowEscapeClose;
            
            var uiForm = await uiComponent.OpenUIFormAwait(uiFormAssetName, drUIForm.UIGroupName, drUIForm.Depth, Constant.AssetPriority.UIFormAsset, drUIForm.PauseCoveredUIForm, uiOpenParams);
            if (drUIForm.ShowScreenDim && UIScreenDim)
            {
                UIScreenDim.Show(uiForm.UIForm.SerialId);
            }

            return (T)uiForm;
        }

        /// <summary>
        /// 打开UI界面
        /// </summary>
        public static async UniTask<UIFormLogic> OpenAwait(this UIComponent uiComponent, int uiFormId, UIOpenParams uiOpenParams = null)
        {
            var dtUIForm = Burnicle.DataTable.GetDataTable<DRUIForm>();
            if (!dtUIForm.HasDataRow(uiFormId))
            {
                Log.Error(Utility.Text.Format("UIForm表不存在id:{0}", uiFormId));
                if (uiOpenParams != null) ReferencePool.Release(uiOpenParams);
                return null;
            }

            var drUIForm = dtUIForm.GetDataRow(uiFormId);
            string uiFormAssetName = AssetsPath.GetUIFormPath(drUIForm.AssetName);
            
            if (!drUIForm.AllowMultiInstance)
            {
                if (uiComponent.IsLoadingUIForm(uiFormAssetName))
                {
                    if (uiOpenParams != null) ReferencePool.Release(uiOpenParams);
                    return null;
                }

                if (uiComponent.HasUIForm(uiFormAssetName))
                {
                    if (uiOpenParams != null) ReferencePool.Release(uiOpenParams);
                    return null;
                }
            }

            uiOpenParams ??= UIOpenParams.Create();
            uiOpenParams.AllowEscapeClose ??= drUIForm.AllowEscapeClose;
            
            var uiForm = await uiComponent.OpenUIFormAwait(uiFormAssetName, drUIForm.UIGroupName, drUIForm.Depth, Constant.AssetPriority.UIFormAsset, drUIForm.PauseCoveredUIForm, uiOpenParams);
            if (drUIForm.ShowScreenDim && UIScreenDim)
            {
                UIScreenDim.Show(uiForm.UIForm.SerialId);
            }

            return uiForm;
        }
        
        /// <summary>
        /// 关闭UI界面(关闭前播放UI界面关闭动画)
        /// </summary>
        /// <param name="uiComponent"></param>
        /// <param name="uiForm"></param>
        public static void Close(this UIComponent uiComponent, UIFormBase uiForm)
        {
            Close(uiComponent, uiForm.SerialId);
        }
        
        public static void Close(this UIComponent uiComponent, EnumUIForm uiFormId)
        {
            var uiForm = uiComponent.GetUIForm(uiFormId);
            if (uiForm != null)
            {
                Close(uiComponent,uiForm.SerialId);
            }
        }
        
        /// <summary>
        /// 关闭UI界面(关闭前播放UI界面关闭动画)
        /// </summary>
        /// <param name="uiComponent"></param>
        /// <param name="uiFormId"></param>
        public static void Close(this UIComponent uiComponent, int uiFormId)
        {
            if (uiComponent.IsLoadingUIForm(uiFormId))
            {
                Burnicle.UI.CloseUIForm(uiFormId);
                return;
            }

            if (!uiComponent.HasUIForm(uiFormId))
            {
                return;
            }

            var uiForm = uiComponent.GetUIForm(uiFormId);
            if (uiForm.Logic is UIFormBase u)
            {
                u.Close();
            }
        }

        /// <summary>
        /// 关闭整个UI组的所有UI界面
        /// </summary>
        /// <param name="uiComponent"></param>
        /// <param name="groupName"></param>
        public static void CloseUIForms(this UIComponent uiComponent, string groupName)
        {
            var group = uiComponent.GetUIGroup(groupName);
            var all = group.GetAllUIForms();
            foreach (var item in all)
            {
                uiComponent.CloseUIForm(item.SerialId);
            }
        }
        
        /// <summary>
        /// 界面是否已打开或正在加载中
        /// </summary>
        public static bool ContainUIForm(this UIComponent uiComponent, EnumUIForm uiFormId, string uiGroupName = null)
        {
            return uiComponent.ContainUIForm((int)uiFormId, uiGroupName);
        }
        
        /// <summary>
        /// 界面是否已打开或正在加载中
        /// </summary>
        public static bool ContainUIForm(this UIComponent uiComponent, int uiFormId, string uiGroupName = null)
        {
            IDataTable<DRUIForm> dtUIForm = Burnicle.DataTable.GetDataTable<DRUIForm>();
            DRUIForm drUIForm = dtUIForm.GetDataRow(uiFormId);
            if (drUIForm == null)
            {
                return false;
            }

            string assetName = AssetsPath.GetUIFormPath(drUIForm.AssetName);
            if (string.IsNullOrEmpty(uiGroupName))
            {
                return uiComponent.HasUIForm(assetName) || uiComponent.IsLoadingUIForm(assetName);
            }

            IUIGroup uiGroup = uiComponent.GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                return false;
            }

            return uiGroup.HasUIForm(assetName);
        }
        
        /// <summary>
        /// 界面是否已打开
        /// </summary>
        public static bool HasUIForm(this UIComponent uiComponent, EnumUIForm uiFormId, string uiGroupName = null)
        {
            return uiComponent.HasUIForm((int)uiFormId, uiGroupName);
        }
        
        /// <summary>
        /// 界面是否已打开
        /// </summary>
        public static bool HasUIForm(this UIComponent uiComponent, int uiFormId, string uiGroupName = null)
        {
            IDataTable<DRUIForm> dtUIForm = Burnicle.DataTable.GetDataTable<DRUIForm>();
            DRUIForm drUIForm = dtUIForm.GetDataRow(uiFormId);
            if (drUIForm == null)
            {
                return false;
            }

            string assetName = AssetsPath.GetUIFormPath(drUIForm.AssetName);
            if (string.IsNullOrEmpty(uiGroupName))
            {
                return uiComponent.HasUIForm(assetName);
            }

            IUIGroup uiGroup = uiComponent.GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                return false;
            }

            return uiGroup.HasUIForm(assetName);
        }

        public static UIFormBase GetUIForm(this UIComponent uiComponent, EnumUIForm uiFormId, string uiGroupName = null)
        {
            return uiComponent.GetUIForm((int)uiFormId, uiGroupName);
        }
        
        public static T GetUIForm<T>(this UIComponent uiComponent, EnumUIForm uiFormId, string uiGroupName = null) where T:UIFormBase
        {
            return (T)uiComponent.GetUIForm((int)uiFormId, uiGroupName);
        }
        
        public static T GetUIForm<T>(this UIComponent uiComponent, int serialId) where T:UIFormBase
        {
            var uiForm = uiComponent.GetUIForm(serialId);
            if (uiForm == null)
            {
                return null;
            }

            return (T)uiForm.Logic;
        }
        
        public static UIFormBase GetUIForm(this UIComponent uiComponent, int uiFormId, string uiGroupName = null)
        {
            IDataTable<DRUIForm> dtUIForm = Burnicle.DataTable.GetDataTable<DRUIForm>();
            DRUIForm drUIForm = dtUIForm.GetDataRow(uiFormId);
            if (drUIForm == null)
            {
                return null;
            }

            string assetName = AssetsPath.GetUIFormPath(drUIForm.AssetName);
            UIForm uiForm = null;
            if (string.IsNullOrEmpty(uiGroupName))
            {
                uiForm = uiComponent.GetUIForm(assetName);
                if (uiForm == null)
                {
                    return null;
                }

                return (UIFormBase)uiForm.Logic;
            }

            IUIGroup uiGroup = uiComponent.GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                return null;
            }

            uiForm = (UIForm)uiGroup.GetUIForm(assetName);
            if (uiForm == null)
            {
                return null;
            }

            return (UIFormBase)uiForm.Logic;
        }
        
        /// <summary>
        /// 关闭所有打开的某个界面
        /// </summary>
        /// <param name="uiComponent"></param>
        /// <param name="uiFormType"></param>
        /// <param name="uiGroup"></param>
        public static void CloseUIForms(this UIComponent uiComponent, EnumUIForm uiFormType, string uiGroup = null)
        {
            var drUIForm = Burnicle.DataTable.GetDataTable<DRUIForm>().GetDataRow((int)uiFormType);
            GameFramework.UI.IUIForm[] uIForms;
            if (string.IsNullOrEmpty(uiGroup))
            {
                uIForms = uiComponent.GetUIForms(drUIForm.AssetName);
            }
            else
            {
                if (!uiComponent.HasUIGroup(uiGroup))
                {
                    return;
                }

                uIForms = uiComponent.GetUIGroup(uiGroup).GetUIForms(drUIForm.UIGroupName);
            }

            foreach (var item in uIForms)
            {
                uiComponent.CloseUIForm(item.SerialId);
            }
        }

        /// <summary>
        /// 刷新所有UI的多语言文本(当语言切换时需调用),用于即时改变多语言文本
        /// </summary>
        /// <param name="uiComponent"></param>
        public static void UpdateLocalizationTexts(this UIComponent uiComponent)
        {
            foreach (UIForm uiForm in uiComponent.GetAllLoadedUIForms())
            {
                ((UIFormBase)uiForm.Logic).InitLocalization();
            }

            var uiObjectPool = Burnicle.ObjectPool.GetObjectPool(pool =>
                pool.FullName == "GameFramework.UI.UIManager+UIFormInstanceObject.UI Instance Pool");
            if (uiObjectPool != null)
            {
                uiObjectPool.ReleaseAllUnused();
            }
        }

        /// <summary>
        /// 获取当前顶层的UI界面id
        /// </summary>
        /// <param name="uiComponent"></param>
        /// <returns></returns>
        public static int GetTopUIFormId(this UIComponent uiComponent)
        {
            var allUIGroups = uiComponent.GetAllUIGroups().ToList();
            allUIGroups.Sort((g1, g2) => g1.Depth.CompareTo(g2.Depth));
            foreach (var uiGroup in allUIGroups)
            {
                if (uiGroup.CurrentUIForm != null)
                {
                    return uiGroup.CurrentUIForm.SerialId;
                }
                
            }
            
            return -1;
        }
        
        
        /// <summary>
        /// 异步加载并设置Sprite
        /// </summary>
        public static void SetSprite(this Image image, string spritePath,bool hideFirst = true,bool setNativeSize = false, Action onComponent = null)
        {
            if (hideFirst)
            {
                image.gameObject.SetActive(false);
            }
            
            Burnicle.Procedure.LoadAsset<Sprite>(spritePath, (s) =>
            {
                if (s != null)
                {
                    image.sprite = s;
                    if (hideFirst)
                    {
                        image.gameObject.SetActive(true);
                    }

                    if (setNativeSize)
                    {
                        image.SetNativeSize();
                    }
                }
                onComponent?.Invoke();
            });
        }
        
        public static void FitSpriteBySize(this Image image, float size, bool adaptMin)
        {
            if (image.sprite == null)
            {
                return;
            }
            
            RectTransform rectTransform = image.rectTransform;
            Vector2 spriteSize = image.sprite.rect.size;
            
            if (Mathf.Approximately(spriteSize.x ,spriteSize.y))
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
                return;
            }
        
            // 计算原始宽高比
            float aspectRatio = spriteSize.x / spriteSize.y;
        
            float targetWidth;
            float targetHeight;
        
            if (adaptMin)
            {
                // 适配最小边：将较短的一边设置为 size，另一边按比例缩放
                if (spriteSize.x <= spriteSize.y)
                {
                    // 宽度是较小的边
                    targetWidth = size;
                    targetHeight = size / aspectRatio;
                }
                else
                {
                    // 高度是较小的边
                    targetHeight = size;
                    targetWidth = size * aspectRatio;
                }
            }
            else
            {
                // 适配最大边：将较长的一边设置为 size，另一边按比例缩放
                if (spriteSize.x >= spriteSize.y)
                {
                    // 宽度是较大的边
                    targetWidth = size;
                    targetHeight = size / aspectRatio;
                }
                else
                {
                    // 高度是较大的边
                    targetHeight = size;
                    targetWidth = size * aspectRatio;
                }
            }
        
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
        }

        public static void FitSpriteByHeight(this Image image, float height)
        {
            if (image.sprite == null)
            {
                return;
            }
            
            RectTransform rectTransform = image.rectTransform;
            Vector2 spriteSize = image.sprite.rect.size;
            
            if (Mathf.Approximately(spriteSize.x ,spriteSize.y))
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, height);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                return;
            }

            var targetWidth = height * spriteSize.x / spriteSize.y;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
        
        
        public static void FitSpriteByWeight(this Image image, float width)
        {
            if (image.sprite == null)
            {
                return;
            }
            
            RectTransform rectTransform = image.rectTransform;
            Vector2 spriteSize = image.sprite.rect.size;
            
            if (Mathf.Approximately(spriteSize.x ,spriteSize.y))
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, width);
                return;
            }

            var targetHeight = width * spriteSize.y / spriteSize.x;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
        }
        
        /// <summary>
        /// 判断是否点击在UI上
        /// </summary>
        /// <param name="uiComponent"></param>
        /// <param name="mousePosition"></param>
        /// <returns></returns>
        public static bool IsPointerOverUIObject(this UIComponent uiComponent, Vector3 mousePosition)
        {
#if UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(mousePosition.x, mousePosition.y);

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            return results.Count > 0;
#else
            return EventSystem.current.IsPointerOverGameObject();
#endif
        }
        
        /// <summary>
        /// 加载Sprite图集
        /// </summary>
        /// <param name="uiComponent"></param>
        /// <param name="atlasAssetPath"></param>
        /// <param name="onSpriteAtlasLoaded"></param>
        public static void LoadSpriteAtlas(this UIComponent uiComponent, string atlasAssetPath,GameFrameworkAction<SpriteAtlas> onSpriteAtlasLoaded)
        {
            if (Burnicle.Resource.HasAsset(atlasAssetPath) == GameFramework.Resource.HasAssetResult.NotExist)
            {
                Log.Warning("LoadSpriteAtlas失败, 资源不存在:{0}", atlasAssetPath);
                return;
            }

            Burnicle.Resource.LoadAsset(atlasAssetPath, typeof(SpriteAtlas),
                new GameFramework.Resource.LoadAssetCallbacks(
                    (string assetName, object asset, float duration, object userData) =>
                    {
                        var spAtlas = asset as SpriteAtlas;
                        onSpriteAtlasLoaded.Invoke(spAtlas);
                    }));
        }
        
        /// <summary>
        /// 异步加载Texture
        /// </summary>
        /// <param name="uiComponent"></param>
        /// <param name="textureAssetName"></param>
        /// <param name="onSpriteLoaded"></param>
        public static void LoadTexture(this UIComponent uiComponent, string textureAssetName, GameFrameworkAction<Texture2D> onSpriteLoaded)
        {
            if (Burnicle.Resource.HasAsset(textureAssetName) == GameFramework.Resource.HasAssetResult.NotExist)
            {
                Log.Warning("UIExtension.LoadTexture()失败, 资源不存在:{0}", textureAssetName);
                return;
            }

            Burnicle.Resource.LoadAsset(textureAssetName, typeof(Texture2D),
                new GameFramework.Resource.LoadAssetCallbacks(
                    (string assetName, object asset, float duration, object userData) =>
                    {
                        Texture2D resultSp = asset as Texture2D;
                        onSpriteLoaded.Invoke(resultSp);
                    }));
        }

#region ScreenDim

        private static UIScreenDim _uiScreenDim;
        private static UIScreenDim UIScreenDim
        {
            get
            {
                if (_uiScreenDim == null)
                {
                    GetOrCreateScreenDim();
                }
                return _uiScreenDim;
            }
        }

        private static void GetOrCreateScreenDim()
        {
            _uiScreenDim = Burnicle.UI.transform.GetComponentInChildren<UIScreenDim>(true);
            if (_uiScreenDim == null)
            {
                _uiScreenDim = UIScreenDim.CreateScreenDim(Burnicle.UI.InstanceRoot);
            }
        }
        
        public static void CloseScreenDim(int serialId,bool fade)
        {
            UIScreenDim.Hide(serialId,fade);
        }
        
        public static void CheckScreenDim()
        {
            UIScreenDim.CheckLayer();
        }
#endregion
        
        private static UISequence _uiSequence;

        private static void EnsureUISequence(UIComponent uiComponent)
        {
            if (_uiSequence == null)
            {
                _uiSequence = uiComponent.gameObject.GetOrAddComponent<UISequence>();
                _uiSequence.Init();
            }
        }
        //添加ui界面到序列中
        public static void AppendUIForm(this UIComponent uiComponent, EnumUIForm uiFormId, UIOpenParams openParams = null,int priority = 0)
        {
            EnsureUISequence(uiComponent);
            _uiSequence.AppendUIForm((int)uiFormId,openParams,priority);
        }
        //添加ui界面到序列最顶部
        public static void AppendUIFormNext(this UIComponent uiComponent, EnumUIForm uiFormId, UIOpenParams openParams = null)
        {
            EnsureUISequence(uiComponent);
            _uiSequence.AppendUIForm((int)uiFormId,openParams,999);
        }

        /// <summary>
        /// 强制推进 UISequence：当前有界面正在展示时，立即打开序列中下一个待展示界面（叠加在当前界面之上，不等待其关闭）。
        /// </summary>
        public static void ForceAdvanceUISequence(this UIComponent uiComponent)
        {
            EnsureUISequence(uiComponent);
            _uiSequence.ForceAdvance();
        }

        /// <summary>
        /// 以中断模式打开表单：直接打开（不走 UISequence 队列），
        /// 打开期间自动暂停 UISequence 推进，关闭后自动恢复。
        /// </summary>
        public static int? OpenInterruptForm(this UIComponent uiComponent, EnumUIForm uiFormId, UIOpenParams openParams = null)
        {
            EnsureUISequence(uiComponent);
            _uiSequence.Pause();

            var serialId = uiComponent.Open(uiFormId, openParams);
            if (serialId == null)
            {
                _uiSequence.Resume();
                return null;
            }

            _uiSequence.RegisterInterrupt(serialId.Value);
            return serialId;
        }

        public static void StartUISequence(this UIComponent uiComponent)
        {
            EnsureUISequence(uiComponent);
            _uiSequence.StartSequence();
        }

        public static void StopUISequence(this UIComponent uiComponent)
        {
            EnsureUISequence(uiComponent);
            _uiSequence.StopSequence();
        }

        /// <summary>UISequence 序列中是否有等待打开的界面。</summary>
        public static bool UISequenceHasPendingItems(this UIComponent uiComponent)
        {
            EnsureUISequence(uiComponent);
            return _uiSequence.HasPendingItems;
        }

        /// <summary>UISequence 当前是否有界面正在展示。</summary>
        public static bool UISequenceIsDisplaying(this UIComponent uiComponent)
        {
            EnsureUISequence(uiComponent);
            return _uiSequence.IsDisplaying;
        }

        /// <summary>注册 UISequence 空闲事件。</summary>
        public static void AddUISequenceIdleListener(this UIComponent uiComponent, System.Action callback)
        {
            EnsureUISequence(uiComponent);
            _uiSequence.OnSequenceIdle += callback;
        }

        /// <summary>取消注册 UISequence 空闲事件。</summary>
        public static void RemoveUISequenceIdleListener(this UIComponent uiComponent, System.Action callback)
        {
            if (_uiSequence != null)
                _uiSequence.OnSequenceIdle -= callback;
        }
    }
}
