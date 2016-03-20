using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MatchingDialog : MonoBehaviour, IMatchingRequestsHandler
{
    public ServerRequest serverRequest;
    public Text lookingForText;
    public MessageDialog messageDialog;

    public delegate void OnClose();
    private OnClose onCloseHandler;

    private ProfileData profileData;
    private ProfileData opponentData;

    private bool isAnimating = false;
    private string[] animatedText = new string[] { ".", "..", "..." };
    private int textAnimationIndex = 0;
    private float animationStartTime;

    private bool needRequest = false;

    public void Start()
    {
        gameObject.SetActive(false);
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
                lookingForText.text = "Looking for" + animatedText[textAnimationIndex];
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

    public void Open(ProfileData profileData, OnClose onCloseHandler)
    {
        this.profileData = profileData;
        this.onCloseHandler = onCloseHandler;
        gameObject.SetActive(true);

        isAnimating = true;
        animationStartTime = Time.time;

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
        //TODO
    }

    public void OnLookingForOpponent()
    {
        StartCoroutine(StartRequesting(0.3f));
    }

    public void OnOpponentNotFound()
    {
        messageDialog.Open("Opponent is not found", () => { this.Close(); });
    }

    public void OnMatchingError(int code)
    {
        messageDialog.Open("Server is unavailable (" + code + ")", () => { this.Close(); });
    }

    private IEnumerator StartRequesting(float delay)
    {
        yield return new WaitForSeconds(delay);
        needRequest = true;
    }
}
