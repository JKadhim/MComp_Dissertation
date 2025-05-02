using UnityEngine;

public static class MapFalloff
{
    public static float[,] GenerateFalloff(int size, float steepness, float shift)
    {
        float[,] falloffMap = new float[size, size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Normalize coordinates to the range [-1, 1]
                float normalizedX = (float)x / (size - 1) * 2 - 1;
                float normalizedY = (float)y / (size - 1) * 2 - 1;

                // Calculate the maximum distance from the center
                float value = Mathf.Max(Mathf.Abs(normalizedX), Mathf.Abs(normalizedY));

                // Apply the falloff function
                falloffMap[x, y] = FalloffFunction(value, steepness, shift);
            }
        }

        return falloffMap;
    }

    private static float FalloffFunction(float value, float steepness, float shift)
    {
        return Mathf.Pow(value, steepness) / (Mathf.Pow(value, steepness) + Mathf.Pow(shift - shift * value, steepness));
    }
}
