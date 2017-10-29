using System;

[System.Serializable]
public class GameConfig
{
  public ProfileData profile = null;
  public float musicVolume;
  public float sfxVolume;

  public GameConfig()
  {
    this.profile = null;
    this.musicVolume = 1.0f;
    this.sfxVolume = 1.0f;
  }
}
