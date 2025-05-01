using Unity.Mathematics;
using UnityEngine;

public class MapGenerator: MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh,
        falloffMap
    }

    public DrawMode drawMode;

    static int mapChunkSize = 481;

    [Range(1, 6)]
    public int lOD;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistence;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool useFalloff;

    [Range(1, 10)]
    public float falloffSteepness;
    [Range(0.5f, 10f)]
    public float falloffShift;

    public float meshHeightMultiplier;
    public AnimationCurve meshCurve;

    public bool autoUpdate;

    public TerrainType[] regions;

    float[,] falloffMap;

    private void Awake()
    {
        falloffMap = MapFalloff.GenerateFalloff(mapChunkSize, falloffSteepness, falloffShift);
    }

    public void Generate()
    {
        float[,] map = PerlinNoise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistence, lacunarity, offset);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if (useFalloff)
                {
                    map[x, y] = Mathf.Clamp01(map[x, y] - falloffMap[x, y]);
                }
                float currentHeight = map[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindFirstObjectByType<MapDisplay>();

        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(map));
                break;
            case DrawMode.ColorMap:
                display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
                break;
            case DrawMode.Mesh:
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(map, meshHeightMultiplier, meshCurve, lOD), TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
                break;
            case DrawMode.falloffMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(MapFalloff.GenerateFalloff(mapChunkSize, falloffSteepness, falloffShift)));
                break;
        }
    }

    private void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 1)
        {
            octaves = 1;
        }

        falloffMap = MapFalloff.GenerateFalloff(mapChunkSize, falloffSteepness, falloffShift);
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}
