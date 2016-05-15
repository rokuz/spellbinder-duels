using UnityEngine;
using System.Collections;
using Facebook.Unity;
using System.Collections.Generic;

public class FBHolder : MonoBehaviour {

    private bool facebookInitializeCalled = false;
    private bool facebookInitialized = false;
    private bool loginInProgress = false;

    private string facebookName = "Anonymous";
    private string facebookId = "";

    public delegate void OnLoginFinished(bool success);
    private OnLoginFinished loginCallback;

    public bool FacebookInitialized
    {
        get { return facebookInitialized; }
    }

	public void Awake()
    {
        if (!facebookInitializeCalled)
        {
            FB.Init(OnInitComplete);
            facebookInitializeCalled = true;
        }
    }

    public void Login(OnLoginFinished callback)
    {
        if (!FB.IsLoggedIn && !loginInProgress)
        {
            loginCallback = callback;
            loginInProgress = true;
            FB.LogInWithReadPermissions(new List<string>(){ "public_profile", "user_friends" }, AuthCallback);
        }
    }

    public void Logout()
    {
        if (FB.IsLoggedIn)
        {
            FB.LogOut();
        }
    }

    public bool FacebookLoggedIn
    {
        get { return FB.IsLoggedIn; }
    }

    public string FacebookName
    {
        get { return facebookName; }
    }

    public string FacebookID
    {
        get { return facebookId; }
    }

    public bool FacebookLoginInProgress
    {
        get { return loginInProgress; }
    }

    private void AuthCallback(ILoginResult result)
    {
        if (!FB.IsLoggedIn)
        {
            if (result.Error != null && result.Error.Length != 0)
                Debug.Log(result.Error);
            Debug.Log("User didn't log into Facebook");
            if (loginCallback != null)
                loginCallback(false);

            loginInProgress = false;
            loginCallback = null;
        }
        else
        {
            Debug.Log("User logged into Facebook");
            FB.API("/me?fields=first_name", HttpMethod.GET, NameCallback);
        }
    }

    private void NameCallback(IGraphResult result)
    {
        this.facebookId = result.ResultDictionary["id"].ToString();
        string name = result.ResultDictionary["first_name"].ToString();
        if (name != null)
            this.facebookName = name;

        if (loginCallback != null) loginCallback(true);
        loginInProgress = false;
        loginCallback = null;
    }

    private void OnInitComplete()
    {
        Debug.Log("Facebook Initialized");
        facebookInitialized = true;
    }
}
