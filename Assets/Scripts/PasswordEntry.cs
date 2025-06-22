using NaughtyAttributes;
using UnityEngine;

public class PasswordEntry : MonoBehaviour
{
    public PasswordData data;

    private Animator copiedTextAnimator;
    private PasswordManager manager;
    private void Start()
    {
        copiedTextAnimator = GetComponentInChildren<Animator>();
        manager = PasswordManager.instance;
    }
    [Button]
    private void Display()
    {
        print("Name: " + data.name);
        print("Value: " + data.value);
        print("Date: " + data.dateAdded);
    }
    public void CopyValueToClipboard()
    {
        GUIUtility.systemCopyBuffer = data.value;
        copiedTextAnimator.SetTrigger("Play");
    }
    public void Delete()
    {
        PageManager.instance.ShowDeletePasswordWindow(data.name);
        PasswordManager.instance.SetPasswordToDelete(data);
    }
    public void UpdateEntry()
    {
        PasswordManager.instance.SetPasswordToUpdate(data);
        PasswordManager.instance.ShowPasswordUpdateWindow();
    }
    public void ChangeCursorToLink()
    {
        MouseHoverElement.instance.ChangeCursorToLink();
    }
    public void ChangeCursorToDefault()
    {
        MouseHoverElement.instance.ChangeCursorToDefault();
    }
}
