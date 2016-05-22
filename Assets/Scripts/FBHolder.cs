﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Facebook.Unity;
using System.Collections.Generic;
using System.IO;

public class FBHolder : MonoBehaviour {

    private static bool facebookInitializeCalled = false;
    private static bool facebookInitialized = false;
    private bool loginInProgress = false;

    private string facebookName = "Anonymous";
    private string facebookId = "";

    public delegate void OnLoginFinished(bool success);
    private OnLoginFinished loginCallback;

    private class PictureHandler
    {
        private Dictionary<string, Sprite> picturesCache;
        private Image image;
        private string id;

        public PictureHandler(Dictionary<string, Sprite> picturesCache, Image image, string id)
        {
            this.picturesCache = picturesCache;
            this.image = image;
            this.id = id;
        }

        public void OnGetPicture(IGraphResult result)
        {
            if (result.Error != null || result.Texture == null)
                return;

            byte[] bytes = result.Texture.EncodeToPNG();
            File.WriteAllBytes(FBHolder.GetPicturePath(id), bytes);

            int w = result.Texture.width;
            int h = result.Texture.height;
            image.sprite = Sprite.Create(result.Texture, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
            picturesCache.Add(id, image.sprite);
        }
    }

    private Dictionary<string, Sprite> picturesCache = new Dictionary<string, Sprite>();

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

    public void GetPicture(Image image, string id)
    {
        if (!facebookInitialized || image == null || id == null || id.Length == 0)
            return;

        if (picturesCache.ContainsKey(id))
        {
            image.sprite = picturesCache[id];
            return;
        }
        else if (File.Exists(GetPicturePath(id)))
        {
            StartCoroutine(LoadImageFromFile(GetPicturePath(id), image, id));
            return;
        }

        PictureHandler handler = new PictureHandler(picturesCache, image, id);
        FB.API("/" + id + "/picture?width=100&height=100", HttpMethod.GET, handler.OnGetPicture);
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

    private static string GetPicturePath(string id)
    {
        return Application.temporaryCachePath + "/" + id + ".png";
    }

    private IEnumerator LoadImageFromFile(string picturePath, Image image, string id)
    {
        WWW www = new WWW("file://" + picturePath);
        yield return www;

        Texture2D result = www.texture;
        if (result != null)
        {
            int w = result.width;
            int h = result.height;
            image.sprite = Sprite.Create(result, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
            picturesCache.Add(id, image.sprite);
        }
    }
}
