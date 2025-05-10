using UnityEngine;
using System.Threading.Tasks;

public static class PerlinNoise
{
    public enum NormalizationMode
    {
        Global,
        Local
    }

    public static float[,] GenerateNoiseMap(
        int mapSize,
        int randomSeed,
        float noiseScale,
        int numOctaves,
        float persistence,
        float lacunarity,
        Vector2 mapOffset,
        NormalizationMode normalizationMode)
    {
        float[,] heightMap = new float[mapSize, mapSize];

        System.Random randomGenerator = new System.Random(randomSeed);
        Vector2[] octaveShifts = new Vector2[numOctaves];

        float maxPossibleHeight = 0f;
        float[] frequencies = new float[numOctaves];
        float[] amplitudes = new float[numOctaves];

        // Precompute octave offsets, frequencies, and amplitudes
        for (int i = 0; i < numOctaves; i++)
        {
            float shiftX = randomGenerator.Next(-50000, 50000) - mapOffset.x;
            float shiftY = randomGenerator.Next(-50000, 50000) - mapOffset.y;
            octaveShifts[i] = new Vector2(shiftX, shiftY);

            frequencies[i] = Mathf.Pow(lacunarity, i);
            amplitudes[i] = Mathf.Pow(persistence, i);

            maxPossibleHeight += amplitudes[i];
        }

        float highestNoiseValue = float.MinValue;
        float lowestNoiseValue = float.MaxValue;

        float halfMapWidth = mapSize / 2f;
        float halfMapHeight = mapSize / 2f;

        // Generate Perlin noise values in parallel
        Parallel.For(0, mapSize, row =>
        {
            for (int col = 0; col < mapSize; col++)
            {
                float totalNoiseValue = 0f;

                for (int octave = 0; octave < numOctaves; octave++)
                {
                    float sampleX = ((col - halfMapWidth + octaveShifts[octave].x) / noiseScale) * frequencies[octave];
                    float sampleY = ((row - halfMapHeight + octaveShifts[octave].y) / noiseScale) * frequencies[octave];

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // Range from -1 to 1
                    totalNoiseValue += perlinValue * amplitudes[octave];
                }

                lock (heightMap)
                {
                    if (totalNoiseValue > highestNoiseValue) highestNoiseValue = totalNoiseValue;
                    if (totalNoiseValue < lowestNoiseValue) lowestNoiseValue = totalNoiseValue;
                }

                heightMap[col, row] = totalNoiseValue;
            }
        });

        // Normalize the noise values
        Parallel.For(0, mapSize, row =>
        {
            for (int col = 0; col < mapSize; col++)
            {
                if (normalizationMode == NormalizationMode.Local)
                {
                    heightMap[col, row] = Mathf.InverseLerp(lowestNoiseValue, highestNoiseValue, heightMap[col, row]);
                }
                else
                {
                    float normalizedValue = (heightMap[col, row] + 1) / (2f * maxPossibleHeight / 1.75f);
                    heightMap[col, row] = Mathf.Clamp(normalizedValue, 0, 1);
                }
            }
        });

        return heightMap;
    }
}
