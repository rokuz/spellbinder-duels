using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text;

public class GameController : MonoBehaviour, IGameRequestsHandler
{
    private const int kCardsCount = 12;

    public ServerRequest serverRequest;
    public MessageDialog messageDialog;
    public Text gameInfo;
    public GameObject[] cards;
    public Sprite fireSprite;
    public Sprite waterSprite;
    public Sprite airSprite;
    public Sprite earthSprite;
    public Sprite natureSprite;
    public Sprite lightSprite;
    public Sprite darknessSprite;
    public Sprite bloodSprite;
    public Sprite illusionSprite;

    private SceneConnector.MatchData matchData = null;
    private GameData gameData = null;

    private bool needRequest = false;
    private bool cardsShown = false;
    private bool youTurn = false;
    private bool cardsOpened = false;
    private int[] cardIndices = null;

    class SwappingInfo
    {
        public bool isBackShowing = true;
        public float swapRequestTime = 0.0f;
        public bool isSwapping = false;
    }

    private SwappingInfo allCardsSwapping = new SwappingInfo();
    private SwappingInfo[] openedCards = new SwappingInfo[kCardsCount];

    public void Start()
    {
        SpriteRenderer renderer = (from r in cards[0].GetComponentsInChildren<SpriteRenderer>()
                                         where r.gameObject.name == "front"
                                         select r).Single();
        Vector3 cardSize = renderer.bounds.size;

        const float kGapSize = 2.0f;
        Vector3 origin = new Vector3(-1.5f * cardSize.x - 1.5f * kGapSize, cardSize.y + kGapSize, 0.0f);
        for (int i = 0; i < 3; i++)
        {
            float offsetY = (i == 0) ? 0.0f : kGapSize * i;
            for (int j = 0; j < 4; j++)
            {
                float offsetX = (j == 0) ? 0.0f : kGapSize * j;
                cards[4 * i + j].transform.position = origin + new Vector3(j * cardSize.x + offsetX, -i * cardSize.y - offsetY, 0.0f);
            }
        }

        for (int i = 0; i < cards.Length; i++)
        {
            CardCollider collider = (from r in cards[i].GetComponentsInChildren<CardCollider>() where r.gameObject.name == "back" select r).Single();
            if (collider != null)
                collider.Setup(i, (int cardIndex) => { this.OnCardTapped(cardIndex); });

            openedCards[i] = new SwappingInfo();
        }

        gameInfo.gameObject.SetActive(false);

        matchData = SceneConnector.Instance.PopMatch();
        if (matchData != null)
        {
            Dictionary<string, string> p = new Dictionary<string, string>();
            p["id"] = matchData.player.id;
            p["match"] = matchData.matchId;
            if (serverRequest != null)
                serverRequest.Send(GameRequests.START_MATCH, p, (WWW response) => { GameRequests.OnStartMatchResponse(response, this); });
        }
	}

    public void Update()
    {
        AnimateCardsSwapping();

        if (needRequest && matchData != null)
        {
            needRequest = false;
            Dictionary<string, string> p = new Dictionary<string, string>();
            p["id"] = matchData.player.id;
            p["match"] = matchData.matchId;
            if (serverRequest != null)
                serverRequest.Send(GameRequests.PING, p, (WWW response) => { GameRequests.OnPingResponse(response, this); });
        }
	}

    public void OnStartMatch(GameData gameData)
    {
        this.gameData = gameData;
        youTurn = !this.gameData.firstTurn;
        SetupGameField();
        Ping();
    }

    public void OnYouDisconnected(RewardData rewardData)
    {
        //TODO: show reward
        messageDialog.Open("The connection was lost", () => { this.BackToMainMenu(); });
    }

    public void OnOpponentDisconnected(RewardData rewardData)
    {
        //TODO: show reward
        messageDialog.Open("Opponent disconnected", () => { this.BackToMainMenu(); });
    }

    public void OnShowCards()
    {
        if (!cardsShown)
        {
            cardsShown = true;
            StartCoroutine(StartMatchRoutine());
        }
        Ping();
    }

    public void OnWaitForStart()
    {
        Ping();
    }

    public void OnWin(PlayerData player, PlayerData opponent, string spell, RewardData reward)
    {
        //TODO
        this.BackToMainMenu();
    }

    public void OnLose(PlayerData player, PlayerData opponent, RewardData reward)
    {
        //TODO
        this.BackToMainMenu();
    }

    public void OnYourTurn(PlayerData player, PlayerData opponent)
    {
        if (!youTurn)
        {
            cardsOpened = false;
            cardIndices = null;
            youTurn = true;
            StartCoroutine(ShowGameInfo("Your turn", 1.0f));
        }
        Ping();
    }

    public void OnOpponentTurn(PlayerData player, PlayerData opponent)
    {
        if (youTurn)
        {
            youTurn = false;
            StartCoroutine(ShowGameInfo("Opponent's turn", 1.0f));
        }
        Ping();
    }

    public void OnSpellCasted(PlayerData player, PlayerData opponent, string spell, Dictionary<int, Magic> substitutes)
    {
        //TODO: show spell animation

        //TEMP
        StartCoroutine(ShowGameInfo("You casted", 1.0f));

        //TODO: show cards substitution

        StartCoroutine(StartPingRequesting(1.5f));
    }

    public void OnSpellMiscasted()
    {
        //TODO: show spell animation

        //TEMP
        StartCoroutine(ShowGameInfo("You miscasted", 1.0f));

        //TODO: close cards

        StartCoroutine(StartPingRequesting(1.5f)); 
    }

    public void OnOpponentSpellCasted(int[] openedCards, string spell, Dictionary<int, Magic> substitutes)
    {

    }

    public void OnOpponentSpellMiscasted(int[] openedCards, string spell)
    {
        
    }

    public void OnError(int code)
    {
        messageDialog.Open("Server is unavailable (" + code + ")", () => { this.BackToMainMenu(); });
    }

    public void OnCardTapped(int cardIndex)
    {
        if (youTurn && !cardsOpened)
        {
            if (openedCards[cardIndex].isBackShowing && !openedCards[cardIndex].isSwapping)
                SwapCard(cardIndex);
            int[] cards = GetOpenCards();
            if (cards.Length == 2)
            {
                if (gameData != null && gameData.gameField[cards[0]] == gameData.gameField[cards[1]])
                    CastSpell(cards);
            }
            else if (cards.Length == 3)
            {
                CastSpell(cards);
            }
        }
    }

    private int[] GetOpenCards()
    {
        List<int> cards = new List<int>();
        for (int i = 0; i < openedCards.Length; i++)
            if (!openedCards[i].isBackShowing) cards.Add(i);
        return cards.ToArray();
    }

    private void CastSpell(int[] indices)
    {
        cardsOpened = true;
        cardIndices = indices;
    }

    private string CardsIndicesToString(int[] indices)
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < indices.Length; i++)
        {
            builder.Append(indices[i]);
            if (i != indices.Length - 1) builder.Append(",");
        }
        return builder.ToString();
    }

    private void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void Ping()
    {
        if (cardIndices != null)
        {
            if (matchData != null)
            {
                Dictionary<string, string> p = new Dictionary<string, string>();
                p["id"] = matchData.player.id;
                p["match"] = matchData.matchId;
                p["cards"] = CardsIndicesToString(cardIndices);
                if (serverRequest != null)
                    serverRequest.Send(GameRequests.TURN, p, (WWW response) => { GameRequests.OnTurnResponse(response, this); });
            }
            cardIndices = null;
        }
        else
        {
            StartCoroutine(StartPingRequesting(0.3f));
        }
    }

    private void SetupGameField()
    {
        if (gameData == null)
            return;

        for (int i = 0; i < cards.Length; i++)
        {
            SpriteRenderer renderer = (from r in cards[i].GetComponentsInChildren<SpriteRenderer>() where r.gameObject.name == "front" select r).Single();
            if (renderer != null)
                renderer.sprite = GetSprite(gameData.gameField[i]);
        }
    }

    private IEnumerator StartMatchRoutine()
    {
        SwapAllCards();
        yield return new WaitForSeconds(this.gameData.showTime);
        SwapAllCards();
    }

    private IEnumerator ShowGameInfo(string text, float time)
    {
        gameInfo.text = text;
        gameInfo.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        gameInfo.gameObject.SetActive(false);
    }

    private IEnumerator StartPingRequesting(float delay)
    {
        yield return new WaitForSeconds(delay);
        needRequest = true;
    }

    private void SwapCard(int index)
    {
        SwappingInfo info = openedCards[index];
        if (!info.isSwapping)
        {
            info.swapRequestTime = Time.time;
            info.isBackShowing = !info.isBackShowing;
            cards[index].transform.rotation = Quaternion.AngleAxis(info.isBackShowing ? 180.0f : 0.0f, Vector3.up);
            info.isSwapping = true;
        }
    }
        
    private void SwapAllCards()
    {
        if (!allCardsSwapping.isSwapping)
        {
            allCardsSwapping.swapRequestTime = Time.time;
            allCardsSwapping.isBackShowing = !allCardsSwapping.isBackShowing;
            for (int i = 0; i < cards.Length; i++)
                cards[i].transform.rotation = Quaternion.AngleAxis(allCardsSwapping.isBackShowing ? 180.0f : 0.0f, Vector3.up);
            allCardsSwapping.isSwapping = true;
        }
    }

    private void AnimateCardsSwapping()
    {
        const float kAnimationTimeSec = 1.0f;
        if (allCardsSwapping.isSwapping)
        {
            float delta = Time.time - allCardsSwapping.swapRequestTime;
            if (delta <= kAnimationTimeSec)
            {
                for (int i = 0; i < cards.Length; i++)
                    cards[i].transform.Rotate(new Vector3(0, 180.0f * Time.deltaTime / kAnimationTimeSec, 0));
            }
            else
            {
                for (int i = 0; i < cards.Length; i++)
                    cards[i].transform.rotation = Quaternion.AngleAxis(allCardsSwapping.isBackShowing ? 0.0f : 180.0f, Vector3.up);
                allCardsSwapping.isSwapping = false;
            }
        }

        for (int cardIndex = 0; cardIndex < openedCards.Length; cardIndex++)
        {
            SwappingInfo info = openedCards[cardIndex];
            if (info.isSwapping)
            {
                float delta = Time.time - info.swapRequestTime;
                if (delta <= kAnimationTimeSec)
                {
                    cards[cardIndex].transform.Rotate(new Vector3(0, 180.0f * Time.deltaTime / kAnimationTimeSec, 0));
                }
                else
                {
                    cards[cardIndex].transform.rotation = Quaternion.AngleAxis(info.isBackShowing ? 0.0f : 180.0f, Vector3.up);
                    info.isSwapping = false;
                }
            }
        }
    }

    private Sprite GetSprite(Magic magic) 
    {
        switch (magic) 
        {
            case Magic.FIRE:
                return this.fireSprite;
            case Magic.WATER:
                return this.waterSprite;
            case Magic.AIR:
                return this.airSprite;
            case Magic.EARTH:
                return this.earthSprite;
            case Magic.NATURE:
                return this.natureSprite;
            case Magic.LIGHT:
                return this.lightSprite;
            case Magic.DARKNESS:
                return this.darknessSprite;
            case Magic.BLOOD:
                return this.bloodSprite;
            case Magic.ILLUSION:
                return this.illusionSprite;
        }
        return null;
    }
}
