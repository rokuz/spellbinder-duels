using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SmartLocalization;

public class TutorialMainMenu : MonoBehaviour
{
  public GameObject tutorial1;
  public GameObject tutorial2;
  public GameObject markers;

  public void InitTutorial()
  {
    if (Persistence.gameConfig.tutorialMainMenuShown)
      return;

    if (Persistence.gameConfig.tutorialMainMenuStep < 0 || Persistence.gameConfig.tutorialMainMenuStep > 5)
    {
      Persistence.gameConfig.tutorialMainMenuShown = true;
      Persistence.Save();
      return;
    }

    GetButtonText(tutorial1, "ButtonYes").text = LanguageManager.Instance.GetTextValue("Message.Yes");
    GetButtonText(tutorial1, "ButtonNo").text = LanguageManager.Instance.GetTextValue("Message.No");
    GetButtonText(tutorial2, "Button").text = LanguageManager.Instance.GetTextValue("Message.Next");

    SetActive();
  }
   
  private string GetTutorialText(int step)
  {
    int i = step + 1;
    return LanguageManager.Instance.GetTextValue("Tutorial.MainMenu." + i);
  }

  private GameObject GetTutorial(int step)
  {
    return step == 0 ? tutorial1 : tutorial2;
  }

  public void OnYesClicked()
  {
    ResetCurrent();

    Persistence.gameConfig.tutorialMainMenuStep = 1;
    Persistence.Save();

    SetActive();
  }

  private void SetActive()
  {
    var tut = GetTutorial(Persistence.gameConfig.tutorialMainMenuStep);
    tut.SetActive(true);
    GetText(tut).text = GetTutorialText(Persistence.gameConfig.tutorialMainMenuStep);

    var m = GetMarker(Persistence.gameConfig.tutorialMainMenuStep);
    if (m != null)
      m.SetActive(true);
  }

  private void ResetCurrent()
  {
    var tut = GetTutorial(Persistence.gameConfig.tutorialMainMenuStep);
    tut.SetActive(false);

    var m = GetMarker(Persistence.gameConfig.tutorialMainMenuStep);
    if (m != null)
      m.SetActive(false);
  }

  public void OnNoClicked()
  {
    ResetCurrent();

    Persistence.gameConfig.tutorialMainMenuStep = 0;
    Persistence.gameConfig.tutorialMainMenuShown = true;
    Persistence.Save();
  }

  public void OnNextClicked()
  {
    ResetCurrent();

    Persistence.gameConfig.tutorialMainMenuStep++;
    if (Persistence.gameConfig.tutorialMainMenuStep > 5)
    {
      Persistence.gameConfig.tutorialMainMenuStep = 0;
      Persistence.gameConfig.tutorialMainMenuShown = true;
    }
    else if (Persistence.gameConfig.tutorialMainMenuStep == 5)
    {
      GetButtonText(tutorial2, "Button").text = LanguageManager.Instance.GetTextValue("Message.Finish");
    }
    Persistence.Save();

    if (!Persistence.gameConfig.tutorialMainMenuShown)
      SetActive();
  }

  public void OnSpellbookClicked()
  {
    GetMarker(1).SetActive(false);
  }

  public void OnShopClicked()
  {
    GetMarker(2).SetActive(false);
  }

  public void OnLeaderboardClicked()
  {
    GetMarker(3).SetActive(false);
  }

  public void OnCharacterClicked()
  {
    GetMarker(4).SetActive(false);
  }

  public void OnSettingsClicked()
  {
    GetMarker(5).SetActive(false);
  }

  public void OnPlayClicked()
  {
    ResetCurrent();
  }

  private Text GetText(GameObject tutorial)
  {
    return (from t in tutorial.GetComponentsInChildren<Text>() where t.gameObject.name == "TutorialText" select t).Single();
  }

  private Text GetButtonText(GameObject tutorial, string buttonName)
  {
    var b = (from t in tutorial.GetComponentsInChildren<Button>() where t.gameObject.name == buttonName select t).Single();
    return (from t in b.GetComponentsInChildren<Text>() where t.gameObject.name == "Text" select t).Single();
  }

  private GameObject GetMarker(int step)
  {
    int i = step + 1;
    Transform t = markers.gameObject.transform.Find("Marker" + i);
    if (t == null)
      return null;
    return t.gameObject;
  }
}
