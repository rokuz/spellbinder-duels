using System;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceCalculator
{
  private int initialExperience = 0;
  private int initialLevel = 1;

  private int experience = 0;
  private int levelsUp = 0;

  public ExperienceCalculator(){}

  public int Experience
  {
    get { return experience; }
  }

  public int LevelsUp
  {
    get { return levelsUp; }
  }

  public int InitialExperience
  {
    set { this.initialExperience = value; }
  }

  public int InitialLevel
  {
    set { this.initialLevel = value; }
  }

  public void onWin()
  {
    AddExperience(100);
  }

  public void onLose()
  {
    AddExperience(25);
  }
  
  public void onSurrender()
  {
    AddExperience(15);
  }

  public static int GetExperienceToNextLevel(int level)
  {
    if (level <= 0 || level >= Constants.MAX_LEVEL)
      return -1;
    return Constants.LEVEL_EXP[level - 1];
  }

  public static int GetExperienceProgress(int level, int experience)
  {
    if (level <= 0 || level >= Constants.MAX_LEVEL)
      return 0;
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
      int newLevel = FindLevel(resultExp);
      if (newLevel != initialLevel)
      {
        levelsUp += (newLevel - initialLevel);
        initialLevel = newLevel;
      }
    }
  }

  private int FindLevel(int exp)
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
