using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SmartLocalization;

public class DefeatDialog : MonoBehaviour
{
  public Text messageText;

  public delegate void OnAction();
  private OnAction onCloseHandler;
  private OnAction onReplayHandler;

  public void Open(string message, OnAction onCloseHandler, OnAction onReplayHandler)
  {
    messageText.text = message;
    this.onCloseHandler = onCloseHandler;
    this.onReplayHandler = onReplayHandler;

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

  public void Replay()
  {
    gameObject.SetActive(false);

    if (onReplayHandler != null)
      onReplayHandler();
  }
}
