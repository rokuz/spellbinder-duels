using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarHolder : MonoBehaviour
{
  public Sprite spriteAlatel;
  public Sprite spriteChristian;
  public Sprite spriteMelissa;
  public Sprite spriteRichard;
  public Sprite spriteAnya;
  public Sprite spriteCorvin;
  public Sprite spriteMerlin;
  public Sprite spriteRosa;
  public Sprite spriteAstrid;
  public Sprite spriteDesmond;
  public Sprite spriteOlaf;
  public Sprite spriteServin;
  public Sprite spriteBastila;
  public Sprite spriteJacob;
  public Sprite spriteOlivia;
  public Sprite spriteTalion;
  public Sprite spriteChi;
  public Sprite spriteLi;
  public Sprite spriteRashid;
  public Sprite spriteZed;

  public Sprite GetAvatar(ProfileData profile)
  {
    switch (profile.name)
    {
      case "Corvin": return spriteCorvin;
      case "Rosa": return spriteRosa;
      case "Melissa": return spriteMelissa;
      case "Christian": return spriteChristian;
      case "Zed": return spriteZed;
      case "Olivia": return spriteOlivia;
      case "Alatel": return spriteAlatel;
      case "Desmond": return spriteDesmond;
      case "Servin": return spriteServin;
      case "Olaf": return spriteOlaf;
      case "Richard": return spriteRichard;
      case "Jacob": return spriteJacob;
      case "Chi": return spriteChi;
      case "Bastila": return spriteBastila;
      case "Anya": return spriteAnya;
      case "Li": return spriteLi;
      case "Astrid": return spriteAstrid;
      case "Rashid": return spriteRashid;
      case "Talion": return spriteTalion;
      case "Merlin": return spriteMerlin;
    }
    return null;
  }
}
