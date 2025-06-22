using NaughtyAttributes;
using UnityEngine;

public class EditorShortcuts : MonoBehaviour
{
    [SerializeField]
    private GameObject regScene;
    [SerializeField]
    private GameObject loginScene;
    [SerializeField]
    private GameObject regEmailScene;
    [SerializeField]
    private GameObject loginEmailScene;
    [SerializeField]
    private GameObject resetPasswordScene;
    [SerializeField]
    private GameObject mainPage;

    [Button]
    void SwitchToRegistrationScene()
    {
        ClearScenes();
        regScene.SetActive(true);
    }
    [Button]
    void SwitchToLoginScene()
    {
        ClearScenes();
        loginScene.SetActive(true);
    }
    [Button]
    void SwitchToRegEmailScene()
    {
        ClearScenes();
        regEmailScene.SetActive(true);
    }
    [Button]
    void SwitchToLoginEmailScene()
    {
        ClearScenes();
        loginEmailScene.SetActive(true);
    }
    [Button]
    void SwitchToResetPasswordScene()
    {
        ClearScenes();
        resetPasswordScene.SetActive(true);
    }
    [Button]
    void SwitchToMainPage()
    {
        ClearScenes();
        mainPage.SetActive(true);
    }
    void ClearScenes()
    {
        regScene.SetActive(false);
        loginScene.SetActive(false);
        regEmailScene.SetActive(false);
        loginEmailScene.SetActive(false);
        mainPage.SetActive(false);
        resetPasswordScene.SetActive(false);
    }
}
