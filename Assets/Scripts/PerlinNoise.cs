using UnityEngine;
using UnityEngine.Rendering;

public static class PerlinNoise
{
    public static float[,] GenerateNoiseMap(int width, int height, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[width, height];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0f)
        {
            scale = 0.0001f; // Prevent division by zero
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float frequency = 1f;
                float amplitude = 1f;
                float noiseValue = 0f;

                for (int octave = 0; octave < octaves; octave++)
                {
                    float xCoord = ((x - halfWidth) / scale * frequency) + (octaveOffsets[octave].x / scale * frequency);
                    float yCoord = ((y - halfHeight) / scale * frequency) + (octaveOffsets[octave].y / scale * frequency);

                    float perlinValue = Mathf.PerlinNoise(xCoord, yCoord) * 2 - 1; // Range from -1 to 1
                    noiseValue += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseValue > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseValue;
                }
                if (noiseValue < minNoiseHeight)
                {
                    minNoiseHeight = noiseValue;
                }
                noiseMap[x, y] = noiseValue;
            }
        }
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}
