using UnityEngine;

public static class VoronoiNoise
{
    public static float[,] GenerateNoiseMap(int seed, int size, int seedCount)
    {
        System.Random prng = new System.Random(seed);

        float[,] heightMap = new float[size, size];

        Vector2[] seeds = new Vector2[seedCount];

        for (int i = 0; i < seedCount; i++)
        {
            seeds[i] = new Vector2(prng.Next(0, size), prng.Next(0, size));
        }

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float minDistance = float.MaxValue;
                foreach (var value in seeds)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), value);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        float normalizedHeight = 1 - (distance / size);
                        heightMap[x, y] = 1 - (1 - Mathf.Pow(normalizedHeight, 2f) + 0.4f); // Invert the height values
                    }
                }
            }
        }

        return heightMap;
    }
}
