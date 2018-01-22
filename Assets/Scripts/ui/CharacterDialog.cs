using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;
using System.Collections;
using System.Linq;
using SmartLocalization;

public class CharacterDialog : MonoBehaviour
{
  public Image splash;

  public Text nameText;
  public Text levelText;
  public Text experienceText;
  public Text fireDamageText;
  public Text waterDamageText;
  public Text airDamageText;
  public Text fireResistanceText;
  public Text waterResistanceText;
  public Text airResistanceText;

  private ProfileData profileData;

  private bool enableClosing = false;

  public delegate void OnClose();
  private OnClose onCloseHandler;

  public bool IsOpened()
  {
    return gameObject.activeSelf;
  }

  public void Update()
  {
    CloseIfClickedOutside(this.gameObject);
  }

  public void Setup()
  {
    experienceText.text = LanguageManager.Instance.GetTextValue("Char.Experience");
    fireDamageText.text = LanguageManager.Instance.GetTextValue("Char.FDB");
    waterDamageText.text = LanguageManager.Instance.GetTextValue("Char.WDB");
    airDamageText.text = LanguageManager.Instance.GetTextValue("Char.ADB");
    fireResistanceText.text = LanguageManager.Instance.GetTextValue("Char.FRB");
    waterResistanceText.text = LanguageManager.Instance.GetTextValue("Char.WRB");
    airResistanceText.text = LanguageManager.Instance.GetTextValue("Char.ARB");
  }

  public void Open(ProfileData profileData, OnClose onCloseHandler)
  {
    Analytics.CustomEvent("Character_Open");

    this.profileData = profileData;
    if (this.profileData != null)
    {
      nameText.text = this.profileData.name;
      levelText.text = LanguageManager.Instance.GetTextValue("Player.Level") + " " + profileData.level;

      SetupBarProgress(experienceText, profileData.experienceProgress / 100.0f);
      GetBarText(experienceText).text = "" + profileData.experience + "/" + 
        (profileData.level != Constants.MAX_LEVEL ? profileData.experienceToNextLevel : Constants.LEVEL_EXP[Constants.MAX_LEVEL - 2]);

      SetupBarProgress(fireDamageText, GetBonusProgress(profileData.bonuses[Spell.FIRE_INDEX]));
      FormatBonusText(fireDamageText, profileData.bonuses[Spell.FIRE_INDEX]);

      SetupBarProgress(waterDamageText, GetBonusProgress(profileData.bonuses[Spell.WATER_INDEX]));
      FormatBonusText(waterDamageText, profileData.bonuses[Spell.WATER_INDEX]);

      SetupBarProgress(airDamageText, GetBonusProgress(profileData.bonuses[Spell.AIR_INDEX]));
      FormatBonusText(airDamageText, profileData.bonuses[Spell.AIR_INDEX]);

      SetupBarProgress(fireResistanceText, GetBonusProgress(profileData.resistance[Spell.FIRE_INDEX]));
      FormatBonusText(fireResistanceText, profileData.resistance[Spell.FIRE_INDEX]);

      SetupBarProgress(waterResistanceText, GetBonusProgress(profileData.resistance[Spell.WATER_INDEX]));
      FormatBonusText(waterResistanceText, profileData.resistance[Spell.WATER_INDEX]);

      SetupBarProgress(airResistanceText, GetBonusProgress(profileData.resistance[Spell.AIR_INDEX]));
      FormatBonusText(airResistanceText, profileData.resistance[Spell.AIR_INDEX]);
    }
    this.onCloseHandler = onCloseHandler;
    this.enableClosing = false;

    gameObject.SetActive(true);
    if (splash != null && !splash.IsActive())
      splash.gameObject.SetActive(true);

    StartCoroutine(EnableClosing());
  }

  public void Close()
  {
    if (splash != null && splash.IsActive())
      splash.gameObject.SetActive(false);

    gameObject.SetActive(false);

    if (onCloseHandler != null)
      onCloseHandler();
  }

  private IEnumerator EnableClosing()
  {
    yield return null;
    enableClosing = true;
  }

  private void CloseIfClickedOutside(GameObject panel)
  {
    if (!enableClosing)
      return;

    if (Input.GetMouseButtonDown(0) && panel.activeSelf && 
        !RectTransformUtility.RectangleContainsScreenPoint(panel.GetComponent<RectTransform>(), Input.mousePosition, null))
    {
      Close();
    }

    if (Input.GetKeyDown(KeyCode.Escape))
      Close();
  }

  private float GetBonusProgress(int bonus)
  {
    int i = bonus / 1000;
    return (float)(bonus - i * 1000) / 1000.0f;
  }

  private void SetupBarProgress(Text bar, float progress)
  {
    if (progress < 0.0f) progress = 0.0f;
    if (progress > 1.0f) progress = 1.0f;
    var b = (from t in bar.GetComponentsInChildren<RectTransform>() where t.gameObject.name == "Bar" select t).Single();
    var c = (from t in bar.GetComponentsInChildren<RectTransform>() where t.gameObject.name == "Cur" select t).Single();
    float x = b.sizeDelta.x;
    c.sizeDelta = new Vector2(progress * x, c.sizeDelta.y);
  }

  private Text GetBarText(Text bar)
  {
    return (from t in bar.GetComponentsInChildren<Text>() where t.gameObject.name == "Text" select t).Single();
  }

  private void FormatBonusText(Text bar, int bonus)
  {
    int i = bonus / 1000;
    int n = (i + 1) * 1000;
    GetBarText(bar).text = (i != 0 ? "+" : "") + i + " (" + bonus + "/" + n + ")";
  }
}
