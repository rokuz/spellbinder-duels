using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmartLocalization;

public class LeaderboardDialog : MonoBehaviour
{
  public GameObject profileInfoPrefab;
  public Image splash;
  public GameObject content;
  public AvatarHolder avatarHolder;
  public FBHolder facebookHolder;
  public Button inviteButton;
  public Text inviteButtonText;
  public Sprite maleImage;
  public Sprite femaleImage;
  public MessageDialog messageDialog;
  public MatchingDialog matchingDialog;

  public Text title;

  private ProfileData[] profiles;
  private GameObject[] profileInfos;

  public delegate void OnClose();
  private OnClose onCloseHandler;

  private int playerIndex = 0;

	public void Setup()
  {
    var profilesList = (from r in Persistence.gameConfig.rivals
                          select r).ToList();
    profilesList.Add(Persistence.gameConfig.profile);
    profiles = profilesList.ToArray().OrderByDescending(x => x.victories - x.defeats).ThenByDescending(x => x.level).ThenBy(x => x.name).ToArray();

    for (int i = content.transform.childCount - 1; i >= 0; --i)
      GameObject.Destroy(content.transform.GetChild(i).gameObject);
    content.transform.DetachChildren();

    const float kSpacing = 10.0f;
    float startOffsetY = -kSpacing;
    float height = 0;
    profileInfos = new GameObject[profiles.Length];
    for (int i = 0; i < profiles.Length; i++)
    {    
      GameObject profileInfo = Instantiate(profileInfoPrefab);
      profileInfo.transform.SetParent(content.transform, false);
      Rect r = profileInfo.GetComponent<RectTransform>().rect;
      float h = r.height + kSpacing;
      profileInfo.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(kSpacing, -i * h + startOffsetY, 0.0f);
      height += h;

      if (Persistence.gameConfig.profile == profiles[i])
        playerIndex = i;

      profileInfos[i] = profileInfo;

      var spellIcon = (from t in profileInfo.GetComponentsInChildren<Image>()
                          where t.gameObject.name == "Profile"
                          select t).Single();

      if (profiles[i].facebookId.Length != 0)
      {
        facebookHolder.GetPicture(spellIcon, profiles[i].facebookId);
      }
      else
      {
        var avatarSprite = avatarHolder.GetAvatar(profiles[i]);
        if (avatarSprite != null)
          spellIcon.sprite = avatarSprite;
        #if UNITY_STANDALONE
        if (avatarSprite == null)
          spellIcon.sprite = Persistence.preferences.IsMale() ? maleImage : femaleImage;
        #endif
      }

      Text title = (from t in profileInfo.GetComponentsInChildren<Text>()
                       where t.gameObject.name == "Name"
                       select t).Single();
      title.text = profiles[i].name;

      Text desc = (from t in profileInfo.GetComponentsInChildren<Text>()
                      where t.gameObject.name == "ProfileDesc"
                      select t).Single();
      desc.text = UIUtils.GetProfileDesc(profiles[i]);

      Text posText = (from t in profileInfo.GetComponentsInChildren<Text>()
                         where t.gameObject.name == "PositionValue"
                         select t).Single();
      posText.text = "" + (i + 1);

      Button btn = (from t in profileInfo.GetComponentsInChildren<Button>()
                       where t.gameObject.name == "DuelButton"
                       select t).Single();
      if (profiles[i] != Persistence.gameConfig.profile && !(profiles[i].name == "Merlin" && !Persistence.gameConfig.tutorialCoreGameShown))
      {
        int buttonIndex = i;
        btn.onClick.AddListener(() => { btn.GetComponent<AudioSource>().Play(); this.OnDuel(buttonIndex); });
        Text btnText = (from t in btn.GetComponentsInChildren<Text>()
                         where t.gameObject.name == "Text"
                         select t).Single();
        btnText.text = LanguageManager.Instance.GetTextValue("Leaderboard.Duel");
      }
      else
      {
        btn.gameObject.SetActive(false);
      }
		}
		height += kSpacing;
		content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    this.title.text = LanguageManager.Instance.GetTextValue("MainMenu.Leaderboard");
    inviteButtonText.text = LanguageManager.Instance.GetTextValue("Leaderboard.Invite") + " " + 
                            Constants.INVITE_PRICE + " " + LanguageManager.Instance.GetTextValue("Reward.CoinsForm3");
	}

  public void Update()
  {
    CloseIfClickedOutside(this.gameObject);
  }

  public bool IsOpened()
  {
    return gameObject.activeSelf;
  }

  public void Open(ProfileData profileData, OnClose onCloseHandler)
  {
    MyAnalytics.CustomEvent("Leaderboard_Open");

    this.onCloseHandler = onCloseHandler;

    #if UNITY_STANDALONE
    inviteButton.gameObject.SetActive(false);
    var rt = this.gameObject.transform.Find("Scroll View").GetComponent<RectTransform>();
    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -30.0f);
    #endif

    ScrollToPlayer();

    gameObject.SetActive(true);
    if (splash != null && !splash.IsActive())
      splash.gameObject.SetActive(true);
  }

  private void ScrollToPlayer()
  {
    ScrollRect scroll = gameObject.GetComponentInChildren<ScrollRect>();
    Vector3[] corners = new Vector3[4];
    content.GetComponent<RectTransform>().GetWorldCorners(corners);
    float ymax = 0.0f;
    for (int i = 0; i < corners.Length; i++)
    {
      if (corners[i].y > ymax)
        ymax = corners[i].y;
    }

    for (float pos = 0.0f; pos < 1.0f; pos += 0.01f)
    {
      var rt = profileInfos[playerIndex].GetComponent<RectTransform>();
      var p = rt.position.y + rt.rect.height * 0.5f;
      scroll.verticalNormalizedPosition = 1.0f - pos;
      if (p >= ymax)
        break;
    }
  }

  public void Close()
  {
    if (splash != null && splash.IsActive())
      splash.gameObject.SetActive(false);

    gameObject.SetActive(false);

    if (onCloseHandler != null)
      onCloseHandler();
  }

  public void OnInvite()
  {
    MyAnalytics.CustomEvent("Leaderboard_Invite");
    facebookHolder.Invite((bool success, string[] friends, bool cancelled) => {
      if (cancelled)
      {
        MyAnalytics.CustomEvent("Leaderboard_Invite_Cancel");
        return;
      }
      if (success && friends != null)
      {
        var newFriends = Persistence.gameConfig.GetNewFriends(friends);
        if (newFriends != null && newFriends.Length > 0)
        {
          var p = new Dictionary<string, object>();
          p.Add("new_friends", newFriends.Length);
          MyAnalytics.CustomEvent("Leaderboard_Invite_Success", p);

          Persistence.gameConfig.AddFriends(newFriends, facebookHolder);
          Persistence.gameConfig.profile.coins += (newFriends.Length * Constants.INVITE_PRICE);
          Persistence.Save();
          StartCoroutine(DeferredSetupAndScroll());
        }
        else
        {
          messageDialog.Open(LanguageManager.Instance.GetTextValue("Message.Error"),
            LanguageManager.Instance.GetTextValue("Leaderboard.NoNewFriends"), null);
        }
      }
      else
      {
        var p = new Dictionary<string, object>();
        p.Add("no_auth", Persistence.gameConfig.profile.facebookId.Length == 0);
        MyAnalytics.CustomEvent("Leaderboard_Invite_Failure", p);

        messageDialog.Open(LanguageManager.Instance.GetTextValue("Message.Error"),
          LanguageManager.Instance.GetTextValue("Leaderboard.InviteError"), null);
      }
    });
  }

  IEnumerator DeferredSetupAndScroll()
  {
    yield return new WaitForEndOfFrame();
    Setup();
    ScrollToPlayer();
  }

  public void OnDuel(int profileIndex)
  {
    var p = new Dictionary<string, object>();
    p.Add("is_friend", profiles[profileIndex].facebookId.Length != 0);
    p.Add("player_level", Persistence.gameConfig.profile.level);
    p.Add("opponent_level", profiles[profileIndex].level);
    MyAnalytics.CustomEvent("Leaderboard_Duel", p);

    StartCoroutine(StartDuel(profileIndex));
  }

  IEnumerator StartDuel(int profileIndex)
  {
    yield return new WaitForSeconds(0.1f);
    Close();
    matchingDialog.Open(Persistence.gameConfig.profile, profiles[profileIndex], null);
  }

  private void CloseIfClickedOutside(GameObject panel)
  {
    if (messageDialog.IsOpened())
      return;

    if (Input.GetMouseButtonDown(0) && panel.activeSelf &&
        !RectTransformUtility.RectangleContainsScreenPoint(panel.GetComponent<RectTransform>(), Input.mousePosition, null))
    {
      Close();
    }

    if (Input.GetKeyDown(KeyCode.Escape))
      Close();
  }
}