using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SmartLocalization;

public class MessageDialog : MonoBehaviour
{
  public Text title;
  public Text text;
  public Button okButton;
  public Image splash;

  public delegate void OnClose();
  private OnClose onCloseHandler;

  public bool IsOpened()
  {
    return gameObject.activeSelf;
  }

  public void Open(string titleStr, string message, OnClose onCloseHandler)
  {
    title.text = (titleStr.Length == 0) ? LanguageManager.Instance.GetTextValue("Message.Title") : titleStr;
    text.text = message;
    this.onCloseHandler = onCloseHandler;

    gameObject.SetActive(true);
    if (splash != null && !splash.IsActive())
      splash.gameObject.SetActive(true);
  }

  public void Close()
  {
    if (splash != null && splash.IsActive())
      splash.gameObject.SetActive(false);

    gameObject.SetActive(false);

    if (onCloseHandler != null)
      onCloseHandler();
  }

  public void OnOkButtonClicked()
  {
    Close();
  }
}
