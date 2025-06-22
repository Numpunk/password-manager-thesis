using Firebase.Firestore;
using System;
using UnityEngine;

public class PasswordData
{
    public string name;
    public string username;
    public string email;
    public string value;
    public Timestamp dateAdded;

    public PasswordData(string name, string value, Timestamp dateAdded)
    {
        this.name = name;
        this.value = value;
        this.dateAdded = dateAdded;
    }
    public PasswordData(string name, string username, string email, string value, Timestamp dateAdded)
    {
        this.name = name;
        this.username = username;
        this.email = email;
        this.value = value;
        this.dateAdded = dateAdded;
    }
    public PasswordData()
    {
        
    }
}
