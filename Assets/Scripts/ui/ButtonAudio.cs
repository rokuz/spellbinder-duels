using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAudio : MonoBehaviour
{
  public AudioSource buttonDefault;
  public AudioSource buttonNo;
  public AudioSource buttonYes;

  public enum Type
  {
    Default,
    Yes,
    No
  }

  public void Play(Type type)
  {
    switch(type)
    {
      case Type.Default: buttonDefault.Play(); break;
      case Type.Yes: buttonYes.Play(); break;
      case Type.No: buttonNo.Play(); break;
    }
  }
}
