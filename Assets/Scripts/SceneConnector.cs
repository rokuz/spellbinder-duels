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

  private Match match = null;

  public void PushMatch(ProfileData player, ProfileData opponent)
  {
    match = new Match(player, opponent);
  }

  public void Replay()
  {
    if (match == null)
      return;

    ProfileData player = match.User.profile;
    ProfileData opponent = match.Opponent.profile;
    match = new Match(player, opponent);
  }

  public Match GetMatch()
  {
    return match;
  }

  public void PopMatch()
  {
    match = null;
  }
}
