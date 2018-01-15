using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SmartLocalization;

public class DefeatDialog : MonoBehaviour
{
  public Text messageText;

  public Text leaveText;
  public Text replayText;

  public Image splash;

  public delegate void OnAction();
  private OnAction onCloseHandler;
  private OnAction onReplayHandler;

  public void Open(string message, OnAction onCloseHandler, OnAction onReplayHandler)
  {
    messageText.text = message;
    this.onCloseHandler = onCloseHandler;
    this.onReplayHandler = onReplayHandler;

    leaveText.text = LanguageManager.Instance.GetTextValue("Defeat.Leave");
    replayText.text = LanguageManager.Instance.GetTextValue("Defeat.Replay");

    gameObject.SetActive(true);

    if (splash != null && !splash.IsActive())
      splash.gameObject.SetActive(true);
  }

  public bool IsOpened()
  {
    return this.gameObject.activeSelf;
  }

  public void Close()
  {
    if (splash != null && splash.IsActive())
      splash.gameObject.SetActive(false);

    gameObject.SetActive(false);

    if (onCloseHandler != null)
      onCloseHandler();
  }

  public void Replay()
  {
    gameObject.SetActive(false);

    if (onReplayHandler != null)
      onReplayHandler();
  }
}
