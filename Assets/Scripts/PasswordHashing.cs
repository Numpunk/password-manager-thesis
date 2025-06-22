using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NaughtyAttributes;
using UnityEngine;

public class PasswordHashing : MonoBehaviour
{
    [SerializeField]
    private string _password;
    SHA256Managed sHA256 = new SHA256Managed();
    [Button]
    void GenerateHash()
    {
        print(CreateSHA256Hash(_password));
    }
    string CreateSHA256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            Array.Resize(ref bytes, 16);

            /*StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();*/

            return Convert.ToBase64String(bytes);
        }
    }
}

