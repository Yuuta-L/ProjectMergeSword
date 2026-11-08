using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI效果控制器 - 支持Dim、Gray等多种效果，兼容Image和TMP
/// </summary>
public static class UIEffectController
{
    public enum EffectType
    {
        None,
        Dim, // 黑色遮罩
        Gray, // 灰度
        Desaturate, // 去饱和度
        CustomColor // 自定义颜色叠加
    }

    private static readonly Dictionary<EffectType, string> ShaderPaths = new Dictionary<EffectType, string>
    {
        { EffectType.Dim, "UI/Dim_Simple" },
        { EffectType.Gray, "UI/Gray_Simple" }
    };

    private static readonly Dictionary<EffectType, Material> SharedMaterials =
        new Dictionary<EffectType, Material>();

    private static readonly Dictionary<EffectType, bool> ShaderAvailable = new Dictionary<EffectType, bool>
    {
        { EffectType.Dim, true },
        { EffectType.Gray, true }
    };

    // 存储每个对象的效果状态
    private static readonly Dictionary<GameObject, ObjectEffectState> _objectStates =
        new Dictionary<GameObject, ObjectEffectState>();

    // TMP原始数据存储类
    private class TMPOriginalData
    {
        public Color color;
    }

    // UI组件信息
    private struct UIComponentInfo
    {
        public Component component;
        public object originalData;
        public UIComponentType type;
    }

    private enum UIComponentType
    {
        Image,
        TMP_Text
    }

    // 对象效果状态
    private class ObjectEffectState
    {
        public EffectType currentEffect = EffectType.None;
        public float currentIntensity = 1.0f;
        public readonly List<UIComponentInfo> componentInfos = new List<UIComponentInfo>();
    }

    private static readonly int DimIntensity = Shader.PropertyToID("_DimIntensity");
    private static readonly int GrayIntensity = Shader.PropertyToID("_GrayIntensity");

    /// <summary>
    /// 缓存所有UI组件
    /// </summary>
    private static void CacheUIComponents(GameObject targetObject, ObjectEffectState state)
    {
        state.componentInfos.Clear();
        if (targetObject == null) return;
        CollectUIComponents(targetObject.transform, state);
    }

    private static void CollectUIComponents(Transform current, ObjectEffectState state)
    {
        // 标记了UIEffectIgnore的对象及其子树跳过效果处理
        if (current.GetComponent<UIEffectIgnore>() != null)
            return;

        // 缓存Image组件
        var image = current.GetComponent<Image>();
        if (image != null)
        {
            state.componentInfos.Add(new UIComponentInfo
            {
                component = image,
                originalData = image.material,
                type = UIComponentType.Image
            });
        }

        // 缓存TMP_Text组件
        var tmpText = current.GetComponent<TMP_Text>();
        if (tmpText != null)
        {
            state.componentInfos.Add(new UIComponentInfo
            {
                component = tmpText,
                originalData = new TMPOriginalData
                {
                    color = tmpText.color
                },
                type = UIComponentType.TMP_Text
            });
        }

        // 递归处理子节点
        for (int i = 0; i < current.childCount; i++)
        {
            CollectUIComponents(current.GetChild(i), state);
        }
    }

    /// <summary>
    /// 检查指定效果的Shader是否可用
    /// </summary>
    private static bool IsShaderAvailable(EffectType effectType, bool tryFindShader = true)
    {
        // 如果已经知道不可用，直接返回
        if (ShaderAvailable.TryGetValue(effectType, out var available) && !available)
            return false;

        // 如果已经有材质，说明Shader可用
        if (SharedMaterials.ContainsKey(effectType))
            return true;

        // 如果需要尝试查找Shader
        if (tryFindShader && ShaderPaths.TryGetValue(effectType, out var shaderPath))
        {
            var shader = Shader.Find(shaderPath);
            if (shader == null)
            {
                Debug.LogWarning($"找不到Shader: {shaderPath}");
                ShaderAvailable[effectType] = false;
                return false;
            }

            if (!shader.isSupported)
            {
                Debug.LogWarning($"Shader不支持当前平台: {shaderPath}");
                ShaderAvailable[effectType] = false;
                return false;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 获取或创建共享材质（仅用于Image）
    /// </summary>
    private static Material GetOrCreateMaterial(EffectType effectType)
    {
        // 首先检查Shader是否可用
        if (!IsShaderAvailable(effectType, true))
        {
            Debug.LogWarning($"Shader {effectType} 不可用");
            return null;
        }

        // 尝试从缓存获取材质
        if (SharedMaterials.TryGetValue(effectType, out var material))
            return material;

        // 创建新材质
        if (ShaderPaths.TryGetValue(effectType, out var shaderPath))
        {
            var shader = Shader.Find(shaderPath);
            if (shader != null)
            {
                material = new Material(shader);
                material.enableInstancing = true;
                material.hideFlags = HideFlags.DontSave;
                SharedMaterials[effectType] = material;
                return material;
            }
        }

        return null;
    }

    /// <summary>
    /// 获取或创建对象状态
    /// </summary>
    private static ObjectEffectState GetOrCreateObjectState(GameObject targetObject)
    {
        if (!_objectStates.TryGetValue(targetObject, out var state))
        {
            state = new ObjectEffectState();
            _objectStates[targetObject] = state;
            CacheUIComponents(targetObject, state);
        }

        return state;
    }

    /// <summary>
    /// 应用Dim效果
    /// </summary>
    public static bool SetDim(GameObject targetObject, bool enabled, float intensity = 0.5f)
    {
        if (targetObject == null)
        {
            Debug.LogWarning("目标对象为空");
            return false;
        }

        if (enabled)
        {
            return ApplyEffect(targetObject, EffectType.Dim, intensity);
        }
        else
        {
            return RemoveEffect(targetObject, EffectType.Dim);
        }
    }

    /// <summary>
    /// 应用灰度效果
    /// </summary>
    public static bool SetGray(GameObject targetObject, bool enabled, float intensity = 1.0f)
    {
        if (targetObject == null)
        {
            Debug.LogWarning("目标对象为空");
            return false;
        }

        if (enabled)
        {
            return ApplyEffect(targetObject, EffectType.Gray, intensity);
        }
        else
        {
            return RemoveEffect(targetObject, EffectType.Gray);
        }
    }

    /// <summary>
    /// 应用指定效果
    /// </summary>
    /// <returns>是否成功应用效果</returns>
    private static bool ApplyEffect(GameObject targetObject, EffectType effectType, float intensity)
    {
        var state = GetOrCreateObjectState(targetObject);
        state.currentIntensity = intensity;

        // 应用到所有UI组件
        bool applied = false;

        for (int i = 0; i < state.componentInfos.Count; i++)
        {
            var info = state.componentInfos[i];
            if (info.component == null) continue;

            bool componentApplied = false;

            switch (info.type)
            {
                case UIComponentType.Image:
                    componentApplied = ApplyEffectToImage((Image)info.component, effectType, intensity, ref info);
                    break;

                case UIComponentType.TMP_Text:
                    componentApplied = ApplyEffectToTMP((TMP_Text)info.component, effectType, intensity, ref info);
                    break;
            }

            if (componentApplied)
            {
                state.componentInfos[i] = info; // 更新缓存信息
                applied = true;
            }
        }

        if (applied)
        {
            state.currentEffect = effectType;
            return true;
        }
        else
        {
            Debug.LogWarning($"应用{effectType}效果失败，没有找到可用的UI组件");
            return false;
        }
    }

    /// <summary>
    /// 为Image应用效果
    /// </summary>
    private static bool ApplyEffectToImage(Image image, EffectType effectType, float intensity,
        ref UIComponentInfo info)
    {
        // 获取材质
        var material = GetOrCreateMaterial(effectType);
        if (material == null)
            return false;

        // 设置效果参数
        switch (effectType)
        {
            case EffectType.Dim:
                material.SetFloat(DimIntensity, intensity);
                break;
            case EffectType.Gray:
                material.SetFloat(GrayIntensity, intensity);
                break;
        }

        // 缓存原始材质
        if (info.originalData == null)
        {
            info.originalData = image.material;
        }

        // 应用材质
        image.material = material;
        return true;
    }

    /// <summary>
    /// 为TMP应用效果（通过直接修改颜色）
    /// </summary>
    private static bool ApplyEffectToTMP(TMP_Text tmpText, EffectType effectType, float intensity,
        ref UIComponentInfo info)
    {
        // 缓存原始颜色
        if (info.originalData == null)
        {
            info.originalData = new TMPOriginalData
            {
                color = tmpText.color
            };
        }

        // 根据效果类型修改颜色
        Color originalColor = ((TMPOriginalData)info.originalData).color;
        Color newColor = originalColor;

        switch (effectType)
        {
            case EffectType.Dim:
                // Dim效果：降低亮度
                newColor.r = originalColor.r * (1 - intensity);
                newColor.g = originalColor.g * (1 - intensity);
                newColor.b = originalColor.b * (1 - intensity);
                break;

            case EffectType.Gray:
                // 灰度效果：转换为灰度
                float grayValue = (originalColor.r * 0.299f + originalColor.g * 0.587f + originalColor.b * 0.114f);
                float finalGray = Mathf.Lerp(originalColor.r, grayValue, intensity);
                newColor.r = finalGray;
                newColor.g = finalGray;
                newColor.b = finalGray;
                break;

            case EffectType.Desaturate:
                // 去饱和度效果
                float luminance = (originalColor.r + originalColor.g + originalColor.b) / 3f;
                newColor.r = Mathf.Lerp(originalColor.r, luminance, intensity);
                newColor.g = Mathf.Lerp(originalColor.g, luminance, intensity);
                newColor.b = Mathf.Lerp(originalColor.b, luminance, intensity);
                break;
        }

        // 应用新颜色
        tmpText.color = newColor;
        return true;
    }

    /// <summary>
    /// 移除指定效果
    /// </summary>
    private static bool RemoveEffect(GameObject targetObject, EffectType effectType)
    {
        if (!_objectStates.TryGetValue(targetObject, out var state))
            return false;

        if (state.currentEffect != effectType)
            return false;

        bool restored = false;

        for (int i = 0; i < state.componentInfos.Count; i++)
        {
            var info = state.componentInfos[i];
            if (info.component == null) continue;

            bool componentRestored = false;

            switch (info.type)
            {
                case UIComponentType.Image:
                    componentRestored = RestoreImage((Image)info.component, info.originalData);
                    break;

                case UIComponentType.TMP_Text:
                    componentRestored = RestoreTMP((TMP_Text)info.component, info.originalData);
                    break;
            }

            if (componentRestored)
                restored = true;
        }

        if (restored)
        {
            state.currentEffect = EffectType.None;
            state.currentIntensity = 1.0f;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 移除对象上的所有效果
    /// </summary>
    public static bool RemoveAllEffects(GameObject targetObject)
    {
        if (!_objectStates.TryGetValue(targetObject, out var state))
            return false;

        if (state.currentEffect == EffectType.None)
            return false;

        bool restored = false;

        for (int i = 0; i < state.componentInfos.Count; i++)
        {
            var info = state.componentInfos[i];
            if (info.component == null) continue;

            bool componentRestored = false;

            switch (info.type)
            {
                case UIComponentType.Image:
                    componentRestored = RestoreImage((Image)info.component, info.originalData);
                    break;

                case UIComponentType.TMP_Text:
                    componentRestored = RestoreTMP((TMP_Text)info.component, info.originalData);
                    break;
            }

            if (componentRestored)
                restored = true;
        }

        if (restored)
        {
            state.currentEffect = EffectType.None;
            state.currentIntensity = 1.0f;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 恢复Image的原始状态
    /// </summary>
    private static bool RestoreImage(Image image, object originalData)
    {
        if (image == null) return false;

        // 恢复原始材质
        if (originalData is Material originalMaterial)
        {
            image.material = originalMaterial;
        }
        else if (originalData == null)
        {
            image.material = null; // 使用默认材质
        }

        return true;
    }

    /// <summary>
    /// 恢复TMP的原始状态
    /// </summary>
    private static bool RestoreTMP(TMP_Text tmpText, object originalData)
    {
        if (tmpText == null) return false;

        // 恢复原始颜色
        if (originalData is TMPOriginalData tmpData)
        {
            tmpText.color = tmpData.color;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 获取当前效果类型
    /// </summary>
    public static EffectType GetCurrentEffect(GameObject targetObject)
    {
        if (targetObject == null || !_objectStates.TryGetValue(targetObject, out var state))
            return EffectType.None;

        return state.currentEffect;
    }

    /// <summary>
    /// 获取当前效果强度
    /// </summary>
    public static float GetCurrentIntensity(GameObject targetObject)
    {
        if (targetObject == null || !_objectStates.TryGetValue(targetObject, out var state))
            return 1.0f;

        return state.currentIntensity;
    }

    /// <summary>
    /// 检查指定效果是否可用（仅对Image有效）
    /// </summary>
    public static bool IsEffectAvailable(EffectType effectType)
    {
        return IsShaderAvailable(effectType, false);
    }

    /// <summary>
    /// 清理对象资源
    /// </summary>
    public static void DisposeObject(GameObject targetObject)
    {
        if (targetObject == null) return;

        if (_objectStates.TryGetValue(targetObject, out var state))
        {
            if (state.currentEffect != EffectType.None)
            {
                RemoveAllEffects(targetObject);
            }

            _objectStates.Remove(targetObject);
        }
    }

    /// <summary>
    /// 清理所有资源
    /// </summary>
    public static void DisposeAll()
    {
        // 移除所有对象的效果
        foreach (var targetObject in new List<GameObject>(_objectStates.Keys))
        {
            DisposeObject(targetObject);
        }

        // 清理所有材质
        foreach (var material in SharedMaterials.Values)
        {
            if (material != null)
                Object.Destroy(material);
        }

        SharedMaterials.Clear();
        _objectStates.Clear();

        // 重置可用性状态
        foreach (var key in new List<EffectType>(ShaderAvailable.Keys))
        {
            ShaderAvailable[key] = true;
        }
    }

    /// <summary>
    /// 强制重置所有Shader状态
    /// </summary>
    public static void ResetAllShaderAvailability()
    {
        // 清理所有材质
        foreach (var material in SharedMaterials.Values)
        {
            if (material != null)
                Object.Destroy(material);
        }

        SharedMaterials.Clear();

        // 重置可用性状态
        foreach (var key in new List<EffectType>(ShaderAvailable.Keys))
        {
            ShaderAvailable[key] = true;
        }
    }

    /// <summary>
    /// 获取效果是否可用（针对特定对象）
    /// </summary>
    public static bool IsEffectAvailableForObject(GameObject targetObject)
    {
        if (targetObject == null || !_objectStates.TryGetValue(targetObject, out var state))
            return true;

        if (state.currentEffect == EffectType.None)
            return true;

        return IsEffectAvailable(state.currentEffect);
    }

    /// <summary>
    /// 刷新效果（当UI组件动态改变时调用）
    /// </summary>
    public static void RefreshEffect(GameObject targetObject)
    {
        if (targetObject == null) return;

        if (!_objectStates.TryGetValue(targetObject, out var state))
            return;

        var oldEffect = state.currentEffect;
        var oldIntensity = state.currentIntensity;

        if (oldEffect != EffectType.None)
        {
            RemoveAllEffects(targetObject);
        }

        // 重新缓存组件
        _objectStates.Remove(targetObject); // 移除旧状态
        state = GetOrCreateObjectState(targetObject); // 创建新状态

        // 重新应用效果
        if (oldEffect != EffectType.None)
        {
            ApplyEffect(targetObject, oldEffect, oldIntensity);
        }
    }

    /// <summary>
    /// 检查对象是否有应用效果
    /// </summary>
    public static bool HasEffect(GameObject targetObject)
    {
        if (targetObject == null) return false;
        return _objectStates.ContainsKey(targetObject) &&
               _objectStates[targetObject].currentEffect != EffectType.None;
    }
}