using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HidePasswordManager : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField loginPasswordInput;
    [SerializeField]
    private TMP_InputField addPasswordValueText;
    [SerializeField]
    private TMP_InputField updatePasswordValueText;

    public static HidePasswordManager instance;
    private void Awake()
    {
        instance = this;
    }

    public void TogglePasswordVisibility(TMP_InputField input)
    {
        if(input.contentType==TMP_InputField.ContentType.Password)
        {
            input.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            input.contentType = TMP_InputField.ContentType.Password;
        }
        StartCoroutine(RefreshAddPasswordValueInput());
        IEnumerator RefreshAddPasswordValueInput()
        {
            input.Select();
            yield return null;
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
    public void HidePasswordInputs()
    {
        addPasswordValueText.contentType = TMP_InputField.ContentType.Password;
        updatePasswordValueText.contentType = TMP_InputField.ContentType.Password;
        loginPasswordInput.contentType = TMP_InputField.ContentType.Password;
    }
}
