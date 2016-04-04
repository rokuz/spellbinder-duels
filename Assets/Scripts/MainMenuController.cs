using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SmartLocalization;

public class MainMenuController : MonoBehaviour, IProfileRequestsHandler, ISpellRequestsHandler
{
    public ServerRequest serverRequest;
    public SetNameDialog setNameDialog;
    public MatchingDialog matchingDialog;
    public MessageDialog messageDialog;
    public SpellbookDialog spellbookDialog;
    public Image playerLogo;
    public Button playButton;
    public Button tournamentButton;
    public Button spellbookButton;
    public Button settingsButton;
    public Button shopButton;

    private ProfileData profileData;
    private Text playerText;

	public void Start()
    {
        //TEMP
        LanguageManager.Instance.ChangeLanguage("ru");

        this.playerText = playerLogo.GetComponentInChildren<Text>();
        this.playerLogo.gameObject.SetActive(false);

        this.playButton.interactable = false;
        this.playButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("MainMenu.Play");

        this.tournamentButton.interactable = false;
        this.tournamentButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("MainMenu.Tournament");

        this.spellbookButton.interactable = false;
        this.shopButton.interactable = false;
        this.settingsButton.interactable = false;

        Persistence.Load();
        SynchronizeWithServer();
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

    public void OnTournamentButtonClicked()
    {
        this.tournamentButton.interactable = false;
        messageDialog.Open(LanguageManager.Instance.GetTextValue("Temp.Tournament"), () => { this.tournamentButton.interactable = true; });
    }

    public void OnSpellbookButtonClicked()
    {
        this.spellbookButton.interactable = false;
        spellbookDialog.Open(() => { this.spellbookButton.interactable = true; });
    }

    public void OnSettingsButtonClicked()
    {
    }

    public void OnShopButtonClicked()
    {
        this.shopButton.interactable = false;
        messageDialog.Open(LanguageManager.Instance.GetTextValue("Temp.Shop"), () => { this.shopButton.interactable = true; });
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
        messageDialog.Open(LanguageManager.Instance.GetTextValue("Message.ServerUnavailable") + " (" + code + ")",
                           () => { StartCoroutine(SynchronizeWithServerDeferred()); });
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
        yield return new WaitForSeconds(0.5f);
        this.SynchronizeWithServer();
    }

	private IEnumerator GetAllSpellsDeferred()
	{
		yield return new WaitForSeconds(0.3f);
		this.SynchronizeWithServer();
	}
		
    private void SynchronizeWithServer()
    {
        if (Persistence.gameConfig.playerID.Length == 0)
            CreateProfile();
        else
            GetProfile();
    }

    private void UpdatePlayerText()
    {
        this.playerText.text = UIUtils.GetFormattedString(profileData);
        this.playerLogo.gameObject.SetActive(true);
        this.settingsButton.interactable = true;

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
