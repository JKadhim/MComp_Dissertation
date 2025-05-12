
using UnityEngine;

public static class DiamondSquare
{
    public static float[,] GenerateNoiseMap(int size, float roughness, int seed, float decayFactor = 0.55f, bool normalize = true)
    {
        // Validate size (must be a power of 2 plus 1)
        if ((size - 1 & (size - 2)) != 0)
            throw new System.ArgumentException("Size must be a power of 2 plus 1 (e.g., 129, 257, 513).");

        System.Random prng = new System.Random(seed);
        float[,] map = new float[size, size];
        int stepSize = size - 1;

        // Initialize corners with random values
        map[0, 0] = (float)prng.NextDouble();
        map[0, stepSize] = (float)prng.NextDouble();
        map[stepSize, 0] = (float)prng.NextDouble();
        map[stepSize, stepSize] = (float)prng.NextDouble();

        // Perform the Diamond-Square algorithm
        while (stepSize > 1)
        {
            int halfStep = stepSize / 2;

            // Diamond step
            for (int y = 0; y < size - 1; y += stepSize)
            {
                for (int x = 0; x < size - 1; x += stepSize)
                {
                    float avg = (map[x, y] + map[x + stepSize, y] + map[x, y + stepSize] + map[x + stepSize, y + stepSize]) / 4.0f;
                    map[x + halfStep, y + halfStep] = avg + ((float)prng.NextDouble() * 2 - 1) * roughness;
                }
            }

            // Square step
            for (int y = 0; y < size; y += halfStep)
            {
                for (int x = (y + halfStep) % stepSize; x < size; x += stepSize)
                {
                    float sum = 0f;
                    int count = 0;

                    // Add neighbors
                    if (x - halfStep >= 0) { sum += map[x - halfStep, y]; count++; }
                    if (x + halfStep < size) { sum += map[x + halfStep, y]; count++; }
                    if (y - halfStep >= 0) { sum += map[x, y - halfStep]; count++; }
                    if (y + halfStep < size) { sum += map[x, y + halfStep]; count++; }

                    float avg = sum / count;
                    map[x, y] = avg + ((float)prng.NextDouble() * 2 - 1) * roughness;
                }
            }

            stepSize /= 2;
            roughness *= decayFactor; // Use configurable decay factor
        }

        // Normalize the map values to the range [0, 1]
        if (normalize)
        {
            float min = float.MaxValue;
            float max = float.MinValue;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (map[x, y] < min) min = map[x, y];
                    if (map[x, y] > max) max = map[x, y];
                }
            }

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    map[x, y] = (map[x, y] - min) / (max - min);
                }
            }
        }

        return map;
    }
}
