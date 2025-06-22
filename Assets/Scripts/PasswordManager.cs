using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Firestore;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PasswordManager : MonoBehaviour
{
    [SerializeField]
    private GameObject passwordAddWindow;
    [SerializeField]
    private GameObject passwordUpdateWindow;
    [SerializeField]
    private GameObject passwordDeleteWindow;
    [SerializeField]
    private string passwordName;
    [SerializeField]
    private string passwordValue;
    [SerializeField]
    private TMP_InputField addPasswordName;
    [SerializeField]
    private TMP_InputField addPasswordValue;
    [SerializeField]
    private TMP_Text addPasswordNameError;
    [SerializeField]
    private TMP_Text addPasswordValueError;
    [SerializeField]
    private TMP_InputField updatePasswordName;
    [SerializeField]
    private TMP_InputField updatePasswordValue;
    [SerializeField]
    private TMP_Text updatePasswordNameError;
    [SerializeField]
    private TMP_Text updatePasswordValueError;
    [SerializeField]
    private GameObject passwordEntryPrefab;
    [SerializeField]
    private Transform passwordsList;
    [SerializeField]
    private TMP_Dropdown sortDropDown;

    public List<PasswordData> loadedPasswords;

    private FireStoreWriter writer;
    private FirebaseUser user;
    private PasswordData passwordToDelete;
    private int indexOfPasswordToUpdate;
    private bool updatingPassword;
    private PasswordData passwordToUpdate;
    private bool addingPassword;
    private PasswordData passwordToAdd;
    private SearchManager searchManager;

    public static PasswordManager instance;

    public enum SortingType
    {
        Descending = 0,
        Ascending = 1,
        DateDescending = 2,
        DateAscending = 3
    }
    [HideInInspector]
    public SortingType sortingType;
    private void Awake()
    {
        if (instance != this) instance = this;
    }
    private void Start()
    {
        writer = FireStoreWriter.instance;
        user = FirebaseManager.instance.auth.CurrentUser;
        searchManager = SearchManager.instance;
    }
    
    public void ShowPasswordAddWindow()
    {
        passwordAddWindow.SetActive(true);
    }
    public void ShowPasswordUpdateWindow()
    {
        if (passwordToUpdate == null) throw new UnityException("No password is set to be updated");

        passwordUpdateWindow.SetActive(true);
        updatePasswordName.text = passwordToUpdate.name;
        updatePasswordValue.text = passwordToUpdate.value;
    }
    public void AddPassword()
    {
        addPasswordNameError.text = "Name";
        addPasswordValueError.text = "Password";
        if (addPasswordName.text.Length == 0)
        {
            addPasswordNameError.text = "<color=#FF6D73>Name - <i>cannot be empty";
            return;
        }
        if (addPasswordValue.text.Length == 0)
        {
            addPasswordValueError.text = "<color=#FF6D73>Password - <i>cannot be empty";
            return;
        }
        foreach (Transform entry in passwordsList.transform)
        {
            if (entry.GetComponent<PasswordEntry>().data.name == addPasswordName.text)
            {
                addPasswordNameError.text = "<color=#FF6D73>Name - <i>already exists";
                return;
            }
        }

        passwordName = addPasswordName.text;
        passwordValue = addPasswordValue.text;
        Timestamp currentTimeStamp = Timestamp.GetCurrentTimestamp();

        user = FirebaseManager.instance.auth.CurrentUser;
        if (user == null) { Debug.LogError("User is not signed in"); return; }

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            {"Name", passwordName},
            {"Value", passwordValue},
            {"DateAdded", currentTimeStamp}
        };
        addingPassword = true;
        updatingPassword = false;

        passwordToAdd = new PasswordData(passwordName, passwordValue, currentTimeStamp);
        writer.AddOrUpdateDocument("userPs/users/" + user.UserId, passwordName, data);

        passwordName = null;
        passwordValue = null;
    }
    public void OnPasswordAddedOrUpdated()
    {
        passwordAddWindow.SetActive(false);
        passwordUpdateWindow.SetActive(false);

        ClearPasswordAddOrUpdateInputs();

        HidePasswordManager.instance.HidePasswordInputs();

        if (addingPassword)
        {
            if (loadedPasswords.Count == 0) { PageManager.instance.HideMainPagePlaceholder(); }
            loadedPasswords.Add(passwordToAdd);
            addingPassword = false;
        }
        else if (updatingPassword)
        {
            loadedPasswords[indexOfPasswordToUpdate] = passwordToUpdate;
            updatingPassword = false;
        }
        searchManager.ApplySearchFilter();
    }
    public void OnPasswordDeleted()
    {
        passwordDeleteWindow.SetActive(false);
        loadedPasswords.Remove(passwordToDelete);
        if (loadedPasswords.Count == 0) { PageManager.instance.ShowMainPagePlaceholder();}

        searchManager.ApplySearchFilter();
    }
    public void ClearPasswordAddOrUpdateInputs()
    {
        addPasswordName.text = null;
        addPasswordValue.text = null;

        updatePasswordName.text = null;
        updatePasswordValue.text = null;
    }
    public void UpdatePassword()
    {
        updatePasswordNameError.text = "Name";
        updatePasswordValueError.text = "Password";
        if (updatePasswordName.text.Length == 0)
        {
            updatePasswordNameError.text = "<color=#FF6D73>Name - <i>cannot be empty";
            return;
        }
        if (updatePasswordValue.text.Length == 0)
        {
            updatePasswordValueError.text = "<color=#FF6D73>Password - <i>cannot be empty";
            return;
        }
        if (passwordToUpdate == null) throw new UnityException("No password is set to be updated");
        foreach (Transform entry in passwordsList.transform)
        {
            if (entry.GetComponent<PasswordEntry>().data.name == passwordToUpdate.name) continue;

            if (entry.GetComponent<PasswordEntry>().data.name == updatePasswordName.text)
            {
                updatePasswordNameError.text = "<color=#FF6D73>Name - <i>already exists";
                return;
            }
        }

        string newPasswordName = updatePasswordName.text;
        string newPasswordValue = updatePasswordValue.text;

        string oldPasswordName = passwordToUpdate.name;
        Timestamp dateAdded = passwordToUpdate.dateAdded;

        user = FirebaseManager.instance.auth.CurrentUser;
        if (user == null) { Debug.LogError("User is not signed in"); return; }

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            {"Name", newPasswordName},
            {"Value", newPasswordValue},
            {"DateAdded", dateAdded}
        };

        addingPassword = false;
        updatingPassword = true;

        indexOfPasswordToUpdate = loadedPasswords.IndexOf(passwordToUpdate);
        passwordToUpdate = new PasswordData(newPasswordName, newPasswordValue, dateAdded);

        writer.AddOrUpdateDocument("userPs/users/" + user.UserId, newPasswordName, updates);
        if (newPasswordName != oldPasswordName) writer.DeleteDocument("userPs/users/" + user.UserId, oldPasswordName);
    }
    public void AddNewPassword()
    {
        user = FirebaseManager.instance.auth.CurrentUser;
        if (user == null) { Debug.LogError("User is not signed in"); return; }

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            {"Name", passwordName},
            {"Value", passwordValue},
            {"DateAdded", Timestamp.GetCurrentTimestamp()}
        };
        writer.AddOrUpdateDocument("userPs/users/" + user.UserId, passwordName, updates);
    }
    [Button]
    public void GetFieldValue()
    {
        if (user == null) user = FirebaseManager.instance.auth.CurrentUser;
        writer.Get("userPs", user.UserId, passwordName);
    }
    public void RebuildPasswordsList()
    {
        ClearAndSortPasswords();
        InstantiatePasswordsList(loadedPasswords);
    }
    public void ClearAndSortPasswords()
    {
        ClearPasswordsList();
        SortPasswords();
    }
    private void SortPasswords()
    {
        if (sortingType == SortingType.Descending) SortByNameDescending();
        else if (sortingType == SortingType.Ascending) SortByNameAscending();
        else if (sortingType == SortingType.DateDescending) SortByDateAddedDescending();
        else if (sortingType == SortingType.DateAscending) SortByDateAddedAscending();
    }
    [Button]
    public void GetAllPasswords()
    {
        if (user == null) user = FirebaseManager.instance.auth.CurrentUser;

        writer.GetAllDocuments("userPs/users/" + user.UserId);
    }
    public void OnGetAllPasswords(List<PasswordData> passwords)
    {
        ClearPasswordsList();

        loadedPasswords = passwords;
        if (loadedPasswords.Count == 0) { PageManager.instance.ShowMainPagePlaceholder(); }
        else { PageManager.instance.HideMainPagePlaceholder(); }

        SortPasswords();

        InstantiatePasswordsList(loadedPasswords);
    }
    public void InstantiatePasswordsList(List<PasswordData> passwords)
    {
        GameObject[] passwordObjects = new GameObject[passwords.Count];
        float passwordsSpacing = ScrollManager.instance.passwordsSpacing;
        for (int i = 0; i < passwords.Count; i++)
        {
            PasswordData data = passwords[i];
            GameObject passwordEntry = Instantiate(passwordEntryPrefab);
            passwordEntry.GetComponent<PasswordEntry>().data = data;

            passwordEntry.transform.SetParent(passwordsList);
            passwordEntry.transform.localPosition = new Vector3(0, i * -passwordsSpacing + 1000, 0);
            passwordEntry.transform.localScale = Vector3.one;
            passwordEntry.name = "Password Entry " + i;

            TMP_Text passwordText = passwordEntry.GetComponentInChildren<TMP_Text>();
            passwordText.text = data.name;
        }
        ScrollManager.instance.OnInstantiatedList(passwords.Count);
    }
    [Button]
    public void DeletePassword()
    {
        if (user == null) user = FirebaseManager.instance.auth.CurrentUser;
        if (passwordToDelete == null) { Debug.LogError("No password is set to delete"); return; }

        writer.DeleteDocument("userPs/users/" + user.UserId, passwordToDelete.name);
    }
    public void SetPasswordName(TMP_InputField text)
    {
        passwordName = text.text;
    }
    public void SetPasswordNameString(string newName)
    {
        passwordName = newName;
    }
    public void SetPasswordNameText(TMP_Text text)
    {
        passwordName = text.text;
    }
    public void SetPasswordValue(TMP_InputField text)
    {
        passwordValue = text.text;
    }
    public void RefreshPasswordsList()
    {
        GetAllPasswords();
    }
    public void ClearPasswordsList()
    {
        foreach (Transform entry in passwordsList.transform)
        {
            Destroy(entry.gameObject);
        }
    }
    public void OnSortValueChanged(TMP_Dropdown dropdown)
    {
        switch (dropdown.value)
        {
            case 0: sortingType = SortingType.Descending; break;
            case 1: sortingType = SortingType.Ascending; break;
            case 2: sortingType = SortingType.DateDescending; break;
            case 3: sortingType = SortingType.DateAscending; break;
        }
        searchManager.ApplySearchFilter();
        AppStartupManager.instance.SaveSortingChoice(sortingType);
    }
    public void LoadSortingType(int type)
    {
        if (type == 0) sortingType = SortingType.Descending;
        else if (type == 1) sortingType = SortingType.Ascending;
        else if (type == 2) sortingType = SortingType.DateDescending;
        else if (type == 3) sortingType = SortingType.DateAscending;
        sortDropDown.value = type;
    }
    private void SortByNameDescending()
    {
        for (int i = 0; i < loadedPasswords.Count; i++)
        {
            for (int j = 0; j < loadedPasswords.Count - 1; j++)
            {
                if (loadedPasswords[j].name.ToUpper().CompareTo(loadedPasswords[j + 1].name.ToUpper()) == 1)
                {
                    PasswordData sw = loadedPasswords[j];
                    loadedPasswords[j] = loadedPasswords[j + 1];
                    loadedPasswords[j + 1] = sw;
                }
            }
        }
    }
    private void SortByNameAscending()
    {
        for (int i = 0; i < loadedPasswords.Count; i++)
        {
            for (int j = 0; j < loadedPasswords.Count - 1; j++)
            {
                if (loadedPasswords[j].name.ToUpper().CompareTo(loadedPasswords[j + 1].name.ToUpper()) == -1)
                {
                    PasswordData sw = loadedPasswords[j];
                    loadedPasswords[j] = loadedPasswords[j + 1];
                    loadedPasswords[j + 1] = sw;
                }
            }
        }
    }
    private void SortByDateAddedDescending()
    {
        for (int i = 0; i < loadedPasswords.Count; i++)
        {
            for (int j = 0; j < loadedPasswords.Count - 1; j++)
            {
                if (loadedPasswords[j].dateAdded.CompareTo(loadedPasswords[j + 1].dateAdded) == -1)
                {
                    PasswordData sw = loadedPasswords[j];
                    loadedPasswords[j] = loadedPasswords[j + 1];
                    loadedPasswords[j + 1] = sw;
                }
            }
        }
    }
    private void SortByDateAddedAscending()
    {
        for (int i = 0; i < loadedPasswords.Count; i++)
        {
            for (int j = 0; j < loadedPasswords.Count - 1; j++)
            {
                if (loadedPasswords[j].dateAdded.CompareTo(loadedPasswords[j + 1].dateAdded) == 1)
                {
                    PasswordData sw = loadedPasswords[j];
                    loadedPasswords[j] = loadedPasswords[j + 1];
                    loadedPasswords[j + 1] = sw;
                }
            }
        }
    }
    public void SetPasswordToDelete(PasswordData password)
    {
        passwordToDelete = password;
    }
    public void SetPasswordToUpdate(PasswordData password)
    {
        passwordToUpdate = password;
    }
}
