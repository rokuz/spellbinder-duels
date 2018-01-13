using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SmartLocalization;

public class RewardDialog : MonoBehaviour
{
  public Text title;
  public Text rewardText;
  public Image coinsImage;
  public Text coinsText;
  public Text finishText;

  public delegate void OnClose();
  private OnClose onCloseHandler;

  public void Open(Match.Player winner, OnClose onCloseHandler)
  {
    title.text = LanguageManager.Instance.GetTextValue("Message.Win");
    this.onCloseHandler = onCloseHandler;

    if (winner.data.experienceCalculator.Experience != 0)
    {
      rewardText.gameObject.SetActive(true);
      if (winner.data.experienceCalculator.LevelsUp > 0)
      {
        rewardText.text = LanguageManager.Instance.GetTextValue("Reward.NewLevel");
      }
      else
      {
        rewardText.text = "+" + winner.data.experienceCalculator.Experience + " " 
                          + LanguageManager.Instance.GetTextValue("Reward.Exp");
      }
    }
    else
    {
      rewardText.gameObject.SetActive(false);
    }

    if (winner.data.experienceCalculator.Coins != 0)
    {
      coinsImage.gameObject.SetActive(true);
      string coinsStr = "+" + winner.data.experienceCalculator.Coins + " ";
      if (LanguageManager.Instance.GetSystemLanguageEnglishName() == "Russian")
      {
        if (winner.data.experienceCalculator.Coins == 1)
          coinsStr += LanguageManager.Instance.GetTextValue("Reward.CoinsForm1");
        else if (winner.data.experienceCalculator.Coins >= 2 && winner.data.experienceCalculator.Coins <= 4)
          coinsStr += LanguageManager.Instance.GetTextValue("Reward.CoinsForm2");
        else
          coinsStr += LanguageManager.Instance.GetTextValue("Reward.CoinsForm3");
      }
      else
      {
        coinsStr += LanguageManager.Instance.GetTextValue("Reward.CoinsUni");
      }
      coinsText.text = coinsStr;
    }
    else
    {
      coinsImage.gameObject.SetActive(false);
    }

    finishText.text = LanguageManager.Instance.GetTextValue("Victory.Finish");

    gameObject.SetActive(true);
  }

  public bool IsOpened()
  {
    return this.gameObject.activeSelf;
  }

  public void Close()
  {
    gameObject.SetActive(false);

    if (onCloseHandler != null)
      onCloseHandler();
  }
}
