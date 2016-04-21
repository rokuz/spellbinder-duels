using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SmartLocalization;

public class SetNameDialog : MonoBehaviour, ISetProfileNameRequestHandler
{
    public ServerRequest serverRequest;
    public Text headerText;
    public Text warningText;
    public InputField nameEditbox;
    public Button okButton;
    public Image splash;
    public Text placeholderText;

    public delegate void OnClose();

    private bool splashWasActive;
    private ProfileData profileData;
    private OnClose onCloseHandler;

	public void Start()
    {
        gameObject.SetActive(false);
        warningText.gameObject.SetActive(false);
        okButton.interactable = true;
        nameEditbox.interactable = true;
	}

	public void Update()
    {
	}

    public void Open(ProfileData profileData, OnClose onCloseHandler)
    {
        headerText.text = LanguageManager.Instance.GetTextValue("SetName.SetupName");
        placeholderText.text = LanguageManager.Instance.GetTextValue("SetName.EnterName");
        warningText.gameObject.SetActive(false);
        okButton.interactable = true;
        nameEditbox.interactable = true;

        this.profileData = profileData;
        this.onCloseHandler = onCloseHandler;

        this.nameEditbox.text = profileData.name;

        gameObject.SetActive(true);
        splashWasActive = splash.IsActive();
        if (!splashWasActive)
            splash.gameObject.SetActive(true);
    }

    public void Close()
    {
        if (!splashWasActive && splash.IsActive())
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
        else if (name.Length >= 15)
        {
            warningText.text = LanguageManager.Instance.GetTextValue("SetName.NameLong");
            warningText.gameObject.SetActive(true);
            return;
        }

        if (this.profileData.name == name)
        {
            Close();
            return;
        }

        nameEditbox.interactable = false;
        okButton.interactable = false;
        Dictionary<string, string> p = new Dictionary<string, string>();
        p["id"] = profileData.id;
        p["name"] = name;
        if (serverRequest != null)
            serverRequest.Send(ProfileRequests.SET_NAME, p, (WWW response) => { ProfileRequests.OnSetNameResponse(response, this); });
    }

    public void OnSetupProfileName()
    {
        this.profileData.name = nameEditbox.text;
        Close();
    }

    public void OnDuplicateProfileName()
    {
        warningText.text = LanguageManager.Instance.GetTextValue("SetName.NameInUse");
        warningText.gameObject.SetActive(true);
        okButton.interactable = true;
        nameEditbox.interactable = true;
    }

    public void OnProfileError(int code)
    {
        warningText.text = LanguageManager.Instance.GetTextValue("Message.ServerUnavailable") + " (" + code + ")";
        warningText.gameObject.SetActive(true);
        okButton.interactable = true;
        nameEditbox.interactable = true;
    }
}
