using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
  public int health = Constants.HEALTH_POINTS;
  public int defense = 0;

  public int[] bonuses = new int[] { 0, 0, 0 };
  public int[] resistance = new int[] { 0, 0, 0 };
  private int level = 1;
  private int experience = 0;
  private int mana = 1;
  private int usedMana = 0;

  public bool surrender = false;

  public int blockedDamageTurns = 0;
  public int blockedDamageTurnIndex = 0;

  public int blockedHealingTurns = 0;
  public int blockedHealingTurnIndex = 0;

  public int blockedDefenseTurns = 0;
  public int blockedDefenseTurnIndex = 0;

  public int[] blockedBonusTurns = new int[] { 0, 0, 0 };
  public int[] blockedBonusTurnIndex = new int[] { 0, 0, 0 };

  public int[] blockedResistanceTurns = new int[] { 0, 0, 0 };
  public int[] blockedResistanceTurnIndex = new int[] { 0, 0, 0 };

  public SpellCalculator spellCalculator = null;

  public ExperienceCalculator experienceCalculator = null;

  public PlayerData()
  {
    this.spellCalculator = new SpellCalculator(this.bonuses, this.resistance);
    this.experienceCalculator = new ExperienceCalculator();
  }

  public int Level
  {
    get { return level; }
    set
    {
      this.level = value;
      experienceCalculator.InitialLevel = level;
    }
  }

  public int Experience
  {
    get { return experience; }
    set
    {
      this.experience = value;
      experienceCalculator.InitialExperience = experience;
    }
  }

  public int Mana
  {
    get { return mana; }
    set { this.mana = value; }
  }

  public int UsedMana
  {
    get { return usedMana; }
    set { this.usedMana = value; }
  }

  public void IncrementMana(int val)
  {
    this.mana += val;
    if (this.mana > Constants.MAX_MANA)
      this.mana = Constants.MAX_MANA;
    this.usedMana = 0;
  }
}
