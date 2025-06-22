using System;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;

public class PasswordGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject addPasswordWindow;
    [SerializeField]
    private TMP_InputField passwordInput;

    const string charsUpper = "ABCDEFGHIJKLMNOPQRTSUVWXYZ";
    const string charsLower = "abcdefghijklmnopqrstuvwxyz";
    const string nums = "1234567890";
    const string specialChars = "~`!@#$%^&*()-_+={}[]|\\;:\"<>,./?";


    int byteToCharsRatio = Mathf.CeilToInt(255f / 26f);
    int byteToNumsRation = Mathf.CeilToInt(255f / 10f);
    int byteToSpecialRatio = Mathf.CeilToInt(255f / 31f);


    [SerializeField]
    private string key="0000000000000000";
    [SerializeField]
    private string iv;

    private void Start()
    {
        //generate the key
        char[] newKey = new char[16];
        for (int i = 0; i < 16; i++)
        {
            newKey[i] = GetRandomChar();
        }
        key = new string(newKey);

        GenerateIv();
    }
    char GetRandomChar()
    {
        int randomInt = UnityEngine.Random.Range(0, 26);
        return charsUpper[randomInt];
    }
    void GenerateIv()
    {
        DateTime currentTime = DateTime.Now;
        string timeString = currentTime.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
        iv = CreateSHA256HashToBase64(timeString);
    }
    public void GeneratePassword()
    {
        string password= "";
        string seed = GenerateSeed();
        byte[] bytes = Convert.FromBase64String(seed);

        bool hasUpper = false, hasLower = false, hasNum = false, hasSpecial = false;
        int passwordIndex = 0;
        for (int j = 0; j < 16; j++)
        {
            if (!hasUpper && bytes[j] > 196)
            {
                hasUpper = true;
                ChooseRandomChar(1, bytes[passwordIndex], ref password);
                passwordIndex++;
            }
            else if (!hasLower && bytes[j] > 128)
            {
                hasLower = true;
                ChooseRandomChar(2, bytes[passwordIndex], ref password);
                passwordIndex++;
            }
            else if (!hasNum && bytes[j] > 64)
            {
                hasNum = true;
                ChooseRandomChar(3, bytes[passwordIndex], ref password);
                passwordIndex++;
            }
            else if (!hasSpecial && bytes[j] < 65)
            {
                hasSpecial = true;
                ChooseRandomChar(4, bytes[passwordIndex], ref password);
                passwordIndex++;
            }
            if (passwordIndex == 4) break;
        }

        if (!(hasUpper && hasLower && hasNum && hasSpecial)) print("NOT EVERY CONDITION WAS MET: \n" + "Upper: " + hasUpper + "\n" + "Lower: " + hasLower + "\n" + "Number: " + hasNum + "\n" + "Special: " + hasSpecial);
        if (!hasUpper)
        {
            ChooseRandomChar(1, bytes[passwordIndex], ref password);
            passwordIndex++;
        }
        if (!hasLower)
        {
            ChooseRandomChar(2, bytes[passwordIndex], ref password);
            passwordIndex++;
        }
        if (!hasNum)
        {
            ChooseRandomChar(3, bytes[passwordIndex], ref password);
            passwordIndex++;
        }
        if (!hasSpecial)
        {
            ChooseRandomChar(4, bytes[passwordIndex], ref password);
            passwordIndex++;
        }
        for (int i = passwordIndex; i < 10; i++)
        {
            if (bytes[i + 6] > 196)
            {
                ChooseRandomChar(1, bytes[i], ref password);
            }
            else if (bytes[i + 6] > 128)
            {
                ChooseRandomChar(2, bytes[i], ref password);
            }
            else if (bytes[i + 6] > 64)
            {
                ChooseRandomChar(3, bytes[i], ref password);
            }
            else
            {
                ChooseRandomChar(4, bytes[i], ref password);
            }
        }
        passwordInput.text = password;
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
        GenerateIv();
        string seed = "";

        byte[] keyBytes = Convert.FromBase64String(CreateSHA256HashToBase64(key));
        byte[] ivBytes = Convert.FromBase64String(CreateSHA256HashToBase64(iv));

        byte[] newBytes = new byte[16];

        for (int i = 0; i < 16; i++)
        {
            if (keyBytes[i] + ivBytes[i] > 255) newBytes[i] = (byte)((keyBytes[i] + ivBytes[i]) - 256);
            else newBytes[i] = (byte)(keyBytes[i] + ivBytes[i]);
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
}
