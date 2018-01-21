using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
  public Attribute<int> health = new Attribute<int>(Constants.HEALTH_POINTS);
  public Attribute<int> defense = new Attribute<int>(0);

  public int mana = 0;
  public int usedMana = 0;

  public int blockedDamageTurns = 0;
  public int blockedHealingTurns = 0;
  public int blockedDefenseTurns = 0;
  public int[] blockedBonusTurns = new int[] { 0, 0, 0 };
  public int[] blockedResistanceTurns = new int[] { 0, 0, 0 };

  public SpellCalculator spellCalculator;
  public ExperienceCalculator experienceCalculator;

  public PlayerData(ProfileData profile, bool isPlayer)
  {
    if (!Persistence.gameConfig.tutorialCoreGameShown && !isPlayer)
      health = new Attribute<int>(Constants.HEALTH_POINTS / 2);
    this.spellCalculator = new SpellCalculator(profile.bonuses, profile.resistance);
    this.experienceCalculator = new ExperienceCalculator(profile.experience, profile.level);
  }

  public int RestMana
  {
    get { return this.mana - this.usedMana; }
  }

  public void IncrementMana(int val)
  {
    this.mana += val;
    if (this.mana > Constants.MAX_MANA)
      this.mana = Constants.MAX_MANA;
  }

  public void UseMana(int val)
  {
    this.usedMana += val;
    if (this.usedMana > this.mana)
      this.usedMana = this.mana;
  }

  public void ResetPreviousAttributeValues()
  {
    health.ResetPreviousValue();
    defense.ResetPreviousValue();
  }
}
