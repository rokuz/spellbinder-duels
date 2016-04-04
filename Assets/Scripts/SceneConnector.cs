using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	private List<SpellData> spells;

	public List<SpellData> Spells
	{
		get { return spells; }
		set { spells = value; }
	}

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
