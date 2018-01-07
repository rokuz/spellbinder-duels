using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ProfileData
{
  public string name;
  public string facebookId;
  public int level;
  public int experience;
  public int experienceToNextLevel;
  public int experienceProgress;
  public int[] bonuses;
  public int[] resistance;
  public int coins;
  public int victories;
  public int defeats;
  public string[] spells;

  public ProfileData()
  {
    this.name = "";
    this.facebookId = "";
    this.level = 1;
    this.experience = 0;
    this.experienceToNextLevel = ExperienceCalculator.GetExperienceToNextLevel(this.level);
    this.experienceProgress = ExperienceCalculator.GetExperienceProgress(this.level, this.experience);
    this.bonuses = new int[] { 0, 0, 0 };
    this.resistance = new int[] { 0, 0, 0 };
    this.coins = 0;
    this.victories = 0;
    this.defeats = 0;
    this.spells = (from s in Spellbook.Spells where s.minLevel <= this.level select s.Code).ToArray();
  }

  public ProfileData(string name, int level, int experience)
  {
    this.name = name;
    this.facebookId = "";
    this.level = level;
    this.experience = experience;
    this.experienceToNextLevel = ExperienceCalculator.GetExperienceToNextLevel(this.level);
    this.experienceProgress = ExperienceCalculator.GetExperienceProgress(this.level, this.experience);
    this.bonuses = new int[] { 0, 0, 0 };
    this.resistance = new int[] { 0, 0, 0 };
    this.coins = 0;
    this.victories = 0;
    this.defeats = 0;
    this.spells = (from s in Spellbook.Spells where s.minLevel <= this.level select s.Code).ToArray();
  }

  public void ApplyExperience(int experience)
  {
    this.experience += experience;
    if (this.experience > Constants.LEVEL_EXP[Constants.MAX_LEVEL - 2])
      this.experience = Constants.LEVEL_EXP[Constants.MAX_LEVEL - 2];

    this.level = ExperienceCalculator.FindLevel(this.experience);
    this.experienceToNextLevel = ExperienceCalculator.GetExperienceToNextLevel(this.level);
    this.experienceProgress = ExperienceCalculator.GetExperienceProgress(this.level, this.experience);
  }

  public void ApplyBonusesAndResistance(int[] bonuses, int[] resistance)
  {
    for (int i = 0; i < this.bonuses.Length; i++)
      this.bonuses[i] += bonuses[i];

    for (int i = 0; i < this.resistance.Length; i++)
      this.resistance[i] += resistance[i];
  }

  public void ApplyCoins(int coins)
  {
    this.coins += coins;
  }
}
