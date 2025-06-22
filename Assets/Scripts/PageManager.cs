using Firebase.Auth;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PageManager : MonoBehaviour
{
    [SerializeField]
    private GameObject loginPage;
    [SerializeField]
    private GameObject registrationPage;
    [SerializeField]
    private GameObject emailConfirmationPage;
    [SerializeField]
    private GameObject unrecognizedDevicePage;
    [SerializeField]
    private GameObject resetPasswordPage;
    [SerializeField]
    private GameObject mainPage;
    [SerializeField]
    private GameObject lostConnectionPage;

    [Space(10)]
    [SerializeField]
    private TMP_Text pleaseWaitBeforeLoginText;
    [SerializeField]
    private TMP_Text emailLoginText;
    [SerializeField]
    private TMP_Text passwordLoginText;
    [SerializeField]
    private TMP_Text errorLoginText;
    [SerializeField]
    private TMP_Text emailRegistrationText;
    [SerializeField]
    private TMP_Text passwordRegistrationText;
    [SerializeField]
    private TMP_Text errorRegistrationText;
    private bool pleaseWaitAnimActive;
    [SerializeField]
    private TMP_Text mailConfirmationErrorText;
    [SerializeField]
    private TMP_Text mailConfirmationLoginErrorText;
    [SerializeField]
    private TMP_Text passwordResetErrorText;
    [SerializeField]
    private TMP_InputField passwordResetEmailInput;
    [SerializeField]
    private TMP_InputField emailRegistrationInput;
    [SerializeField]
    private TMP_InputField passwordRegistrationInput;
    [SerializeField]
    private TMP_InputField confirmPasswordRegistrationInput;
    [SerializeField]
    private TMP_InputField emailLoginInput;
    [SerializeField]
    private TMP_InputField passwordLoginInput;
    [SerializeField]
    private TMP_Text emailWasSentRegisterText;
    [SerializeField]
    private TMP_Text emailWasSentLoginText;
    [SerializeField]
    private Button continueButtonConfirm;
    [SerializeField]
    private Button continueButtonRecognize;
    [SerializeField]
    private GameObject mainPagePlaceholder;

    [Space(10)]
    [SerializeField]
    private GameObject deletePasswordWindow;
    [SerializeField]
    private TMP_Text deletePasswordText;

    public static PageManager instance;
    private void Awake()
    {
        if (instance != this) instance = this;
    }
    private void Start()
    {
        if (AppStartupManager.instance.hasRegistered) SwitchToLoginPage();
    }
    public void SwitchToLoginPage()
    {
        registrationPage.SetActive(false);
        ClearRegistrationInputFields();
        ResetLoginAndRegisterPageTexts();
        HidePasswordManager.instance.HidePasswordInputs();
        loginPage.SetActive(true);
    }
    public void SwitchToRegistrationPage()
    {
        loginPage.SetActive(false);
        ClearLoginInputFields();
        ResetLoginAndRegisterPageTexts();
        registrationPage.SetActive(true);
    }
    public void LoadMainPageOnRegister()
    {
        loginPage.SetActive(false);
        registrationPage.SetActive(false);
        emailConfirmationPage.SetActive(false);
        mainPage.SetActive(true);
        PasswordManager.instance.GetAllPasswords();
    }
    public void PlayWaitBeforeLoginTextAnimation(float time)
    {
        if (pleaseWaitAnimActive) return;

        pleaseWaitBeforeLoginText.GetComponent<Animator>().SetTrigger("Play");
        StartCoroutine(WaitBeforeHidingPleaseWaitText(time));
    }
    IEnumerator WaitBeforeHidingPleaseWaitText(float time)
    {
        pleaseWaitAnimActive = true;
        if (time < 0.5) yield return new WaitForSeconds(0.5f);
        else yield return new WaitForSeconds(time);
        pleaseWaitBeforeLoginText.GetComponent<Animator>().SetTrigger("Exit");
        pleaseWaitAnimActive = false;
    }
    public void ErrorOnLoginPage(Firebase.FirebaseException firebaseEx)
    {
        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                emailLoginText.text = "<color=#FF6D73>Email - <i>please provide an email address";
                break;
            case AuthError.InvalidEmail:
                emailLoginText.text = "<color=#FF6D73>Email - <i>invalid email";
                break;
            case AuthError.WrongPassword:
                passwordLoginText.text = "<color=#FF6D73>Password - <i>incorrect password";
                break;
            case AuthError.MissingPassword:
                passwordLoginText.text = "<color=#FF6D73>Password - <i>please provide a password";
                break;
            case AuthError.UserNotFound:
                emailLoginText.text = "<color=#FF6D73>Email - <i>user not found";
                break;
            case AuthError.TooManyRequests:
                errorLoginText.text = "<color=#FF6D73>Too many login attempts.\nPlease try again later.";
                StartCoroutine(ClearTooManyAttemptsError());
                break;
            default:
                errorLoginText.text = "<color=#FF6D73>Login Failed: " + firebaseEx.Message;
                break;
        }
    }
    IEnumerator ClearTooManyAttemptsError()
    {
        yield return new WaitForSeconds(25);
        errorLoginText.text = "";
    }
    public void NoConnectionOnRegistrationPage()
    {
        errorRegistrationText.text = "No internet connection!";
    }
    public void ConnectedOnRegistrationPage()
    {
        errorRegistrationText.text = "";
    }
    public void NoConnectionOnLoginPage()
    {
        errorLoginText.text = "No internet connection!";
    }
    public void ConnectedOnLoginPage()
    {
        errorLoginText.text = "";
    }
    public void NoConnectionOnMailConfirmationPage()
    {
        mailConfirmationErrorText.text = "No internet connection";
    }
    public void ConnectedOnMailConfirmationPage()
    {
        mailConfirmationErrorText.text = "";
    }
    public void NoConnectionOnMailConfirmationLoginPage()
    {
        mailConfirmationLoginErrorText.text = "No internet connection";
    }
    public void ConnectedOnMailConfirmationLoginPage()
    {
        mailConfirmationLoginErrorText.text = "";
    }
    public void NoConnectionOnPasswordResetPage()
    {
        passwordResetErrorText.text = "No internet connection";
    }
    public void ConnectedOnPasswordResetPage()
    {
        passwordResetErrorText.text = "";
    }
    public void NoConnectionOnMainPage()
    {
        lostConnectionPage.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        MouseHoverElement.instance.ChangeCursorToDefault();
    }
    public void ConnectedOnMainPage()
    {
        lostConnectionPage.SetActive(false);
    }
    public bool OnMainPage()
    {
        if (mainPage.activeInHierarchy) return true;
        if (lostConnectionPage.activeInHierarchy) return true;
        return false;
    }
    //TODO
    public void UnknownErrorOnLoginPage(AggregateException ex)
    {
        errorLoginText.text = ex.GetBaseException().Message;
    }
    public void UnknownErrorOnRegistrationPage(AggregateException ex)
    {
        errorRegistrationText.text = ex.GetBaseException().Message;
    }
    public void ErrorOnRegistrationPage(Firebase.FirebaseException firebaseEx)
    {
        GoBackToRegistration();

        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
        switch (errorCode)
        {
            case AuthError.MissingEmail:
                emailRegistrationText.text = "<color=#FF6D73>Email - <i>please provide an email address";
                break;
            case AuthError.InvalidEmail:
                emailRegistrationText.text = "<color=#FF6D73>Email - <i>invalid email";
                break;
            case AuthError.EmailAlreadyInUse:
                emailRegistrationText.text = "<color=#FF6D73>Email - <i>email is already in use";
                break;
            case AuthError.MissingPassword:
                passwordRegistrationText.text = "<color=#FF6D73>Password - <i>please provide a password";
                break;
            default:
                errorRegistrationText.text = "<color=#FF6D73>Registration Failed: " + firebaseEx.Message;
                break;
        }
    }
    public void ErrorOnResetPasswordPage(AggregateException ex)
    {
        passwordResetErrorText.text = ex.GetBaseException().Message;
    }
    public void ClearErrorOnResetPasswordPage()
    {
        passwordResetErrorText.text = null;
    }
    public void PasswordResetEmailSentSuccessfully()
    {
        passwordResetErrorText.text = "<color=#FFFFFF>Sent!";
    }
    private void InvalidEmailOnRegistration()
    {
        emailRegistrationText.text = "<color=#FF6D73>Email - <i>invalid email address";
    }
    public bool CheckIfRegistrationPasswordIsValid(string password)
    {
        if (password.Length < 10)
        {
            passwordRegistrationText.text = "<color=#FF6D73>Password - <i><size=80%>must be at least 10 characters long";
            return false;
        }
        else if (password.Length > 64)
        {
            passwordRegistrationText.text = "<color=#FF6D73>Password - <i><size=80%>must be less than 65 characters long";
            return false;
        }
        else
        {
            bool hasLower = false, hasUpper = false, hasNumber = false;
            foreach (char c in password)
            {
                if (Char.IsLower(c)) hasLower = true;
                if (Char.IsUpper(c)) hasUpper = true;
                if (Char.IsDigit(c)) hasNumber = true;
            }
            if (!hasLower)
            {
                passwordRegistrationText.text = "<color=#FF6D73>Password - <i><size=88%>must contain a lower-case letter";
                return false;
            }
            if (!hasUpper)
            {
                passwordRegistrationText.text = "<color=#FF6D73>Password - <i><size=89%>must contain an upper-case letter";
                return false;
            }
            if (!hasNumber)
            {
                passwordRegistrationText.text = "<color=#FF6D73>Password - <i>must contain a number";
                return false;
            }
            return true;
        }
    }
    public void DisplayNonMatchingPasswordError()
    {
        passwordRegistrationText.text = "<color=#FF6D73>Password - <i>passwords do not match";
    }
    public void ResetLoginAndRegisterPageTexts()
    {
        emailLoginText.text = "Email";
        passwordLoginText.text = "Password";
        errorLoginText.text = "";

        emailRegistrationText.text = "Email";
        passwordRegistrationText.text = "Password";
        errorRegistrationText.text = "";
    }
    public void ClearRegistrationInputFields()
    {
        emailRegistrationInput.text = "";
        passwordRegistrationInput.text = "";
        confirmPasswordRegistrationInput.text = "";
        UserAuthManager.instance.ChangeMailValueString(null);
        UserAuthManager.instance.ChangePasswordValueString(null);
        UserAuthManager.instance.ChangeConfirmationPasswordValueString(null);
    }
    public void ClearLoginInputFields()
    {
        emailLoginInput.text = "";
        passwordLoginInput.text = "";
        UserAuthManager.instance.ChangeMailValueString(null);
        UserAuthManager.instance.ChangePasswordValueString(null);
    }
    public void AttemptRegistration()
    {
        if (!InternetManager.isConnected) return;

        ResetLoginAndRegisterPageTexts();

        if (!UserAuthManager.instance.IsEmailValid(emailRegistrationInput.text))
        {
            InvalidEmailOnRegistration();
            return;
        }

        if (!CheckIfRegistrationPasswordIsValid(passwordRegistrationInput.text)) return;

        if (!UserAuthManager.instance.RegistrationPasswordMatches())
        {
            DisplayNonMatchingPasswordError();
            return;
        }

        ShowMailConfirmationPage();
    }
    public void IncorrectVerificationCodeRegister()
    {
        mailConfirmationErrorText.text = "Incorrect code";
    }
    public void IncorrectVerificationCodeLogin()
    {
        mailConfirmationLoginErrorText.text = "Incorrect code";
    }
    public void CorrectVerificationCodeRegister()
    {
        continueButtonConfirm.interactable = false;
        mailConfirmationErrorText.text = null;
    }
    public void CorrectVerificationCodeLogin()
    {
        continueButtonRecognize.interactable = false;
        mailConfirmationLoginErrorText.text = null;
    }
    public void ShowMailConfirmationPage()
    {
        registrationPage.SetActive(false);
        emailConfirmationPage.SetActive(true);
        continueButtonConfirm.interactable = true;
        bool emailSentSuccesfully = UserAuthManager.instance.SendConfirmationEmail(emailRegistrationInput.text, mailConfirmationErrorText);
        if (emailSentSuccesfully) emailWasSentRegisterText.text = $"A six digit number was sent to {emailRegistrationInput.text}. Please enter it in the field below to confirm your e-mail";
        EmailResendTimer.instance.StartTimer();
    }
    public void ShowUnrecognizedDevicePage()
    {
        loginPage.SetActive(false);
        unrecognizedDevicePage.SetActive(true);
        continueButtonRecognize.interactable = true;
        emailWasSentLoginText.text = $"A six digit number was sent to {FirebaseManager.instance.auth.CurrentUser.Email}. Please enter it in the field below to confirm this is you.";
        EmailResendTimer.instance.StartTimer();
    }
    public void GoBackToRegistration()
    {
        emailConfirmationPage.SetActive(false);
        registrationPage.SetActive(true);
        mailConfirmationErrorText.text = null;
        VerificationCodeInput.instance.ClearInput();
    }
    public void GoBackToLogin()
    {
        HidePasswordManager.instance.HidePasswordInputs();
        resetPasswordPage.SetActive(false);
        loginPage.SetActive(true);
        passwordResetErrorText.text = null;
    }
    public void GoToResetPasswordPage()
    {
        loginPage.SetActive(false);
        passwordResetEmailInput.text = emailLoginInput.text;
        resetPasswordPage.SetActive(true);
    }
    public void ResendConfirmationEmail()
    {
        if (!InternetManager.isConnected) return;

        bool emailSentSuccesfully = UserAuthManager.instance.SendConfirmationEmail(emailRegistrationInput.text, mailConfirmationErrorText);
        EmailResendTimer.instance.StartTimer();
    }
    public void ResendConfirmationLoginEmail()
    {
        if (!InternetManager.isConnected) return;

        UserAuthManager.instance.Send2FAEmail();
        EmailResendTimer.instance.StartTimer();
    }
    public void OnLogIn()
    {
        registrationPage.SetActive(false);
        unrecognizedDevicePage.SetActive(false);
        loginPage.SetActive(false);
        ClearCredentials();
        ClearLoginInputFields();

        HidePasswordManager.instance.HidePasswordInputs();
        mainPage.SetActive(true);
        StartCoroutine(LoadPasswordsOnLogin());
    }
    IEnumerator LoadPasswordsOnLogin()
    {
        float timer = 0f;
        FirebaseUser user = FirebaseManager.instance.auth.CurrentUser;
        while (user == null)
        {
            yield return null;
            timer += Time.deltaTime;
            if (timer > 10f)
            {
                Debug.LogError("Timed out - closing application");
                Application.Quit();
            }
        }

        PasswordManager.instance.GetAllPasswords();
    }
    private void ClearCredentials()
    {
        emailRegistrationInput.text = null;
        passwordRegistrationInput.text = null;
        confirmPasswordRegistrationInput.text = null;
        emailLoginInput.text = null;
        passwordLoginInput.text = null;
    }
    public void SignOut()
    {
        UserAuthManager.instance.SignOut();
    }
    public void OnSignOut()
    {
        mainPage.SetActive(false);
        PasswordManager.instance.ClearPasswordsList();
        loginPage.SetActive(true);
    }
    public void ShowDeletePasswordWindow(string name)
    {
        deletePasswordWindow.SetActive(true);
        deletePasswordText.text = $"Are you sure you want to delete \"{name}\"?";
    }
    public void ShowMainPagePlaceholder()
    {
        mainPagePlaceholder.SetActive(true);
    }
    public void HideMainPagePlaceholder()
    {
        mainPagePlaceholder.SetActive(false);
    }
}
