using System;

[System.Serializable]
public class GameConfig
{
    public string playerID;
    public string serverAddress;

    public GameConfig()
    {
        this.playerID = "";
        this.serverAddress = "83.220.170.252";
    }
}
