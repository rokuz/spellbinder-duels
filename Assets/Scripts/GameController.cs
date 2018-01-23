using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;
using UnityEngine.Analytics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartLocalization;
using GoogleMobileAds;
using GoogleMobileAds.Api;

public class GameController : MonoBehaviour
{
  const float kAnimationTimeSec = 1.0f;
  const float kShiverDuration = 0.5f;

  public FBHolder facebookHolder;
  public AvatarHolder avatarHolder;
  public DefeatDialog defeatDialog;
  public RewardDialog rewardDialog;
  public GameObject player1Info;
  public GameObject player2Info;
  public GameObject crystals1;
  public GameObject crystals2;
  public Text gameInfo;
  public Text gameInfo2;
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
  public GameObject lightningPrefab;
  public GameObject natureCallPrefab;
  public GameObject blessingPrefab;
  public GameObject bleedingPrefab;
  public GameObject blindnessPrefab;
  public GameObject stoneskinPrefab;
  public GameObject doppelgangerPrefab;
  public GameObject miscastPrefab;
  public GameObject meteoritePrefab;
  public GameObject iceRainPrefab;

  public Button settingsButton;
  public Button finishTurnButton;
  public Button surrenderButton;
  public Button spellbookButton;

  public SpellbookWidget spellbookWidget;

  public TutorialCoreGame tutorialCoreGame;

  public CoreGameAudio audio;

  class PlayerInfo
  {
    public Text name;
    public Text health;
    public Text defense;
    public Text fire;
    public Text water;
    public Text air;
    public Text blockedAttack;
    public Text blockedHealing;
    public Text blockedDefense;
    public GameObject fireObj;
    public GameObject waterObj;
    public GameObject airObj;

    public Animator healthPlus;
    public Animator healthMinus;
    public Animator defenseChanged;

    public Vector2[] positions = new Vector2[3];
    public int freePosition = 0;
  }

  private PlayerInfo player1 = new PlayerInfo();
  private PlayerInfo player2 = new PlayerInfo();

  private Match matchData = null;

  private bool yourTurn = false;
  private bool cardsOpened = false;
  private bool spellInProgress = false;

  private Vector3 playerInfo1Pos;
  private Vector3 playerInfo2Pos;
  private float startShiverTime1 = -1.0f;
  private float startShiverTime2 = -1.0f;

  class SwappingInfo
  {
    public bool isBackShowing = true;
    public float swapRequestTime = 0.0f;
    public bool isSwapping = false;
  }

  private SwappingInfo allCardsSwapping = new SwappingInfo();
  private SwappingInfo[] openedCards = new SwappingInfo[GameField.CARDS_COUNT];
  delegate void OnCardsSwappingFinised();
  private OnCardsSwappingFinised onCardsSwappingFinished;

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

  private Bot bot;

  private BannerView bannerView;

  private List<int> tappedCards = new List<int>();

  private Spell[] allUserSpells3;

  private bool recommendationEnabled = true;

  private float lastCastTime = 0.0f;

  void Start()
  {
    SmartCultureInfo systemLanguage = LanguageManager.Instance.GetDeviceCultureIfSupported();
    if (systemLanguage != null)
      LanguageManager.Instance.ChangeLanguage(systemLanguage);

    matchData = SceneConnector.Instance.GetMatch();

    if (matchData != null)
      Persistence.LoadWithProfilesData(matchData.User.profile, matchData.Opponent.profile);
    else
      Persistence.Load();

    if (!Persistence.gameConfig.removedAds)
    {
    #if UNITY_ANDROID
      string adUnitId = "ca-app-pub-8904882368983998/5568213619";
    #elif UNITY_IPHONE
      string adUnitId = "ca-app-pub-8904882368983998/9567168370";
    #else
      string adUnitId = "unexpected_platform";
    #endif
      bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);
      AdRequest request = new AdRequest.Builder().Build();
      bannerView.LoadAd(request);
    }

    GetComponent<AudioSource>().volume = Persistence.gameConfig.musicVolume;

    Spellbook.Init();

    SpriteRenderer renderer = (from r in cards[0].GetComponentsInChildren<SpriteRenderer>()
                               where r.gameObject.name == "front" select r).Single();
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
    gameInfo2.gameObject.SetActive(false);

    InitPlayerInfo(player1, player1Info);
    InitPlayerInfo(player2, player2Info);
    playerInfo1Pos = player1Info.transform.position;
    playerInfo2Pos = player2Info.transform.position;

    crystals1.SetActive(false);
    crystals2.SetActive(false);

    #if UNITY_EDITOR
    if (matchData == null)
      matchData = new Match(new ProfileData("Player1", 12, 0), new ProfileData("Player2", 12, 0));
    #endif
    if (matchData != null)
    {
      spellbookWidget.Setup(matchData.User.profile);
      if (Persistence.gameConfig.showSpellbookWidget)
        spellbookWidget.Open();
      else
        spellbookWidget.Close();

      bot = new Bot(matchData, matchData.Opponent);

      player1.name.text = matchData.User.profile.name;
      player2.name.text = matchData.Opponent.profile.name;
      UpdatePlayerInfo(matchData.User, player1);
      UpdatePlayerInfo(matchData.Opponent, player2);

      Image logo1 = (from r in player1Info.GetComponentsInChildren<Image>() where r.gameObject.name == "Logo" select r).Single();
      Image logo2 = (from r in player2Info.GetComponentsInChildren<Image>() where r.gameObject.name == "Logo" select r).Single();
      facebookHolder.GetPicture(logo1, matchData.User.profile.facebookId);

      if (matchData.Opponent.profile.facebookId.Length != 0)
      {
        facebookHolder.GetPicture(logo2, matchData.Opponent.profile.facebookId);
      }
      else
      {
        var avatarSprite = avatarHolder.GetAvatar(matchData.Opponent.profile);
        if (avatarSprite != null)
          logo2.sprite = avatarSprite;
      }
    }

    finishTurnButton.gameObject.SetActive(false);
    surrenderButton.gameObject.SetActive(false);
    spellbookButton.gameObject.SetActive(false);
    finishTurnButton.interactable = false;
    surrenderButton.interactable = false;
    spellbookButton.interactable = true;

    finishTurnButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("Game.FinishTurn");
    surrenderButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("Game.Surrender");
    spellbookButton.GetComponentInChildren<Text>().text = LanguageManager.Instance.GetTextValue("Spellbook.Title");

    tutorialCoreGame.InitTutorial();

    StartMatch();
  }

  private void InitPlayerInfo(PlayerInfo info, GameObject obj)
  {
    info.name = (from r in obj.GetComponentsInChildren<Text>() where r.gameObject.name == "PlayerName" select r).Single();
    info.health = (from r in obj.GetComponentsInChildren<Text>() where r.gameObject.name == "HealthText" select r).Single();
    info.defense = (from r in obj.GetComponentsInChildren<Text>() where r.gameObject.name == "DefenseText" select r).Single();
    info.fire = (from r in obj.GetComponentsInChildren<Text>() where r.gameObject.name == "FireText" select r).Single();
    info.water = (from r in obj.GetComponentsInChildren<Text>() where r.gameObject.name == "WaterText" select r).Single();
    info.air = (from r in obj.GetComponentsInChildren<Text>() where r.gameObject.name == "AirText" select r).Single();
    info.fireObj = obj.transform.Find("BgImage").gameObject;
    info.waterObj = obj.transform.Find("BgImage2").gameObject;
    info.airObj = obj.transform.Find("BgImage3").gameObject;
    info.blockedAttack = (from r in obj.GetComponentsInChildren<Text>() where r.gameObject.name == "AttackText" select r).Single();
    info.blockedAttack.gameObject.SetActive(false);
    info.positions[0] = info.blockedAttack.GetComponent<RectTransform>().anchoredPosition;
    info.blockedHealing = (from r in obj.GetComponentsInChildren<Text>() where r.gameObject.name == "HealingText" select r).Single();
    info.blockedHealing.gameObject.SetActive(false);
    info.positions[1] = info.blockedHealing.GetComponent<RectTransform>().anchoredPosition;
    info.blockedDefense = (from r in obj.GetComponentsInChildren<Text>() where r.gameObject.name == "DefText" select r).Single();
    info.blockedDefense.gameObject.SetActive(false);
    info.positions[2] = info.blockedDefense.GetComponent<RectTransform>().anchoredPosition;
    info.healthPlus = (from r in obj.GetComponentsInChildren<Animator>() where r.gameObject.name == "HealthPlus" select r).Single();
    info.healthPlus.gameObject.SetActive(false);
    info.healthMinus = (from r in obj.GetComponentsInChildren<Animator>() where r.gameObject.name == "HealthMinus" select r).Single();
    info.healthMinus.gameObject.SetActive(false);
    info.defenseChanged = (from r in obj.GetComponentsInChildren<Animator>() where r.gameObject.name == "DefenseChanged" select r).Single();
    info.defenseChanged.gameObject.SetActive(false);
  }

  private bool HasBonusResistance(Match.Player player)
  {
    return player.profile.bonuses[Spell.FIRE_INDEX] >= 1000 || player.profile.resistance[Spell.FIRE_INDEX] >= 1000 ||
           player.profile.bonuses[Spell.WATER_INDEX] >= 1000 || player.profile.resistance[Spell.WATER_INDEX] >= 1000 ||
           player.profile.bonuses[Spell.AIR_INDEX] >= 1000 || player.profile.resistance[Spell.AIR_INDEX] >= 1000;
  }

  private void UpdatePlayerInfo(Match.Player player, PlayerInfo info)
  { 
    if (player == null)
      return;

    info.health.text = "" + player.data.health;
    info.defense.text = "" + player.data.defense;

    if (HasBonusResistance(player))
    {
      info.fireObj.SetActive(true);
      info.waterObj.SetActive(true);
      info.airObj.SetActive(true);
      info.fire.text = GetBonus(player, Spell.FIRE_INDEX) + "/" + GetResistance(player, Spell.FIRE_INDEX);
      info.water.text = GetBonus(player, Spell.WATER_INDEX) + "/" + GetResistance(player, Spell.WATER_INDEX);
      info.air.text = GetBonus(player, Spell.AIR_INDEX) + "/" + GetResistance(player, Spell.AIR_INDEX);
    }
    else
    {
      info.fireObj.SetActive(false);
      info.waterObj.SetActive(false);
      info.airObj.SetActive(false);
    }

    info.freePosition = 0;
    if (player.data.blockedDamageTurns > 0)
    {
      string blockedStr = LanguageManager.Instance.GetTextValue("Player.AttackBlocked");
      info.blockedAttack.gameObject.SetActive(true);
      info.blockedAttack.GetComponent<RectTransform>().anchoredPosition = info.positions[info.freePosition++];
      info.blockedAttack.text = blockedStr + (player.data.blockedDamageTurns > 1 ? " (" + player.data.blockedDamageTurns + ")" : "");
    }
    else
    {
      info.blockedAttack.gameObject.SetActive(false);
    }

    if (player.data.blockedHealingTurns > 0)
    {
      string blockedStr = LanguageManager.Instance.GetTextValue("Player.HealingBlocked");
      info.blockedHealing.gameObject.SetActive(true);
      info.blockedHealing.GetComponent<RectTransform>().anchoredPosition = info.positions[info.freePosition++];
      info.blockedHealing.text = blockedStr + (player.data.blockedHealingTurns > 1 ? " (" + player.data.blockedHealingTurns + ")" : "");
    }
    else
    {
      info.blockedHealing.gameObject.SetActive(false);
    }

    if (player.data.blockedDefenseTurns > 0)
    {
      string blockedStr = LanguageManager.Instance.GetTextValue("Player.DefenseBlocked");
      info.blockedDefense.gameObject.SetActive(true);
      info.blockedDefense.GetComponent<RectTransform>().anchoredPosition = info.positions[info.freePosition++];
      info.blockedDefense.text = blockedStr + (player.data.blockedDefenseTurns > 1 ? " (" + player.data.blockedDefenseTurns + ")" : "");
    }
    else
    {
      info.blockedDefense.gameObject.SetActive(false);
    }
  }

  private string GetBonus(Match.Player player, int index)
  {
    if (player.data.blockedBonusTurns[index] > 0) return "<color=red>0</color>";
    return "" + player.profile.bonuses[index] / 1000;
  }

  private string GetResistance(Match.Player player, int index)
  {
    if (player.data.blockedResistanceTurns[index] > 0) return "<color=red>0</color>";
    return "" + player.profile.resistance[index] / 1000;
  }

  public void OnCardTapped(int cardIndex)
  {
    if (matchData == null)
      return;

    if (matchData.Status == Match.MatchStatus.STARTED && yourTurn && matchData.User.data.RestMana != 0 && !cardsOpened)
    {
      if (openedCards[cardIndex].isBackShowing && !openedCards[cardIndex].isSwapping)
      {
        SwapCard(cardIndex);
        tappedCards.Add(cardIndex);
      }

      tutorialCoreGame.DeactivateMarker(cardIndex);

      int[] cards = tappedCards.ToArray();
      if (cards.Length > 3)
        throw new UnityException("Bad logic");

      if (cards.Length == 2)
      {
        if (matchData != null && matchData.Field.Cards[cards[0]] == matchData.Field.Cards[cards[1]])
        {
          cardsOpened = true;
          bot.OnOpponentOpenedCards(cards);
          spellInProgress = true;
          tappedCards.Clear();
          StartCoroutine(CastSpellWithDelay(cards, matchData.User, kAnimationTimeSec));
        }
        else
        {
          if (!matchData.Field.HasSpellWith3Components(allUserSpells3, cards))
          {
            cardsOpened = true;
            bot.OnOpponentOpenedCards(cards);
            spellInProgress = true;
            tappedCards.Clear();
            StartCoroutine(CastSpellWithDelay(cards, matchData.User, kAnimationTimeSec));
          }
        }
      }
      else if (cards.Length == 3)
      {
        cardsOpened = true;
        bot.OnOpponentOpenedCards(cards);
        spellInProgress = true;
        tappedCards.Clear();
        StartCoroutine(CastSpellWithDelay(cards, matchData.User, kAnimationTimeSec));
      }
    }
  }

  private void SwapCard(int index)
  {
    audio.Play(CoreGameAudio.Type.CardFlip);
    SwappingInfo info = openedCards[index];
    if (!info.isSwapping)
    {
      info.swapRequestTime = Time.time;
      info.isBackShowing = !info.isBackShowing;
      cards[index].transform.rotation = Quaternion.AngleAxis(info.isBackShowing ? 180.0f : 0.0f, Vector3.up);
      info.isSwapping = true;
    }
  }

  private IEnumerator CastSpellWithDelay(int[] indices, Match.Player caster, float delay)
  {
    yield return new WaitForSeconds(delay);
    Spell s = this.matchData.CastSpell(indices, caster);
    bool youAreCaster = caster == matchData.User;

    this.lastCastTime = Time.time;

    if (!Persistence.gameConfig.tutorialCoreGameShown)
      tutorialCoreGame.OnSpellCasted(s);

    audio.PlayForSpell(s);

    if (s != null)
    {
      if (youAreCaster)
      {
        var p = new Dictionary<string, object>();
        p.Add("spell", s.SpellType);
        p.Add("caster_level", caster.profile.level);
        Analytics.CustomEvent("Spell_Casted", p);

        this.recommendationEnabled = false;
        tutorialCoreGame.DeactivateMarkers();
      }

      AnimateCastedSpell(s, youAreCaster, () => 
      {
        StartCoroutine(ApplySpellWithDelay(indices, s, caster, 1.0f));
      });
      string spellName = LanguageManager.Instance.GetTextValue("Spell." + s.SpellType.ToString());
      if (spellName == null || spellName.Length == 0) spellName = s.SpellType.ToString();

      StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue(youAreCaster ? "Game.YouCasted" : "Game.OpponentCasted") + " " + spellName, 1.0f));
    }
    else
    {
      if (youAreCaster)
      {
        var p = new Dictionary<string, object>();
        p.Add("caster_level", caster.profile.level);
        Analytics.CustomEvent("Spell_Miscasted", p);

        this.recommendationEnabled = true;
      }
      else
      {
        this.recommendationEnabled = false;
        tutorialCoreGame.DeactivateMarkers();
      }

      // Miscast
      MiscastSpell(() =>
      {
        StartCoroutine(ApplySpellWithDelay(indices, s, caster, 1.0f));
      });
      if (matchData.Reason == Match.MiscastReason.NO_REASON)
        StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue(youAreCaster ? "Game.YouMiscasted" : "Game.OpponentMiscasted"), 1.0f));
      else if (matchData.Reason == Match.MiscastReason.UNKNOWN_SPELL)
        StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue(youAreCaster ? "Game.YouMiscasted.Unknown" : "Game.OpponentMiscasted.Unknown"), 1.0f));
      else if (matchData.Reason == Match.MiscastReason.NO_ENOUGH_MANA)
        StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue(youAreCaster ? "Game.YouMiscasted.NoMana" : "Game.OpponentMiscasted.NoMana"), 1.0f));
    }

    UpdateManaUI();
  }

  private IEnumerator ApplySpellWithDelay(int[] indices, Spell spell, Match.Player caster, float delay)
  {
    audio.OnFinishSpell(spell);
    yield return new WaitForSeconds(delay);

    Magic[] substitutes = matchData.ApplySpell(spell, indices, caster);

    UpdatePlayersStats();

    if (!matchData.CheckMatchStatus(OnWin))
    {
      // Substitute cards.
      if (substitutes != null)
        SubstituteCards(indices, substitutes);

      StartCoroutine(FinishSpell(caster, substitutes != null ? 2.0f : 0.0f));
    }
  }

  private IEnumerator FinishSpell(Match.Player caster, float delay)
  {
    yield return new WaitForSeconds(delay);
    CloseAllOpenedCards();
    yield return new WaitForSeconds(kAnimationTimeSec);
    if (caster.data.RestMana == 0)
    {
      this.recommendationEnabled = false;
      tutorialCoreGame.DeactivateMarkers();

      if (!yourTurn)
        bot.Forget();

      matchData.FinishTurn(caster);
      yourTurn = !yourTurn;
      StartTurn();
    }
    else
    {
      // Try to cast another spell.
      if (!yourTurn)
      {
        var turn = bot.MakeTurn();
        StartCoroutine(MakeOpponentTurn(turn));
      }
      else
      {
        if (this.recommendationEnabled)
        {
          tutorialCoreGame.DeactivateMarkers();
          tutorialCoreGame.ActivateMarkers(bot.RecommendationForPlayer());
        }
      }
    }
    spellInProgress = false;
  }

  public void StartMatch()
  {
    if (matchData == null)
      return;

    matchData.User.profile.matchCounter++;
    matchData.Opponent.profile.matchCounter++;
    Persistence.Save();

    var p = new Dictionary<string, object>();
    p.Add("match_counter", matchData.User.profile.matchCounter);
    Analytics.CustomEvent("Match_Started", p);

    SetupGameField();

    allUserSpells3 = (from s in Spellbook.Spells 
                      where Array.Exists(matchData.User.profile.spells, x => x == s.Code) && s.Combination.Length == 3
                      select s).ToArray();
    
    StartCoroutine(InitialShowCardsRoutine());
  }

  private Text GetGameInfo()
  {
    if (Persistence.gameConfig.removedAds)
    {
      if (gameInfo2.gameObject.activeSelf)
        gameInfo2.gameObject.SetActive(false);
      return gameInfo;
    }
    return gameInfo2;
  }

  private IEnumerator InitialShowCardsRoutine()
  {
    this.audio.Play(CoreGameAudio.Type.Toss);
    yield return new WaitUntil(() => { return this.tutorialCoreGame.EnabledToss(); });

    var gi = GetGameInfo();
    gi.text = LanguageManager.Instance.GetTextValue("Game.Toss");
    gi.gameObject.SetActive(true);
    yield return new WaitForSeconds(2.0f);
    yourTurn = this.matchData.User.hasFirstTurn;
    gi.text = LanguageManager.Instance.GetTextValue(yourTurn ? "Game.YourTurnFirst" : "Game.OpponentTurnFirst");
    yield return new WaitForSeconds(2.0f);

    yield return new WaitUntil(() => { return this.tutorialCoreGame.EnabledShowCards(); });

    gi.gameObject.SetActive(false);
    crystals1.SetActive(true);
    crystals2.SetActive(true);
    UpdateManaUI();

    SwapAllCards();
    yield return new WaitForSeconds(Constants.CARDS_SHOW_TIME);

    yield return new WaitUntil(() => { return this.tutorialCoreGame.EnabledTurnOverAll(); });

    SwapAllCards();
    yield return new WaitForSeconds(kAnimationTimeSec);
    matchData.Status = Match.MatchStatus.STARTED;

    StartTurn();
  }

  private void SwapAllCards()
  {
    if (!allCardsSwapping.isSwapping)
    {
      this.audio.Play(CoreGameAudio.Type.CardFlip);
      allCardsSwapping.swapRequestTime = Time.time;
      allCardsSwapping.isBackShowing = !allCardsSwapping.isBackShowing;
      for (int i = 0; i < cards.Length; i++)
        cards[i].transform.rotation = Quaternion.AngleAxis(allCardsSwapping.isBackShowing ? 180.0f : 0.0f, Vector3.up);
      allCardsSwapping.isSwapping = true;
    }
  }

  private void SetupGameField()
  {
    if (matchData == null)
      return;

    for (int i = 0; i < cards.Length; i++)
    {
      SpriteRenderer renderer = (from r in cards[i].GetComponentsInChildren<SpriteRenderer>()
                                    where r.gameObject.name == "front"
                                    select r).Single();
      if (renderer != null)
      {
        renderer.sprite = GetSprite(matchData.Field.Cards[i]);
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

  private void SetMana(GameObject crystals, int curMana)
  {
    int kMaxOverallMana = 5;
    for (int i = 1; i <= kMaxOverallMana; i++)
    {
      Image img = (from r in crystals.GetComponentsInChildren<Image>(true) where r.gameObject.name == ("crystal" + i) select r).Single();
      img.gameObject.SetActive(true);
      if (i <= curMana)
        img.color = new Color(47.0f / 255.0f, 61.0f / 255.0f, 128.0f / 255.0f);
      else
        img.color = new Color(60.0f / 255.0f, 60.0f / 255.0f, 60.0f / 255.0f);
    }
  }

  private void StartTurn()
  {
    matchData.StartTurn(yourTurn ? matchData.User : matchData.Opponent);
    surrenderButton.interactable = true;
    finishTurnButton.interactable = yourTurn;
    this.lastCastTime = Time.time;

    if (!Persistence.gameConfig.tutorialCoreGameShown)
    {
      Spell[] spells = new Spell[] { Spellbook.Find(Spell.Type.FIREBALL), Spellbook.Find(Spell.Type.LIGHTNING),
                                     Spellbook.Find(Spell.Type.ICE_SPEAR), Spellbook.Find(Spell.Type.DEATH_LOOK) };
      foreach (var s in spells)
      {
        var indices = matchData.Field.FindSpell(s);
        if (indices != null)
        {
          tutorialCoreGame.OnCrystalActivated(indices[0], indices[1], s);
          break;
        }
      }     
    }

    UpdateManaUI();
    StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue(yourTurn ? "Game.YourTurn" : "Game.OpponentTurn"), 1.0f));

    UpdatePlayersStats();

    if (!yourTurn)
    {
      var turn = bot.MakeTurn();
      StartCoroutine(MakeOpponentTurn(turn));
    }
  }

  private IEnumerator MakeOpponentTurn(int[] indices)
  {
    yield return new WaitForSeconds(1.0f); // Opponent thinks.

    if (indices != null)
    {
      for (int i = 0; i < indices.Length; i++)
      {
        SwapCard(indices[i]);
        yield return new WaitForSeconds(kAnimationTimeSec); 
      }
    }
    StartCoroutine(CastSpellWithDelay(indices, matchData.Opponent, 0.0f));
  }

  private void CloseAllOpenedCards()
  {
    onCardsSwappingFinished = () => { cardsOpened = false; };
    bool wasSwapping = false;
    for (int i = 0; i < openedCards.Length; i++)
    {
      if (!openedCards[i].isBackShowing)
      {
        SwapCard(i);
        wasSwapping = true;
      }
    }

    if (!wasSwapping)
      cardsOpened = false;
  }

  private void UpdateManaUI()
  {
    SetMana(crystals1, matchData.User.data.RestMana);
    SetMana(crystals2, matchData.Opponent.data.RestMana);
  }

  private IEnumerator ShowGameInfo(string text, float time)
  {
    var gi = GetGameInfo();
    gi.text = text;
    gi.gameObject.SetActive(true);
    yield return new WaitForSeconds(time);
    gi.gameObject.SetActive(false);
  }

  private void UpdatePlayersStats()
  {
    UpdatePlayerInfo(matchData.User, player1);

    if (Attribute<int>.Delta(matchData.User.data.health) > 0)
      PlayTextAnimation(player1.healthPlus, "HealthPlus", Attribute<int>.Delta(matchData.User.data.health));
    else if (Attribute<int>.Delta(matchData.User.data.health) < 0)
      PlayTextAnimation(player1.healthMinus, "HealthMinus", Attribute<int>.Delta(matchData.User.data.health));

    if (Attribute<int>.Delta(matchData.User.data.defense) != 0)
      PlayTextAnimation(player1.defenseChanged, "DefenseChanged", Attribute<int>.Delta(matchData.User.data.defense));

    matchData.User.data.ResetPreviousAttributeValues();

    UpdatePlayerInfo(matchData.Opponent, player2);

    if (Attribute<int>.Delta(matchData.Opponent.data.health) > 0)
      PlayTextAnimation(player2.healthPlus, "HealthPlus", Attribute<int>.Delta(matchData.Opponent.data.health));
    else if (Attribute<int>.Delta(matchData.Opponent.data.health) < 0)
      PlayTextAnimation(player2.healthMinus, "HealthMinus", Attribute<int>.Delta(matchData.Opponent.data.health));

    if (Attribute<int>.Delta(matchData.Opponent.data.defense) != 0)
      PlayTextAnimation(player2.defenseChanged, "DefenseChanged", Attribute<int>.Delta(matchData.Opponent.data.defense));

    matchData.Opponent.data.ResetPreviousAttributeValues();
  }

  private void PlayTextAnimation(Animator animator, string name, int textValue)
  {
    animator.GetComponent<Text>().text = ((textValue > 0) ? "+" : "") + textValue;
    PlayAnimation(animator, name);
  }

  private void PlayAnimation(Animator animator, string name)
  {
    animator.gameObject.SetActive(true);
    animator.Play(name);
  }

  void Update()
  {
    UpdateShiver(player1Info, playerInfo1Pos, startShiverTime1);
    UpdateShiver(player2Info, playerInfo2Pos, startShiverTime2);
    UpdateCastedSpell();
    UpdateMiscastedSpell();

    AnimateCardsSwapping();

    if (Persistence.gameConfig.tutorialCoreGameShown && matchData.Status == Match.MatchStatus.STARTED && 
        yourTurn && (Time.time - this.lastCastTime) > 10.0f)
    {
      this.lastCastTime = Time.time;
      StartCoroutine(ShowGameInfo(LanguageManager.Instance.GetTextValue("Game.YouHaveMana"), 3.0f));
    }
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

  private void UpdateMiscastedSpell()
  {
    if (miscastedSpell != null)
    {
      float delta = Time.time - miscastedSpell.castTime;
      if (delta >= 1.0f)
      {
        if (miscastedSpell.spellObject != null)
          miscastedSpell.spellObject.SetActive(false);
        if (miscastedSpell.onFinished != null) miscastedSpell.onFinished();
        miscastedSpell = null;
      }
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
        if (onCardsSwappingFinished != null)
        {
          onCardsSwappingFinished();
          onCardsSwappingFinished = null;
        }
      }
    }

    bool checkSwapping = false;
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
          checkSwapping = true;
        }
      }
    }

    if (checkSwapping)
    {
      for (int cardIndex = 0; cardIndex < openedCards.Length; cardIndex++)
      {
        if (openedCards[cardIndex].isSwapping)
          return;
      }
      if (onCardsSwappingFinished != null)
      {
        onCardsSwappingFinished();
        onCardsSwappingFinished = null;
      }
    }
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
        spell.spellObject.SetActive(false);
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

  private void ShiverPlayer(bool isPlayer)
  {
    if (isPlayer)
      startShiverTime1 = Time.time;
    else
      startShiverTime2 = Time.time;
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

  private void CastInstantSpell(GameObject spellPrefab, bool isPlayer, Vector3 targetPos, float zRotation, bool alignDir,
                                float distance, float duration, OnSpellAnimationFinished onFinished)
  {
    GameObject spellObject = Instantiate(spellPrefab);
    if (alignDir)
    {
      Vector3 dir = targetPos - spellObject.transform.position;
      dir.z = 0;
      dir.Normalize();
      spellObject.transform.forward = dir;
      spellObject.transform.Rotate(0.0f, 0.0f, zRotation);
    }
    else
    {
      var q = Quaternion.AngleAxis(zRotation, Vector3.forward);
      spellObject.transform.localRotation = q;
      var dir = q * Vector3.left;
      spellObject.transform.localPosition = targetPos + new Vector3(dir.y, -dir.x, 0.0f) * distance;
    }

    StartCoroutine(CastInstantSpellRoutine(spellObject, isPlayer, duration, onFinished));
  }

  private IEnumerator CastInstantSpellRoutine(GameObject spellObject, bool isPlayer, float duration, OnSpellAnimationFinished onFinished)
  {
    yield return new WaitForSeconds(duration);
    spellObject.SetActive(false);
    ShiverPlayer(!isPlayer);
    if (onFinished != null)
      onFinished();
  }

  private void CastTargetedSpell(GameObject spellPrefab, Vector3 targetPos, float duration, OnSpellAnimationFinished onFinished)
  {
    GameObject spellObject = Instantiate(spellPrefab);
    spellObject.transform.position = targetPos;
    StartCoroutine(CastTargetedSpellRoutine(spellObject, duration, onFinished));
  }

  private IEnumerator CastTargetedSpellRoutine(GameObject spellObject, float duration, OnSpellAnimationFinished onFinished)
  {
    yield return new WaitForSeconds(duration);
    spellObject.SetActive(false);
    if (onFinished != null)
      onFinished();
  }

  private void MiscastSpell(OnSpellAnimationFinished onFinished)
  {
    GameObject spellObject = Instantiate(miscastPrefab);
    miscastedSpell = new CastedSpell(spellObject, onFinished);
  }

  private void AnimateCastedSpell(Spell spell, bool userCasted, OnSpellAnimationFinished onFinished)
  {
    if (spell.SpellType == Spell.Type.FIREBALL)
    {
      CastProjectileSpell(fireballPrefab, userCasted ? playerInfo2Pos : playerInfo1Pos, onFinished);
    }
    else if (spell.SpellType == Spell.Type.ICE_SPEAR)
    {
      CastProjectileSpell(iceSpearPrefab, userCasted ? playerInfo2Pos : playerInfo1Pos, onFinished);
    }
    else if (spell.SpellType == Spell.Type.LIGHTNING || spell.SpellType == Spell.Type.STORM || spell.SpellType == Spell.Type.TORNADO)
    {
      CastInstantSpell(lightningPrefab, userCasted, userCasted ? playerInfo2Pos : playerInfo1Pos,
                       90.0f, true, 0.0f, 0.5f, onFinished);
    }
    else if (spell.SpellType == Spell.Type.NATURE_CALL)
    {
      CastTargetedSpell(natureCallPrefab, userCasted ? playerInfo1Pos : playerInfo2Pos, 2.5f, onFinished);
    }
    else if (spell.SpellType == Spell.Type.WILD_VINE)
    {
      CastTargetedSpell(natureCallPrefab, userCasted ? playerInfo2Pos : playerInfo1Pos, 2.5f, onFinished);
    }
    else if (spell.SpellType == Spell.Type.BLESSING || spell.SpellType == Spell.Type.HYPNOSIS ||
             spell.SpellType == Spell.Type.ASTRAL_PROJECTION)
    {
      Vector3 offset = new Vector3(0.0f, 10.0f, 0.0f);
      CastTargetedSpell(blessingPrefab, userCasted ? (playerInfo1Pos + offset) : (playerInfo2Pos + offset), 2.5f, onFinished);
    }
    else if (spell.SpellType == Spell.Type.BLEEDING || spell.SpellType == Spell.Type.BLOOD_SIGN || spell.SpellType == Spell.Type.VAMPIRE)
    {
      Vector3 offset = new Vector3(0.0f, 10.0f, 0.0f);
      CastTargetedSpell(bleedingPrefab, userCasted ? (playerInfo2Pos + offset) : (playerInfo1Pos + offset), 2.5f, onFinished);
    }
    else if (spell.SpellType == Spell.Type.DEATH_LOOK || spell.SpellType == Spell.Type.POISONING ||
             spell.SpellType == Spell.Type.PHANTOM)
    {
      CastTargetedSpell(blindnessPrefab, userCasted ? playerInfo2Pos : playerInfo1Pos, 2.5f, onFinished);
    }
    else if (spell.SpellType == Spell.Type.STONESKIN || spell.SpellType == Spell.Type.DARKNESS_SHIELD ||
             spell.SpellType == Spell.Type.BURNING_SHIELD)
    {
      CastTargetedSpell(stoneskinPrefab, userCasted ? playerInfo1Pos : playerInfo2Pos, 1.5f, onFinished);
    }
    else if (spell.SpellType == Spell.Type.DOPPELGANGER)
    {
      CastTargetedSpell(doppelgangerPrefab, userCasted ? playerInfo1Pos : playerInfo2Pos, 2.5f, onFinished);
    }
    else if (spell.SpellType == Spell.Type.SOUL_ABRUPTION)
    {
      CastTargetedSpell(doppelgangerPrefab, userCasted ? playerInfo2Pos : playerInfo1Pos, 2.5f, onFinished);
    }
    else if (spell.SpellType == Spell.Type.METEORITE || spell.SpellType == Spell.Type.INFERNO)
    {
      CastInstantSpell(meteoritePrefab, userCasted, userCasted ? playerInfo2Pos : playerInfo1Pos,
                       userCasted ? 80.0f : -80.0f, false, 75.0f, 3.5f, onFinished);
    }
    else if (spell.SpellType == Spell.Type.ICE_RAIN || spell.SpellType == Spell.Type.ICE_FETTERS)
    {
      CastInstantSpell(iceRainPrefab, userCasted, userCasted ? playerInfo2Pos : playerInfo1Pos,
                       userCasted ? 80.0f : -80.0f, false, 75.0f, 3.5f, onFinished);
    }
    else
    {
      if (onFinished != null) onFinished();
    }
  }

  private void SubstituteCards(int[] indices, Magic[] substitutes)
  {
    for(int i = 0; i < indices.Length; i++)
    {
      int index = indices[i];

      SpriteRenderer renderer = (from r in cards[index].GetComponentsInChildren<SpriteRenderer>() where r.gameObject.name == "front_sub" select r).Single();
      if (renderer != null)
        renderer.sprite = GetSprite(substitutes[i]);

      Animator anim = cards[index].GetComponent<Animator>();
      anim.Play("CardSubstitute");
    }

    StartCoroutine(SubstituteCardsRoutine(indices, substitutes));
  }

  private IEnumerator SubstituteCardsRoutine(int[] indices, Magic[] substitutes)
  {
    yield return new WaitForSeconds(1.0f);
    for (int i = 0; i < indices.Length; i++)
    {
      int index = indices[i];

      SpriteRenderer renderer = (from r in cards[index].GetComponentsInChildren<SpriteRenderer>()
                                    where r.gameObject.name == "front"
                                    select r).Single();
      if (renderer != null)
        renderer.sprite = GetSprite(substitutes[i]);

      Animator anim = cards[index].GetComponent<Animator>();
      anim.Play("CardDefault");
    }
  }

  public void OnWin(Match.Player winner)
  {
    if (winner == matchData.User)
    {
      var p = new Dictionary<string, object>();
      p.Add("match_counter", matchData.User.profile.matchCounter);
      Analytics.CustomEvent("Match_Win", p);

      if (!rewardDialog.IsOpened())
      {
        this.audio.Play(CoreGameAudio.Type.Victory);
        rewardDialog.Open(winner, () =>
          {
            this.BackToMainMenu();
          });
      }
    }
    else
    {
      var p = new Dictionary<string, object>();
      p.Add("match_counter", matchData.User.profile.matchCounter);
      Analytics.CustomEvent("Match_Lose", p);

      if (!defeatDialog.IsOpened())
      {
        this.audio.Play(CoreGameAudio.Type.Defeat);
        defeatDialog.Open(LanguageManager.Instance.GetTextValue("Message.Lose"), () =>
          { this.BackToMainMenu(); }, () => { this.Replay(); });
      }
    }
  }

  private void BackToMainMenu()
  {
    StartCoroutine(BackToMainMenuRoutine());
  }

  private IEnumerator BackToMainMenuRoutine()
  {
    StartCoroutine(AudioFadeOut.FadeOut(GetComponent<AudioSource>(), 0.25f));
    yield return new WaitForSeconds(0.5f);
    SceneConnector.Instance.PopMatch();

    if (!Persistence.gameConfig.removedAds)
    {
      ShowOptions options = new ShowOptions();
      options.resultCallback = HandleShowResultMainMenu;
      Advertisement.Show("video", options);
    }
    else
    {
      SceneManager.LoadScene("MainMenu");
    }
  }

  private void Replay()
  {
    Analytics.CustomEvent("Match_Replay");

    SceneConnector.Instance.Replay();
    if (!Persistence.gameConfig.removedAds)
    {
      ShowOptions options = new ShowOptions();
      options.resultCallback = HandleShowResultCore;
      Advertisement.Show("video", options);
    }
    else
    {
      SceneManager.LoadScene("CoreGame");
    }
  }

  private void HandleShowResultCore(ShowResult result)
  {
    SceneManager.LoadScene("CoreGame");
  }

  private void HandleShowResultMainMenu(ShowResult result)
  {
    SceneManager.LoadScene("MainMenu");
  }

  public void OnSettingsClicked()
  {
    this.audio.Play(CoreGameAudio.Type.ButtonDefault);
    bool active = finishTurnButton.gameObject.activeSelf;
    finishTurnButton.gameObject.SetActive(!active);
    surrenderButton.gameObject.SetActive(!active);
    spellbookButton.gameObject.SetActive(!active);
  }

  public void OnFinishTurnClicked()
  {
    this.audio.Play(CoreGameAudio.Type.ButtonDefault);
    if (spellInProgress)
      return;

    finishTurnButton.gameObject.SetActive(false);
    surrenderButton.gameObject.SetActive(false);
    spellbookButton.gameObject.SetActive(false);

    if (yourTurn)
    {
      matchData.User.data.UseMana(matchData.User.data.RestMana);
      StartCoroutine(FinishSpell(matchData.User, 0.0f));
    }
  }

  public void OnSurrenderClicked()
  {
    this.audio.Play(CoreGameAudio.Type.ButtonDefault);
    finishTurnButton.gameObject.SetActive(false);
    surrenderButton.gameObject.SetActive(false);
    spellbookButton.gameObject.SetActive(false);

    if (defeatDialog.IsOpened())
      return;

    Analytics.CustomEvent("Match_Surrender");

    matchData.Surrender();
    defeatDialog.Open(LanguageManager.Instance.GetTextValue("Message.YouSurrender"),
                      () => { this.BackToMainMenu(); }, () => { this.Replay(); });
  }

  public void OnSpellbookClicked()
  {
    this.audio.Play(CoreGameAudio.Type.ButtonDefault);
    finishTurnButton.gameObject.SetActive(false);
    surrenderButton.gameObject.SetActive(false);
    spellbookButton.gameObject.SetActive(false);

    if (spellbookWidget.IsOpened())
      spellbookWidget.Close();
    else
      spellbookWidget.Open();
  }
}
