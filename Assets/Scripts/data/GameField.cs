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

  public Magic[] Cards { get { return this.cards; } }

  public Magic[] SubstituteCards(int[] indices)
  {
    if (indices == null)
      return null;

    if (indices.Length == 2 && cards[indices[0]] != cards[indices[1]])
      return null;

    if (indices.Length == 3 && cards[indices[0]] != cards[indices[2]])
      return null;

    int card1 = indices[0];
    int card2 = (indices.Length == 2) ? indices[1] : indices[2];

    bool isFight = Array.Exists(FIGHT_MAGIC, c => c == cards[indices[0]]);
    List<Magic> magic = isFight ? GetNewMagic(FIGHT_MAGIC) : GetNewMagic(DEFENSE_MAGIC);
    int magicIndex = UnityEngine.Random.Range(0, magic.Count);
    cards[card1] = magic[magicIndex];
    cards[card2] = magic[magicIndex];

    Magic[] result = new Magic[indices.Length];
    for (int i = 0; i < indices.Length; i++)
      result[i] = cards[indices[i]];
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

  public void ShuffleIndices(int[] indices)
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

  public List<int> GetRandomMagic()
  {
    var indices = new List<int>();
    int index = UnityEngine.Random.Range(0, CARDS_COUNT);
    for (int i = 0; i < CARDS_COUNT; i++)
    {
      if (cards[i] == cards[index])
        indices.Add(i);
    }
    return indices;
  }

  public bool IsBasicFightMagic(List<int> indices)
  {
    Magic m = cards[indices[0]];
    return m == Magic.FIRE || m == Magic.AIR || m == Magic.WATER;
  }

  public List<int> GetRandomCards()
  {
    var indices = new List<int>();
    while (indices.Count != 3)
    {
      int index = UnityEngine.Random.Range(0, CARDS_COUNT);
      if (indices.Exists(x => x == index))
        continue;
      indices.Add(index);
      if (indices.Count == 2 && cards[indices[0]] == cards[indices[1]])
        return indices;
    }
    return indices;
  }

  public List<int> FindSpell(Spell spell)
  {
    List<int> result = new List<int>();
    foreach (Magic m in spell.Combination)
    {
      bool nothingAdded = true;
      for (int i = 0; i < CARDS_COUNT; i++)
      {
        if (result.Exists(x => x == i))
          continue;
        if (cards[i] == m)
        {
          result.Add(i);
          nothingAdded = false;
          break;
        }
      }
      if (nothingAdded)
        return null;
    }
    return result;
  }

  public bool HasSpellWith3Components(Spell[] spells, int[] indices)
  {
    foreach (Spell s in spells)
    {
      if (s.Combination[0] == cards[indices[0]] && s.Combination[1] == cards[indices[1]])
        return true;
    }
    return false;
  }
}
