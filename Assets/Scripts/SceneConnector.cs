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

  public Match PopMatch()
  {
    Match result = match;
    match = null;
    return result;
  }
}
