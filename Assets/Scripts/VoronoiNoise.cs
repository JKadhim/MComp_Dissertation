using UnityEngine;

public static class VoronoiNoise
{
    // Generates a Voronoi noise map based on the given parameters.
    public static float[,] GenerateNoiseMap(int seed, int size, int seedCount)
    {
        // Initialize the pseudo-random number generator with the given seed.
        System.Random prng = new System.Random(seed);

        // Create a 2D array to store the height values.
        float[,] heightMap = new float[size, size];

        // Generate random seed points within the map.
        Vector2[] seeds = new Vector2[seedCount];
        for (int i = 0; i < seedCount; i++)
        {
            seeds[i] = new Vector2(prng.Next(0, size), prng.Next(0, size));
        }

        // Iterate through each point in the map.
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float minDistance = float.MaxValue;

                // Find the closest seed point to the current point.
                foreach (var val in seeds)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), val);
                    if (distance < minDistance)
                    {
                        minDistance = distance;

                        // Normalize the height value based on the distance and invert it.
                        float normalizedHeight = 1 - (distance / size);
                        heightMap[x, y] = 1 - (1 - Mathf.Pow(normalizedHeight, 2f) + 0.4f);
                    }
                }
            }
        }

        return heightMap;
    }
}
