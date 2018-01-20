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
      case Spell.Type.BLEEDING:
      case Spell.Type.BLESSING:
      case Spell.Type.DEATH_LOOK:
      case Spell.Type.DOPPELGANGER:
      case Spell.Type.FIREBALL:
      case Spell.Type.ICE_SPEAR:
      case Spell.Type.LIGHTNING:
      case Spell.Type.NATURE_CALL:
      case Spell.Type.STONESKIN:
        return null;
      case Spell.Type.METEORITE:
        return null;
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
