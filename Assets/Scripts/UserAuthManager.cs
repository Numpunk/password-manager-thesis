using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
public class UserAuthManager : MonoBehaviour
{
    [SerializeField]
    private string email;
    [SerializeField]
    private string password;
    [SerializeField]
    private string confirmationPassword;
    [SerializeField]
    private bool rememberMe;
    [SerializeField]
    private bool checkIfDeviceIsRegistered;
    [SerializeField]
    private string verificationCodeOutput;
    [SerializeField]
    private string verificationCodeInput;

    private DateTime lastLoginAttempt;
    private DateTime lastEmailSent;
    private PageManager pageManager;
    FirebaseAuth auth;
    FirebaseUser user;

    public static UserAuthManager instance;
    private void Awake()
    {
        if (instance != this) instance = this;
    }
    void Start()
    {
        FirebaseApp app = FirebaseManager.instance.app;
        auth = FirebaseManager.instance.auth;
        print("Authentication is ready");
        pageManager = PageManager.instance;

        //check if credentials are remembered
        string path = GetEncryptedCredentialsPath();
        if (File.Exists(path))
        {
            byte[] data = File.ReadAllBytes(path);
            string credentials = PasswordEncryption.instance.AESDecrypt(data, GetDeviceIdBytes(16), GetTimeCreatedBytes(path));
            string[] strings = credentials.Split('\n').ToArray();
            email = strings[0];
            password = strings[1];
            LogIn();
        }
    }
    public void ChangeMailValue(TMP_InputField text)
    {
        email = text.text;
    }
    public void ChangePasswordValue(TMP_InputField text)
    {
        password = text.text;
    }
    public void ChangeConfirmationPasswordValue(TMP_InputField text)
    {
        confirmationPassword = text.text;
    }
    public void ChangeMailValueString(string newEmail)
    {
        email = newEmail;
    }
    public void ChangePasswordValueString(string newPassword)
    {
        password = newPassword;
    }
    public void ChangeConfirmationPasswordValueString(string newPassword)
    {
        confirmationPassword = newPassword;
    }
    public void Set6DigitInputValueRegister(string value)
    {
        if (!InternetManager.isConnected) return;
        
        verificationCodeInput = value;
        if (verificationCodeInput == verificationCodeOutput)
        {
            pageManager.CorrectVerificationCodeRegister();
            Register();
        }
        else
        {
            pageManager.IncorrectVerificationCodeRegister();
        }
    }
    public void Set6DigitInputValueLogin(string value)
    {
        if (!InternetManager.isConnected) return;

        verificationCodeInput = value;
        if (verificationCodeInput == verificationCodeOutput)
        {
            pageManager.CorrectVerificationCodeLogin();
            FireStoreWriter.instance.AddOrUpdate("userDevices", user.UserId, GetHashedDeviceId(), Timestamp.GetCurrentTimestamp());
        }
        else
        {
            pageManager.IncorrectVerificationCodeLogin();
        }
    }
    [Button]
    public void LogIn()
    {
        if (!InternetManager.isConnected) return;
        
        if (!CanAttemptLogin())
        {
            Debug.LogWarning("Wait before another login attempt");
            float timeAfterLastLogin = (float)(DateTime.Now.Subtract(lastLoginAttempt)).TotalSeconds;
            PageManager.instance.PlayWaitBeforeLoginTextAnimation(3 - timeAfterLastLogin);
            return;
        }
        pageManager.ResetLoginAndRegisterPageTexts();
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>  //run the task on the main thread
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                if (task.IsFaulted)
                {
                    if (task.Exception != null)
                    {
                        Firebase.FirebaseException firebaseEx = task.Exception.GetBaseException() as Firebase.FirebaseException;
                        if (firebaseEx != null)
                        {
                            pageManager.ErrorOnLoginPage(firebaseEx);
                            var errorCode = (AuthError)firebaseEx.ErrorCode;
                            switch (errorCode)
                            {
                                case AuthError.MissingEmail:
                                    Debug.LogError("Missing Email");
                                    break;
                                case AuthError.InvalidEmail:
                                    Debug.LogError("Invalid Email");
                                    break;
                                case AuthError.WrongPassword:
                                    Debug.LogError("Wrong Password");
                                    break;
                                case AuthError.UserNotFound:
                                    Debug.LogError("User Not Found");
                                    break;
                                case AuthError.TooManyRequests:
                                    Debug.LogError("Too many login attempts. Try again later.");
                                    break;
                                default:
                                    Debug.LogError("Login Failed: " + firebaseEx.Message);
                                    break;
                            }
                        }
                        else
                        {
                            pageManager.UnknownErrorOnLoginPage(task.Exception);
                            Debug.LogError("Unknown error: " + task.Exception.GetBaseException().Message);
                        }
                    }
                }
                lastLoginAttempt = DateTime.Now;
                return;
            }
            user = auth.CurrentUser;
            Debug.Log("User signed in successfully: " + user.Email);
            AppStartupManager.instance.hasRegistered = true;
            AppStartupManager.instance.UpdateUserData();
            if (rememberMe)
            {
                CreateCredentialsFile();
            }

            if (checkIfDeviceIsRegistered) CheckIfDeviceIsRegistered();
            //else pageManager.OnLogIn();
        });

    }
    public void OnRememberMeToggle()
    {
        rememberMe = !rememberMe;
    }
    [Button]
    void LoginUsingCredentialFile()
    {
        string path = GetEncryptedCredentialsPath();
        if (File.Exists(path))
        {
            byte[] data = File.ReadAllBytes(path);
            string credentials = PasswordEncryption.instance.AESDecrypt(data, GetDeviceIdBytes(16), GetTimeCreatedBytes(path));
            string[] strings = credentials.Split('\n').ToArray();
            email = strings[0];
            password = strings[1];
            LogIn();
        }
    }
    [Button]
    public void Send2FAEmail()
    {
        SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
        client.EnableSsl = true;

        client.Credentials = new NetworkCredential("numpunk.encrypt@gmail.com", "vsse lvpz mutk cezw");

        MailMessage mailMessage = new MailMessage();
        mailMessage.From = new MailAddress("numpunk.encrypt@gmail.com");
        mailMessage.To.Add(user.Email);
        mailMessage.Subject = "Your Two-Factor Authentication Code";
        verificationCodeOutput = UnityEngine.Random.Range(100000, 1000000).ToString();
        mailMessage.Body = $"Your verification code is: {verificationCodeOutput}";

        try
        {
            client.Send(mailMessage);
            print("Verification email sent successfully.");
        }
        catch (Exception ex)
        {
            print("Error sending email: " + ex.Message);
        }
    }
    public bool SendConfirmationEmail(string _email, TMP_Text errorText)
    {
        SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
        client.EnableSsl = true;

        client.Credentials = new NetworkCredential("numpunk.encrypt@gmail.com", "vsse lvpz mutk cezw");

        MailMessage mailMessage = new MailMessage();
        mailMessage.From = new MailAddress("numpunk.encrypt@gmail.com");
        mailMessage.To.Add(_email);
        mailMessage.Subject = "Your Verification Code";
        verificationCodeOutput = UnityEngine.Random.Range(100000, 1000000).ToString();
        mailMessage.Body = $"Your code is: {verificationCodeOutput}";

        try
        {
            client.Send(mailMessage);
            print("Verification email sent successfully.");
            return true;
        }
        catch (Exception ex)
        {
            string errorMsg = "Error sending email: " + ex.Message;
            print(errorMsg);
            errorText.text = errorMsg;
            return false;
        }
    }
    public void SendPasswordResetEmail()
    {
        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                pageManager.ErrorOnResetPasswordPage(task.Exception);
                Debug.LogError("Error: " + task.Exception.ToString());
            }
            else
            {
                pageManager.PasswordResetEmailSentSuccessfully();
            }
        });
    }
    bool CanAttemptLogin()
    {
        if (DateTime.Now > lastLoginAttempt.AddSeconds(3)) return true;
        return false;
    }
    void CreateCredentialsFile()
    {
        string path = GetEncryptedCredentialsPath();
        FileInfo fileInfo = new FileInfo(path);

        if (!File.Exists(path))
        {
            File.Create(path).Dispose();
            fileInfo.CreationTime = DateTime.Now;
            string credentials = email + "\n" + password;
            byte[] data = PasswordEncryption.instance.AESEncrypt(credentials, GetDeviceIdBytes(16), GetTimeNowBytes());
            File.WriteAllBytes(path, data);
        }
    }
    void CheckIfDeviceIsRegistered()
    {
        string hashedId = GetHashedDeviceId();

        DocumentReference docRef = FireStoreWriter.instance.db.Collection("userDevices").Document(user.UserId);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    if (snapshot.ContainsField(hashedId))
                    {
                        object fieldValue = snapshot.GetValue<object>(hashedId);
                        pageManager.OnLogIn();
                    }
                    else
                    {
                        Debug.Log("Device not recognized. Please enter the 6 digit verification code sent to your email " +
                            $"({user.Email})");
                        Send2FAEmail();
                        pageManager.ShowUnrecognizedDevicePage();
                    }
                }
                else
                {
                    Debug.LogError("ERROR! USER DOES NOT HAVE A DEVICES DOCUMENT");
                    FireStoreWriter.instance.CreateFirstDeviceDocument(user.UserId, hashedId);
                }
            }
            else
            {
                Debug.LogError("Error getting field: " + task.Exception);
            }
        });
    }
    [Button]
    public void CreateUserDevicesDocument()
    {
        string hashedDeviceId = GetHashedDeviceId();

        FireStoreWriter.instance.CreateFirstDeviceDocument(user.UserId, hashedDeviceId);
    }
    string GetHashedDeviceId()
    {
        string deviceId = SystemInfo.deviceUniqueIdentifier;
        return PasswordEncryption.instance.CreateSHA256HashHexadecimal(deviceId);
    }
    [Button]
    public void SignOut()
    {
        string path = GetEncryptedCredentialsPath();
        if (File.Exists(path)) File.Delete(path);

        auth.SignOut();
        Debug.Log("User signed out successfully");

        pageManager.OnSignOut();
    }
    [Button]
    public void Register()
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Firebase.FirebaseException firebaseEx = task.Exception.GetBaseException() as Firebase.FirebaseException;
                if (firebaseEx != null)
                {
                    pageManager.ErrorOnRegistrationPage(firebaseEx);
                    Debug.LogError("Registration failed. Error: " + task.Exception?.Flatten().InnerExceptions[0].Message);
                }
                else
                {
                    pageManager.UnknownErrorOnRegistrationPage(task.Exception);
                    Debug.LogError("Unknown error: " + task.Exception.GetBaseException().Message);
                }
                return;
            }
            user = auth.CurrentUser;
            Debug.Log("User registered successfully: " + user.Email);
            CreateUserDevicesDocument();
            pageManager.LoadMainPageOnRegister();
            pageManager.ClearRegistrationInputFields();
            AppStartupManager.instance.hasRegistered = true;
            AppStartupManager.instance.UpdateUserData();

            if (rememberMe)
            {
                CreateCredentialsFile();
            }
        });
    }
    private void OnDisable()
    {
        auth.SignOut();
    }
    byte[] GetDeviceIdBytes(int byteCount)
    {
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        byte[] deviceIDBytes = new byte[byteCount];
        Array.Copy(Encoding.UTF8.GetBytes(deviceID), deviceIDBytes, byteCount);
        return deviceIDBytes;
    }
    byte[] GetTimeNowBytes()
    {
        DateTime dateTime = DateTime.Now;

        byte[] bytes;
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] dateHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(dateTime.ToString()));
            bytes = new byte[16];
            Array.Copy(dateHash, bytes, 16);
        }
        return bytes;
    }
    byte[] GetTimeCreatedBytes(string path)
    {
        DateTime dateTime = File.GetCreationTime(path);
        byte[] bytes;
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] dateHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(dateTime.ToString()));
            bytes = new byte[16];
            Array.Copy(dateHash, bytes, 16);
        }
        return bytes;
    }
    string GetEncryptedCredentialsPath()
    {
        byte[] deviceIDBytes = GetDeviceIdBytes(8);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < deviceIDBytes.Length; i++)
        {
            sb.Append(deviceIDBytes[i].ToString("x2"));
        }
        string fileName = sb.ToString();
        string path = Application.persistentDataPath + "/" + fileName + ".bin";
        return path;
    }
    public bool IsEmailSyntaxValid(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
    public bool IsEmailDomainValid(string email)
    {
        try
        {
            string domain = email.Split('@')[1];
            IPHostEntry host = Dns.GetHostEntry(domain);
            return host.AddressList.Length > 0;
        }
        catch
        {
            return false;
        }
    }
    public bool IsEmailValid(string email)
    {
        return IsEmailSyntaxValid(email) && IsEmailDomainValid(email);
    }
    public bool RegistrationPasswordMatches()
    {
        if (password != confirmationPassword) return false;

        return true;
    }
}
