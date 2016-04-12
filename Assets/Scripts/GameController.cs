using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SmartLocalization;

public class GameController : MonoBehaviour, IGameRequestsHandler
{
    private const int kCardsCount = 12;
    const float kAnimationTimeSec = 1.0f;

    public ServerRequest serverRequest;
    public MessageDialog messageDialog;
    public GameObject player1Info;
    public GameObject player2Info;
    public GameObject crystals1;
    public GameObject crystals2;
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

    private Text player1Text;
    private Text player2Text;

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
        //TEMP
        LanguageManager.Instance.ChangeLanguage("ru");

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

        player1Text = player1Info.GetComponentInChildren<Text>();
        player2Text = player2Info.GetComponentInChildren<Text>();

        crystals1.SetActive(false);
        crystals2.SetActive(false);

        matchData = SceneConnector.Instance.PopMatch();
        if (matchData != null)
        {
            player1Text.text = GetPlayerInfo(matchData.player, null);
            player2Text.text = GetPlayerInfo(matchData.opponent, null);

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
        crystals1.SetActive(true);
        crystals2.SetActive(true);
        SetMana(crystals1, gameData.player, gameData.maxMana);
        SetMana(crystals2, gameData.opponent, gameData.opponentMaxMana);
        Ping();
    }

    public void OnYouDisconnected(RewardData rewardData)
    {
        //TODO: show reward
        messageDialog.Open(LanguageManager.Instance.GetTextValue("Message.ConnectionLost"), () => { this.BackToMainMenu(); });
    }

    public void OnOpponentDisconnected(RewardData rewardData)
    {
        //TODO: show reward
        messageDialog.Open(LanguageManager.Instance.GetTextValue("Message.OpponentDisconnected"), () => { this.BackToMainMenu(); });
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

    public void OnLose(PlayerData player, PlayerData opponent, RewardData reward, bool delay)
    {
        //TODO
        this.BackToMainMenu();
    }

    public void OnYourTurn(PlayerData player, PlayerData opponent, bool delay)
    {
        StartCoroutine(ProcessYourTurn(player, opponent, delay ? 2.5f : 0.0f));
    }

    private IEnumerator ProcessYourTurn(PlayerData player, PlayerData opponent, float delaySec)
    {
        yield return new WaitForSeconds(delaySec);
        if (!youTurn)
        {
            CloseAllOpenedCards();
            cardsOpened = false;
            cardIndices = null;
            youTurn = true;
            StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue("Game.YourTurn"), 1.0f));
        }

        player1Text.text = GetPlayerInfo(matchData.player, player);
        player2Text.text = GetPlayerInfo(matchData.opponent, opponent);
        UpdateMana(player, opponent);

        Ping();
    }

    public void OnOpponentTurn(PlayerData player, PlayerData opponent, bool delay)
    {
        StartCoroutine(ProcessOpponentTurn(player, opponent, delay ? 2.5f : 0.0f));
    }

    private IEnumerator ProcessOpponentTurn(PlayerData player, PlayerData opponent, float delaySec)
    {
        yield return new WaitForSeconds(delaySec);
        if (youTurn)
        {
            CloseAllOpenedCards();
            youTurn = false;
            StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue("Game.OpponentTurn"), 1.0f));
        }

        player1Text.text = GetPlayerInfo(matchData.player, player);
        player2Text.text = GetPlayerInfo(matchData.opponent, opponent);
        UpdateMana(player, opponent);

        Ping();
    }

    public void OnSpellCasted(PlayerData player, PlayerData opponent, string spell, Dictionary<int, Magic> substitutes)
    {
        //TODO: show spell animation
        //TEMP
        string spellName = LanguageManager.Instance.GetTextValue("Spell." + spell);
        if (spellName.Length == 0) spellName = spell;
        StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue("Game.YouCasted") + " " + spellName, 1.0f));

        player1Text.text = GetPlayerInfo(matchData.player, player);
        player2Text.text = GetPlayerInfo(matchData.opponent, opponent);
        UpdateMana(player, opponent);

        SubstituteCards(substitutes);
        StartCoroutine(ProcessCardsAfterTurn());
        StartCoroutine(StartPingRequesting(3 * kAnimationTimeSec));
    }

    public void OnSpellMiscasted()
    {
        //TODO: show spell animation
        //TEMP
        StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue("Game.YouMiscasted"), 1.0f));

        StartCoroutine(ProcessCardsAfterTurn());
        StartCoroutine(StartPingRequesting(3 * kAnimationTimeSec)); 
    }

    public void OnOpponentSpellCasted(int[] openedCards, string spell, Dictionary<int, Magic> substitutes)
    {
        StartCoroutine(OpponentCastsSpell(openedCards, spell, substitutes));
    }

    private IEnumerator OpponentCastsSpell(int[] openedCards, string spell, Dictionary<int, Magic> substitutes)
    {
        for (int i = 0; i < openedCards.Length; i++)
            SwapCard(openedCards[i]);

        yield return new WaitForSeconds(kAnimationTimeSec);

        //TODO: show spell animation
        //TEMP
        string spellName = LanguageManager.Instance.GetTextValue("Spell." + spell);
        if (spellName.Length == 0) spellName = spell;
        StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue("Game.OpponentCasted") + " " + spellName, 1.0f));

        SubstituteCards(substitutes);
        StartCoroutine(ProcessCardsAfterTurn());
    }

    public void OnOpponentSpellMiscasted(int[] openedCards, string spell)
    {
        StartCoroutine(OpponentMiscastsSpell(openedCards, spell));
    }

    private IEnumerator OpponentMiscastsSpell(int[] openedCards, string spell)
    {
        for (int i = 0; i < openedCards.Length; i++)
            SwapCard(openedCards[i]);

        yield return new WaitForSeconds(kAnimationTimeSec);

        //TODO: show spell animation
        //TEMP
        StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue("Game.OpponentMiscasted"), 1.0f));

        StartCoroutine(ProcessCardsAfterTurn());  
    }

    public void OnError(int code)
    {
        messageDialog.Open(LanguageManager.Instance.GetTextValue("Message.ServerUnavailable") + " (" + code + ")", () => { this.BackToMainMenu(); });
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
        StartCoroutine(CastSpellWithDelay(indices, kAnimationTimeSec));
    }

    private IEnumerator CastSpellWithDelay(int[] indices, float delay)
    {
        yield return new WaitForSeconds(delay);
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

    private void SubstituteCards(Dictionary<int, Magic> substitutes)
    {
        if (gameData == null)
            return;

        //TODO: substitute with animation
        foreach(KeyValuePair<int, Magic> entry in substitutes)
        {
            gameData.gameField[entry.Key] = entry.Value;
            SpriteRenderer renderer = (from r in cards[entry.Key].GetComponentsInChildren<SpriteRenderer>() where r.gameObject.name == "front" select r).Single();
            if (renderer != null)
                renderer.sprite = GetSprite(entry.Value);
        }
    }

    private IEnumerator ProcessCardsAfterTurn()
    {
        for (int i = 0; i < openedCards.Length; i++)
        {
            if (openedCards[i].isBackShowing)
                SwapCard(i);
        }
        yield return new WaitForSeconds(2 * kAnimationTimeSec);
        CloseAllOpenedCards();
    }

    private void CloseAllOpenedCards()
    {
        for (int i = 0; i < openedCards.Length; i++)
        {
            if (!openedCards[i].isBackShowing)
                SwapCard(i);
        }
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

    private string GetPlayerInfo(ProfileData profile, PlayerData player)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(profile.name);
        if (player != null)
        {
            builder.Append("\nHealth: ");
            builder.Append(player.health);
            builder.Append("\nDefense: ");
            builder.Append(player.defence);

            if (GetBonus(profile, ProfileData.kFireIndex) > 0)
            {
                builder.Append("\nFire damage: ");
                builder.Append(player.blockedBonusTurns[ProfileData.kFireIndex] == 0 ? ("+" + GetBonus(profile, ProfileData.kFireIndex)) : "0");
            }
            if (GetBonus(profile, ProfileData.kWaterIndex) > 0)
            {
                builder.Append("\nWater damage: ");
                builder.Append(player.blockedBonusTurns[ProfileData.kWaterIndex] == 0 ? ("+" + GetBonus(profile, ProfileData.kWaterIndex)) : "0");
            }
            if (GetBonus(profile, ProfileData.kAirIndex) > 0)
            {
                builder.Append("\nAir damage: ");
                builder.Append(player.blockedBonusTurns[ProfileData.kAirIndex] == 0 ? ("+" + GetBonus(profile, ProfileData.kAirIndex)) : "0");
            }

            if (GetResistance(profile, ProfileData.kFireIndex) > 0)
            {
                builder.Append("\nFire resistance: ");
                builder.Append(player.blockedResistanceTurns[ProfileData.kFireIndex] == 0 ? GetResistance(profile, ProfileData.kFireIndex) : 0);
            }
            if (GetResistance(profile, ProfileData.kWaterIndex) > 0)
            {
                builder.Append("\nWater resistance: ");
                builder.Append(player.blockedResistanceTurns[ProfileData.kWaterIndex] == 0 ? GetResistance(profile, ProfileData.kWaterIndex) : 0);
            }
            if (GetResistance(profile, ProfileData.kAirIndex) > 0)
            {
                builder.Append("\nAir resistance: ");
                builder.Append(player.blockedResistanceTurns[ProfileData.kAirIndex] == 0 ? GetResistance(profile, ProfileData.kAirIndex) : 0);
            }

            if (player.blockedDamageTurns > 0)
                builder.Append("\nDamage blocked for " + player.blockedDamageTurns + " turns");
            if (player.blockedHealingTurns > 0)
                builder.Append("\nHealing blocked for " + player.blockedHealingTurns + " turns");
            if (player.blockedDefenseTurns > 0)
                builder.Append("\nDefense blocked for " + player.blockedDefenseTurns + " turns");

            if (player.blockedBonusTurns[ProfileData.kFireIndex] > 0)
                builder.Append("\nFire damage bonus blocked for " + player.blockedBonusTurns[ProfileData.kFireIndex] + " turns");
            if (player.blockedBonusTurns[ProfileData.kWaterIndex] > 0)
                builder.Append("\nWater damage bonus blocked for " + player.blockedBonusTurns[ProfileData.kWaterIndex] + " turns");
            if (player.blockedBonusTurns[ProfileData.kAirIndex] > 0)
                builder.Append("\nAir damage bonus blocked for " + player.blockedBonusTurns[ProfileData.kAirIndex] + " turns");

            if (player.blockedResistanceTurns[ProfileData.kFireIndex] > 0)
                builder.Append("\nFire resistance blocked for " + player.blockedResistanceTurns[ProfileData.kFireIndex] + " turns");
            if (player.blockedResistanceTurns[ProfileData.kWaterIndex] > 0)
                builder.Append("\nWater resistance blocked for " + player.blockedResistanceTurns[ProfileData.kWaterIndex] + " turns");
            if (player.blockedResistanceTurns[ProfileData.kAirIndex] > 0)
                builder.Append("\nAir resistance blocked for " + player.blockedResistanceTurns[ProfileData.kAirIndex] + " turns");
        }
        return builder.ToString();
    }

    private int GetBonus(ProfileData profile, int index)
    {
        return profile.bonuses[index] / 1000;
    }

    private int GetResistance(ProfileData profile, int index)
    {
        return profile.resistance[index] / 1000;
    }

    private void UpdateMana(PlayerData player, PlayerData opponent)
    {
        SetMana(crystals1, player, gameData.maxMana);
        SetMana(crystals2, opponent, gameData.opponentMaxMana);
    }

    private void SetMana(GameObject crystals, PlayerData data, int maxMana)
    {
        int kMaxOverallMana = 5;
        for (int i = 1; i <= kMaxOverallMana; i++)
        {
            Image img = (from r in crystals.GetComponentsInChildren<Image>(true) where r.gameObject.name == ("crystal" + i) select r).Single();
            img.gameObject.SetActive(i <= maxMana);
            if (i <= data.mana)
            {
                img.color = new Color(47.0f / 255.0f, 61.0f / 255.0f, 128.0f / 255.0f);
            }
        }
    }
}
