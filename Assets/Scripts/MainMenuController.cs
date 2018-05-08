﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmartLocalization;

public class MainMenuController : MonoBehaviour
{
  public FBHolder facebookHolder;
  public SetNameDialog setNameDialog;
  public MatchingDialog matchingDialog;
  public MessageDialog messageDialog;
  public SpellbookDialog spellbookDialog;
  public SettingsDialog settingsDialog;
  public CharacterDialog characterDialog;
  public LeaderboardDialog leaderboardDialog;
  public SelectCharacterDialog selectCharacterDialog;
  public ShopDialog shopDialog;
  public GameObject playerLogo;
  public Button playButton;
  public Button spellbookButton;
  public Button settingsButton;
  public Button shopButton;
  public Button leaderboardButton;
  public Text subtitleText;
  public TutorialMainMenu tutorialMainMenu;
  public Sprite maleImage;
  public Sprite femaleImage;
  public Button playButtonDesktop;
  public Button quitButtonDesktop;
  public MessageYesNoDialog messageYesNoDialog;

	public void Start()
  {
    SmartCultureInfo systemLanguage = LanguageManager.Instance.GetDeviceCultureIfSupported();
    if (systemLanguage != null)
      LanguageManager.Instance.ChangeLanguage(systemLanguage);

    Application.runInBackground = true;
    Persistence.Load();

    GetComponent<AudioSource>().volume = Persistence.gameConfig.musicVolume;

    Spellbook.Init();

    this.playerLogo.gameObject.SetActive(false);

    #if !UNITY_STANDALONE
    this.playButton.interactable = true;
    this.playButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("MainMenu.Play");
    #else
    this.playButton.gameObject.SetActive(false);

    this.playButtonDesktop.gameObject.SetActive(true);
    this.playButtonDesktop.interactable = true;
    this.playButtonDesktop.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("MainMenu.Play");

    this.quitButtonDesktop.gameObject.SetActive(true);
    this.quitButtonDesktop.interactable = true;
    this.quitButtonDesktop.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("MainMenu.Quit");
    #endif

    subtitleText.text = LanguageManager.Instance.GetTextValue("MainMenu.Duels");

    this.spellbookButton.interactable = true;
    this.shopButton.interactable = false;
    this.settingsButton.interactable = true;
    this.leaderboardButton.interactable = false;

    settingsDialog.Setup();
    characterDialog.Setup();
    spellbookDialog.Setup();

    shopDialog.onUpdateCoinsAndLevel = () => {
      UpdateCoinsAndLevel();
    };

    InitProfile();
	}

  public void OnDestroy()
  {
    Persistence.Save();
  }
	   
  public void OnPlayButtonClicked()
  {
    MyAnalytics.CustomEvent("Play_Clicked");

    #if !UNITY_STANDALONE
    this.playButton.interactable = false;
    matchingDialog.Open(Persistence.gameConfig.profile, () => { this.playButton.interactable = true; });
    #else
    this.playButtonDesktop.interactable = false;
    matchingDialog.Open(Persistence.gameConfig.profile, () => { this.playButtonDesktop.interactable = true; });
    #endif
    tutorialMainMenu.OnPlayClicked();
  }

  public void OnQuitButtonClicked()
  {
    messageYesNoDialog.Open(LanguageManager.Instance.GetTextValue("MainMenu.Quit"),
                            LanguageManager.Instance.GetTextValue("MainMenu.QuitApprove"), (bool yes) => {
      if (yes)
        Application.Quit();
    });
  }

  public void OnSpellbookButtonClicked()
  {
    this.spellbookButton.interactable = false;
    spellbookDialog.Open(Persistence.gameConfig.profile, () => { this.spellbookButton.interactable = true; });
    tutorialMainMenu.OnSpellbookClicked();
  }

  public void OnLeaderboardButtonClicked()
  {
    this.leaderboardButton.interactable = false;
    leaderboardDialog.Open(Persistence.gameConfig.profile, () => { this.leaderboardButton.interactable = true; UpdatePlayerUI(); });
    tutorialMainMenu.OnLeaderboardClicked();
  }

  public void OnSettingsButtonClicked()
  {
    if (facebookHolder.FacebookLoginInProgress)
      return;

    this.settingsButton.interactable = false;
    settingsDialog.Open(Persistence.gameConfig.profile, () => { UpdatePlayerUI(); this.settingsButton.interactable = true; },
      () => { UpdateCoinsAndLevel(); });

    tutorialMainMenu.OnSettingsClicked();
  }

  public void OnShopButtonClicked()
  {
    this.shopButton.interactable = false;
    shopDialog.Open(Persistence.gameConfig.profile, null, () =>
    {
      this.shopButton.interactable = true;
      UpdatePlayerUI();
    });

    tutorialMainMenu.OnShopClicked();
  }

  public void OnPlayerInfoClicked()
  {
    if (characterDialog.IsOpened() || matchingDialog.IsOpened() || messageDialog.IsOpened() || settingsDialog.IsOpened() ||
        spellbookDialog.IsOpened() || leaderboardDialog.IsOpened() || selectCharacterDialog.IsOpened())
      return;

    this.playerLogo.gameObject.GetComponentInChildren<Button>().interactable = false;
    characterDialog.Open(Persistence.gameConfig.profile, () =>
    {
      this.playerLogo.gameObject.GetComponentInChildren<Button>().interactable = true;
    });

    tutorialMainMenu.OnCharacterClicked();
  }

  private void InitProfile()
  {
    if (Persistence.gameConfig.profile == null)
    {
      Persistence.gameConfig.profile = new ProfileData();
      Persistence.gameConfig.InitRivals();
      Persistence.Save();
    }

    #if UNITY_STANDALONE
    if (!Persistence.preferences.IsMaleKeyExists())
    {
      MyAnalytics.CustomEvent("SelectCharacter_Started");
      selectCharacterDialog.Open(() =>
      {
        var p = new Dictionary<string, object>();
        p.Add("character", Persistence.preferences.IsMale() ? "male" : "female");
        MyAnalytics.CustomEvent("SelectCharacter_Finished", p);
        InitName();
      });
    }
    else
    {
      InitName();
    }
    #else
    InitName();
    #endif
  }

  private void InitName()
  {
    if (Persistence.gameConfig.profile.name.Length == 0)
    {
      MyAnalytics.CustomEvent("SetName_Started");
      setNameDialog.Open(Persistence.gameConfig.profile, () =>
      {
        MyAnalytics.CustomEvent("SetName_Finished"); 
        this.UpdatePlayerUI();
      });
    }
    else
    {
      this.UpdatePlayerUI();
    }
  }

  private void UpdatePlayerUI()
  {
    this.playerLogo.transform.Find("NameBox/NameText").GetComponent<Text>().text = Persistence.gameConfig.profile.name;
    UpdateCoinsAndLevel();
    this.playerLogo.gameObject.SetActive(true);

    #if !UNITY_STANDALONE
    if (Persistence.gameConfig.profile != null)
      facebookHolder.GetPicture(playerLogo.transform.Find("Logo").GetComponentInChildren<Image>(), Persistence.gameConfig.profile.facebookId);
    #else
      playerLogo.transform.Find("Logo").GetComponentInChildren<Image>().sprite = Persistence.preferences.IsMale() ? maleImage : femaleImage;
    #endif

    this.leaderboardButton.interactable = true;
    leaderboardDialog.Setup();

    this.shopButton.interactable = true;

    tutorialMainMenu.InitTutorial();
  }

  private void UpdateCoinsAndLevel()
  {
    this.playerLogo.transform.Find("NameBox/Coin/CoinText").GetComponent<Text>().text = "" + Persistence.gameConfig.profile.coins;
    this.playerLogo.transform.Find("LevelBox/LevelText").GetComponent<Text>().text = "" + Persistence.gameConfig.profile.level;
    #if UNITY_STANDALONE
      playerLogo.transform.Find("Logo").GetComponentInChildren<Image>().sprite = Persistence.preferences.IsMale() ? maleImage : femaleImage;
    #endif
  }
}
