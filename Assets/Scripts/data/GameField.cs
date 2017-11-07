using System;
using System.Collections.Generic;
using UnityEngine;

public class GameField
{
  public static int CARDS_COUNT = 12;
  private static Magic[] FIGHT_MAGIC = new Magic[] { Magic.FIRE, Magic.AIR, Magic.WATER, Magic.DARKNESS, Magic.BLOOD };
  private static Magic[] DEFENSE_MAGIC = new Magic[] { Magic.EARTH, Magic.NATURE, Magic.LIGHT, Magic.ILLUSION };

  private Magic[] cards = new Magic[CARDS_COUNT];

  public GameField()
  {
    int[] fightIndices = new int[FIGHT_MAGIC.Length];
    for (int i = 0; i < fightIndices.Length; i++) fightIndices[i] = i;
      ShuffleIndices(fightIndices);
    int[] defenseIndices = new int[DEFENSE_MAGIC.Length];
    for (int i = 0; i < defenseIndices.Length; i++) defenseIndices[i] = i;
      ShuffleIndices(defenseIndices);

    int pairsCount = cards.Length / 2;
    int halfPairsCount = pairsCount / 2;
    for (int i = 0; i < halfPairsCount; i++)
    {
      int magicIndex = fightIndices[i];
      cards[i * 2] = FIGHT_MAGIC[magicIndex];
      cards[(i + halfPairsCount) * 2] = FIGHT_MAGIC[magicIndex];

      magicIndex = defenseIndices[i];
      cards[i * 2 + 1] = DEFENSE_MAGIC[magicIndex];
      cards[(i + halfPairsCount) * 2 + 1] = DEFENSE_MAGIC[magicIndex];
    }
    ShuffleCards();
  }

  public Magic[] Cards { get { return this.cards;} }

  public Dictionary<int, Magic> SubstituteCards(int card1, int card2)
  {
    if (card1 < 0 || card1 >= CARDS_COUNT || card2 < 0 || card2 >= CARDS_COUNT)
      return null;

    bool isFight = Array.Exists(FIGHT_MAGIC, c => c == cards[card1]);
    List<Magic> magic = isFight ? GetNewMagic(FIGHT_MAGIC) : GetNewMagic(DEFENSE_MAGIC);
    int magicIndex = UnityEngine.Random.Range(0, magic.Count);
    cards[card1] = magic[magicIndex];
    cards[card2] = magic[magicIndex];

    var result = new Dictionary<int, Magic>();
    result[card1] = cards[card1];
    result[card2] = cards[card2];
    return result;
  }

  private void ShuffleCards()
  {
    for (int i = cards.Length - 1; i > 0; i--)
    {
      int index = UnityEngine.Random.Range(0, i + 1);
      Magic temp = cards[index];
      cards[index] = cards[i];
      cards[i] = temp;
    }
  }

  private void ShuffleIndices(int[] indices)
  {
    for (int i = indices.Length - 1; i > 0; i--)
    {
      int index = UnityEngine.Random.Range(0, i + 1);
      int temp = indices[index];
      indices[index] = indices[i];
      indices[i] = temp;
    }
  }

  private List<Magic> GetNewMagic(Magic[] magic)
  {
    var result = new List<Magic>();
    for (int i = 0; i < magic.Length; i++)
    {
      if (!Array.Exists(cards, m => m == magic[i]))
        result.Add(magic[i]);
    }
    if (result.Count == 0)
      result.Add(magic[UnityEngine.Random.Range(0, magic.Length)]);
    return result;
  }
}
