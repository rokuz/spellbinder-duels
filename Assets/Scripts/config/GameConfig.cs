using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameConfig
{
  public ProfileData profile = null;
  public float musicVolume;
  public float sfxVolume;
  public List<ProfileData> rivals = new List<ProfileData>();
  public bool showSpellbookWidget;
  public bool removedAds;
  public bool tutorialMainMenuShown;
  public bool tutorialCoreGameShown;
  public int tutorialMainMenuStep;
  public int tutorialCoreGameStep;

  public GameConfig()
  {
    this.profile = null;
    this.musicVolume = 0.25f;
    this.sfxVolume = 0.5f;
    this.showSpellbookWidget = true;
    this.removedAds = false;
    this.tutorialMainMenuShown = false;
    this.tutorialCoreGameShown = false;
    this.tutorialMainMenuStep = 0;
    this.tutorialCoreGameStep = 0;
  }

  public void InitRivals()
  {
    string[] names = new string[]{ "Corvin", "Rosa", "Melissa", "Christian", "Zed",
                                   "Olivia", "Alatel", "Desmond", "Servin", "Olaf",
                                   "Richard", "Jacob", "Chi", "Bastila", "Anya",
                                   "Li", "Astrid", "Rashid", "Talion", "Merlin" };
    int[] levels = new int[]{ 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 10, 11, 12 };

    for (int i = 0; i < names.Length; i++)
    {
      var rival = new ProfileData(names[i], levels[i], levels[i] > 1 ? Constants.LEVEL_EXP[levels[i] - 2] : 0);
      rivals.Add(rival);

      if (rival.level == 7)
      {
        rival.bonuses[UnityEngine.Random.Range(0, 3)] = 1000;
      }
      else if (rival.level == 8)
      {
        rival.resistance[UnityEngine.Random.Range(0, 3)] = 1000;
      }
      else if (rival.level > 8)
      {
        rival.bonuses[UnityEngine.Random.Range(0, 3)] = 1000;
        rival.resistance[UnityEngine.Random.Range(0, 3)] = 1000;
      }

      if (rival.level > 1)
      {
        rival.victories = rival.level * 4 + UnityEngine.Random.Range(0, 10);
        rival.defeats = rival.level + UnityEngine.Random.Range(0, 10);
        if (rival.level == 12)
        {
          rival.victories = rival.level * 4 + 10;
          rival.defeats = rival.level;
        }
      }
    }
  }

  public string[] GetNewFriends(string[] friends)
  {
    List<string> result = new List<string>();
    foreach (var s in friends)
    {
      if (this.rivals.FindIndex(x => x.facebookId == s) < 0)
        result.Add(s);
    }
    return result.Count > 0 ? result.ToArray() : null;
  }

  public void AddFriends(string[] friends, FBHolder fb)
  {
    for (int i = 0; i < friends.Length; i++)
    {
      int level = profile.level + UnityEngine.Random.Range(-1, 2);
      if (level < 1)
        level = 1;
      var friend = new ProfileData(fb.GetFriendName(friends[i]), level, level > 1 ? Constants.LEVEL_EXP[level - 2] : 0);
      friend.facebookId = friends[i];
      rivals.Add(friend);

      if (friend.level == 7)
      {
        friend.bonuses[UnityEngine.Random.Range(0, 3)] = 1000;
      }
      else if (friend.level == 8)
      {
        friend.resistance[UnityEngine.Random.Range(0, 3)] = 1000;
      }
      else if (friend.level > 8)
      {
        friend.bonuses[UnityEngine.Random.Range(0, 3)] = 1000;
        friend.resistance[UnityEngine.Random.Range(0, 3)] = 1000;
      }
    }
  }
}
