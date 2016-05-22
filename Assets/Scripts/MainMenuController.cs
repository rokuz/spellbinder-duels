using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SmartLocalization;

public class MainMenuController : MonoBehaviour, IProfileRequestsHandler, ISpellRequestsHandler
{
    public ServerRequest serverRequest;
    public FBHolder facebookHolder;
    public SetNameDialog setNameDialog;
    public MatchingDialog matchingDialog;
    public MessageDialog messageDialog;
    public SpellbookDialog spellbookDialog;
    public SettingsDialog settingsDialog;
    public CharacterDialog characterDialog;
    public GameObject playerLogo;
    public Button playButton;
    public Button tournamentButton;
    public Button spellbookButton;
    public Button settingsButton;
    public Button shopButton;

    private ProfileData profileData;
    private Text playerText;

    private bool synchInProgress = false;

	public void Start()
    {
        #if UNITY_EDITOR
            LanguageManager.Instance.ChangeLanguage("ru");
        #endif

        Application.runInBackground = true;
        Persistence.Load();

        SettingsDialog.ApplyGamma();

        #if !UNITY_EDITOR
        if (Persistence.gameConfig.facebookId != null && Persistence.gameConfig.facebookId.Length != 0)
            facebookHolder.Login(null);
        #endif

        this.playerText = playerLogo.GetComponentInChildren<Text>();
        this.playerLogo.gameObject.SetActive(false);
        playerLogo.GetComponent<PlayerInfoCollider>().Setup(OnPlayerInfoClicked);

        this.playButton.interactable = false;
        this.playButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("MainMenu.Play");

        this.tournamentButton.interactable = false;
        this.tournamentButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("MainMenu.Tournament");

        this.spellbookButton.interactable = false;
        this.shopButton.interactable = false;
        this.settingsButton.interactable = true;

        settingsDialog.Setup();
        characterDialog.Setup();

        SynchronizeWithServer();
	}

    public void OnDestroy()
    {
        Persistence.Save();
    }
	   
    public void OnPlayButtonClicked()
    {
        if (this.synchInProgress)
            return;

        this.playButton.interactable = false;
        matchingDialog.Open(profileData, () => { this.playButton.interactable = true; });
    }

    public void OnTournamentButtonClicked()
    {
        if (this.synchInProgress)
            return;

        this.tournamentButton.interactable = false;
        messageDialog.Open(LanguageManager.Instance.GetTextValue("Temp.Tournament"), () => { this.tournamentButton.interactable = true; });
    }

    public void OnSpellbookButtonClicked()
    {
        if (this.synchInProgress)
            return;

        this.spellbookButton.interactable = false;
        spellbookDialog.Open(profileData, () => { this.spellbookButton.interactable = true; });
    }

    public void OnSettingsButtonClicked()
    {
        if (facebookHolder.FacebookLoginInProgress)
            return;

        this.settingsButton.interactable = false;
        settingsDialog.Open(this.profileData, () => 
        {
            this.settingsButton.interactable = true;
            SynchronizeWithServer();
        });
    }

    public void OnShopButtonClicked()
    {
        if (this.synchInProgress)
            return;

        this.shopButton.interactable = false;
        messageDialog.Open(LanguageManager.Instance.GetTextValue("Temp.Shop"), () => { this.shopButton.interactable = true; });
    }

    public void OnPlayerInfoClicked()
    {
        if (characterDialog.IsOpened() || matchingDialog.IsOpened() || messageDialog.IsOpened() || settingsDialog.IsOpened() ||
            spellbookDialog.IsOpened() || this.synchInProgress)
            return;

        characterDialog.Open(profileData, () => {});
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

        this.synchInProgress = false;

        facebookHolder.GetPicture(playerLogo.GetComponentInChildren<Image>(), Persistence.gameConfig.facebookId);
    }

    public void OnUnknownProfile()
    {
        CreateProfile();
    }

    public void OnProfileError(int code)
    {
        if (settingsDialog.IsOpened())
            return;
        messageDialog.Open(LanguageManager.Instance.GetTextValue("Message.ServerUnavailable") + " (" + code + ")",
                           () => { StartCoroutine(SynchronizeWithServerDeferred()); });
    }

    public void OnBindFacebook()
    {
        Persistence.gameConfig.profileSynchronized = true;
        Persistence.Save();
        GetProfile();
    }

    public void OnBindFacebookFailed()
    {
        GetProfile();
    }

#endregion

#region ISpellRequestsHandler

	public void OnGetAllSpells(List<SpellData> spells)
	{
		SceneConnector.Instance.Spells = spells;
		spellbookDialog.Setup(spells);

		this.spellbookButton.interactable = true;
		this.playButton.interactable = true;
		this.tournamentButton.interactable = true;
		this.shopButton.interactable = true;
	}

	public void OnSpellError(int code)
	{
		messageDialog.Open(LanguageManager.Instance.GetTextValue("Message.ServerUnavailable") + " (" + code + ")",
						   () => { StartCoroutine(GetAllSpellsDeferred()); });
	}

#endregion

	private IEnumerator SynchronizeWithServerDeferred()
    {
        yield return new WaitForSeconds(5.0f);
        this.SynchronizeWithServer();
    }

	private IEnumerator GetAllSpellsDeferred()
	{
		yield return new WaitForSeconds(0.3f);
		this.SynchronizeWithServer();
	}
		
    private void SynchronizeWithServer()
    {
        this.synchInProgress = true;
        if (Persistence.gameConfig.playerID.Length == 0)
        {
            CreateProfile();
        }
        else
        {
            if (!Persistence.gameConfig.profileSynchronized && Persistence.gameConfig.facebookId != null &&
                Persistence.gameConfig.facebookId.Length != 0)
            {
                Dictionary<string, string> p = new Dictionary<string, string>();
                p["id"] = Persistence.gameConfig.playerID;
                p["fid"] = Persistence.gameConfig.facebookId;
                Request(ProfileRequests.BIND_FACEBOOK, p, (WWW response) => { ProfileRequests.OnBindFacebook(response, this); });
            }
            else
            {
                GetProfile();
            }
        }
    }

    private void UpdatePlayerText()
    {
        this.playerText.text = UIUtils.GetFormattedString(profileData);
        this.playerLogo.gameObject.SetActive(true);

		GetAllSpells();
    }

	private void GetAllSpells()
	{
		Request(SpellRequests.GET_ALL, null, (WWW response) => { SpellRequests.OnGetAllResponse(response, this); });
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
