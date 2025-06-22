using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class VerificationCodeInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField digit1;
    [SerializeField] private TMP_InputField digit2;
    [SerializeField] private TMP_InputField digit3;
    [SerializeField] private TMP_InputField digit4;
    [SerializeField] private TMP_InputField digit5;
    [SerializeField] private TMP_InputField digit6;

    [SerializeField] private Color hoverColor;
    [SerializeField] private Color activeColor;

    [SerializeField] private TMP_InputField inputField;

    public static VerificationCodeInput instance;
    private void Awake()
    {
        if (instance != this) instance = this;
    }
    public void OnValueChanged()
    {
        string input = inputField.text;
        if (input.Length > 0 && input[0] == '-') { inputField.text= inputField.text.Remove(0, 1); return; }

        ClearDigits();
        if (input.Length > 0) digit1.text = input[0].ToString();
        if (input.Length > 1) digit2.text = input[1].ToString();
        if (input.Length > 2) digit3.text = input[2].ToString();
        if (input.Length > 3) digit4.text = input[3].ToString();
        if (input.Length > 4) digit5.text = input[4].ToString();
        if (input.Length > 5) digit6.text = input[5].ToString();

        if (input.Length == 6)
        {
             UserAuthManager.instance.Set6DigitInputValueRegister(input);
        }
    }
    public void Set6Digits()
    {
        if (inputField.text.Length == 6)
        {
            UserAuthManager.instance.Set6DigitInputValueRegister(inputField.text);
        }
    }
    private void ClearDigits()
    {
        digit1.text = null;
        digit2.text = null;
        digit3.text = null;
        digit4.text = null;
        digit5.text = null;
        digit6.text = null;
    }
    public void ClearInput()
    {
        ClearDigits();
        inputField.text = null;
    }
    public void OnCursorEnter(TMP_InputField inputField)
    {
        if (inputField.isFocused) return;

        digit1.GetComponent<Image>().color = hoverColor;
        digit2.GetComponent<Image>().color = hoverColor;
        digit3.GetComponent<Image>().color = hoverColor;
        digit4.GetComponent<Image>().color = hoverColor;
        digit5.GetComponent<Image>().color = hoverColor;
        digit6.GetComponent<Image>().color = hoverColor;
    }
    public void OnCursorExit(TMP_InputField inputField)
    {
        if (inputField.isFocused) return;

        digit1.GetComponent<Image>().color = Color.white;
        digit2.GetComponent<Image>().color = Color.white;
        digit3.GetComponent<Image>().color = Color.white;
        digit4.GetComponent<Image>().color = Color.white;
        digit5.GetComponent<Image>().color = Color.white;
        digit6.GetComponent<Image>().color = Color.white;
    }
    public void OnFieldSelected()
    {
        digit1.GetComponent<Image>().color = activeColor;
        digit2.GetComponent<Image>().color = activeColor;
        digit3.GetComponent<Image>().color = activeColor;
        digit4.GetComponent<Image>().color = activeColor;
        digit5.GetComponent<Image>().color = activeColor;
        digit6.GetComponent<Image>().color = activeColor;
    }
    public void OnFieldDeselected()
    {
        digit1.GetComponent<Image>().color = Color.white;
        digit2.GetComponent<Image>().color = Color.white;
        digit3.GetComponent<Image>().color = Color.white;
        digit4.GetComponent<Image>().color = Color.white;
        digit5.GetComponent<Image>().color = Color.white;
        digit6.GetComponent<Image>().color = Color.white;
    }
}
