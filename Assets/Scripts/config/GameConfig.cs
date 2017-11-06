using System;
using System.Collections.Generic;

[System.Serializable]
public class GameConfig
{
  public ProfileData profile = null;
  public float musicVolume;
  public float sfxVolume;
  public List<ProfileData> rivals = new List<ProfileData>();

  public GameConfig()
  {
    this.profile = null;
    this.musicVolume = 1.0f;
    this.sfxVolume = 1.0f;

    string[] names = new string[]{ "Name1", "Name2", "Name3", "Name4", "Name5",
                                   "Name6", "Name7", "Name8", "Name9", "Name10",
                                   "Name11", "Name12", "Name13", "Name14", "Name15",
                                   "Name16", "Name17", "Name18", "Name19", "Name20" };
    for (int i = 0; i < names.Length; i++)
    {
      var rival = new ProfileData();
      rival.name = names[i];
      rivals.Add(rival);
    }
  }
}
