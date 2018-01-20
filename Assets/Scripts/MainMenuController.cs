using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;
using UnityEngine.Advertisements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmartLocalization;
using GoogleMobileAds;
using GoogleMobileAds.Api;

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
  public ShopDialog shopDialog;
  public GameObject playerLogo;
  public Button playButton;
  public Button spellbookButton;
  public Button settingsButton;
  public Button shopButton;
  public Button leaderboardButton;
  public Text subtitleText;
  public TutorialMainMenu tutorialMainMenu;

  private BannerView bannerView;

	public void Start()
  {
    SmartCultureInfo systemLanguage = LanguageManager.Instance.GetDeviceCultureIfSupported();
    if (systemLanguage != null)
      LanguageManager.Instance.ChangeLanguage(systemLanguage);

    Application.runInBackground = true;
    Persistence.Load();

    InitializeAds();

    GetComponent<AudioSource>().volume = Persistence.gameConfig.musicVolume;

    Spellbook.Init();

    this.playerLogo.gameObject.SetActive(false);

    this.playButton.interactable = true;
    this.playButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("MainMenu.Play");
    subtitleText.text = LanguageManager.Instance.GetTextValue("MainMenu.Duels");

    this.spellbookButton.interactable = true;
    this.shopButton.interactable = false;
    this.settingsButton.interactable = true;
    this.leaderboardButton.interactable = false;

    settingsDialog.Setup();
    characterDialog.Setup();
    spellbookDialog.Setup();

    shopDialog.onRemovedAds = () => {
      DestroyBanner();
    };

    InitProfile();
	}

  public void OnDestroy()
  {
    Persistence.Save();
  }
	   
  public void OnPlayButtonClicked()
  {
    Analytics.CustomEvent("Play_Clicked");

    this.playButton.interactable = false;
    matchingDialog.Open(Persistence.gameConfig.profile, () => { this.playButton.interactable = true; });
    tutorialMainMenu.OnPlayClicked();
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
    settingsDialog.Open(Persistence.gameConfig.profile, () => { this.settingsButton.interactable = true; },
      () => { UpdatePlayerUI(); DestroyBanner(); });

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
        spellbookDialog.IsOpened() || leaderboardDialog.IsOpened())
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

    if (Persistence.gameConfig.profile.name.Length == 0)
      setNameDialog.Open(Persistence.gameConfig.profile, () => { this.UpdatePlayerUI(); });
    else 
      this.UpdatePlayerUI();
  }

  private void UpdatePlayerUI()
  {
    this.playerLogo.transform.Find("NameBox/NameText").GetComponent<Text>().text = Persistence.gameConfig.profile.name;
    this.playerLogo.transform.Find("NameBox/Coin/CoinText").GetComponent<Text>().text = "" + Persistence.gameConfig.profile.coins;
    this.playerLogo.transform.Find("LevelBox/LevelText").GetComponent<Text>().text = "" + Persistence.gameConfig.profile.level;
    this.playerLogo.gameObject.SetActive(true);

    if (Persistence.gameConfig.profile != null)
      facebookHolder.GetPicture(playerLogo.transform.Find("Logo").GetComponentInChildren<Image>(), Persistence.gameConfig.profile.facebookId);

    this.leaderboardButton.interactable = true;
    leaderboardDialog.Setup();

    this.shopButton.interactable = true;

    tutorialMainMenu.InitTutorial();
  }

  private void InitializeAds()
  {
    #if UNITY_ANDROID
      string adUnitId = "ca-app-pub-8904882368983998/8500008022";
      string appId = "ca-app-pub-8904882368983998~7590428642";
      string gameId = "1590383";
    #elif UNITY_IPHONE
      string adUnitId = "ca-app-pub-8904882368983998/3926338199";
      string appId = "ca-app-pub-8904882368983998~2864537965";
      string gameId = "1590384";
    #else
      string adUnitId = "unexpected_platform";
      string appId = "unexpected_platform";
      string gameId = "unexpected_platform";
    #endif

    MobileAds.Initialize(appId);

    if (Advertisement.isSupported)
      Advertisement.Initialize(gameId, false);

    if (!Persistence.gameConfig.removedAds && bannerView == null)
    {
      bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);
      AdRequest request = new AdRequest.Builder().Build();
      bannerView.LoadAd(request);
    }
  }

  private void DestroyBanner()
  {
    if (bannerView != null)
      bannerView.Destroy();
  }
}
