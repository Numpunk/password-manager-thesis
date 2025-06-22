using System;
using System.IO;
using System.Linq;
using System.Text;
using NaughtyAttributes;
using UnityEngine;
public class AppStartupManager : MonoBehaviour
{
    [HideInInspector]
    public bool hasRegistered;
    public static AppStartupManager instance;

    private string[] userData;

    private void Awake()
    {
        int windowWidth = (int)(Screen.currentResolution.width / 4.4848f); //1920x1080 should equal 428*761
        int windowHeight = (int)(windowWidth / 0.5625f);
        Screen.SetResolution(windowWidth, windowHeight, false);

        if (instance != this) instance = this;

        userData = GetUserData();
        if (userData != null)
        {
            hasRegistered = HasRegistered();
        }
    }
    private void Start()
    {
        LoadSortingChoice();
    }
    /*[Button]
    private void CreateUserData()
    {
        string path = Application.persistentDataPath + "/userData.bin";

        string contents = "hasRegistered: " + true + "\npasswordsListIsEmpty: " + true;
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        byte[] key = new byte[16];
        Array.Copy(Encoding.UTF8.GetBytes(deviceID), key, 16);
        byte[] iv = key;
        byte[] data = PasswordEncryption.instance.AESEncrypt(contents, key, iv);
        File.WriteAllBytes(path, data);
    }*/
    public void SaveSortingChoice(PasswordManager.SortingType sortingType)
    {
        string path = Application.persistentDataPath + "/sortChoice.txt";

        string contents;
        if (sortingType == PasswordManager.SortingType.Descending) contents = "0";
        else if (sortingType == PasswordManager.SortingType.Ascending) contents = "1";
        else if (sortingType == PasswordManager.SortingType.DateDescending) contents = "2";
        else contents = "3";

        File.WriteAllText(path, contents);
    }
    public void LoadSortingChoice()
    {
        string path = Application.persistentDataPath + "/sortChoice.txt";
        if (!File.Exists(path)) return;

        string contents = File.ReadAllText(path);
        PasswordManager.instance.LoadSortingType(int.Parse(contents));
    }
    private string[] GetUserData()
    {
        string path = Application.persistentDataPath + "/userData.bin";
        if (!File.Exists(path)) return null;

        byte[] data = File.ReadAllBytes(path);

        string deviceID = SystemInfo.deviceUniqueIdentifier;
        byte[] key = new byte[16];
        Array.Copy(Encoding.UTF8.GetBytes(deviceID), key, 16);
        byte[] iv = key;
        string contents = PasswordEncryption.instance.AESDecrypt(data, key, iv);

        string[] contentsArr = contents.Split('\n').ToArray();

        return contentsArr;
    }
    private bool HasRegistered()
    {
        if (userData[0].EndsWith("True")) return true;
        return false;
    }
    private bool PasswordsListIsEmpty()
    {
        if (userData[1].EndsWith("True")) return true;
        return false;
    }
    public void UpdateUserData()
    {
        string path = Application.persistentDataPath + "/userData.bin";

        string contents = "hasRegistered: " + hasRegistered;
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        byte[] key = new byte[16];
        Array.Copy(Encoding.UTF8.GetBytes(deviceID), key, 16);
        byte[] iv = key;
        byte[] data = PasswordEncryption.instance.AESEncrypt(contents, key, iv);
        File.WriteAllBytes(path, data);
    }
}
