using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Match
{
  public enum MatchStatus { PREPARED, STARTED, FINISHED, INTERRUPTED }
  public enum MiscastReason { NO_REASON, NO_ENOUGH_MANA, UNKNOWN_SPELL }

  public class Player
  {
    public ProfileData profile;
    public PlayerData data;
    public bool hasFirstTurn;

    public Player(ProfileData prof)
    {
      this.profile = prof;
      this.data = new PlayerData(prof);
      this.hasFirstTurn = false;
    }
  }

  private GameField gameField;

  private Player user;
  private Player opponent;

  private float timestamp;
  private MatchStatus matchStatus;
  private MiscastReason miscastReason;

  public Match(ProfileData profile1, ProfileData profile2)
  {
    this.user = new Player(profile1);
    this.opponent = new Player(profile2);
    this.timestamp = Time.time;
    this.matchStatus = MatchStatus.PREPARED;
    this.gameField = new GameField();
    this.user.hasFirstTurn = Persistence.gameConfig.tutorialCoreGameShown ? (UnityEngine.Random.Range(0, 2) == 0) : true;
    this.opponent.hasFirstTurn = !this.user.hasFirstTurn;
    this.user.data.mana = this.user.hasFirstTurn ? 0 : 1;
    this.opponent.data.mana = this.opponent.hasFirstTurn ? 0 : 1;
  }

  float Timestamp
  {
    get { return timestamp; }
  }

  public Player User
  {
    get { return user; }
  }

  public Player Opponent
  {
    get { return opponent; }
  }

  public MatchStatus Status
  {
    get { return matchStatus; }
    set { this.matchStatus = value; }
  }

  public GameField Field
  {
    get { return this.gameField; }
  }

  public MiscastReason Reason
  {
    get { return this.miscastReason; }
  }

  public void StartTurn(Player caster)
  {
    caster.data.IncrementMana(1);
    caster.data.usedMana = 0;
  }

  public void FinishTurn(Player caster)
  {
    caster.data.blockedDamageTurns--;
    if (caster.data.blockedDamageTurns < 0)
      caster.data.blockedDamageTurns = 0;

    caster.data.blockedHealingTurns--;
    if (caster.data.blockedHealingTurns < 0)
      caster.data.blockedHealingTurns = 0;

    caster.data.blockedDefenseTurns--;
    if (caster.data.blockedDefenseTurns < 0)
      caster.data.blockedDefenseTurns = 0;

    for (int i = 0; i < 3; i++)
    {
      caster.data.blockedBonusTurns[i]--;
      if (caster.data.blockedBonusTurns[i] < 0)
        caster.data.blockedBonusTurns[i] = 0;

      caster.data.blockedResistanceTurns[i]--;
      if (caster.data.blockedResistanceTurns[i] < 0)
        caster.data.blockedResistanceTurns[i] = 0;
    }
  }

  public Spell CastSpell(int[] indices, Player caster)
  {
    if (indices == null)
      return null;

    Magic[] combination = new Magic[indices.Length];
    for (int i = 0; i < indices.Length; i++)
      combination[i] = gameField.Cards[indices[i]];
    var code = SpellCoder.Encode(combination);
    Spell s = Spellbook.Find(code);
    if (s == null)
    {
      this.miscastReason = MiscastReason.NO_REASON;
      caster.data.UseMana(1);
      return null;
    }
    else if ((from sp in caster.profile.spells where sp == s.Code select sp).Count() == 0)
    {
      this.miscastReason = MiscastReason.UNKNOWN_SPELL;
      caster.data.UseMana(1);
      return null;
    }
    else if (caster.data.RestMana < s.manaCost)
    {
      this.miscastReason = MiscastReason.NO_ENOUGH_MANA;
      caster.data.UseMana(1);
      return null;
    }

    caster.data.UseMana(s.manaCost);
    return s;
  }

  // Returns substitutes.
  public Magic[] ApplySpell(Spell spell, int[] indices, Player caster)
  {
    if (spell == null)
      return null;

    Player receiver = (caster == user) ? opponent : user;

    // Apply positive effects.
    if (spell.clearDamageCurse) caster.data.blockedDamageTurns = 0;
    if (spell.clearHealingCurse) caster.data.blockedHealingTurns = 0;
    if (spell.clearDefenseCurse) caster.data.blockedDefenseTurns = 0;
    for (int i = 0; i < 3; i++)
    {
      if (spell.clearBonusCurse[i]) caster.data.blockedBonusTurns[i] = 0;
      if (spell.clearResistanceCurse[i]) caster.data.blockedResistanceTurns[i] = 0;
    }
    if (caster.data.blockedHealingTurns == 0)
    {
      var newHealth = caster.data.health.Value + spell.healing;
      if (newHealth > Constants.HEALTH_POINTS)
        newHealth = Constants.HEALTH_POINTS;
      caster.data.health.Value = newHealth;
    }
    if (caster.data.blockedDefenseTurns == 0)
      caster.data.defense.Value = caster.data.defense.Value + spell.defense;

    // Apply negative effects.
    if (spell.blockDamageTurns != 0)
      receiver.data.blockedDamageTurns = spell.blockDamageTurns;
    if (spell.blockHealingTurns != 0)
      receiver.data.blockedHealingTurns = spell.blockHealingTurns;
    if (spell.blockDefenseTurns != 0)
      receiver.data.blockedDefenseTurns = spell.blockDefenseTurns;
    for (int i = 0; i < 3; i++)
    {
      if (spell.blockBonusTurns[i] != 0)
        receiver.data.blockedBonusTurns[i] = spell.blockBonusTurns[i];
      if (spell.blockResistanceTurns[i] != 0)
        receiver.data.blockedResistanceTurns[i] = spell.blockResistanceTurns[i];
    }

    if (spell.removeDefense)
      receiver.data.defense.Value = 0;

    if (caster.data.blockedDamageTurns == 0) 
    {
      int elementIndex = spell.Index;
      int d = spell.damage;
      if (elementIndex != -1)
      {
        if (caster.data.blockedBonusTurns[elementIndex] == 0)
          d += (caster.profile.bonuses[elementIndex] / 1000);
      }
      if (spell.considerDefense)
      {
        var newDefense = receiver.data.defense.Value - d;
        if (newDefense < 0)
        {
          d = -newDefense;
          receiver.data.defense.Value = 0;
        }
        else
        {
          d = 0;
          receiver.data.defense.Value = newDefense;
        }
      }
      if (spell.considerResistance && elementIndex != -1)
      {
        if (receiver.data.blockedResistanceTurns[elementIndex] == 0)
        {
          d -= (receiver.profile.resistance[elementIndex] / 1000);
          if (d < 0) d = 0;
        }
      }
      var newHealth = receiver.data.health.Value - d;
      if (newHealth < 0)
        newHealth = 0;
      receiver.data.health.Value = newHealth;
    }

    caster.data.spellCalculator.OnCastSpell(spell);
    receiver.data.spellCalculator.OnReceiveSpell(spell);

    Magic[] substitutes = gameField.SubstituteCards(indices);
    return substitutes;
  }

  public delegate void OnMatchFinished(Player winner);
  public bool CheckMatchStatus(OnMatchFinished onFinishedHandler)
  {
    if (opponent.data.health.Value <= 0)
    {
      this.matchStatus = MatchStatus.FINISHED;
      user.data.experienceCalculator.OnWin();
      opponent.data.experienceCalculator.OnLose();
      ApplyReward();
      if (onFinishedHandler != null)
        onFinishedHandler(user);
      return true;
    }
    else if (user.data.health.Value <= 0)
    {
      this.matchStatus = MatchStatus.FINISHED;
      user.data.experienceCalculator.OnLose();
      opponent.data.experienceCalculator.OnWin();
      ApplyReward();
      if (onFinishedHandler != null)
        onFinishedHandler(opponent);
      return true;
    }
    return false;
  }

  private void ApplyReward()
  {
    this.User.profile.ApplyExperience(user.data.experienceCalculator.Experience);
    if (this.User.data.health.Value > 0)
      this.User.profile.victories++;
    else
      this.User.profile.defeats++;
    
    this.User.profile.ApplyBonusesAndResistance(user.data.spellCalculator.Bonuses,
                                                user.data.spellCalculator.Resistance);
    this.User.profile.ApplyCoins(user.data.experienceCalculator.Coins);

    if (this.Opponent.data.health.Value > 0)
      this.Opponent.profile.victories++;
    else
      this.Opponent.profile.defeats++;
    //this.Opponent.profile.ApplyExperience(opponent.data.experienceCalculator.Experience);
    //this.Opponent.profile.ApplyBonusesAndResistance(opponent.data.spellCalculator.Bonuses,
    //                                                opponent.data.spellCalculator.Resistance);

    Persistence.Save();
  }

  public void Surrender()
  {
    user.data.health.Value = 0;
    CheckMatchStatus(null);
  }
}
