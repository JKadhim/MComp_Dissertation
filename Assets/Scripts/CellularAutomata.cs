using UnityEditor.Experimental.GraphView;
using UnityEngine;

public static class CellularAutomata
{
    // Generates a noise map using cellular automata
    public static float[,] GenerateNoiseMap(int size, int seed, int steps, int blurPasses, float aliveChance)
    {
        float[,] map = new float[size, size];
        System.Random prng = new System.Random(seed);

        // Initialize the map with random values based on the aliveChance
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float rand = (float)prng.NextDouble();
                map[x, y] = rand <= aliveChance ? 1.0f : 0.0f;
            }
        }

        // Precompute neighbor counts for the initial map
        int[,] neighborCounts = PrecomputeAliveNeighbors(map, size);

        // Perform the specified number of simulation steps
        for (int i = 0; i < steps; i++)
        {
            map = SimulationStep(map, neighborCounts, size);
        }

        // Apply blur passes to smooth the noise map
        for (int i = 0; i < blurPasses; i++)
        {
            map = ApplyBlur(map, size);
        }

        return map;
    }

    // Performs a single simulation step based on cellular automata rules
    private static float[,] SimulationStep(float[,] map, int[,] neighborCounts, int size)
    {
        float[,] newMap = new float[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                int neighbors = neighborCounts[x, y];
                bool isAlive = map[x, y] == 1.0f;

                // Apply survival and birth rules
                if (isAlive)
                {
                    if (neighbors < 3)
                    {
                        newMap[x, y] = 0.0f; // Cell dies
                        UpdateNeighborCounts(neighborCounts, x, y, size, false);
                    }
                    else
                    {
                        newMap[x, y] = 1.0f; // Cell survives
                    }
                }
                else
                {
                    if (neighbors >= 4)
                    {
                        newMap[x, y] = 1.0f; // Cell becomes alive
                        UpdateNeighborCounts(neighborCounts, x, y, size, true);
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

    // Precomputes the number of alive neighbors for each cell in the map
    private static int[,] PrecomputeAliveNeighbors(float[,] map, int size)
    {
        int[,] neighborCounts = new int[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        int newX = x + i;
                        int newY = y + j;

                        if (i == 0 && j == 0) continue; // Skip the current cell
                        if (newX >= 0 && newX < size && newY >= 0 && newY < size)
                        {
                            neighborCounts[newX, newY] += (map[x, y] == 1.0f) ? 1 : 0;
                        }
                    }
                }
            }
        }

        return neighborCounts;
    }

    // Updates the neighbor counts when a cell changes state
    private static void UpdateNeighborCounts(int[,] neighborCounts, int x, int y, int size, bool isAlive)
    {
        int delta = isAlive ? 1 : -1;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int newX = x + i;
                int newY = y + j;

                if (i == 0 && j == 0) continue; // Skip the current cell
                if (newX >= 0 && newX < size && newY >= 0 && newY < size)
                {
                    neighborCounts[newX, newY] += delta;
                }
            }
        }
    }

    // Applies a blur effect to the map to smooth it
    private static float[,] ApplyBlur(float[,] map, int size)
    {
        float[,] tempMap = new float[size, size];
        float[,] blurredMap = new float[size, size];

        // Horizontal blur
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float sum = 0f;
                int count = 0;

                for (int i = -1; i <= 1; i++)
                {
                    int newY = y + i;
                    if (newY >= 0 && newY < size)
                    {
                        sum += map[x, newY];
                        count++;
                    }
                }

                tempMap[x, y] = sum / count;
            }
        }

        // Vertical blur
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float sum = 0f;
                int count = 0;

                for (int i = -1; i <= 1; i++)
                {
                    int newX = x + i;
                    if (newX >= 0 && newX < size)
                    {
                        sum += tempMap[newX, y];
                        count++;
                    }
                }

                blurredMap[x, y] = sum / count;
            }
        }

        return blurredMap;
    }
}
