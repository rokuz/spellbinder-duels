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
    Toss
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
    }
  }

  private AudioSource GetSpellSource(Spell s)
  {
    if (s == null)
      return miscast;

    switch (s.SpellType) 
    {
      case Spell.Type.BLEEDING: case Spell.Type.BLOOD_SIGN: return bleeding;
      case Spell.Type.BLESSING: return blessing;
      case Spell.Type.DEATH_LOOK: case Spell.Type.POISONING: return deathLook;
      case Spell.Type.DOPPELGANGER: return doppelganger;
      case Spell.Type.FIREBALL: return fireball;
      case Spell.Type.ICE_SPEAR: case Spell.Type.ICE_RAIN: return iceSpear;
      case Spell.Type.LIGHTNING: case Spell.Type.STORM: return lightning;
      case Spell.Type.NATURE_CALL: return natureCall;
      case Spell.Type.STONESKIN: case Spell.Type.DARKNESS_SHIELD: return stoneskin;
      case Spell.Type.METEORITE: return meteorite;
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
