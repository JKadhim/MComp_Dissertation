using UnityEngine;

public static class VoronoiNoise
{
    public static float[,] GenerateNoiseMap(int seed, int size)
    {
        System.Random prng = new System.Random(seed);

        float[,] heightMap = new float[size, size];

        int seedCount = 10;
        
        Vector2[] seeds = new Vector2[seedCount];

        for (int i = 0; i < seedCount; i++)
        {
            seeds[i] = new Vector2(Random.Range(0, size), Random.Range(0, size));
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
                        heightMap[x, y] = (1 - (distance / size)) * 0.5f;
                    }
                }
            }
        }

        return heightMap;
    }
}
