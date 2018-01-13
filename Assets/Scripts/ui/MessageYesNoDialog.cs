using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SmartLocalization;

public class MessageYesNoDialog : MonoBehaviour
{
  public Text title;
  public Text text;
  public Image splash;
  public Text yesText;
  public Text noText;

  public delegate void OnClose(bool yes);
  private OnClose onCloseHandler;

  public bool IsOpened()
  {
    return gameObject.activeSelf;
  }

  public void Open(string titleStr, string message, OnClose onCloseHandler)
  {
    title.text = (titleStr.Length == 0) ? LanguageManager.Instance.GetTextValue("Message.Title") : titleStr;
    text.text = message;
    yesText.text = LanguageManager.Instance.GetTextValue("Message.Yes");
    noText.text = LanguageManager.Instance.GetTextValue("Message.No");
    this.onCloseHandler = onCloseHandler;

    gameObject.SetActive(true);
    if (splash != null && !splash.IsActive())
      splash.gameObject.SetActive(true);
  }

  public void Close(bool yes)
  {
    if (splash != null && splash.IsActive())
      splash.gameObject.SetActive(false);

    gameObject.SetActive(false);

    if (onCloseHandler != null)
      onCloseHandler(yes);
  }

  public void OnYesButtonClicked()
  {
    Close(true);
  }

  public void OnNoButtonClicked()
  {
    Close(false);
  }
}
