using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class InternetManager : MonoBehaviour
{
    public static bool isConnected = true;
    public static InternetManager instance;

    private PageManager pageManager;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        pageManager = PageManager.instance;
        StartCoroutine(CheckInternetConnection());

    }
    private IEnumerator CheckInternetConnection()
    {
        yield return new WaitForSeconds(2);
        while (true)
        {
            UnityWebRequest request = UnityWebRequest.Get("https://www.google.com");
            request.timeout = 4;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                isConnected = false;
                OnDisconnectedEvent();
            }
            else
            {
                if (!isConnected)
                {
                    isConnected = true;
                    OnConnectedEvent();
                }
            }

            yield return new WaitForSeconds(4);
        }
    }
    private void OnConnectedEvent()
    {
        pageManager.ConnectedOnRegistrationPage();
        pageManager.ConnectedOnLoginPage();
        pageManager.ConnectedOnMailConfirmationPage();
        pageManager.ConnectedOnMailConfirmationLoginPage();
        pageManager.ConnectedOnPasswordResetPage();
        if (pageManager.OnMainPage()) pageManager.ConnectedOnMainPage();
    }
    private void OnDisconnectedEvent()
    {
        pageManager.NoConnectionOnRegistrationPage();
        pageManager.NoConnectionOnLoginPage();
        pageManager.NoConnectionOnMailConfirmationPage();
        pageManager.NoConnectionOnMailConfirmationLoginPage();
        pageManager.NoConnectionOnPasswordResetPage();
        if (pageManager.OnMainPage()) pageManager.NoConnectionOnMainPage();
    }

    //DOESN'T WORK IN BUILD
    /*IEnumerator MonitorInternet()
    {
        while (true)
        {
            Debug.Log(Application.internetReachability);
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                isConnected = false;
                OnDisconnectedEvent();
            }
            else
            {
                isConnected = true;
                OnConnectedEvent();
            }
            yield return new WaitForSeconds(1);
        }
    }*/
}
