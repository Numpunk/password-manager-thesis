using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NaughtyAttributes;
using UnityEngine;

public class PasswordEncryption : MonoBehaviour
{
    const string charsUpper = "ABCDEFGHIJKLMNOPQRTSUVWXYZ";
    const string charsLower = "abcdefghijklmnopqrstuvwxyz";
    const string nums = "1234567890";
    const string specialChars = "~`!@#$%^&*()-_+={}[]|\\;:\"<>,./?";


    int byteToCharsRatio = Mathf.CeilToInt(255f / 26f);
    int byteToNumsRation = Mathf.CeilToInt(255f / 10f);
    int byteToSpecialRatio = Mathf.CeilToInt(255f / 31f);


    [SerializeField]
    private string password;
    [SerializeField]
    private string seedPassword;

    private DateTime passwordLastAccessedTime;
    public static PasswordEncryption instance;
    private void Awake()
    {
        if (instance != this) instance = this;
    }
    [Button]
    public void GeneratePassword()
    {
        print("NEW PASSWORD: " + GeneratePasswordString());
    }
    public string GeneratePasswordString()
    {
        string seed = GenerateSeed();
        string newPassword = "";
        byte[] bytes = Convert.FromBase64String(seed);

        bool hasUpper = false, hasLower = false, hasNum = false, hasSpecial = false;
        int passwordIndex = 0;
        for (int j = 0; j < 16; j++)
        {
            if (!hasUpper && bytes[j] > 196)
            {
                hasUpper = true;
                ChooseRandomChar(1, bytes[passwordIndex], ref newPassword);
                passwordIndex++;
            }
            else if (!hasLower && bytes[j] > 128)
            {
                hasLower = true;
                ChooseRandomChar(2, bytes[passwordIndex], ref newPassword);
                passwordIndex++;
            }
            else if (!hasNum && bytes[j] > 64)
            {
                hasNum = true;
                ChooseRandomChar(3, bytes[passwordIndex], ref newPassword);
                passwordIndex++;
            }
            else if (!hasSpecial && bytes[j] < 65)
            {
                hasSpecial = true;
                ChooseRandomChar(4, bytes[passwordIndex], ref newPassword);
                passwordIndex++;
            }
            if (passwordIndex == 4) break;
        }

        if (!(hasUpper && hasLower && hasNum && hasSpecial)) print("NOT EVERY CONDITION WAS MET: \n" + "Upper: " + hasUpper + "\n" + "Lower: " + hasLower + "\n" + "Number: " + hasNum + "\n" + "Special: " + hasSpecial);
        if (!hasUpper)
        {
            ChooseRandomChar(1, bytes[passwordIndex], ref newPassword);
            passwordIndex++;
        }
        if (!hasLower)
        {
            ChooseRandomChar(2, bytes[passwordIndex], ref newPassword);
            passwordIndex++;
        }
        if (!hasNum)
        {
            ChooseRandomChar(3, bytes[passwordIndex], ref newPassword);
            passwordIndex++;
        }
        if (!hasSpecial)
        {
            ChooseRandomChar(4, bytes[passwordIndex], ref newPassword);
            passwordIndex++;
        }
        for (int i = passwordIndex; i < 10; i++)
        {
            if (bytes[i + 6] > 196)
            {
                ChooseRandomChar(1, bytes[i], ref newPassword);
            }
            else if (bytes[i + 6] > 128)
            {
                ChooseRandomChar(2, bytes[i], ref newPassword);
            }
            else if (bytes[i + 6] > 64)
            {
                ChooseRandomChar(3, bytes[i], ref newPassword);
            }
            else
            {
                ChooseRandomChar(4, bytes[i], ref newPassword);
            }
        }
        return newPassword;
    }
    [Button]
    void GenerateAES()
    {

        //create AES key using the current dateTime
        DateTime dateTime = DateTime.Now;
        byte[] aesKey;
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] dateHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(dateTime.ToString()));
            aesKey = new byte[16];
            Array.Copy(dateHash, aesKey, 16);
        }

        //create the AES IV using the device's unique ID
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        byte[] deviceIDBytes = new byte[16];
        Array.Copy(Encoding.UTF8.GetBytes(deviceID), deviceIDBytes, 16);

        byte[] AESData = AESEncrypt(password, aesKey, deviceIDBytes);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < AESData.Length; i++)
        {
            sb.Append(AESData[i].ToString("x2"));
        }
        string fileName = sb.ToString();
        string path = Application.persistentDataPath + "/password-" + fileName + ".txt";
        print(path);
        string[] files = Directory.GetFiles(Application.persistentDataPath);
        foreach (string file in files)
        {
            //check if file exists
            if (file.Substring(Application.persistentDataPath.Length + 1, 8) == "password")
            {
                //delete file in order to replace
                File.Delete(file);
            }
        }
        File.WriteAllBytes(path, AESData);
    }
    [Button]
    void DecryptAES()
    {
        string path = GetPasswordPath();

        //create AES key using the dateTime when the file was created
        DateTime fileCreationTime = File.GetCreationTime(path);
        byte[] aesKey;
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] dateHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(fileCreationTime.ToString()));
            aesKey = new byte[16];
            Array.Copy(dateHash, aesKey, 16);
        }

        //create the AES IV using the device's unique ID
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        byte[] deviceIDBytes = new byte[16];
        Array.Copy(Encoding.UTF8.GetBytes(deviceID), deviceIDBytes, 16);

        string decryptedPassword = AESDecrypt(File.ReadAllBytes(path), aesKey, deviceIDBytes);
        print(decryptedPassword);
    }
    [Button]
    void CheckFileValidity()
    {
        string path = GetPasswordPath();
        print(IsValidAES(path));
    }
    private void ChooseRandomChar(int arrayNum, byte value, ref string password)
    {
        if (arrayNum == 1)
        {
            if (value == 0) { password += charsUpper[0]; return; }

            int index = value / byteToCharsRatio;

            password += charsUpper[index];
        }
        else if (arrayNum == 2)
        {
            if (value == 0) { password += charsLower[0]; return; }

            int index = value / byteToCharsRatio;

            password += charsLower[index];
        }
        else if (arrayNum == 3)
        {
            if (value == 0) { password += nums[0]; return; }

            int index = value / byteToNumsRation;

            password += nums[index];
        }
        else if (arrayNum == 4)
        {
            if (value == 0) { password += specialChars[0]; return; }

            int index = value / byteToSpecialRatio;

            password += specialChars[index];
        }
    }
    string GenerateSeed()
    {
        string seed = "";

        byte[] passwordBytes = Convert.FromBase64String(CreateSHA256HashToBase64(password));
        byte[] seedBytes = Convert.FromBase64String(seedPassword);

        byte[] newBytes = new byte[16];

        for (int i = 0; i < 16; i++)
        {
            if (passwordBytes[i] + seedBytes[i] > 255) newBytes[i] = (byte)((passwordBytes[i] + seedBytes[i]) - 256);
            else newBytes[i] = (byte)(passwordBytes[i] + seedBytes[i]);
        }
        seed = Convert.ToBase64String(newBytes);
        return seed;
    }
    public string CreateSHA256HashToBase64(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            Array.Resize(ref bytes, 16);

            return Convert.ToBase64String(bytes);
        }
    }
    public string CreateSHA256HashHexadecimal(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
    public byte[] AESEncrypt(string plaintext, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plaintext);
                    }
                    return ms.ToArray();
                }
            }
        }
    }
    public string AESDecrypt(byte[] cipherText, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream(cipherText))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
    string GetPasswordPath()
    {
        string path = "";
        string[] files = Directory.GetFiles(Application.persistentDataPath);

        foreach (string file in files)
        {
            //find the password file
            if (file.Substring(Application.persistentDataPath.Length + 1, 8) == "password")
            {
                path = file;
                passwordLastAccessedTime = File.GetLastAccessTime(path);
                byte[] AESData = File.ReadAllBytes(path);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < AESData.Length; i++)
                {
                    sb.Append(AESData[i].ToString("x2"));
                }

                //make sure the contents match the file name
                if (file == Application.persistentDataPath + "\\password-" + sb.ToString() + ".txt") break;
                else path = "";
            }
        }
        if (path == "") throw new UnityException("Password file not found in appdata");

        return path;
    }
    bool IsValidAES(string path)
    {
        DateTime fileCreationTime = File.GetCreationTime(path);
        DateTime fileModificationTime = File.GetLastWriteTime(path);
        if (fileCreationTime != fileModificationTime) return false;
        if (passwordLastAccessedTime != fileCreationTime) return false;
        byte[] aesKey;
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] dateHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(fileCreationTime.ToString()));
            aesKey = new byte[16];
            Array.Copy(dateHash, aesKey, 16);
        }

        //create the AES IV using the device's unique ID
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        byte[] deviceIDBytes = new byte[16];
        Array.Copy(Encoding.UTF8.GetBytes(deviceID), deviceIDBytes, 16);

        string decryptedPassword;
        try { decryptedPassword = AESDecrypt(File.ReadAllBytes(path), aesKey, deviceIDBytes); }
        catch { return false; }

        byte[] aesData = AESEncrypt(decryptedPassword, aesKey, deviceIDBytes);
        byte[] readBytes = File.ReadAllBytes(path);
        for (int i = 0; i < 16; i++)
        {
            if (aesData[i] != readBytes[i])
            {
                print("BYTE NUMBER " + i + " DOES NOT MATCH, RETURNING FALSE");
                return false;
            }
        }
        return true;
    }
}
