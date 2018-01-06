using UnityEngine;
using UnityEngine.UI;
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
  public GameObject playerLogo;
  public Button playButton;
  public Button spellbookButton;
  public Button settingsButton;
  public Button shopButton;

	public void Start()
  {
    SmartCultureInfo systemLanguage = LanguageManager.Instance.GetDeviceCultureIfSupported();
    if (systemLanguage != null)
      LanguageManager.Instance.ChangeLanguage(systemLanguage);

    Application.runInBackground = true;
    Persistence.Load();

    #if !UNITY_EDITOR
    if (Persistence.gameConfig.profile != null && Persistence.gameConfig.profile.facebookId.Length != 0)
      facebookHolder.Login(null);
    #endif

    Spellbook.Init();

    this.playerLogo.gameObject.SetActive(false);

    this.playButton.interactable = true;
    this.playButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("MainMenu.Play");

    this.spellbookButton.interactable = true;
    this.shopButton.interactable = true;
    this.settingsButton.interactable = true;

    settingsDialog.Setup();
    characterDialog.Setup();
    spellbookDialog.Setup();

    InitProfile();
	}

  public void OnDestroy()
  {
    Persistence.Save();
  }
	   
  public void OnPlayButtonClicked()
  {
    this.playButton.interactable = false;
    matchingDialog.Open(Persistence.gameConfig.profile, () => { this.playButton.interactable = true; });
  }

  public void OnSpellbookButtonClicked()
  {
    this.spellbookButton.interactable = false;
    spellbookDialog.Open(Persistence.gameConfig.profile, () => { this.spellbookButton.interactable = true; });
  }

  public void OnSettingsButtonClicked()
  {
    if (facebookHolder.FacebookLoginInProgress)
      return;

    this.settingsButton.interactable = false;
    settingsDialog.Open(Persistence.gameConfig.profile, () => 
    {
      this.settingsButton.interactable = true;
    });
  }

  public void OnShopButtonClicked()
  {
    this.shopButton.interactable = false;
    messageDialog.Open("", LanguageManager.Instance.GetTextValue("Temp.Shop"), () => { this.shopButton.interactable = true; });
  }

  public void OnPlayerInfoClicked()
  {
    if (characterDialog.IsOpened() || matchingDialog.IsOpened() || messageDialog.IsOpened() || settingsDialog.IsOpened() ||
        spellbookDialog.IsOpened())
      return;

    this.playerLogo.gameObject.GetComponentInChildren<Button>().interactable = false;
    characterDialog.Open(Persistence.gameConfig.profile, () =>
    {
      this.playerLogo.gameObject.GetComponentInChildren<Button>().interactable = true;
    });
  }

  private void InitProfile()
  {
    if (Persistence.gameConfig.profile == null)
    {
      Persistence.gameConfig.profile = new ProfileData();
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
  }
}
