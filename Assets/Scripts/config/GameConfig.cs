using System;

[System.Serializable]
public class GameConfig
{
    public string playerID;
    public string serverAddress;

    public GameConfig()
    {
        this.playerID = "";
        this.serverAddress = "127.0.0.1"; //TODO: change to default gameserver
    }
}
