using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour, IProfileRequestsHandler
{
    public ServerRequest serverRequest;
    public SetNameDialog setNameDialog;
    public MatchingDialog matchingDialog;
    public Image playerLogo;
    public Button playButton;

    private ProfileData profileData;
    private Text playerText;

	public void Start()
    {
        this.playerText = playerLogo.GetComponentInChildren<Text>();
        this.playButton.interactable = false;

        Persistence.Load();
        if (Persistence.gameConfig.playerID.Length == 0)
            CreateProfile();
        else
            GetProfile();
	}

    public void OnDestroy()
    {
        Persistence.Save();
    }
	
    public void Update()
    {
	    
	}

    public void OnPlayButtonClicked()
    {
        this.playButton.interactable = false;
        matchingDialog.Open(profileData, () => { this.playButton.interactable = true; });
    }

#region IProfileRequestsHandler

    public void OnGetProfile(ProfileData profileData)
    {
        if (profileData.id != null)
        {
            Persistence.gameConfig.playerID = profileData.id;
            Persistence.Save();
        }
        else
        {
            profileData.id = Persistence.gameConfig.playerID;
        }
        this.profileData = profileData;

        if (this.profileData.name.Length == 0)
            setNameDialog.Open(this.profileData, () => { this.UpdatePlayerText(); });
        else
            this.UpdatePlayerText();
    }

    public void OnUnknownProfile()
    {
        CreateProfile();
    }

    public void OnProfileError(int code)
    {

    }

#endregion

    private void UpdatePlayerText()
    {
        this.playerText.text = string.Format("{0}\nLevel {1}", profileData.name, profileData.level);
        this.playButton.interactable = true;
    }

    private void CreateProfile()
    {
        Request(ProfileRequests.CREATE, null, (WWW response) => { ProfileRequests.OnCreateResponse(response, this); });
    }

    private void GetProfile()
    {
        Dictionary<string, string> p = new Dictionary<string, string>();
        p["id"] = Persistence.gameConfig.playerID;
        Request(ProfileRequests.GET, p, (WWW response) => { ProfileRequests.OnGetResponse(response, this); });
    }

    private void Request(string command, Dictionary<string, string> parameters, ServerRequest.OnResponse onResponseHandler)
    {
        if (serverRequest != null)
            serverRequest.Send(command, parameters, onResponseHandler);
    }
}
