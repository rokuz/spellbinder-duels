using System;

public static class Utils
{
    public static int[] ToIntArray(JSONObject[] arr)
    {
        int[] result = new int[arr.Length];
        for (int i = 0; i < arr.Length; i++)
            result[i] = (int)arr[i].i;
        return result;
    }
}
