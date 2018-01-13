using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellSpritesHolder : MonoBehaviour
{
  public Sprite fireIconSprite;
  public Sprite waterIconSprite;
  public Sprite airIconSprite;
  public Sprite earthIconSprite;
  public Sprite natureIconSprite;
  public Sprite lightIconSprite;
  public Sprite darknessIconSprite;
  public Sprite bloodIconSprite;
  public Sprite illusionIconSprite;

  public Sprite bleedingSprite;
  public Sprite blessingSprite;
  public Sprite deathLookSprite;
  public Sprite doppelgangerSprite;
  public Sprite fireballSprite;
  public Sprite iceSpearSprite;
  public Sprite lightningSprite;
  public Sprite natureCallSprite;
  public Sprite stoneskinSprite;
  public Sprite meteoriteSprite;

  public Sprite GetSprite(Magic magic) 
  {
    switch (magic) 
    {
      case Magic.FIRE:
        return this.fireIconSprite;
      case Magic.WATER:
        return this.waterIconSprite;
      case Magic.AIR:
        return this.airIconSprite;
      case Magic.EARTH:
        return this.earthIconSprite;
      case Magic.NATURE:
        return this.natureIconSprite;
      case Magic.LIGHT:
        return this.lightIconSprite;
      case Magic.DARKNESS:
        return this.darknessIconSprite;
      case Magic.BLOOD:
        return this.bloodIconSprite;
      case Magic.ILLUSION:
        return this.illusionIconSprite;
    }
    return null;
  }

  public Sprite GetSprite(Spell spell) 
  {
    switch (spell.SpellType) 
    {
      case Spell.Type.BLEEDING:
        return this.bleedingSprite;
      case Spell.Type.BLESSING:
        return this.blessingSprite;
      case Spell.Type.DEATH_LOOK:
        return this.deathLookSprite;
      case Spell.Type.DOPPELGANGER:
        return this.doppelgangerSprite;
      case Spell.Type.FIREBALL:
        return this.fireballSprite;
      case Spell.Type.ICE_SPEAR:
        return this.iceSpearSprite;
      case Spell.Type.LIGHTNING:
        return this.lightningSprite;
      case Spell.Type.NATURE_CALL:
        return this.natureCallSprite;
      case Spell.Type.STONESKIN:
        return this.stoneskinSprite;
      case Spell.Type.METEORITE:
        return this.meteoriteSprite;
    }
    return null;
  }
}
