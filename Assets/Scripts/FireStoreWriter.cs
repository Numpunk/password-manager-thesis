using Firebase.Extensions;
using Firebase.Firestore;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class FireStoreWriter : MonoBehaviour
{
    public FirebaseFirestore db;
    private PasswordEncryption passEncryption;

    [SerializeField]
    private string data;
    [SerializeField]
    private string customDocumentName;
    [SerializeField]
    private string customCollectionName;

    public static FireStoreWriter instance;
    private void Awake()
    {
        if (instance != this) instance = this;
    }
    private void Start()
    {
        db = FirebaseManager.instance.db;
        passEncryption = GetComponent<PasswordEncryption>();
    }

    [Button]
    void StoreDataInTestDocument()
    {
        Set("passwords-test", "passwords-document-test", "key1", data);
    }
    [Button]
    void StoreUserData()
    {
        if (FirebaseManager.instance.auth.CurrentUser == null) { Debug.LogWarning("Warning! User is not logged in."); return; }

        string pass = passEncryption.GeneratePasswordString();
        Set("users", FirebaseManager.instance.auth.CurrentUser.UserId, "password", pass);
    }
    public void CreateEmptyDocument(string collection, string documentId)
    {
        db.Collection(collection).Document(documentId).SetAsync(new Dictionary<string, object>()).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (FirebaseManager.instance.auth.CurrentUser == null) Debug.LogWarning("Warning! User is NOT logged in");
                else print("Document created successfully");
            }
            else
            {
                Debug.LogError("Error creating document: " + task.Exception);
            }
        });
    }
    public void CreateFirstDeviceDocument(string userId, string hashedDeviceId)
    {
        db.Collection("userDevices").Document(userId).SetAsync(new Dictionary<string, object>()).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (FirebaseManager.instance.auth.CurrentUser == null) Debug.LogWarning("Warning! User is NOT logged in");
                else
                {
                    print("Devices document created successfully.");
                    DocumentReference docRef = db.Collection("userDevices").Document(userId);
                    Timestamp dateCreated = Timestamp.GetCurrentTimestamp();

                    Dictionary<string, object> updates = new Dictionary<string, object>
                    {
                        {hashedDeviceId, dateCreated}
                    };

                    docRef.UpdateAsync(updates).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompleted)
                        {
                            print("Device id added successfully.");

                            //add email to make debugging easier
                            updates = new Dictionary<string, object>
                            {
                                {"Email", FirebaseManager.instance.auth.CurrentUser.Email}
                            };
                            docRef.UpdateAsync(updates).ContinueWithOnMainThread(task =>
                            {
                                if (!task.IsCompleted)
                                {
                                    Debug.LogError("Error adding email: " + task.Exception);
                                }
                            });
                        }
                        else
                        {
                            Debug.LogError("Error adding device id: " + task.Exception);
                        }
                    });

                }
            }
            else
            {
                Debug.LogError("Error creating devices document: " + task.Exception);
            }
        });
    }
    public void AddNewPassword(string userId, string passwordName, string passwordValue)
    {
        DocumentReference docRef = db.Collection("userPs").Document(userId);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Dictionary<string, object> updates = new Dictionary<string, object>
                    {
                       {passwordName, passwordValue}
                    };

                    docRef.UpdateAsync(updates).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompleted)
                        {
                            print("Password added/updated successfully.");
                        }
                        else
                        {
                            Debug.LogError("Error adding/updating password: " + task.Exception);
                        }
                    });
                }
                else
                {
                    print("Creating a new passwords document");

                }
            }
            else
            {
                Debug.LogError("Error adding password: " + task.Exception);
            }
        });
    }
    public async void AddPasswordWithTimestamp(string userId, string passwordName, string passwordValue)
    {
        var docRef = db.Collection("userPs").Document(userId);

        var data = new Dictionary<string, object>
        {
            {passwordName,passwordValue },
            {passwordName+"_dateAdded", FieldValue.ServerTimestamp }
        };
        await docRef.SetAsync(data, SetOptions.MergeAll);
    }
    private void CreatePasswordsDocumentAndAddPassword(string userId, string passwordName, string passwordValue, DocumentReference docRef)
    {
        db.Collection("userPs").Document(userId).SetAsync(new Dictionary<string, object>()).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (FirebaseManager.instance.auth.CurrentUser == null) Debug.LogWarning("Warning! User is NOT logged in");
                else
                {
                    Dictionary<string, object> updates = new Dictionary<string, object>
                    {
                       {passwordName, passwordValue}
                    };

                    docRef.UpdateAsync(updates).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompleted)
                        {
                            print("Password added/updated successfully.");
                        }
                        else
                        {
                            Debug.LogError("Error adding/updating password: " + task.Exception);
                        }
                    });
                }
            }
            else
            {
                Debug.LogError("Error creating document: " + task.Exception);
            }
        });
    }
    public void Set(string collection, string documentId, string name, string value)
    {
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { name, value }
        };

        db.Collection(collection).Document(documentId).SetAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (FirebaseManager.instance.auth.CurrentUser == null) Debug.LogWarning("Warning! User is NOT logged in");
                else print("Data stored successfully");
            }
            else
            {
                Debug.LogError("Error storing data: " + task.Exception);
            }
        });
    }
    public void AddOrUpdate(string collection, string documentId, string name, object value)
    {
        DocumentReference docRef = db.Collection(collection).Document(documentId);

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            {name, value}
        };

        docRef.UpdateAsync(updates).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (collection == "userPs") PasswordManager.instance.OnPasswordAddedOrUpdated();
                else if(collection=="userDevices") PageManager.instance.OnLogIn();
                print("Field added/updated successfully.");
            }
            else
            {
                Debug.LogError("Error updating field: " + task.Exception);
            }
        });
    }
    public void AddOrUpdateDocument(string collection, string documentId, Dictionary<string, object> updates)
    {
        DocumentReference docRef = db.Collection(collection).Document(documentId);

        docRef.SetAsync(updates).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (collection.StartsWith("userPs"))
                {
                    PasswordManager.instance.OnPasswordAddedOrUpdated();
                }
                print("Field added/updated successfully.");
            }
            else
            {
                Debug.LogError("Error updating field: " + task.Exception);
            }
        });
    }
    public void Get(string collection, string documentId, string name)
    {
        DocumentReference docRef = db.Collection(collection).Document(documentId);

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            {name, FieldValue.Delete}
        };

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    if (snapshot.ContainsField(name))
                    {
                        string fieldValue = snapshot.GetValue<string>(name);
                        print(name + "'s value is " + fieldValue);
                    }
                    else
                    {
                        Debug.LogError("Field \"" + name + "\" does not exist in document");
                    }
                }
                else Debug.LogError("Error finding the document specified");
            }
            else
            {
                Debug.LogError("Error getting field: " + task.Exception);
            }
        });
    }
    public void GetAll(string collection, string documentId)
    {
        DocumentReference docRef = db.Collection(collection).Document(documentId);


        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {

                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Dictionary<string, object> dictionary = snapshot.ToDictionary();
                    if (dictionary.Count == 0) print("No fields found in document");
                    else
                    {
                        //if (collection == "userPs") PasswordManager.instance.OnGetAllPasswords(dictionary.ToDictionary(k => k.Key, k => k.Value.ToString()));

                        print("Fields in document \"" + documentId + "\" in collection \"" + collection + "\":");
                        print(new string('-', 15));
                        foreach (string key in dictionary.Keys)
                        {
                            print(key + ": " + dictionary[key]);
                        }
                        print(new string('-', 15));
                    }
                }
                else Debug.LogError("Error finding the document specified");
            }
            else
            {
                Debug.LogError("Error getting field: " + task.Exception);
            }
        });
    }
    public void GetAllDocuments(string collection)
    {
        CollectionReference colRef = db.Collection(collection);

        colRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;

                List<PasswordData> passwords = new List<PasswordData>(snapshot.Count);
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    Dictionary<string, object> dictionary = document.ToDictionary();
                    PasswordData password = new PasswordData();
                    foreach (KeyValuePair<string, object> field in dictionary)
                    {
                        //print($"Field: {field.Key}, Value: {field.Value}");
                        if (field.Key == "Name") password.name = field.Value.ToString();
                        if (field.Key == "Value") password.value = field.Value.ToString();
                        if (field.Key == "DateAdded") password.dateAdded = (Timestamp)field.Value;
                    }
                    passwords.Add(password);
                }
                if (collection.StartsWith("userPs")) PasswordManager.instance.OnGetAllPasswords(passwords);
            }
            else
            {
                Debug.LogError("Error getting field: " + task.Exception);
            }
        });
    }
    public void Delete(string collection, string documentId, string name)
    {
        DocumentReference docRef = db.Collection(collection).Document(documentId);

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            {name, FieldValue.Delete}
        };

        //Check if the field exists
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    if (snapshot.ContainsField(name))
                    {
                        //Delete the field
                        docRef.UpdateAsync(updates).ContinueWithOnMainThread(task =>
                        {
                            if (task.IsCompleted)
                            {
                                print("Field deleted successfully.");
                            }
                            else
                            {
                                Debug.LogError("Error deleting field: " + task.Exception);
                            }
                        });
                    }
                    else Debug.LogError("Field \"" + name + "\" does not exist in document");
                }
                else Debug.LogError("Error finding the document specified");
            }
            else
            {
                Debug.LogError("Error getting field: " + task.Exception);
            }
        });
    }
    public void DeleteDocument(string collection, string documentId)
    {
        DocumentReference docRef = db.Collection(collection).Document(documentId);

        docRef.DeleteAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (collection.StartsWith("userPs")) PasswordManager.instance.OnPasswordDeleted();

                print("Document deleted successfully");
            }
            else
            {
                Debug.LogError("Error deleting document: " + task.Exception);
            }
        });
    }
    [Button]
    void StoreUserDataInCustomDocument()
    {
        if (FirebaseManager.instance.auth.CurrentUser == null) { Debug.LogWarning("Warning! User is not logged in."); return; }

        string pass = passEncryption.GeneratePasswordString();
        Set("users", customDocumentName, "password", pass);
    }
    [Button]
    void StoreDataInCustomCollection()
    {
        if (FirebaseManager.instance.auth.CurrentUser == null) { Debug.LogWarning("Warning! User is not logged in."); return; }

        Set(customCollectionName, customDocumentName, "password", data);
    }
}
