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
    this.spells = (from s in Spellbook.Spells where s.minLevel <= this.level select s.Code).ToArray();
  }
}
