using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class Persistence {

    private const string kConfigName = "/config.dat";

    public static GameConfig gameConfig = new GameConfig();

    public static void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + kConfigName);
        bf.Serialize(file, gameConfig);
        file.Close();
    }   

    public static void Load()
    {
        if(File.Exists(Application.persistentDataPath + kConfigName)) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + kConfigName, FileMode.Open);
            gameConfig = (GameConfig)bf.Deserialize(file);
            file.Close();
        }
    }
}
