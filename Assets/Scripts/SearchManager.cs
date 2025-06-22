using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SearchManager : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;
    [SerializeField]
    private Transform passwordsList;

    private PasswordManager passwordManager;

    public static SearchManager instance;
    private void Awake()
    {
        if(instance!=this) instance = this;
    }
    private void Start()
    {
        passwordManager = PasswordManager.instance;
    }
    public void ApplySearchFilter()
    {
        string input = inputField.text.ToLower();
        if (input == "") { passwordManager.RebuildPasswordsList(); return; }

        passwordManager.ClearAndSortPasswords();
        List<PasswordData> passwords = passwordManager.loadedPasswords.ToList(); //use ToList() to create a new list instead of using the same address
        List<PasswordData> passwordsToRemove = new List<PasswordData>();
        foreach (PasswordData entry in passwords)
        {
            if (!entry.name.ToLower().Contains(input))
            {
                passwordsToRemove.Add(entry);
            }
        }
        foreach (PasswordData entry in passwordsToRemove)
        {
            passwords.Remove(entry);
        }

        for (int i = 0; i < passwords.Count; i++)
        {
            int minIndex = i;
            int bestMatchPos = passwords[i].name.ToLower().IndexOf(input);

            for (int j = i + 1; j < passwords.Count; j++)
            {
                int matchPos = passwords[j].name.ToLower().IndexOf(input);
                if (matchPos < bestMatchPos)
                {
                    bestMatchPos = matchPos;
                    minIndex = j;
                }
            }

            if (minIndex != i)
            {
                PasswordData sw = passwords[i];
                passwords[i] = passwords[minIndex];
                passwords[minIndex] = sw;
            }
        }

        passwordManager.InstantiatePasswordsList(passwords);
    }
    public void ClearSearchField()
    {
        inputField.text = null;
    }
}
