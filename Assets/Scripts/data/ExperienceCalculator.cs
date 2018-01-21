using System;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceCalculator
{
  private int initialExperience = 0;
  private int initialLevel = 1;

  private int experience = 0;
  private int levelsUp = 0;

  private int coins = 0;

  public ExperienceCalculator(int initialExperience, int initialLevel)
  {
    this.initialExperience = initialExperience;
    this.initialLevel = initialLevel;
  }

  public int Experience
  {
    get { return experience; }
  }

  public int LevelsUp
  {
    get { return levelsUp; }
  }

  public int Coins
  {
    get { return coins; }
  }

  public void OnWin(int oppopentLevel)
  {
    float modifier = 1.0f;
    int delta = initialLevel - oppopentLevel;
    if (delta == 2)
      modifier = 0.5f;
    else if (delta == 3)
      modifier = 0.25f;
    else if (delta > 3)
      modifier = 0.0f;

    int exp = (100 * (initialLevel + 1) / 2 + oppopentLevel > initialLevel ? 100 * (oppopentLevel - initialLevel) : 0);
    float e = exp * modifier;
    AddExperience((int)e);
    coins = UnityEngine.Random.Range(Constants.COINS_MIN, Constants.COINS_MAX + 1);
  }

  public void OnLose()
  {
    AddExperience(10);
  }

  public static int GetExperienceToNextLevel(int level)
  {
    if (level <= 0)
      return 0;

    if (level >= Constants.MAX_LEVEL)
      return Constants.LEVEL_EXP[level - 2];

    return Constants.LEVEL_EXP[level - 1];
  }

  public static int GetExperienceProgress(int level, int experience)
  {
    if (level <= 0)
      return 0;
    if (level >= Constants.MAX_LEVEL)
      return 100;

    if (level == 1)
    {
      int e = (int)(100.0f * (float)experience / Constants.LEVEL_EXP[0]);
      if (e > 100) e = 100;
      if (e < 0) e = 0;
      return e;
    }
    int diff = Constants.LEVEL_EXP[level - 1] - Constants.LEVEL_EXP[level - 2];
    int expDiff = experience - Constants.LEVEL_EXP[level - 2];
    int exp = (int)(100.0f * (float)expDiff / diff);
    if (exp > 100) exp = 100;
    if (exp < 0) exp = 0;
    return exp;
  }

  private void AddExperience(int exp)
  {
    experience += exp;
    if (experience != 0)
    {
      int resultExp = initialExperience + experience;
      if (resultExp > Constants.LEVEL_EXP[Constants.MAX_LEVEL - 2])
        resultExp = Constants.LEVEL_EXP[Constants.MAX_LEVEL - 2];

      experience = resultExp - initialExperience;
      if (experience < 0)
        experience = 0;

      int newLevel = FindLevel(resultExp);
      if (newLevel != initialLevel)
      {
        levelsUp += (newLevel - initialLevel);
        initialLevel = newLevel;
      }
    }
  }

  public static int FindLevel(int exp)
  {
    int i = 0;
    for (; i < Constants.LEVEL_EXP.Length; i++)
    {
      if (Constants.LEVEL_EXP[i] > exp) break;
    }
    if (i == Constants.LEVEL_EXP.Length)
      return Constants.MAX_LEVEL;
    return i + 1;
  }
}
