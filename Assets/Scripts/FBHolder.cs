﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Facebook.Unity;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class FBHolder : MonoBehaviour
{
  private static bool facebookInitializeCalled = false;
  private static bool facebookInitialized = false;
  private bool loginInProgress = false;

  private string facebookName = "Anonymous";
  private string facebookId = "";

  public delegate void OnLoginFinished(bool success);
  private OnLoginFinished loginCallback;

  public delegate void OnInviteFinished(bool success, string[] friends);
  private OnInviteFinished onInviteFinished;

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
      if (!picturesCache.ContainsKey(id))
        picturesCache.Add(id, image.sprite);
      else
        picturesCache[id] = image.sprite;
    }
  }

  private class CachedRequest
  {
    public Image image;
    public string id;
  }

  private List<CachedRequest> cachedRequests = new List<CachedRequest>();

  private Dictionary<string, Sprite> picturesCache = new Dictionary<string, Sprite>();

  private Dictionary<string, string> friendNames = new Dictionary<string, string>();

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
    if (FB.IsLoggedIn && callback != null)
    {
      callback(true);
      return;
    }

    if (!FB.IsLoggedIn && !loginInProgress)
    {
      this.loginCallback = callback;
      loginInProgress = true;
      FB.LogInWithReadPermissions(new List<string>(){ "public_profile", "user_friends" }, AuthCallback);
    }
  }

  public void GetPicture(Image image, string id)
  {
    if (image == null || id == null || id.Length == 0)
      return;

    if (!facebookInitialized)
    {
      var req = new CachedRequest();
      req.id = id;
      req.image = image;
      cachedRequests.Add(req);
      return;
    }

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
    FB.API("/" + id + "/picture?width=256&height=256", HttpMethod.GET, handler.OnGetPicture);
  }

  public void Logout()
  {
    if (FB.IsLoggedIn)
      FB.LogOut();
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

    if (loginCallback != null)
      loginCallback(true);
    loginInProgress = false;
    loginCallback = null;
  }

  private void OnInitComplete()
  {
    Debug.Log("Facebook Initialized");
    facebookInitialized = true;

    foreach (var r in cachedRequests)
      GetPicture(r.image, r.id);
    cachedRequests.Clear();
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
      if (!picturesCache.ContainsKey(id))
        picturesCache.Add(id, image.sprite);
      else
        picturesCache[id] = image.sprite;
    }
  }

  private void InviteFriends()
  {
    string friendSelectorMessage = "Invite friends to play";
    string[] friendSelectorFilters = new string[]{"app_non_users"};
    FB.AppRequest(friendSelectorMessage, null, friendSelectorFilters, null, 5, "", "", InviteCallback);
  }

  private void InviteCallback(IAppRequestResult result)
  {
    if (result == null || result.Error != null || (result.Error != null && result.Error.Length != 0) || result.To == null)
    {
      if (result != null)
        Debug.Log(result.Error);
      Debug.Log("Friends were not invited");
      if (this.onInviteFinished != null)
        this.onInviteFinished(false, null);
      this.onInviteFinished = null;
    }
    else
    {
      StartCoroutine(RequestNamesOfFriends((from f in result.To select f).ToArray()));
    }
  }

  private IEnumerator RequestNamesOfFriends(string[] ids)
  {
    foreach (var friendId in ids)
    {
      if (!friendNames.ContainsKey(friendId))
      {
        string req = "/" + friendId + "?fields=first_name";
        FB.API(req, HttpMethod.GET, FriendNameCallback);
        string fid = friendId;
        yield return new WaitUntil(() => { return friendNames.ContainsKey(fid); });
      }
    }

    Debug.Log("Friends were invited");
    if (this.onInviteFinished != null)
      this.onInviteFinished(true, ids);
    this.onInviteFinished = null;
  }

  public string GetFriendName(string id)
  {
    if (friendNames.ContainsKey(id))
      return friendNames[id];
    return "NoName";
  }

  private void FriendNameCallback(IGraphResult result)
  {
    if (result != null && result.Error != null)
      Debug.Log(result.Error);

    var fid = result.ResultDictionary["id"].ToString();
    string name = result.ResultDictionary["first_name"].ToString();
    if (name == null)
      name = "Friend";
    if (!friendNames.ContainsKey(fid))
      friendNames.Add(fid, name);
    else
      friendNames[fid] = name;
  }

  public void Invite(OnInviteFinished onInviteFinished)
  {
    this.onInviteFinished = onInviteFinished;
    Login((bool success) => {
      if (success)
      {
        InviteFriends();
      }
      else
      {
        if (onInviteFinished != null)
          onInviteFinished(false, null);
      }
    });
  }
}
