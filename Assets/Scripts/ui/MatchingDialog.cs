using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using SmartLocalization;

public class MatchingDialog : MonoBehaviour
{
  public FBHolder facebookHolder;
  public AvatarHolder avatarHolder;
  public Text lookingForText;
  public MessageDialog messageDialog;
  public GameObject player;
  public GameObject opponent;
  public Text versusText;

  public AudioSource backgroundMusic;

  public delegate void OnClose();
  private OnClose onCloseHandler;

  private ProfileData profileData;
  private ProfileData opponentData;

  private bool isAnimating = false;
  private string[] animatedText = new string[] { "", ".", "..", "..." };
  private int textAnimationIndex = 0;
  private float animationStartTime;

  public void Start()
  {
    lookingForText.text = LanguageManager.Instance.GetTextValue("Matching.LookingFor");
    if (LanguageManager.Instance.GetSystemLanguageEnglishName() == "Russian")
    {
      RectTransform t = lookingForText.GetComponent<RectTransform>();
      t.anchoredPosition = new Vector2(t.anchoredPosition.x - 30.0f, t.anchoredPosition.y);
    }
    versusText.text = LanguageManager.Instance.GetTextValue("Matching.Versus");
    player.gameObject.SetActive(false);
    opponent.gameObject.SetActive(false);
    versusText.gameObject.SetActive(false);
    lookingForText.gameObject.SetActive(true);
	}

  public void Update()
  {
    if (isAnimating)
    {
      const float kShowTime = 0.3f;
      int newIndex = (int)Mathf.Round((Time.time - animationStartTime) / kShowTime) % animatedText.Length;
      if (textAnimationIndex != newIndex)
      {
        textAnimationIndex = newIndex;
        lookingForText.text = LanguageManager.Instance.GetTextValue("Matching.LookingFor") + animatedText[textAnimationIndex];
      }
    }
  }

  public bool IsOpened()
  {
    return gameObject.activeSelf;
  }

  public void Open(ProfileData profileData, OnClose onCloseHandler)
  {
    this.profileData = profileData;
    this.onCloseHandler = onCloseHandler;
    gameObject.SetActive(true);

    isAnimating = true;
    animationStartTime = Time.time;

    facebookHolder.GetPicture(player.GetComponent<Image>(), profileData.facebookId);

    StartCoroutine(StartFinding(2.0f));
  }

  public void Close()
  {
    gameObject.SetActive(false);
    if (onCloseHandler != null)
      onCloseHandler();
  }

  public void OnMatchingSuccess(ProfileData opponentData)
  {
    player.gameObject.SetActive(true);
    opponent.gameObject.SetActive(true);
    versusText.gameObject.SetActive(true);
    lookingForText.gameObject.SetActive(false);

    player.gameObject.GetComponentInChildren<Text>().text = UIUtils.GetFormattedString(profileData);
    opponent.gameObject.GetComponentInChildren<Text>().text = UIUtils.GetFormattedString(opponentData);

    if (opponentData.facebookId.Length != 0)
    {
      facebookHolder.GetPicture(opponent.GetComponent<Image>(), opponentData.facebookId);
    }
    else
    {
      var avatarSprite = avatarHolder.GetAvatar(opponentData);
      if (avatarSprite != null)
        opponent.GetComponent<Image>().sprite = avatarSprite;
    }

    SceneConnector.Instance.PushMatch(profileData, opponentData);

    StartCoroutine(LoadLevelDeferred());
  }

  private IEnumerator LoadLevelDeferred()
  {
    StartCoroutine(AudioFadeOut.FadeOut(backgroundMusic, 2.0f));
    yield return new WaitForSeconds(3.0f);
    SceneManager.LoadScene("CoreGame");
  }

  private IEnumerator StartFinding(float delay)
  {
    yield return new WaitForSeconds(delay);
    var rivals = (from r in Persistence.gameConfig.rivals where Math.Abs(profileData.level - r.level) <= 1 select r).ToArray();
    if (rivals.Length == 0)
    {
      int rivalIndex = UnityEngine.Random.Range(0, Persistence.gameConfig.rivals.Count);
      OnMatchingSuccess(Persistence.gameConfig.rivals[rivalIndex]);
    }
    else
    {
      int rivalIndex = UnityEngine.Random.Range(0, rivals.Length);
      OnMatchingSuccess(rivals[rivalIndex]);
    }
  }
}
