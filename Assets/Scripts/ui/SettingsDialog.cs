using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SmartLocalization;

public class SettingsDialog : MonoBehaviour
{
    public Image splash;
    public Image splash2;
    public Text title;
    public Text changeNameText;
    public Button changeNameButton;
    public Text loginText;
    public Text loginDescText;
    public Button loginButton;
    public Text volumeText;
    public Slider volumeSlider;
    public Text gameServerText;
    public InputField gameServerEditbox;
    public SetNameDialog setNameDialog;
    public Text playerText;

    public delegate void OnClose();
    private OnClose onCloseHandler;

    private ProfileData profileData;

    public void Start()
    {
        gameObject.SetActive(false);
    }

    public void Setup()
    {
        title.text = LanguageManager.Instance.GetTextValue("Settings.Title");
        changeNameText.text = LanguageManager.Instance.GetTextValue("Settings.ChangeName");
        changeNameButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("Settings.ChangeNameButton");
        loginText.text = LanguageManager.Instance.GetTextValue("Settings.Login");
        loginDescText.text = LanguageManager.Instance.GetTextValue("Settings.LoginDesc");
        volumeText.text = LanguageManager.Instance.GetTextValue("Settings.MusicVolume");
        gameServerText.text = LanguageManager.Instance.GetTextValue("Settings.GameServer");

        gameServerEditbox.text = Persistence.gameConfig.serverAddress;
    }

    public bool IsOpened()
    {
        return gameObject.activeSelf;
    }

    public void Update()
    {
        CloseIfClickedOutside(this.gameObject);
    }

    public void Open(ProfileData profileData, OnClose onCloseHandler)
    {
        this.profileData = profileData;
        changeNameButton.interactable = (this.profileData != null);
        this.onCloseHandler = onCloseHandler;

        gameObject.SetActive(true);
        if (splash != null && !splash.IsActive())
            splash.gameObject.SetActive(true);
    }

    public void Close()
    {
        Persistence.gameConfig.serverAddress = gameServerEditbox.text;
        Persistence.Save();

        if (splash != null && splash.IsActive())
            splash.gameObject.SetActive(false);

        gameObject.SetActive(false);

        if (onCloseHandler != null)
            onCloseHandler();
    }

    private void CloseIfClickedOutside(GameObject panel)
    {
        if (setNameDialog.IsOpened())
            return;

        if (Input.GetMouseButtonDown(0) && panel.activeSelf && 
             !RectTransformUtility.RectangleContainsScreenPoint(
                 panel.GetComponent<RectTransform>(), 
                 Input.mousePosition, 
                 Camera.main)) {
            Close();
        }
    }

     public void OnChangeNameClicked()
     {
        splash.gameObject.SetActive(false);
        splash2.gameObject.SetActive(true);
        changeNameButton.interactable = false;
        loginButton.interactable = false;
        volumeSlider.interactable = false;
        gameServerEditbox.interactable = false;
        setNameDialog.Open(this.profileData, () =>
        {
            this.playerText.text = UIUtils.GetFormattedString(profileData);

            changeNameButton.interactable = true;
            loginButton.interactable = true;
            volumeSlider.interactable = true;
            gameServerEditbox.interactable = true;
            splash2.gameObject.SetActive(false);
            splash.gameObject.SetActive(true);
        }, false);
     }
}
