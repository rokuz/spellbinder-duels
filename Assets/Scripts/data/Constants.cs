using System;
using System.Collections.Generic;

public static class Constants
{
  public static int CARDS_SHOW_TIME = 5;

  public static int HEALTH_POINTS = 20;

  public static int MAX_LEVEL = 12;
  //                                 Level:   2    3    4    5     6     7     8     9      10     11     12
  public static int[] LEVEL_EXP = new int[] { 200, 400, 600, 1000, 2000, 4000, 8000, 12000, 16000, 20000, 24000 };

  public static int MAX_MANA = 5;
  
  public static int SPELL_MISCAST_MANA = 1;

  public static int COINS_MIN = 10;
  public static int COINS_MAX = 30;

  public static int INVITE_PRICE = 10;

  //                                             Level:   1     2     3     4     5     6     7     8     9     10    11    12
  public static float[] OPEN_RANDOM_CARDS = new float[] { 0.3f, 0.3f, 0.3f, 0.3f, 0.2f, 0.2f, 0.2f, 0.1f, 0.1f, 0.1f, 0.0f, 0.0f };

  public static int GetSpellPrice(Spell spell) 
  {
    switch (spell.SpellType) 
    {
      case Spell.Type.BLEEDING:
      case Spell.Type.BLESSING:
      case Spell.Type.DEATH_LOOK:
      case Spell.Type.DOPPELGANGER:
      case Spell.Type.FIREBALL:
      case Spell.Type.ICE_SPEAR:
      case Spell.Type.LIGHTNING:
      case Spell.Type.NATURE_CALL:
      case Spell.Type.STONESKIN:
        return 0;
      case Spell.Type.METEORITE:
        return 100;
    }
    return 0;
  }
}
