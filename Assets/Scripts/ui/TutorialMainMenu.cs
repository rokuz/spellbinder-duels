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

  void Start()
	{
    if (Persistence.gameConfig.tutorialMainMenuShown)
      return;

    if (Persistence.gameConfig.tutorialMainMenuStep < 0 || Persistence.gameConfig.tutorialMainMenuStep > 5)
    {
      Persistence.gameConfig.tutorialMainMenuShown = true;
      Persistence.Save();
      return;
    }

    UpdateTutorial();
	}

  private void UpdateTutorial()
  {
    var tut = GetTutorial(Persistence.gameConfig.tutorialMainMenuStep);
    tut.SetActive(true);
    GetText(tut).text = GetTutorialText(Persistence.gameConfig.tutorialMainMenuStep);
  }
	
	void Update()
  {
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
    var tut = GetTutorial(Persistence.gameConfig.tutorialMainMenuStep);
    tut.SetActive(false);

    Persistence.gameConfig.tutorialMainMenuStep = 1;
    Persistence.Save();

    UpdateTutorial();
  }

  public void OnNoClicked()
  {
    var tut = GetTutorial(Persistence.gameConfig.tutorialMainMenuStep);
    tut.SetActive(false);
    Persistence.gameConfig.tutorialMainMenuStep = 0;
    //Persistence.gameConfig.tutorialMainMenuShown = true;
    Persistence.Save();
  }

  public void OnNextClicked()
  {
    var tut = GetTutorial(Persistence.gameConfig.tutorialMainMenuStep);
    tut.SetActive(false);

    Persistence.gameConfig.tutorialMainMenuStep++;
    if (Persistence.gameConfig.tutorialMainMenuStep > 5)
    {
      Persistence.gameConfig.tutorialMainMenuStep = 0;
      //Persistence.gameConfig.tutorialMainMenuShown = true;
    }
    Persistence.Save();

    UpdateTutorial();
  }

  public Text GetText(GameObject tutorial)
  {
    return (from t in tutorial.GetComponentsInChildren<Text>() where t.gameObject.name == "TutorialText" select t).Single();
  }
}
