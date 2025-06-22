using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailResendTimer : MonoBehaviour
{
    [SerializeField]
    private TMP_Text textRegister;
    [SerializeField]
    private TMP_Text textLogin;
    [SerializeField]
    private Button btnRegister;
    [SerializeField]
    private Button btnLogin;
    private float timer;
    private bool timerStarted;

    public static EmailResendTimer instance;
    private void Awake()
    {
        if(instance!=this) instance = this;
    }
    private void Start()
    {
        StartTimer();
    }
    void Update()
    {
        if (!timerStarted) return;

        if (timer > 0)
        {
            timer-=Time.deltaTime;
            textRegister.text = $"Resend code ({(int)timer})";
            textLogin.text = $"Resend code ({(int)timer})";
        }
        else
        {
            btnRegister.interactable = true;
            btnLogin.interactable = true;
            textRegister.text = "Resend code";
            textLogin.text = "Resend code";
            timerStarted = false;
        }
    }
    public void StartTimer()
    {
        timer = 20f;
        btnRegister.interactable = false;
        btnLogin.interactable = false;
        timerStarted = true;
    }
}
