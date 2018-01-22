using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using SmartLocalization;

public class SettingsDialog : MonoBehaviour
{
  public FBHolder facebookHolder;
  public Purchaser purchaser;
  public Image splash;
  public Text title;
  public Text changeNameText;
  public Button changeNameButton;
  public Text volumeText;
  public Slider volumeSlider;
  public Text sfxText;
  public Slider sfxSlider;
  public SetNameDialog setNameDialog;
  public Text playerName;
  public Button restorePurchases;
  public Button otherGames;
  public MoreGamesDialog moreGamesDialog;

  public AudioSource musicSource;

  public delegate void OnClose();
  private OnClose onCloseHandler;

  public delegate void OnRemovedAds();
  public OnRemovedAds onRemovedAds;

  private ProfileData profileData;

  public void Setup()
  {
    title.text = LanguageManager.Instance.GetTextValue("Settings.Title");
    changeNameText.text = LanguageManager.Instance.GetTextValue("Settings.ChangeName");
    changeNameButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("Settings.ChangeNameButton");
    volumeText.text = LanguageManager.Instance.GetTextValue("Settings.MusicVolume");
    sfxText.text = LanguageManager.Instance.GetTextValue("Settings.SfxVolume");

    var restorePurchasesText = (from t in restorePurchases.GetComponentsInChildren<Text>() where t.gameObject.name == "Text" select t).Single();
    restorePurchasesText.text = LanguageManager.Instance.GetTextValue("Settings.RestorePurchase");

    var otherGamesText = (from t in otherGames.GetComponentsInChildren<Text>() where t.gameObject.name == "Text" select t).Single();
    otherGamesText.text = LanguageManager.Instance.GetTextValue("Settings.OtherGames");

    volumeSlider.value = Persistence.gameConfig.musicVolume;
    sfxSlider.value = Persistence.gameConfig.sfxVolume;

    moreGamesDialog.Setup();
  }

  public bool IsOpened()
  {
    return gameObject.activeSelf;
  }

  public void Update()
  {
    CloseIfClickedOutside(this.gameObject);
  }

  public void Open(ProfileData profileData, OnClose onCloseHandler, OnRemovedAds onRemovedAds)
  {
    this.profileData = profileData;
    changeNameButton.interactable = (this.profileData != null);
    this.onCloseHandler = onCloseHandler;
    this.onRemovedAds = onRemovedAds;

    this.gameObject.SetActive(true);
    if (splash != null && !splash.IsActive())
      splash.gameObject.SetActive(true);
  }

  public void Close()
  {
    Persistence.gameConfig.musicVolume = volumeSlider.value;
    Persistence.gameConfig.sfxVolume = sfxSlider.value;
    Persistence.Save();

    if (splash != null && splash.IsActive())
      splash.gameObject.SetActive(false);

    gameObject.SetActive(false);

    if (onCloseHandler != null)
      onCloseHandler();
  }

  private void CloseIfClickedOutside(GameObject panel)
  {
    if (setNameDialog.IsOpened() || moreGamesDialog.IsOpened() || facebookHolder.FacebookLoginInProgress)
      return;

    if (Input.GetMouseButtonDown(0) && panel.activeSelf && 
        !RectTransformUtility.RectangleContainsScreenPoint(panel.GetComponent<RectTransform>(), Input.mousePosition, null))
    {
      Close();
    }

    if (Input.GetKeyDown(KeyCode.Escape))
      Close();
  }

  public void OnChangeNameClicked()
  {
    splash.gameObject.SetActive(false);
    changeNameButton.interactable = false;
    volumeSlider.interactable = false;
    setNameDialog.Open(this.profileData, () =>
    {
      this.playerName.text = this.profileData.name;

      changeNameButton.interactable = true;
      volumeSlider.interactable = true;
      splash.gameObject.SetActive(true);
    });
  }

  public void OnChangedMusicVolume()
  {
    musicSource.volume = volumeSlider.value;
  }

  public void OnChangedSfxVolume()
  {
    Persistence.gameConfig.sfxVolume = sfxSlider.value;
  }

  public void OnRestorePurchases()
  {
    purchaser.RestorePurchases(() => {
      if (this.onRemovedAds != null && Persistence.gameConfig.removedAds)
        this.onRemovedAds();
    });
  }

  public void OnMoreGames()
  {
    this.otherGames.interactable = false;
    moreGamesDialog.Open(() => { this.otherGames.interactable = true; });
  }
}
