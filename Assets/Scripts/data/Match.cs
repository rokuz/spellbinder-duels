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
      this.data = new PlayerData();
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
    this.user.hasFirstTurn = (UnityEngine.Random.Range(0, 2) == 0);
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

  }

  public Spell CastSpell(int[] indices, Player caster)
  {
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

    //TODO
    Magic[] substitutes = gameField.SubstituteCards(indices);
    return substitutes;
  }
}
