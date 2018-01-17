﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SmartLocalization;

public class TutorialCoreGame : MonoBehaviour
{
  public GameObject tutorial1;
  public GameObject tutorial2;
  //public GameObject markers;

  private Spell spellToCast;

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

    var b = (from t in tut.GetComponentsInChildren<Button>() where t.gameObject.name == "ButtonNext" select t).Single();
    b.gameObject.SetActive(false);
  }

  public void OnSpellCasted(Spell spell)
  {
    if (Persistence.gameConfig.tutorialCoreGameShown || spellToCast == null)
      return;
    if (Persistence.gameConfig.tutorialCoreGameStep != 3)
      return;

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

    var b = (from t in tut.GetComponentsInChildren<Button>() where t.gameObject.name == "ButtonNext" select t).Single();
    b.gameObject.SetActive(true);
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
    ResetCurrent();

    Persistence.gameConfig.tutorialCoreGameStep = 1;
    Persistence.Save();

    SetActive();
  }

  private void SetActive()
  {
    var tut = GetTutorial(Persistence.gameConfig.tutorialCoreGameStep);
    tut.SetActive(true);
    GetText(tut).text = GetTutorialText(Persistence.gameConfig.tutorialCoreGameStep);

    //var m = GetMarker(Persistence.gameConfig.tutorialCoreGameStep);
    //if (m != null)
    //  m.SetActive(true);
  }

  private void ResetCurrent()
  {
    var tut = GetTutorial(Persistence.gameConfig.tutorialCoreGameStep);
    tut.SetActive(false);

    //var m = GetMarker(Persistence.gameConfig.tutorialCoreGameStep);
    //if (m != null)
    //  m.SetActive(false);
  }

  public void OnNoClicked()
  {
    ResetCurrent();

    Persistence.gameConfig.tutorialCoreGameStep = 0;
    //Persistence.gameConfig.tutorialCoreGameShown = true;
    Persistence.Save();
  }

  public void OnNextClicked()
  {
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
      //Persistence.gameConfig.tutorialCoreGameShown = true;
    }
    else if (Persistence.gameConfig.tutorialCoreGameStep == 5)
    {
      GetButtonText(tutorial2, "Button").text = LanguageManager.Instance.GetTextValue("Message.Finish");
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

  private GameObject GetMarker(int step)
  {
    int i = step + 1;
    Transform t = null;//markers.gameObject.transform.Find("Marker" + i);
    if (t == null)
      return null;
    return t.gameObject;
  }
}
