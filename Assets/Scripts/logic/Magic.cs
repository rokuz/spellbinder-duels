using UnityEngine;

public enum Magic {
    FIRE,
    WATER,
    AIR,
    EARTH,
    NATURE,
    LIGHT,
    DARKNESS,
    BLOOD,
    ILLUSION
}

public static class MagicUtils
{
    public static Magic MagicFromString(string str)
    {
        if (str == "FIRE") return Magic.FIRE;
        else if (str == "WATER") return Magic.WATER;
        else if (str == "AIR") return Magic.AIR;
        else if (str == "EARTH") return Magic.EARTH;
        else if (str == "NATURE") return Magic.NATURE;
        else if (str == "LIGHT") return Magic.LIGHT;
        else if (str == "DARKNESS") return Magic.DARKNESS;
        else if (str == "BLOOD") return Magic.BLOOD;
        else if (str == "ILLUSION") return Magic.ILLUSION;
        throw new UnityException("Unknown magic");
    }
}