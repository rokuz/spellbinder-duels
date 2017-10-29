using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RewardDialog : MonoBehaviour
{
  public Text text;
  public Image crownImage;

  public delegate void OnClose();
  private OnClose onCloseHandler;

  public void Update()
  {
    if (Input.GetMouseButtonDown(0) && this.gameObject.activeSelf)
      Close();
  }

  public void Open(string message, bool isWin, OnClose onCloseHandler)
  {
    text.text = message;
    this.onCloseHandler = onCloseHandler;
    crownImage.gameObject.SetActive(isWin);

    gameObject.SetActive(true);
  }

  public void Close()
  {
    gameObject.SetActive(false);

    if (onCloseHandler != null)
      onCloseHandler();
  }
}
