using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class Persistence
{
  private const string kConfigName = "/config.dat";
  private const string kPreferencesName = "/prefs.dat";

  public static GameConfig gameConfig = new GameConfig();
  public static GamePrefs preferences = new GamePrefs();

  public static void Save()
  {
    // Config.
    {
      BinaryFormatter bf = new BinaryFormatter();
      FileStream file = File.Create(Application.persistentDataPath + kConfigName);
      bf.Serialize(file, gameConfig);
      file.Close();
    }

    // Preferences.
    {
      BinaryFormatter bf = new BinaryFormatter();
      FileStream file = File.Create(Application.persistentDataPath + kPreferencesName);
      bf.Serialize(file, preferences);
      file.Close();
    }
  }   

  public static void Load()
  {
    if (File.Exists(Application.persistentDataPath + kConfigName))
    {
      BinaryFormatter bf = new BinaryFormatter();
      FileStream file = File.Open(Application.persistentDataPath + kConfigName, FileMode.Open);
      gameConfig = (GameConfig)bf.Deserialize(file);
      file.Close();
    }

    if (File.Exists(Application.persistentDataPath + kPreferencesName))
    {
      BinaryFormatter bf = new BinaryFormatter();
      FileStream file = File.Open(Application.persistentDataPath + kPreferencesName, FileMode.Open);
      preferences = (GamePrefs)bf.Deserialize(file);
      file.Close();
    }
  }

  public static void LoadWithProfilesData(ProfileData user, ProfileData opponent)
  {
    if (File.Exists(Application.persistentDataPath + kConfigName))
    {
      BinaryFormatter bf = new BinaryFormatter();
      FileStream file = File.Open(Application.persistentDataPath + kConfigName, FileMode.Open);
      gameConfig = (GameConfig)bf.Deserialize(file);
      gameConfig.profile = user;
      gameConfig.rivals.RemoveAll(x => x.name == opponent.name && x.facebookId == opponent.facebookId);
      gameConfig.rivals.Add(opponent);
      file.Close();
    }

    if (File.Exists(Application.persistentDataPath + kPreferencesName))
    {
      BinaryFormatter bf = new BinaryFormatter();
      FileStream file = File.Open(Application.persistentDataPath + kPreferencesName, FileMode.Open);
      preferences = (GamePrefs)bf.Deserialize(file);
      file.Close();
    }
  }
}
