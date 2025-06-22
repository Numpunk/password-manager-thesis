using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;
    public FirebaseApp app;
    public FirebaseFirestore db;
    public FirebaseAuth auth;
    private void Awake()
    {
        if (instance != this) instance = this;

        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                app = FirebaseApp.DefaultInstance;

                // Initialize Firestore with Web config details
                FirebaseApp.Create(new AppOptions()
                {
                    ApiKey = "AIzaSyDhz8UIVCX-jc1re7U8EsVW18HANSI9uoQ",
                    ProjectId = "numpunk-encrypt",
                    StorageBucket = "numpunk-encrypt.appspot.com",
                    AppId = "1:1018669924370:web:2ab7206026ee73590b98cb"
                });
                Debug.Log("Firebase Initialized and Firestore ready.");
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });
    }
}
