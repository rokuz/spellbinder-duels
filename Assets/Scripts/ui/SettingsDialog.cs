using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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
  public Text giftcodeText;
  public Text giftcodePlaceholderText;
  public Text simplifiedGameplayText;
  public Toggle simplifiedGameplayToggle;
  public Text giftcodeInputText;
  public MessageDialog messageDialog;

  public AudioSource musicSource;

  public ButtonAudio buttonAudio;

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

    simplifiedGameplayToggle.isOn = Persistence.preferences.IsSimplifiedGameplay();

    giftcodeText.text = LanguageManager.Instance.GetTextValue("Settings.Giftcode");
    giftcodePlaceholderText.text = LanguageManager.Instance.GetTextValue("Settings.GiftcodePlaceholder");
    simplifiedGameplayText.text = LanguageManager.Instance.GetTextValue("Settings.SimplifiedGameplay");

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

    simplifiedGameplayToggle.isOn = Persistence.preferences.IsSimplifiedGameplay();
    giftcodeInputText.text = "";

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
    if (setNameDialog.IsOpened() || moreGamesDialog.IsOpened() || facebookHolder.FacebookLoginInProgress || messageDialog.IsOpened())
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

  public void OnGiftCodeEnter()
  {
    var gift = GiftCode.ApplyCode(giftcodeInputText.text);
    if (gift != null && !messageDialog.IsOpened())
    {
      if (gift.removeAds && this.onRemovedAds != null && !Persistence.gameConfig.removedAds)
      {
        Persistence.gameConfig.removedAds = true;
        Persistence.Save();
        this.onRemovedAds();

        buttonAudio.Play(ButtonAudio.Type.Yes);
        giftcodeInputText.text = "";
        messageDialog.Open(LanguageManager.Instance.GetTextValue("Message.Gift"),
          LanguageManager.Instance.GetTextValue("Shop.RemoveAds"), null);
      }
      else if (gift.coins != 0)
      {
        Persistence.gameConfig.profile.coins += gift.coins;
        Persistence.Save();

        buttonAudio.Play(ButtonAudio.Type.Yes);
        giftcodeInputText.text = "";
        messageDialog.Open(LanguageManager.Instance.GetTextValue("Message.Gift"),
          "" + gift.coins + " " + LanguageManager.Instance.GetTextValue("Reward.CoinsUni"), null);
      }
    }
  }

  public void OnSimplifiedGameplay()
  {
    Persistence.preferences.SetSimplifiedGameplay(simplifiedGameplayToggle.isOn);

    var p = new Dictionary<string, object>();
    p.Add("on", simplifiedGameplayToggle.isOn);
    MyAnalytics.CustomEvent("Settings_SimplifiedGameplay", p);
  }
}
