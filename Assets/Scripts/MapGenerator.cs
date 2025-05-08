using Unity.Mathematics;
using UnityEngine;

public class MapGenerator: MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        Mesh
    }

    public enum NoiseType
    {
        PerlinNoise,
        DiamondSquareNoise,
        VoronoiNoise,
        CellularNoise,
        Falloff
    }

    public DrawMode drawMode;
    public NoiseType noiseType;

    //Map generation
    float[,] map;
    public int seed;
    
    public static int mapSize = 241;

    //Perlin noise
    public float noiseScale;
    public Vector2 offset;
    public int octaves;
    [Range(0, 1)]
    public float persistence;
    public float lacunarity;

    //Diamond square
    public float roughness = 2.0f;

    //Voronoi noise
    [Min(5)]
    public int seedCount = 10;

    //Cellular automata
    [Range(1, 15)]
    public int steps;
    [Range(0, 10)]
    public int blurPasses;

    //Falloff map
    public bool useFalloff;
    float[,] falloffMap;
    [Range(1, 10)]
    public float falloffSteepness;
    [Range(0.5f, 10f)]
    public float falloffShift;

    //Mesh generation
    public float meshHeightMultiplier;
    public AnimationCurve meshCurve;
    [Range(1, 6)]
    public int lOD;

    public void GenerateMap()
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
                map = VoronoiNoise.GenerateNoiseMap(seed, mapSize, seedCount);
                break;
            case NoiseType.CellularNoise:
                map = CellularAutomata.GenerateNoiseMap(mapSize, seed, steps, blurPasses);
                break;
            case NoiseType.Falloff:
                map = MapFalloff.GenerateFalloff(mapSize, falloffSteepness, falloffShift);
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
