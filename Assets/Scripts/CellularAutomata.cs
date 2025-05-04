using UnityEditor.Experimental.GraphView;
using UnityEngine;

public static class CellularAutomata
{
    public static float[,] GenerateNoiseMap(int size, int seed, int steps, int blurPasses)
    {
        float[,] map = new float[size, size];

        System.Random prng = new System.Random(seed);
        float aliveChance = 0.4f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float rand = (float)prng.NextDouble();
                if (rand <= aliveChance)
                {
                    map[x, y] = 1.0f;
                }
                else
                {
                    map[x, y] = 0.0f;
                }
            }
        }

        for (int i = 0; i < steps; i++)
        {
            map = SimulationStep(map, size);
        }

        // Apply blur to smooth the noise map
        for (int i = 0; i < blurPasses; i++)
        {
            map = ApplyBlur(map, size);
        }

        return map;
    }

    private static float[,] SimulationStep(float[,] map, int size)
    {
        float[,] newMap = new float[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                int neighbours = CountAliveNeighbours(map, x, y);
                if (map[x, y] == 1.0f)
                {
                    if (neighbours < 4)
                    {
                        newMap[x, y] = 0.0f; // Cell dies
                    }
                    else
                    {
                        newMap[x, y] = 1.0f; // Cell survives
                    }
                }
                else
                {
                    if (neighbours >= 4)
                    {
                        newMap[x, y] = 1.0f; // Cell becomes alive
                    }
                    else
                    {
                        newMap[x, y] = 0.0f; // Cell remains dead
                    }
                }
            }
        }
        return newMap;
    }

    private static int CountAliveNeighbours(float[,] map, int x, int y) 
    {
        int count = 0;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int newX = x + i;
                int newY = y + j;
                
                if (i == 0 && j == 0) 
                {
                    continue;
                }
                else if (newX < 0 || newX >= map.GetLength(0) || newY < 0 || newY >= map.GetLength(1))
                {
                    count++;
                }
                else if (map[newX, newY] == 1.0f)
                {
                    count++;
                }
            }
        }

        return count;
    }
    private static float[,] ApplyBlur(float[,] map, int size)
    {
        float[,] blurredMap = new float[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float sum = 0f;
                int count = 0;

                // Iterate over the 3x3 grid around the current cell
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        int newX = x + i;
                        int newY = y + j;

                        // Check if the neighbor is within bounds
                        if (newX >= 0 && newX < size && newY >= 0 && newY < size)
                        {
                            sum += map[newX, newY];
                            count++;
                        }
                    }
                }

                // Average the values
                blurredMap[x, y] = sum / count;
            }
        }

        return blurredMap;
    }
}
