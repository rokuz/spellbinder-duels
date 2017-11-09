using System;
using System.Collections.Generic;
using UnityEngine;

public class Match
{
  public enum MatchStatus { PREPARED, STARTED, FINISHED, INTERRUPTED }

  private ProfileData player1;
  private ProfileData player2;
  private GameField gameField;
  private PlayerData player1Data;
  private PlayerData player2Data;

  private float timestamp;
  private MatchStatus matchStatus;
  private int winnerIndex;
  private bool player1FirstTurn;

  public Match(ProfileData player1, ProfileData player2)
  {
    this.player1 = player1;
    this.player2 = player2;
    this.timestamp = Time.time;
    this.matchStatus = MatchStatus.PREPARED;
    this.winnerIndex = -1;
    this.gameField = new GameField();
    this.player1FirstTurn = (UnityEngine.Random.Range(0, 1) == 0);
    this.player1Data = new PlayerData();
    this.player2Data = new PlayerData();
    this.player1Data.mana = this.player1FirstTurn ? 1 : 2;
    this.player2Data.mana = this.player1FirstTurn ? 2 : 1;
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

  public GameField Field
  {
    get { return gameField; }
  }

  public bool Player1FirstTurn
  {
    get { return player1FirstTurn; }
  }

  public PlayerData Player1Data
  {
    get { return player1Data; }
  }

  public PlayerData Player2Data
  {
    get { return player2Data; }
  }
}
