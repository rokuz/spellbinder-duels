using System;
using System.Collections.Generic;
using UnityEngine;

public class Spell 
{
  public static int FIRE_INDEX = 0;
  public static int WATER_INDEX = 1;
  public static int AIR_INDEX = 2;

  public enum Type
  {
    NO_SPELL,
    FIREBALL, ICE_SPEAR, LIGHTNING, DEATH_LOOK, BLEEDING, STONESKIN, DOPPELGANGER, NATURE_CALL, BLESSING,
    METEORITE, ICE_RAIN, STORM, DARKNESS_SHIELD, BLOOD_SIGN, POISONING
  }

  private String code;

  // Combination of cards to cast a spell
  private Magic[] combination;

  // Unique type of spell
  private Type type;

  // Minimum player level
  public int minLevel = 1;

  // Mana cost
  public int manaCost = 1;

  // Damage points
  public int damage = 0;

  // Defense points
  public int defense = 0;

  // Healing points
  public int healing = 0;

  // Number of turns in which opponent can not put damage
  public int blockDamageTurns = 0;

  // Number of turns in which opponent can not heal
  public int blockHealingTurns = 0;

  // Number of turns in which opponent can not put defense
  public int blockDefenseTurns = 0;

  // Number of turns in which opponent's damage bonuses are blocked
  public int[] blockBonusTurns = new int[] { 0, 0, 0 };

  // Number of turns in which opponent's resistance bonuses are blocked
  public int[] blockResistanceTurns = new int[] { 0, 0, 0 };

  // If a spell consider opponent's resistance
  public bool considerResistance = true;

    // If a spell consider opponent's defense
  public bool considerDefense = true;

    // If a spell remove opponent's defense
  public bool removeDefense = false;

    // If a spell clear player's damage curse
  public bool clearDamageCurse = false;

    // If a spell clear player's healing curse
  public bool clearHealingCurse = false;

    // If a spell clear player's defense curse
  public bool clearDefenseCurse = false;

    // If a spell clear player's bonus curse
  public bool[] clearBonusCurse = new bool[] { false, false, false };

    // If a spell clear player's resistance curse
  public bool[] clearResistanceCurse = new bool[] { false, false, false };

  public Spell() {}

  public Spell(Magic[] combination, Type type)
  {
    this.combination = combination;
    this.code = SpellCoder.Encode(this.combination);
    this.type = type;
  }

  public Magic[] Combination
  {
    get { return combination; }
  }

  public Type SpellType
  {
    get { return type; }
  }

  public String Code
  {
    get { return code; }
  }

  public bool Unique
  {
    get { return this.combination.Length == 3 && this.combination[0] != this.combination[2]; }
  }

  public bool Complex
  {
    get { return !Unique && this.combination.Length == 3; }
  }

  public int Index
  {
    get
    {
      if (Unique)
        return -1;
      if (this.combination[0] == Magic.FIRE)
        return FIRE_INDEX;
      if (this.combination[0] == Magic.WATER)
        return WATER_INDEX;
      if (this.combination[0] == Magic.AIR)
        return AIR_INDEX;
      return -1;
    }
  }
}
