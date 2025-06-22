using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterUserInput : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField emailInput;
    [SerializeField]
    private TMP_InputField passwordInput;
    [SerializeField]
    private TMP_InputField confirmPasswordInput;
    [SerializeField]
    private Button registerBtn;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if(!emailInput.isFocused&&!passwordInput.isFocused&&!confirmPasswordInput.isFocused) emailInput.Select();
            else if(emailInput.isFocused) passwordInput.Select();
            else if (passwordInput.isFocused) confirmPasswordInput.Select();
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            registerBtn.onClick.Invoke();
        }
    }
}
