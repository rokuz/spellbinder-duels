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

    public class MatchData
    {
        public string matchId;
        public ProfileData player;
        public ProfileData opponent;
    }

    private MatchData matchData = null;

    public void PushMatch(string matchId, ProfileData player, ProfileData opponent)
    {
        matchData = new MatchData();
        matchData.matchId = matchId;
        matchData.player = player;
        matchData.opponent = opponent;
    }

    public MatchData PopMatch()
    {
        MatchData result = matchData;
        matchData = null;
        return result;
    }
}
