using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;
using System.Collections;
using System.Collections.Generic;
using SmartLocalization;

public class SelectCharacterDialog : MonoBehaviour
{
  public Text headerText;
  public Button maleButton;
  public Button femaleButton;
  public Image splash;
  public ButtonAudio buttonAudio;

  public delegate void OnClose();

  private OnClose onCloseHandler;

  public bool IsOpened()
  {
    return gameObject.activeSelf;
  }

  public void Open(OnClose onCloseHandler)
  {
    headerText.text = LanguageManager.Instance.GetTextValue("SelectCharacter.Title");
    maleButton.interactable = true;
    femaleButton.interactable = true;
    maleButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("SelectCharacter.Male");
    femaleButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("SelectCharacter.Female");

    this.onCloseHandler = onCloseHandler;

    gameObject.SetActive(true);
    if (!splash.gameObject.activeSelf)
      splash.gameObject.SetActive(true);
  }

  public void Close()
  {
    if (splash.gameObject.activeSelf)
      splash.gameObject.SetActive(false);

    gameObject.SetActive(false);

    if (onCloseHandler != null)
      onCloseHandler();
  }

  public void OnMaleButtonClicked()
  {
    buttonAudio.Play(ButtonAudio.Type.Default);
    Persistence.preferences.SetMale(true);
    Persistence.Save();
    Close();
  }

  public void OnFemaleButtonClicked()
  {
    buttonAudio.Play(ButtonAudio.Type.Default);
    Persistence.preferences.SetMale(false);
    Persistence.Save();
    Close();
  }
}
