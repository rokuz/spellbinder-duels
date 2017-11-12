using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Spellbook
{
  public static List<Spell> Spells = new List<Spell>();

  public static void Init()
  {
    if (Spells.Count != 0) return;

    // Basic
    var spell = new Spell(new Magic[] { Magic.FIRE, Magic.FIRE }, Spell.Type.FIREBALL);
    spell.manaCost = 1;
    spell.damage = 2;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.WATER, Magic.WATER }, Spell.Type.ICE_SPEAR);
    spell.manaCost = 1;
    spell.damage = 2;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.AIR, Magic.AIR }, Spell.Type.LIGHTNING);
    spell.manaCost = 1;
    spell.damage = 2;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.DARKNESS, Magic.DARKNESS }, Spell.Type.DEATH_LOOK);
    spell.manaCost = 1;
    spell.blockDamageTurns = 1;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.BLOOD, Magic.BLOOD }, Spell.Type.BLEEDING);
    spell.manaCost = 1;
    spell.blockHealingTurns = 1;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.EARTH, Magic.EARTH }, Spell.Type.STONESKIN);
    spell.manaCost = 1;
    spell.clearHealingCurse = true;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.ILLUSION, Magic.ILLUSION }, Spell.Type.DOPPELGANGER);
    spell.manaCost = 1;
    spell.defense = 1;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.NATURE, Magic.NATURE }, Spell.Type.NATURE_CALL);
    spell.manaCost = 1;
    spell.healing = 1;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.LIGHT, Magic.LIGHT }, Spell.Type.BLESSING);
    spell.manaCost = 1;
    spell.clearDamageCurse = true;
    Spells.Add(spell);

    // Special Fire
    /*spell = new Spell(new Magic[] { Magic.FIRE, Magic.FIRE, Magic.FIRE }, Spell.Type.FIRE_STORM);
    spell.manaCost = 2;
    spell.minLevel = 5;
    spell.damage = 5;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.FIRE, Magic.AIR, Magic.FIRE }, Spell.Type.METEORITE);
    spell.manaCost = 2;
    spell.minLevel = 11;
    spell.damage = 7;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.FIRE, Magic.DARKNESS, Magic.FIRE }, Spell.Type.BURNING_SHIELD);
    spell.manaCost = 2;
    spell.minLevel = 12;
    spell.damage = 6;
    spell.removeDefense = true;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.FIRE, Magic.BLOOD, Magic.FIRE }, Spell.Type.FATAL_BURN);
    spell.manaCost = 2;
    spell.minLevel = 10;
    spell.damage = 4;
    spell.considerResistance = false;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.FIRE, Magic.EARTH, Magic.FIRE }, Spell.Type.FIRE_RING);
    spell.manaCost = 2;
    spell.minLevel = 7;
    spell.damage = 4;
    spell.defense = 1;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.FIRE, Magic.ILLUSION, Magic.FIRE }, Spell.Type.INVISIBLE_FIRE);
    spell.manaCost = 2;
    spell.minLevel = 8;
    spell.damage = 4;
    spell.considerDefense = false;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.FIRE, Magic.NATURE, Magic.FIRE }, Spell.Type.WILDFIRE);
    spell.manaCost = 2;
    spell.minLevel = 6;
    spell.damage = 4;
    spell.healing = 1;
    Spells.Add(spell);

    spell = new Spell(new Magic[] { Magic.FIRE, Magic.LIGHT, Magic.FIRE }, Spell.Type.DIVINE_FIRE);
    spell.manaCost = 2;
    spell.minLevel = 9;
    spell.damage = 4;
    spell.healing = 2;
    Spells.Add(spell);*/

    // Special Water

    // Special Air

    // Special Darkness

    // Special Blood

    // Special Earth

    // Special Illusion

    // Special Nature

  }

  public static Spell Find(string code)
  {
    foreach (var s in Spells)
    {
      if (s.Code == code)
        return s;
    }
    return null;
  }
}
