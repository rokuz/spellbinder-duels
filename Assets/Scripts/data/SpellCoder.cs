public static class SpellCoder
{
  public static char Encode(Magic magic)
  {
    switch (magic)
    {
      case Magic.FIRE:
        return 'F';
      case Magic.WATER:
        return 'W';
      case Magic.AIR:
        return 'A';
      case Magic.EARTH:
        return 'E';
      case Magic.NATURE:
        return 'N';
      case Magic.LIGHT:
        return 'L';
      case Magic.DARKNESS:
        return 'D';
      case Magic.BLOOD:
        return 'B';
      case Magic.ILLUSION:
        return 'I';
    }
    return '?';
  }

  public static string Encode(Magic[] magic)
  {
    if (magic == null)
      return "?";
    string result = "";
    foreach (Magic m in magic)
      result += Encode(m);
    return result;
  }
}
