using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GamePrefs
{
  private static string SIMPLIFIED_GAMEPLAY_KEY = "simplifiedGameplay";
  private static string WHATS_NEW_103_SHOWN = "whatsNew103";
  private static string MALE_KEY = "male";
  private static string PRIVACY_POLICY_KEY = "privacyPolicy";

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

  public bool IsMaleKeyExists()
  {
    return preferences.ContainsKey(MALE_KEY);
  }

  public bool IsMale()
  {
    return HasBoolKey(MALE_KEY);
  }

  public void SetMale(bool male)
  {
    SetBoolKey(MALE_KEY, male);
  }

  public bool IsUsedGiftcode(string code)
  {
    return HasBoolKey(code);
  }

  public void SetUsedGiftcode(string code)
  {
    SetBoolKey(code, true);
  }

  public bool IsPrivacyPolicyAccepted()
  {
    return HasBoolKey(PRIVACY_POLICY_KEY);
  }

  public void SetPrivacyPolicyAccepted(bool accepted)
  {
    SetBoolKey(PRIVACY_POLICY_KEY, accepted);
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
