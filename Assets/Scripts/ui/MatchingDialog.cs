using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using SmartLocalization;

public class MatchingDialog : MonoBehaviour, IMatchingRequestsHandler
{
    public ServerRequest serverRequest;
    public FBHolder facebookHolder;
    public Text lookingForText;
    public MessageDialog messageDialog;
    public GameObject player;
    public GameObject opponent;
    public Text versusText;

    public delegate void OnClose();
    private OnClose onCloseHandler;

    private ProfileData profileData;
    private ProfileData opponentData;

    private bool isAnimating = false;
    private string[] animatedText = new string[] { "", ".", "..", "..." };
    private int textAnimationIndex = 0;
    private float animationStartTime;

    private bool needRequest = false;

    public void Start()
    {
        lookingForText.text = LanguageManager.Instance.GetTextValue("Matching.LookingFor");
        if (LanguageManager.Instance.GetSystemLanguageEnglishName() == "Russian")
        {
            RectTransform t = lookingForText.GetComponent<RectTransform>();
            t.anchoredPosition = new Vector2(t.anchoredPosition.x - 30.0f, t.anchoredPosition.y);
        }
        versusText.text = LanguageManager.Instance.GetTextValue("Matching.Versus");
        gameObject.SetActive(false);
        player.gameObject.SetActive(false);
        opponent.gameObject.SetActive(false);
        versusText.gameObject.SetActive(false);
        lookingForText.gameObject.SetActive(true);
	}

    public void Update()
    {
        if (isAnimating)
        {
            const float kShowTime = 0.3f;
            int newIndex = (int)Mathf.Round((Time.time - animationStartTime) / kShowTime) % animatedText.Length;
            if (textAnimationIndex != newIndex)
            {
                textAnimationIndex = newIndex;
                lookingForText.text = LanguageManager.Instance.GetTextValue("Matching.LookingFor") + animatedText[textAnimationIndex];
            }
        }

        if (needRequest)
        {
            needRequest = false;
            Dictionary<string, string> p = new Dictionary<string, string>();
            p["id"] = profileData.id;
            if (serverRequest != null)
                serverRequest.Send(MatchingRequests.MATCH, p, (WWW response) => { MatchingRequests.OnMatchResponse(response, this); });
        }
	}

    public bool IsOpened()
    {
        return gameObject.activeSelf;
    }

    public void Open(ProfileData profileData, OnClose onCloseHandler)
    {
        this.profileData = profileData;
        this.onCloseHandler = onCloseHandler;
        gameObject.SetActive(true);

        isAnimating = true;
        animationStartTime = Time.time;

        facebookHolder.GetPicture(player.GetComponent<Image>(), profileData.facebookId);

        StartCoroutine(StartRequesting(1.0f));
    }

    public void Close()
    {
        gameObject.SetActive(false);
        if (onCloseHandler != null)
            onCloseHandler();
    }

    public void OnMatchingSuccess(string matchId, ProfileData opponentData)
    {
        player.gameObject.SetActive(true);
        opponent.gameObject.SetActive(true);
        versusText.gameObject.SetActive(true);
        lookingForText.gameObject.SetActive(false);

        player.gameObject.GetComponentInChildren<Text>().text = UIUtils.GetFormattedString(profileData);
        opponent.gameObject.GetComponentInChildren<Text>().text = UIUtils.GetFormattedString(opponentData);

        facebookHolder.GetPicture(opponent.GetComponent<Image>(), opponentData.facebookId);

        SceneConnector.Instance.PushMatch(matchId, profileData, opponentData);

        StartCoroutine(LoadLevelDeferred());
    }

    public void OnRecoverMatch(string matchId, ProfileData opponentData)
    {
        SceneConnector.Instance.PushMatch(matchId, profileData, opponentData);
        SceneManager.LoadScene("CoreGame");
    }

    public void OnLookingForOpponent()
    {
        StartCoroutine(StartRequesting(0.3f));
    }

    public void OnOpponentNotFound()
    {
        messageDialog.Open(LanguageManager.Instance.GetTextValue("Message.OpponentNotFound"), () => { this.Close(); });
    }

    public void OnMatchingError(int code)
    {
        messageDialog.Open(LanguageManager.Instance.GetTextValue("Message.ServerUnavailable") + " (" + code + ")", () => { this.Close(); });
    }

    private IEnumerator LoadLevelDeferred()
    {
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene("CoreGame");
    }

    private IEnumerator StartRequesting(float delay)
    {
        yield return new WaitForSeconds(delay);
        needRequest = true;
    }
}
