using UnityEngine;

public static class DiamondSquare
{
    public static float[,] GenerateNoiseMap(float roughness, int seed)
    {
        System.Random prng = new System.Random(seed);
        int size = 257;
        float[,] map = new float[size, size];
        int stepSize = size - 1;

        map[0, 0] = (float)prng.NextDouble();
        map[0, stepSize] = (float)prng.NextDouble();
        map[stepSize, 0] = (float)prng.NextDouble();
        map[stepSize, stepSize] = (float)prng.NextDouble();

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
                    float avg = (map[(x - halfStep + size) % size, y] +
                                 map[(x + halfStep) % size, y] +
                                 map[x, (y + halfStep) % size] +
                                 map[x, (y - halfStep + size) % size]) / 4.0f;
                    map[x, y] = avg + ((float)prng.NextDouble() * 2 - 1) * roughness;
                }
            }

            stepSize /= 2;
            roughness /= 2.0f;
        }

        return map;
    }
}