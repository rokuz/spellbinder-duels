using System;
using System.Collections.Generic;

public static class Constants
{
    public static int CARDS_SHOW_TIME_MS_FIRST_TURN = 5000;
    public static int CARDS_SHOW_TIME_MS_SECOND_TURN = 8000;
    public static int TURN_TIME_MS = 30000;
    public static int TURN_BIAS_MS = 1500;
    public static int SHOW_SUBSTITUTES = 1500;

    public static int LATENCY_TIME_MS = 1000;
    public static int DISCONNECTION_TIME_MS = 30000;

    public static int HEALTH_POINTS = 20;

    public static int MAX_LEVEL = 12;
    //                                 Level:   2    3    4    5     6     7     8     9      10     11     12
    public static int[] LEVEL_EXP = new int[] { 200, 400, 600, 1000, 2000, 4000, 8000, 12000, 16000, 20000, 24000 };

    //                                Level:   0  1  2  3  4  5  6  7  8  9  10 11 12
    public static int[] MAX_MANA = new int[] { 0, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5 };
    
    public static int SPELL_MISCAST_MANA = 1;
}
