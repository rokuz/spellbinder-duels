using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SmartLocalization;

public class PrivacyPolicyDialog : MonoBehaviour
{
  public Text title;
  public Text text;
  public Button playButton;
  public Button showButton;
  public Toggle acceptToggle;
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
    title.text = LanguageManager.Instance.GetTextValue("PP.Title");
    text.text = LanguageManager.Instance.GetTextValue("PP.Message");
    playButton.transform.Find("Text").GetComponent<Text>().text = LanguageManager.Instance.GetTextValue("MainMenu.Play");
    showButton.transform.Find("Text").GetComponent<Text>().text = LanguageManager.Instance.GetTextValue("PP.Website");
    acceptToggle.transform.Find("Label").GetComponent<Text>().text = LanguageManager.Instance.GetTextValue("PP.Accept");
    playButton.interactable = false;

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

  public void OnPlayButtonClicked()
  {
    buttonAudio.Play(ButtonAudio.Type.Default);
    Persistence.preferences.SetPrivacyPolicyAccepted(true);
    Close();
  }

  public void OnShowButtonClicked()
  {
    buttonAudio.Play(ButtonAudio.Type.Default);
    Application.OpenURL("https://rokuzz.wixsite.com/games/spellbinder-duels");
  }

  public void OnAcceptToggle()
  {
    buttonAudio.Play(ButtonAudio.Type.Default);
    playButton.interactable = acceptToggle.isOn;
  }
}
