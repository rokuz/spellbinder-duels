using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GamePrefs
{
  private static string SIMPLIFIED_GAMEPLAY_KEY = "simplifiedGameplay";
  private static string WHATS_NEW_103_SHOWN = "whatsNew103";

  private Dictionary<string, string> preferences;

  public GamePrefs()
  {
    preferences = new Dictionary<string, string>();
  }

  public bool IsSimplifiedGameplay()
  {
    return HasBoolKey(SIMPLIFIED_GAMEPLAY_KEY);
  }

  public void SetSimplifiedGameplay(bool enabled)
  {
    SetBoolKey(SIMPLIFIED_GAMEPLAY_KEY, enabled);
  }

  public bool IsWhatsNew103Shown()
  {
    return HasBoolKey(WHATS_NEW_103_SHOWN);
  }

  public void SetIsWhatsNew103Shown(bool enabled)
  {
    SetBoolKey(WHATS_NEW_103_SHOWN, enabled);
  }

  private bool HasBoolKey(string keyName)
  {
    if (!preferences.ContainsKey(keyName))
      return false;
    return Convert.ToBoolean(preferences[keyName]);
  }

  private void SetBoolKey(string keyName, bool enabled)
  {
    if (!preferences.ContainsKey(keyName))
    {
      preferences.Add(keyName, enabled.ToString());
      return;
    }
    preferences[keyName] = enabled.ToString();
  }
}
