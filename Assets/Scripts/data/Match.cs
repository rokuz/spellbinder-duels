using System;
using System.Collections.Generic;
using UnityEngine;

public class Match
{
  public enum MatchStatus { PREPARED, STARTED, FINISHED, INTERRUPTED }

  private ProfileData player1;

  private ProfileData player2;

  private float timestamp;

  private MatchStatus matchStatus;
  
  private int winnerIndex;

  public Match(ProfileData player1, ProfileData player2)
  {
    this.player1 = player1;
    this.player2 = player2;
    this.timestamp = Time.time;
    this.matchStatus = MatchStatus.PREPARED;
    this.winnerIndex = -1;
  }

  float Timestamp
  {
    get { return timestamp; }
  }

  public ProfileData Player1
  {
    get { return player1; }
  }

  public ProfileData Player2
  {
    get { return player2; }
  }

  public MatchStatus Status
  {
    get { return matchStatus; }
    set { this.matchStatus = value; }
  }

  public int WinnerIndex
  {
    get { return winnerIndex; }
    set { this.winnerIndex = value; }
  }
}
