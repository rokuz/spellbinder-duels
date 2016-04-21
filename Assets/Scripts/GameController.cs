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
    const float kShiverDuration = 0.5f;

    public ServerRequest serverRequest;
    public MessageDialog messageDialog;
    public RewardDialog rewardDialog;
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

    public GameObject fireballPrefab;
    public GameObject iceSpearPrefab;
    public GameObject miscastPrefab;

    public Button settingsButton;
    public Button finishTurnButton;
    public Button surrenderButton;

    private Text player1Text;
    private Text player2Text;

    private SceneConnector.MatchData matchData = null;
    private GameData gameData = null;

    private bool needRequest = false;
    private bool cardsShown = false;
    private bool youTurn = false;
    private bool cardsOpened = false;
    private int[] cardIndices = null;

    private int currentMana = 0;
    private int currentOpponentMana = 0;

    private Vector3 playerInfo1Pos;
    private Vector3 playerInfo2Pos;
    private float startShiverTime1 = -1.0f;
    private float startShiverTime2 = -1.0f;

    private PlayerData lastPlayerData;
    private PlayerData lastOpponentData;

    class SwappingInfo
    {
        public bool isBackShowing = true;
        public float swapRequestTime = 0.0f;
        public bool isSwapping = false;
    }

    private SwappingInfo allCardsSwapping = new SwappingInfo();
    private SwappingInfo[] openedCards = new SwappingInfo[kCardsCount];

    public delegate void OnSpellAnimationFinished();
    public class CastedSpell
    {
        public GameObject spellObject;
        public OnSpellAnimationFinished onFinished;
        public float castTime;
        public CastedSpell(GameObject spellObject, OnSpellAnimationFinished onFinished)
        {
            this.spellObject = spellObject;
            this.onFinished = onFinished;
            this.castTime = Time.time;
        }
    }

    private List<CastedSpell> projectileSpells = new List<CastedSpell>();
    private CastedSpell miscastedSpell;

    public void Start()
    {
        #if UNITY_EDITOR
            LanguageManager.Instance.ChangeLanguage("ru");
        #endif

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

        player1Text = (from r in player1Info.GetComponentsInChildren<Text>() where r.gameObject.name == "PlayerText" select r).Single();
        player2Text = (from r in player2Info.GetComponentsInChildren<Text>() where r.gameObject.name == "PlayerText" select r).Single();
        playerInfo1Pos = player1Info.transform.position;
        playerInfo2Pos = player2Info.transform.position;

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

        finishTurnButton.gameObject.SetActive(false);
        surrenderButton.gameObject.SetActive(false);
        finishTurnButton.interactable = false;
        surrenderButton.interactable = false;

        finishTurnButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("Game.FinishTurn");
        surrenderButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("Game.Surrender");
	}
    
    public void Update()
    {
        UpdateShiver(player1Info, playerInfo1Pos, startShiverTime1);
        UpdateShiver(player2Info, playerInfo2Pos, startShiverTime2);
        UpdateCastedSpell();
        UpdateMiscastedSpell();

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
        currentMana = 0;
        currentOpponentMana = 0;
        SetMana(crystals1, gameData.player, currentMana, gameData.maxMana);
        SetMana(crystals2, gameData.opponent, currentOpponentMana, gameData.opponentMaxMana);
        Ping();
    }

    public void OnYouDisconnected(RewardData rewardData)
    {
        rewardDialog.Open(LanguageManager.Instance.GetTextValue("Message.ConnectionLost"), false, () => { this.BackToMainMenu(); });
    }

    public void OnOpponentDisconnected(RewardData rewardData)
    {
        rewardDialog.Open(LanguageManager.Instance.GetTextValue("Message.OpponentDisconnected"), true, () => { this.BackToMainMenu(); });
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

    public void OnWin(PlayerData player, PlayerData opponent, string spell, int spellCost, RewardData reward)
    {
        UpdateManaAfterSpellCast(true, lastPlayerData, spellCost);

        lastPlayerData = player;
        lastOpponentData = opponent;
        AnimateCastedSpell(spell, true, () => 
        { 
            UpdatePlayersStats();
            rewardDialog.Open(LanguageManager.Instance.GetTextValue("Message.Win"), true, () => { this.BackToMainMenu(); });
        });

        string spellName = LanguageManager.Instance.GetTextValue("Spell." + spell);
        if (spellName.Length == 0) spellName = spell;
        StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue("Game.YouCasted") + " " + spellName, 1.0f));
    }

    public void OnLose(PlayerData player, PlayerData opponent, RewardData reward, bool delay)
    {
        StartCoroutine(ProcessLose(player, opponent, delay ? 2.5f : 0.0f));
    }

    private IEnumerator ProcessLose(PlayerData player, PlayerData opponent, float delaySec)
    {
        yield return new WaitForSeconds(delaySec);

        lastPlayerData = player;
        lastOpponentData = opponent;

        rewardDialog.Open(LanguageManager.Instance.GetTextValue("Message.Lose"), false, () => { this.BackToMainMenu(); });
    }

    public void OnSurrender(RewardData reward)
    {
        rewardDialog.Open(LanguageManager.Instance.GetTextValue("Message.YouSurrender"), false, () => { this.BackToMainMenu(); });
    }

    public void OnOpponentSurrender(RewardData reward)
    {
        rewardDialog.Open(LanguageManager.Instance.GetTextValue("Message.OpponentSurrender"), true, () => { this.BackToMainMenu(); });
    }

    public void OnYourTurn(PlayerData player, PlayerData opponent, bool delay)
    {
        StartCoroutine(ProcessYourTurn(player, opponent, delay ? 2.5f : 0.0f));
    }

    private IEnumerator ProcessYourTurn(PlayerData player, PlayerData opponent, float delaySec)
    {
        yield return new WaitForSeconds(delaySec);
        surrenderButton.interactable = true;
        finishTurnButton.interactable = true;

        lastPlayerData = player;
        lastOpponentData = opponent;

        if (!youTurn)
        {
            CloseAllOpenedCards();
            cardsOpened = false;
            cardIndices = null;
            youTurn = true;
            UpdateManaBeforeTurn(player, opponent);
            StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue("Game.YourTurn"), 1.0f));
        }

        UpdatePlayersStats();
        Ping();
    }

    public void OnOpponentTurn(PlayerData player, PlayerData opponent, bool delay)
    {
        StartCoroutine(ProcessOpponentTurn(player, opponent, delay ? 2.5f : 0.0f));
    }

    private IEnumerator ProcessOpponentTurn(PlayerData player, PlayerData opponent, float delaySec)
    {
        yield return new WaitForSeconds(delaySec);
        surrenderButton.interactable = true;
        finishTurnButton.interactable = false;

        lastPlayerData = player;
        lastOpponentData = opponent;

        if (youTurn)
        {
            CloseAllOpenedCards();
            youTurn = false;
            UpdateManaBeforeTurn(player, opponent);
            StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue("Game.OpponentTurn"), 1.0f));
        }

        UpdatePlayersStats();
        Ping();
    }

    public void OnSpellCasted(PlayerData player, PlayerData opponent, string spell, int spellCost, Dictionary<int, Magic> substitutes)
    {
        UpdateManaAfterSpellCast(true, lastPlayerData, spellCost);

        lastPlayerData = player;
        lastOpponentData = opponent;
        AnimateCastedSpell(spell, true, () => { UpdatePlayersStats(); });

        string spellName = LanguageManager.Instance.GetTextValue("Spell." + spell);
        if (spellName.Length == 0) spellName = spell;
        StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue("Game.YouCasted") + " " + spellName, 1.0f));

        SubstituteCards(substitutes);
        StartCoroutine(ProcessCardsAfterTurn());
        StartCoroutine(StartPingRequesting(3 * kAnimationTimeSec));
    }

    public void OnSpellMiscasted(int spellCost)
    {
        MiscastSpell(() => {});
        StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue("Game.YouMiscasted"), 1.0f));

        UpdateManaAfterSpellCast(true, lastPlayerData, spellCost);

        StartCoroutine(ProcessCardsAfterTurn());
        StartCoroutine(StartPingRequesting(3 * kAnimationTimeSec)); 
    }

    public void OnOpponentSpellCasted(int[] openedCards, string spell, int spellCost, Dictionary<int, Magic> substitutes)
    {
        StartCoroutine(OpponentCastsSpell(openedCards, spell, spellCost, substitutes));
    }

    private IEnumerator OpponentCastsSpell(int[] openedCards, string spell, int spellCost, Dictionary<int, Magic> substitutes)
    {
        for (int i = 0; i < openedCards.Length; i++)
            SwapCard(openedCards[i]);

        yield return new WaitForSeconds(kAnimationTimeSec);

        AnimateCastedSpell(spell, false, () => { UpdatePlayersStats(); });
        string spellName = LanguageManager.Instance.GetTextValue("Spell." + spell);
        if (spellName.Length == 0) spellName = spell;
        StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue("Game.OpponentCasted") + " " + spellName, 1.0f));

        UpdateManaAfterSpellCast(false, lastOpponentData, spellCost);

        SubstituteCards(substitutes);
        StartCoroutine(ProcessCardsAfterTurn());
    }

    public void OnOpponentSpellMiscasted(int[] openedCards, string spell, int spellCost)
    {
        StartCoroutine(OpponentMiscastsSpell(openedCards, spell, spellCost));
    }

    private IEnumerator OpponentMiscastsSpell(int[] openedCards, string spell, int spellCost)
    {
        for (int i = 0; i < openedCards.Length; i++)
            SwapCard(openedCards[i]);

        yield return new WaitForSeconds(kAnimationTimeSec);

        MiscastSpell(() => {});
        StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue("Game.OpponentMiscasted"), 1.0f));

        UpdateManaAfterSpellCast(false, lastOpponentData, spellCost);

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
        cardsOpened = false;
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

    private void UpdateManaBeforeTurn(PlayerData player, PlayerData opponent)
    {
        currentMana = player.mana;
        currentOpponentMana = opponent.mana;
        SetMana(crystals1, player, currentMana, gameData.maxMana);
        SetMana(crystals2, opponent, currentOpponentMana, gameData.opponentMaxMana);
    }

    private void UpdateManaAfterSpellCast(bool isPlayer, PlayerData data, int spellCost)
    {
        if (isPlayer) 
        {
            currentMana -= spellCost;
            SetMana(crystals1, data, currentMana, gameData.maxMana);
        }
        else 
        {
            currentOpponentMana -= spellCost;
            SetMana(crystals2, data, currentOpponentMana, gameData.opponentMaxMana);  
        }
    }

    private void SetMana(GameObject crystals, PlayerData data, int curMana, int maxMana)
    {
        int kMaxOverallMana = 5;
        for (int i = 1; i <= kMaxOverallMana; i++)
        {
            Image img = (from r in crystals.GetComponentsInChildren<Image>(true) where r.gameObject.name == ("crystal" + i) select r).Single();
            img.gameObject.SetActive(i <= maxMana);
            if (i <= data.mana)
            {
                if (i <= curMana)
                    img.color = new Color(47.0f / 255.0f, 61.0f / 255.0f, 128.0f / 255.0f);
                else
                    img.color = new Color(21.0f / 255.0f, 27.0f / 255.0f, 57.0f / 255.0f);
            }
        }
    }

    private void CastProjectileSpell(GameObject projectilePrefab, Vector3 targetPos, OnSpellAnimationFinished onFinished)
    {
        GameObject projectileSpellObject = Instantiate(projectilePrefab);
        Vector3 dir = targetPos - projectileSpellObject.transform.position;
        dir.z = 0;
        dir.Normalize();
        projectileSpellObject.transform.forward = dir;
        projectileSpellObject.transform.Rotate(0.0f, 0.0f, 90.0f);
        projectileSpells.Add(new CastedSpell(projectileSpellObject, onFinished));
    }

    private void MiscastSpell(OnSpellAnimationFinished onFinished)
    {
        GameObject spellObject = Instantiate(miscastPrefab);
        miscastedSpell = new CastedSpell(spellObject, onFinished);
    }

    private void ShiverPlayer(bool isPlayer)
    {
        if (isPlayer)
            startShiverTime1 = Time.time;
        else
            startShiverTime2 = Time.time;
    }

    private void UpdateShiver(GameObject playerInfo, Vector3 playerInfoPos, float startShiverTime)
    {
        if (startShiverTime >= 0.0f)
        {
            float delta = Time.time - startShiverTime;
            if (delta >= kShiverDuration)
            {
                playerInfo.transform.position = playerInfoPos;
                startShiverTime = -1.0f;
            }
            else
            {
                playerInfo.transform.position = playerInfoPos + new Vector3(Mathf.Sin(50.0f * Time.time), 0.0f, 0.0f);
            }
        }
    }

    private void UpdateCastedSpell()
    {
        projectileSpells.RemoveAll(s => UpdateProjectileSpell(player1Info, true, s));
        projectileSpells.RemoveAll(s => UpdateProjectileSpell(player2Info, false, s));
    }

    private bool UpdateProjectileSpell(GameObject playerInfo, bool isPlayer, CastedSpell spell)
    {
        if (spell.spellObject != null)
        {
            Bounds b = playerInfo.GetComponent<CircleCollider2D>().bounds;
            b.center = new Vector3(b.center.x, b.center.y, 0.0f);
            Vector3 pos = spell.spellObject.transform.position;
            pos.z = 0.0f;
            if (b.Intersects(new Bounds(pos, Vector3.one)))
            {
                ShiverPlayer(isPlayer);
                Destroy(spell.spellObject);
                if (spell.onFinished != null) spell.onFinished();
                return true;
            }
        }
        else
        {
            if (spell.onFinished != null) spell.onFinished();
        }
        return false;
    }

    private void UpdateMiscastedSpell()
    {
        if (miscastedSpell != null)
        {
            float delta = Time.time - miscastedSpell.castTime;
            if (delta >= 1.0f)
            {
                if (miscastedSpell.spellObject != null)
                    Destroy(miscastedSpell.spellObject);
                if (miscastedSpell.onFinished != null) miscastedSpell.onFinished();
                miscastedSpell = null;
            }
        }
    }

    private void AnimateCastedSpell(string spell, bool toOpponent, OnSpellAnimationFinished onFinished)
    {
        if (spell == "FIREBALL")
        {
            CastProjectileSpell(fireballPrefab, toOpponent ? playerInfo2Pos : playerInfo1Pos, onFinished);
        }
        else if (spell == "ICE_SPEAR")
        {
            CastProjectileSpell(iceSpearPrefab, toOpponent ? playerInfo2Pos : playerInfo1Pos, onFinished);
        }
        else
        {
            if (onFinished != null) onFinished();
        }
    }

    private void UpdatePlayersStats()
    {
        player1Text.text = GetPlayerInfo(matchData.player, lastPlayerData);
        player2Text.text = GetPlayerInfo(matchData.opponent, lastOpponentData);
    }

    public void OnSettingsClicked()
    {
        bool active = finishTurnButton.gameObject.activeSelf;
        finishTurnButton.gameObject.SetActive(!active);
        surrenderButton.gameObject.SetActive(!active);
    }

    public void OnFinishTurnClicked()
    {
        if (matchData != null)
        {
            Dictionary<string, string> p = new Dictionary<string, string>();
            p["id"] = matchData.player.id;
            p["match"] = matchData.matchId;
            if (serverRequest != null)
                serverRequest.Send(GameRequests.FINISH_TURN, p, (WWW response) => { GameRequests.OnFinishTurnResponse(response, this); });
        }
        finishTurnButton.gameObject.SetActive(false);
        surrenderButton.gameObject.SetActive(false);
        finishTurnButton.interactable = false;
    }

    public void OnSurrenderClicked()
    {
        if (matchData != null)
        {
            Dictionary<string, string> p = new Dictionary<string, string>();
            p["id"] = matchData.player.id;
            p["match"] = matchData.matchId;
            if (serverRequest != null)
                serverRequest.Send(GameRequests.SURRENDER, p, (WWW response) => { GameRequests.OnSurrenderResponse(response, this); });
        }
        finishTurnButton.gameObject.SetActive(false);
        surrenderButton.gameObject.SetActive(false);
        surrenderButton.interactable = false;
    }
}
