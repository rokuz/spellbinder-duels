using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmartLocalization;

public class MoreGamesDialog : MonoBehaviour
{
  public GameObject gameItemPrefab;
  public Image splash;
  public GameObject content;
  public ButtonAudio buttonAudio;

  public Sprite spaceShippersScreen;

  public Text title;

  enum Game
  {
    SpaceShippers
  }

  class GameItem
  {
    public Game game;
    public string title;
    public string desc;
    public string appStoreLink;
    public string googlePlayLink;

    public GameItem(Game game, string title, string desc, string appStoreLink, string googlePlayLink)
    {
      this.game = game;
      this.title = title;
      this.desc = desc;
      this.appStoreLink = appStoreLink;
      this.googlePlayLink = googlePlayLink;
    }
  }

  private GameItem[] games;
  private GameObject[] gameInfos;

  public delegate void OnClose();
  private OnClose onCloseHandler;

	public void Setup()
  {
    var gamesList = new List<GameItem>();
    gamesList.Add(new GameItem(Game.SpaceShippers, "Space Shippers",
      LanguageManager.Instance.GetTextValue("SpaceShippers.Desc"),
      "https://itunes.apple.com/us/app/space-shippers/id1290189939", 
      "https://play.google.com/store/apps/details?id=com.ivprod.spaceshippers"));

    games = gamesList.ToArray();

    for (int i = content.transform.childCount - 1; i >= 0; --i)
      GameObject.Destroy(content.transform.GetChild(i).gameObject);
    content.transform.DetachChildren();

    const float kSpacing = 10.0f;
    float startOffsetY = -kSpacing;
    float height = 0;
    gameInfos = new GameObject[games.Length];
    for (int i = 0; i < games.Length; i++)
    {
      GameObject profileInfo = Instantiate(gameItemPrefab);
      profileInfo.transform.SetParent(content.transform, false);
      Rect r = profileInfo.GetComponent<RectTransform>().rect;
      float h = r.height + kSpacing;
      profileInfo.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(kSpacing, -i * h + startOffsetY, 0.0f);
      height += h;

      gameInfos[i] = profileInfo;

      var img = (from t in profileInfo.GetComponentsInChildren<Image>() where t.gameObject.name == "GameScreen" select t).Single();
      img.sprite = GetSprite(games[i]);

      Text title = (from t in profileInfo.GetComponentsInChildren<Text>() where t.gameObject.name == "GameTitle" select t).Single();
      title.text = games[i].title;

      Text desc = (from t in profileInfo.GetComponentsInChildren<Text>() where t.gameObject.name == "GameDesc" select t).Single();
      desc.text = games[i].desc;

      Button btn = (from t in profileInfo.GetComponentsInChildren<Button>() where t.gameObject.name == "ButtonStore" select t).Single();
      int buttonIndex = i;
      btn.onClick.AddListener(() => { this.OnShowInStoreClicked(buttonIndex); });

      Text btnText = (from t in btn.GetComponentsInChildren<Text>() where t.gameObject.name == "Text" select t).Single();
      btnText.text = LanguageManager.Instance.GetTextValue("MoreGames.ShowInStore");
		}
		height += kSpacing;
    content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    this.title.text = LanguageManager.Instance.GetTextValue("MoreGames.Title");
	}

  public void Update()
  {
    CloseIfClickedOutside(this.gameObject);
  }

  public bool IsOpened()
  {
    return gameObject.activeSelf;
  }

  public void Open(OnClose onCloseHandler)
  {
    Analytics.CustomEvent("MoreGames_Open");

    this.onCloseHandler = onCloseHandler;

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

  public void OnShowInStoreClicked(int index)
  {
    buttonAudio.Play(ButtonAudio.Type.Default);

    var p = new Dictionary<string, object>();
    p.Add("game", games[index].game);
    Analytics.CustomEvent("MoreGames_ShowInStore", p);

    #if UNITY_ANDROID
      Application.OpenURL(games[index].googlePlayLink);
    #elif UNITY_IPHONE
      Application.OpenURL(games[index].appStoreLink);
    #endif
  }

  private void CloseIfClickedOutside(GameObject panel)
  {
    if (Input.GetMouseButtonDown(0) && panel.activeSelf &&
        !RectTransformUtility.RectangleContainsScreenPoint(panel.GetComponent<RectTransform>(), Input.mousePosition, null))
    {
      Close();
    }
  }

  private Sprite GetSprite(GameItem item)
  {
    switch (item.game)
    {
      case Game.SpaceShippers: return spaceShippersScreen;
    }
    return null;
  }
}