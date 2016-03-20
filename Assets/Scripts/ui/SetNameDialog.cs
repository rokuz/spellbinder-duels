using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SetNameDialog : MonoBehaviour, ISetProfileNameRequestHandler
{
    public ServerRequest serverRequest;
    public Text warningText;
    public InputField nameEditbox;
    public Button okButton;
    public Image splash;

    public delegate void OnClose();

    private ProfileData profileData;
    private OnClose onCloseHandler;

    //private Image panel;

	public void Start()
    {
        gameObject.SetActive(false);
        warningText.gameObject.SetActive(false);
        okButton.interactable = true;
        nameEditbox.interactable = true;
        //panel = gameObject.GetComponent<Image>();
	}

	public void Update()
    {
	}

    public void Open(ProfileData profileData, OnClose onCloseHandler)
    {
        this.profileData = profileData;
        this.onCloseHandler = onCloseHandler;

        gameObject.SetActive(true);
        if (!splash.IsActive())
            splash.gameObject.SetActive(true);
    }

    public void Close()
    {
        if (splash.IsActive())
            splash.gameObject.SetActive(false);

        okButton.interactable = true;
        nameEditbox.interactable = true;
        gameObject.SetActive(false);
    }

    public void OnOkButtonClicked()
    {
        string name = nameEditbox.text;
        if (name.Length == 0)
        {
            warningText.text = "Name is empty";
            warningText.gameObject.SetActive(true);
            return;
        }
        else if (name.Length >= 15)
        {
            warningText.text = "Name is very long";
            warningText.gameObject.SetActive(true);
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
        if (onCloseHandler != null)
            onCloseHandler();
    }

    public void OnDuplicateProfileName()
    {
        warningText.text = "Name is already in use";
        warningText.gameObject.SetActive(true);
        okButton.interactable = true;
        nameEditbox.interactable = true;
    }

    public void OnProfileError(int code)
    {
        warningText.text = "Server is unavailable (" + code + ")";
        warningText.gameObject.SetActive(true);
        okButton.interactable = true;
        nameEditbox.interactable = true;
    }
}
