using UnityEngine;

public static class PerlinNoise
{
    public enum NormalizationMode
    {
        Global,
        Local
    }

    public static float[,] GenerateNoiseMap(int size, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizationMode normalization)
    {
        float[,] noiseMap = new float[size, size];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxHeight = 0;
        float frequency = 1f;
        float amplitude = 1f;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) - offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxHeight += amplitude;
            amplitude *= persistance;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = size / 2f;
        float halfHeight = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                frequency = 1f;
                amplitude = 1f;
                float noiseValue = 0f;

                for (int octave = 0; octave < octaves; octave++)
                {
                    float xCoord = ((x - halfWidth + octaveOffsets[octave].x) / scale * frequency);
                    float yCoord = ((y - halfHeight + octaveOffsets[octave].y) / scale * frequency);

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
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (normalization == NormalizationMode.Local) {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    float normalizedValue = (noiseMap[x, y] + 1) / (2f* maxHeight/1.75f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedValue,0, int.MaxValue);
                }
            }
        }

        return noiseMap;
    }
}
