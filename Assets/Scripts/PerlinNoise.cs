using UnityEngine;
using UnityEngine.Rendering;

public static class PerlinNoise
{
    public static float[,] GenerateNoiseMap(int width, int height, float scale)
    {
        float[,] noiseMap = new float[width, height];

        if (scale <= 0f)
        {
            scale = 0.0001f; // Prevent division by zero
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float xCoord = (float)x / width * scale;
                float yCoord = (float)y / height * scale;

                noiseMap[x, y] = Mathf.PerlinNoise(xCoord, yCoord);


            }
        }
        return noiseMap;
    }
}
