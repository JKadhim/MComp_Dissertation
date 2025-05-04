using Unity.Mathematics;
using UnityEngine;

public class MapGenerator: MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        Mesh,
        falloffMap
    }

    public enum NoiseType
    {
        PerlinNoise,
        DiamondSquareNoise,
        VoronoiNoise,
        CellularNoise
    }

    float[,] map;

    public DrawMode drawMode;
    public NoiseType noiseType;

    [Range(1, 15)]
    public int steps;

    static int mapSize = 481;

    [Range(1, 6)]
    public int lOD;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistence;
    public float lacunarity;

    public float roughness = 2.0f;

    [Range(0, 10)]
    public int blurPasses;

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

    float[,] falloffMap;

    //private void Awake()
    //{
    //    falloffMap = MapFalloff.GenerateFalloff(mapSize, falloffSteepness, falloffShift);
    //}

    public void Generate()
    {
        switch (noiseType)
        {
            case NoiseType.PerlinNoise:
                map = PerlinNoise.GenerateNoiseMap(mapSize, seed, noiseScale, octaves, persistence, lacunarity, offset);
                break;
            case NoiseType.DiamondSquareNoise:
                map = DiamondSquare.GenerateNoiseMap(roughness, seed);
                break;
            case NoiseType.VoronoiNoise:
                map = VoronoiNoise.GenerateNoiseMap(seed, mapSize);
                break;
            case NoiseType.CellularNoise:
                map = CellularAutomata.GenerateNoiseMap(mapSize, seed, steps, blurPasses);
                break;
        }

        MapDisplay display = FindFirstObjectByType<MapDisplay>();

        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(map));
                break;
            case DrawMode.Mesh:
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(map, meshHeightMultiplier, meshCurve, lOD));
                break;
            case DrawMode.falloffMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(MapFalloff.GenerateFalloff(mapSize, falloffSteepness, falloffShift)));
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

        falloffMap = MapFalloff.GenerateFalloff(mapSize, falloffSteepness, falloffShift);
    }
}
