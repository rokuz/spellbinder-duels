using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GamePrefs
{
  private static string SIMPLIFIED_GAMEPLAY_KEY = "simplifiedGameplay";

  private Dictionary<string, string> preferences;

  public GamePrefs()
  {
    preferences = new Dictionary<string, string>();
  }

  public bool IsSimplifiedGameplay()
  {
    if (!preferences.ContainsKey(SIMPLIFIED_GAMEPLAY_KEY))
      return false;
    return Convert.ToBoolean(preferences[SIMPLIFIED_GAMEPLAY_KEY]);
  }

  public void SetSimplifiedGameplay(bool enabled)
  {
    if (!preferences.ContainsKey(SIMPLIFIED_GAMEPLAY_KEY))
    {
      preferences.Add(SIMPLIFIED_GAMEPLAY_KEY, enabled.ToString());
      return;
    }
    preferences[SIMPLIFIED_GAMEPLAY_KEY] = enabled.ToString();
  }
}
