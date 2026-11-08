using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
#if BURNICLE_WECHATMINIGAME
using WeChatWASM;
#endif

[RequireComponent(typeof(TMP_InputField))]
public class WXTMPInputFieldAdapter : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    private TMP_InputField _tmpInputField;
    private bool _isKeyboardShowing = false;

    private void Start()
    {
#if BURNICLE_WECHATMINIGAME
        _tmpInputField = GetComponent<TMP_InputField>();
        _tmpInputField.shouldHideMobileInput = true;
#endif
    }

    public void OnPointerClick(PointerEventData eventData)
    {
#if BURNICLE_WECHATMINIGAME
        if (!_isKeyboardShowing)
        {
            ShowWXKeyboard();
        }
#endif
    }

    public void OnPointerExit(PointerEventData eventData)
    {
#if BURNICLE_WECHATMINIGAME
        if (!_tmpInputField.isFocused && _isKeyboardShowing)
        {
            HideWXKeyboard();
        }
#endif
    }

#if BURNICLE_WECHATMINIGAME
    private void ShowWXKeyboard()
    {
        WX.ShowKeyboard(new ShowKeyboardOption
        {
            defaultValue = _tmpInputField.text,
            maxLength = _tmpInputField.characterLimit > 0 ? _tmpInputField.characterLimit : 999,
            confirmType = "done"
        });

        WX.OnKeyboardInput(OnKeyboardInput);
        WX.OnKeyboardConfirm(OnKeyboardConfirm);
        WX.OnKeyboardComplete(OnKeyboardComplete);

        _isKeyboardShowing = true;
        _tmpInputField.ActivateInputField();
    }

    private void HideWXKeyboard()
    {
        WX.HideKeyboard(new HideKeyboardOption());
        WX.OffKeyboardInput(OnKeyboardInput);
        WX.OffKeyboardConfirm(OnKeyboardConfirm);
        WX.OffKeyboardComplete(OnKeyboardComplete);

        _isKeyboardShowing = false;
        _tmpInputField.DeactivateInputField();
    }

    private void OnKeyboardInput(OnKeyboardInputListenerResult result)
    {
        if (_tmpInputField.isFocused)
        {
            _tmpInputField.text = result.value;
        }
    }

    private void OnKeyboardConfirm(OnKeyboardInputListenerResult result)
    {
        _tmpInputField.text = result.value;
        HideWXKeyboard();
    }

    private void OnKeyboardComplete(OnKeyboardInputListenerResult result)
    {
        _tmpInputField.text = result.value;
        HideWXKeyboard();
    }
#endif
}