using UnityEngine;
using System.Collections;

public class SceneConnector
{
    private static SceneConnector instance;
    private SceneConnector() {}
    public static SceneConnector Instance
    {
        get 
        {
            if (instance == null)
                instance = new SceneConnector();
            return instance;
        }
    }

    public struct MatchData
    {
        public string matchId;
        public ProfileData player;
        public ProfileData opponent;
    }

    private MatchData matchData;

    public void PushMatch(string matchId, ProfileData player, ProfileData opponent)
    {
        matchData.matchId = matchId;
        matchData.player = player;
        matchData.player = opponent;
    }

    public MatchData PopMatch()
    {
        MatchData result = matchData;
        matchData.matchId = null;
        matchData.player = null;
        matchData.player = null;
        return result;
    }
}
