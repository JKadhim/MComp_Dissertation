using UnityEngine;

public class MapGenerator: MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public bool autoUpdate;

    public void Generate()
    {
        float[,] map = PerlinNoise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);

        MapDisplay display = FindFirstObjectByType<MapDisplay>();
        display.DrawNoiseMap(map);
    }
}
