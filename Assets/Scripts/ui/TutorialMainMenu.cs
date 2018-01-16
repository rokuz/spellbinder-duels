using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
  }

  public void OnNoClicked()
  {
  }

  public void OnNextClicked()
  {
  }
}
