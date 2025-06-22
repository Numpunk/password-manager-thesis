using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUserInput : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField passwordInput;
    [SerializeField]
    private Button loginBtn;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            passwordInput.Select();
        }
        if (Input.GetKeyDown(KeyCode.Return)|| Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            loginBtn.onClick.Invoke();
        }
    }
}
