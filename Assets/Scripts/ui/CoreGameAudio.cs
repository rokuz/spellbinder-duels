using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreGameAudio : MonoBehaviour
{
  public AudioSource buttonDefault;
  public AudioSource buttonNo;
  public AudioSource buttonYes;
  public AudioSource cardFlip;
  public AudioSource toss;
  public AudioSource victory;
  public AudioSource defeat;

  public AudioSource miscast;
  public AudioSource bleeding;
  public AudioSource blessing;
  public AudioSource deathLook;
  public AudioSource doppelganger;
  public AudioSource fireball;
  public AudioSource iceSpear;
  public AudioSource lightning;
  public AudioSource natureCall;
  public AudioSource stoneskin;
  public AudioSource meteorite;

  public enum Type
  {
    ButtonDefault,
    ButtonYes,
    ButtonNo,
    CardFlip,
    Toss,
    Victory,
    Defeat
  }

  public void Play(Type type)
  {
    switch(type)
    {
      case Type.ButtonDefault: buttonDefault.Play(); break;
      case Type.ButtonYes: buttonYes.Play(); break;
      case Type.ButtonNo: buttonNo.Play(); break;
      case Type.CardFlip: cardFlip.Play(); break;
      case Type.Toss: toss.Play(); break;
      case Type.Victory: victory.Play(); break;
      case Type.Defeat: defeat.Play(); break;
    }
  }

  private AudioSource GetSpellSource(Spell s)
  {
    if (s == null)
      return miscast;

    switch (s.SpellType) 
    {
      case Spell.Type.BLEEDING: case Spell.Type.BLOOD_SIGN: case Spell.Type.VAMPIRE: return bleeding;
      case Spell.Type.BLESSING: case Spell.Type.ASTRAL_PROJECTION: return blessing;
      case Spell.Type.DEATH_LOOK: case Spell.Type.POISONING: case Spell.Type.SOUL_ABRUPTION: return deathLook;
      case Spell.Type.DOPPELGANGER: case Spell.Type.PHANTOM: case Spell.Type.HYPNOSIS: return doppelganger;
      case Spell.Type.FIREBALL: case Spell.Type.BURNING_SHIELD: return fireball;
      case Spell.Type.ICE_SPEAR: case Spell.Type.ICE_RAIN: case Spell.Type.ICE_FETTERS: return iceSpear;
      case Spell.Type.LIGHTNING: case Spell.Type.STORM: return lightning;
      case Spell.Type.NATURE_CALL: case Spell.Type.TORNADO: case Spell.Type.WILD_VINE: return natureCall;
      case Spell.Type.STONESKIN: case Spell.Type.DARKNESS_SHIELD: return stoneskin;
      case Spell.Type.METEORITE: case Spell.Type.INFERNO: return meteorite;
    }
    return null;
  }

  public void PlayForSpell(Spell spell)
  {
    AudioSource s = GetSpellSource(spell);
    if (s != null)
      s.Play();
  }

  public void OnFinishSpell(Spell spell)
  {
    AudioSource src = GetSpellSource(spell);
    if (src != null)
      StartCoroutine(AudioFadeOut.FadeOut(src, 0.5f));
  }
}
