using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;
using SmartLocalization;

public class TutorialCoreGame : MonoBehaviour
{
  public GameObject tutorial1;
  public GameObject tutorial2;
  public GameObject[] markers = new GameObject[GameField.CARDS_COUNT];
  public GameObject tutorialCrystalSelector;
  public CoreGameAudio audio;

  private Spell spellToCast;
  private Button nextButton;

  public void InitTutorial()
  {
    if (Persistence.gameConfig.tutorialCoreGameShown)
      return;

    if (Persistence.gameConfig.tutorialCoreGameStep < 0 || Persistence.gameConfig.tutorialCoreGameStep > 5)
    {
      Persistence.gameConfig.tutorialCoreGameShown = true;
      Persistence.Save();
      return;
    }

    if (Persistence.gameConfig.tutorialCoreGameStep == 3)
      Persistence.gameConfig.tutorialCoreGameStep = 2;
    else if (Persistence.gameConfig.tutorialCoreGameStep == 4)
      Persistence.gameConfig.tutorialCoreGameStep = 5;
    Persistence.Save();

    GetButtonText(tutorial1, "ButtonYes").text = LanguageManager.Instance.GetTextValue("Message.Yes");
    GetButtonText(tutorial1, "ButtonNo").text = LanguageManager.Instance.GetTextValue("Message.No");
    GetButtonText(tutorial2, "ButtonNext").text = LanguageManager.Instance.GetTextValue("Message.Next");

    SetActive();
  }

  public void OnCrystalActivated(int index1, int index2, Spell spellToCast)
  {
    if (Persistence.gameConfig.tutorialCoreGameShown)
      return;
    if (Persistence.gameConfig.tutorialCoreGameStep != 3)
      return;

    this.spellToCast = spellToCast;

    SetActive();

    var tut = GetTutorial(Persistence.gameConfig.tutorialCoreGameStep);
    string spellName = LanguageManager.Instance.GetTextValue("Spell." + spellToCast.SpellType.ToString());
    GetText(tut).text = GetTutorialText(Persistence.gameConfig.tutorialCoreGameStep) + " \"" + spellName+ "\".";

    nextButton = (from t in tut.GetComponentsInChildren<Button>() where t.gameObject.name == "ButtonNext" select t).Single();
    nextButton.gameObject.SetActive(false);

    this.markers[index1].SetActive(true);
    this.markers[index2].SetActive(true);
    this.markers[index1].GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    this.markers[index2].GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    
    tutorialCrystalSelector.SetActive(true);
  }

  public void OnSpellCasted(Spell spell)
  {
    if (Persistence.gameConfig.tutorialCoreGameShown || spellToCast == null)
      return;
    if (Persistence.gameConfig.tutorialCoreGameStep != 3)
      return;

    if (nextButton != null)
    {
      nextButton.gameObject.SetActive(true);
      nextButton = null;
    }

    ResetCurrent();

    Persistence.gameConfig.tutorialCoreGameStep = 4;
    Persistence.Save();

    SetActive();
    var tut = GetTutorial(Persistence.gameConfig.tutorialCoreGameStep);
    if (spell == null)
      GetText(tut).text = LanguageManager.Instance.GetTextValue("Tutorial.Battle.5.3");
    else if (spell.Code == spellToCast.Code)
      GetText(tut).text = LanguageManager.Instance.GetTextValue("Tutorial.Battle.5.1");
    else
      GetText(tut).text = LanguageManager.Instance.GetTextValue("Tutorial.Battle.5.2");

    DeactivateMarkers();
    tutorialCrystalSelector.SetActive(false);
  }

  public bool EnabledToss()
  {
    if (Persistence.gameConfig.tutorialCoreGameShown)
      return true;
    return Persistence.gameConfig.tutorialCoreGameStep >= 1;
  }

  public bool EnabledShowCards()
  {
    if (Persistence.gameConfig.tutorialCoreGameShown)
      return true;
    return Persistence.gameConfig.tutorialCoreGameStep >= 2;
  }

  public bool EnabledTurnOverAll()
  {
    if (Persistence.gameConfig.tutorialCoreGameShown)
      return true;
    return Persistence.gameConfig.tutorialCoreGameStep >= 3;
  }

  private string GetTutorialText(int step)
  {
    int i = step + 1;
    return LanguageManager.Instance.GetTextValue("Tutorial.Battle." + i);
  }

  private GameObject GetTutorial(int step)
  {
    return step == 0 ? tutorial1 : tutorial2;
  }

  public void OnYesClicked()
  {
    audio.Play(CoreGameAudio.Type.ButtonDefault);
    ResetCurrent();

    Persistence.gameConfig.tutorialCoreGameStep = 1;
    Persistence.Save();

    SetActive();

    Analytics.CustomEvent("Tutorial_CoreGame_Started");
  }

  private void SetActive()
  {
    var tut = GetTutorial(Persistence.gameConfig.tutorialCoreGameStep);
    tut.SetActive(true);
    GetText(tut).text = GetTutorialText(Persistence.gameConfig.tutorialCoreGameStep);
  }

  private void ResetCurrent()
  {
    var tut = GetTutorial(Persistence.gameConfig.tutorialCoreGameStep);
    tut.SetActive(false);
  }

  public void ActivateMarkers(int[] indices)
  {
    if (indices == null)
      return;
    
    for (int i = 0; i < indices.Length; i++)
    {
      markers[indices[i]].SetActive(true);
      if (i == 1 && indices.Length == 3)
        markers[indices[i]].GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
      else
        markers[indices[i]].GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }
  }

  public void DeactivateMarker(int index)
  {
    markers[index].SetActive(false);
  }

  public void DeactivateMarkers()
  {
    foreach (var m in this.markers)
      m.SetActive(false);
  }

  public void OnNoClicked()
  {
    audio.Play(CoreGameAudio.Type.ButtonNo);
    ResetCurrent();

    Persistence.gameConfig.tutorialCoreGameStep = 0;
    Persistence.gameConfig.tutorialCoreGameShown = true;
    Persistence.Save();

    Analytics.CustomEvent("Tutorial_CoreGame_Discarded");
  }

  public void OnNextClicked()
  {
    audio.Play(CoreGameAudio.Type.ButtonDefault);
    ResetCurrent();
    Persistence.gameConfig.tutorialCoreGameStep++;
    if (Persistence.gameConfig.tutorialCoreGameStep == 3)
    {
      // Wait for OnCrystalActivated to SetActive.
      Persistence.Save();
      return;
    }
    else if (Persistence.gameConfig.tutorialCoreGameStep > 5)
    {
      Persistence.gameConfig.tutorialCoreGameStep = 0;
      Persistence.gameConfig.tutorialCoreGameShown = true;
      Analytics.CustomEvent("Tutorial_CoreGame_Finished");
    }
    else if (Persistence.gameConfig.tutorialCoreGameStep == 5)
    {
      GetButtonText(tutorial2, "ButtonNext").text = LanguageManager.Instance.GetTextValue("Message.Finish");
    }
    Persistence.Save();

    if (!Persistence.gameConfig.tutorialCoreGameShown)
      SetActive();
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
}
