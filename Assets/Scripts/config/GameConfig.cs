using System;

[System.Serializable]
public class GameConfig
{
    public string playerID;
    public string serverAddress;
    public float musicVolume;
    public float gamma;

    public GameConfig()
    {
        this.playerID = "";
        this.serverAddress = "83.220.170.252";
        this.musicVolume = 1.0f;
        this.gamma = 0.5f;
    }
}
