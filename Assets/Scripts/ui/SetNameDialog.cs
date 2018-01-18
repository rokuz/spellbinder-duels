using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;
using System.Collections;
using System.Collections.Generic;
using SmartLocalization;

public class SetNameDialog : MonoBehaviour
{
  public FBHolder facebookHolder;
  public Text headerText;
  public Text warningText;
  public InputField nameEditbox;
  public Button okButton;
  public Image splash;
  public Text placeholderText;
  public Button fbButton;

  public delegate void OnClose();

  private ProfileData profileData;
  private OnClose onCloseHandler;

  public bool IsOpened()
  {
    return gameObject.activeSelf;
  }

  public void Open(ProfileData profileData, OnClose onCloseHandler)
  {
    headerText.text = LanguageManager.Instance.GetTextValue("SetName.SetupName");
    placeholderText.text = LanguageManager.Instance.GetTextValue("SetName.EnterName");
    warningText.gameObject.SetActive(false);
    okButton.interactable = true;
    nameEditbox.interactable = true;
    fbButton.interactable = !facebookHolder.FacebookLoggedIn;
    if (facebookHolder.FacebookLoggedIn)
      fbButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("SetName.LoginFinished");
    else
      fbButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("SetName.Login");

    this.profileData = profileData;
    this.onCloseHandler = onCloseHandler;

    this.nameEditbox.text = profileData.name;

    gameObject.SetActive(true);
    if (!splash.gameObject.activeSelf)
      splash.gameObject.SetActive(true);
  }

  public void Close()
  {
    if (splash.gameObject.activeSelf)
      splash.gameObject.SetActive(false);

    okButton.interactable = true;
    nameEditbox.interactable = true;
    gameObject.SetActive(false);

    if (onCloseHandler != null)
      onCloseHandler();
  }

  public void OnOkButtonClicked()
  {
    string name = nameEditbox.text;
    if (name.Length == 0)
    {
      warningText.text = LanguageManager.Instance.GetTextValue("SetName.NameEmpty");
      warningText.gameObject.SetActive(true);
      return;
    }
    else if (name.Length >= 30)
    {
      warningText.text = LanguageManager.Instance.GetTextValue("SetName.NameLong");
      warningText.gameObject.SetActive(true);
      return;
    }

    warningText.gameObject.SetActive(false);
    this.profileData.name = name;
    Close();
  }

  public void OnFbButtonClicked()
  {
    if (!facebookHolder.FacebookInitialized || facebookHolder.FacebookLoggedIn)
      return;

    Analytics.CustomEvent("SetName_FB");

    fbButton.interactable = false;
    facebookHolder.Login((bool success) =>
    {
      fbButton.interactable = !success;
      if (success)
      {
        if (nameEditbox.text.Length == 0)
          nameEditbox.text = facebookHolder.FacebookName;
        profileData.facebookId = facebookHolder.FacebookID;
        fbButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("SetName.LoginFinished");
        Persistence.Save();
      }
    });
  }
}
