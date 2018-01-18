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
      }

      Text title = (from t in profileInfo.GetComponentsInChildren<Text>() where t.gameObject.name == "Name" select t).Single();
      title.text = profiles[i].name;

      Text desc = (from t in profileInfo.GetComponentsInChildren<Text>() where t.gameObject.name == "ProfileDesc" select t).Single();
      desc.text = UIUtils.GetProfileDesc(profiles[i]);

      Text posText = (from t in profileInfo.GetComponentsInChildren<Text>() where t.gameObject.name == "PositionValue" select t).Single();
      posText.text = "" + (i + 1);
		}
		height += kSpacing;
		content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    this.title.text = LanguageManager.Instance.GetTextValue("MainMenu.Leaderboard");
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
    Analytics.CustomEvent("Leaderboard_Open");

    this.onCloseHandler = onCloseHandler;

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

    gameObject.SetActive(true);
    if (splash != null && !splash.IsActive())
      splash.gameObject.SetActive(true);
  }

  public void Close()
  {
    if (splash != null && splash.IsActive())
      splash.gameObject.SetActive(false);

    gameObject.SetActive(false);

    if (onCloseHandler != null)
      onCloseHandler();
  }

  private void CloseIfClickedOutside(GameObject panel)
  {
    if (Input.GetMouseButtonDown(0) && panel.activeSelf &&
        !RectTransformUtility.RectangleContainsScreenPoint(panel.GetComponent<RectTransform>(), Input.mousePosition, null))
    {
      Close();
    }
  }
}