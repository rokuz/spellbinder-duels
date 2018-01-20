using System;

public class SpellCalculator
{
  private int[] initialBonuses = new int[] { 0, 0, 0 };
  private int[] initialResistance = new int[] { 0, 0, 0 };

  private int[] bonuses = new int[] { 0, 0, 0 };
  private int[] resistance = new int[] { 0, 0, 0 };

  public SpellCalculator(int[] initialBonuses, int[] initialResistance)
  {
    Array.Copy(initialBonuses, this.initialBonuses, this.initialBonuses.Length);
    Array.Copy(initialResistance, this.initialResistance, this.initialResistance.Length);
  }

  public int[] Bonuses
  {
    get { return bonuses; }
  }

  public int[] Resistance
  {
    get { return resistance; }
  }

  public void OnCastSpell(Spell spell)
  {
    int index = spell.Index;
    if (index < 0)
      return;

    bonuses[index] += spell.Complex ? 2 : 1;
  }

  public void OnReceiveSpell(Spell spell)
  {
    int index = spell.Index;
    if (index < 0)
      return;

    resistance[index] += spell.Complex ? 2 : 1;
  }
}
