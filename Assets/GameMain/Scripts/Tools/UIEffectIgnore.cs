using UnityEngine;

/// <summary>
/// 标记此GameObject及其子节点不受UIEffectController效果影响。
/// 添加此组件后，该对象子树内的所有Image和TMP_Text组件都不会被Dim/Gray等效果修改。
/// </summary>
[DisallowMultipleComponent]
public class UIEffectIgnore : MonoBehaviour
{
}
