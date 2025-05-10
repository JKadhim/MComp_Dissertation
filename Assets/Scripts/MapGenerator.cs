using Unity.Mathematics;
using UnityEngine;
using System.Threading;
using System;
using System.Collections.Generic;

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

    public PerlinNoise.NormalizationMode normalizationMode;

    //Map generation
    float[,] map;
    public int seed;
    
    public static int mapSize = 241;
    public static int sizeDS = 257; //Diamond square size

    //Perlin noise
    [Min(0.01f)]
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
    [Range(0.1f, 0.4f)]
    public float aliveChance;

    //Falloff map
    public bool useFalloff;
    float[,] falloffMap;
    [Range(1, 10)]
    public float falloffSteepness;
    [Range(0.5f, 10f)]
    public float falloffShift;

    //Mesh generation
    public float meshHeightMultiplier = 10;
    public AnimationCurve meshCurve;
    [Range(1, 6)]
    public int editorLOD = 1;

    Queue<MapThreadData<float[,]>> mapDataQueue = new Queue<MapThreadData<float[,]>>();
    Queue<MapThreadData<MeshData>> meshDataQueue = new Queue<MapThreadData<MeshData>>();


    public void EditorMapGeneration()
    {
        map = GenerateMap(Vector2.zero);
        MapDisplay display = FindFirstObjectByType<MapDisplay>();

        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(map));
                break;
            case DrawMode.Mesh:
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(map, meshHeightMultiplier, meshCurve, editorLOD, noiseType));
                break;
        }
    }

    public void RequestMap(Vector2 centre, Action<float[,]> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapThread(centre, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapThread(Vector2 centre, Action<float[,]> callback)
    {
        float[,] mapData = GenerateMap(centre);
        lock (mapDataQueue)
        {
            mapDataQueue.Enqueue(new MapThreadData<float[,]>(callback, mapData));
        }

    }

    public void RequestMesh(Action<MeshData> callback, float[,] mapData, int levelOfDetail)
    {
        ThreadStart threadStart = delegate
        {
            MeshThread(callback, mapData, levelOfDetail);
        };
        new Thread(threadStart).Start();
    }

    void MeshThread(Action<MeshData> callback, float[,] mapData, int levelOfDetail)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData, meshHeightMultiplier, meshCurve, levelOfDetail, noiseType);
        lock (meshDataQueue)
        {
            meshDataQueue.Enqueue(new MapThreadData<MeshData>(callback, meshData));
        }
    }

    private void Start()
    {
        GameObject.Find("Plane").SetActive(false);
        GameObject.Find("Mesh").SetActive(false);
    }

    public void Update()
    {
        lock (mapDataQueue)
        {
            if (mapDataQueue.Count > 0)
            {
                for (int i = 0; i < mapDataQueue.Count; i++)
                {
                    MapThreadData<float[,]> threadData = mapDataQueue.Dequeue();
                    threadData.callback(threadData.data);
                }
            }
        }

        lock (meshDataQueue)
        {
            if (meshDataQueue.Count > 0)
            {
                for (int i = 0; i < meshDataQueue.Count; i++)
                {
                    MapThreadData<MeshData> threadData = meshDataQueue.Dequeue();
                    threadData.callback(threadData.data);
                }
            }
        }
    }

    float[,] GenerateMap(Vector2 centre)
    {
        switch (noiseType)
        {
            case NoiseType.PerlinNoise:
                map = PerlinNoise.GenerateNoiseMap(mapSize, seed, noiseScale, octaves, persistence, lacunarity, centre + offset, normalizationMode);
                break;
            case NoiseType.DiamondSquareNoise:
                map = DiamondSquare.GenerateNoiseMap(sizeDS, roughness, seed);
                break;
            case NoiseType.VoronoiNoise:
                map = VoronoiNoise.GenerateNoiseMap(seed, mapSize, seedCount);
                break;
            case NoiseType.CellularNoise:
                map = CellularAutomata.GenerateNoiseMap(mapSize, seed, steps, blurPasses, aliveChance);
                break;
            case NoiseType.Falloff:
                map = MapFalloff.GenerateFalloff(mapSize, falloffSteepness, falloffShift);
                break;
        }

        return map;
    }

#if UNITY_EDITOR

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

#endif

    struct MapThreadData<T>
    {
        public readonly Action<T> callback;
        public readonly T data;

        public MapThreadData(Action<T> callback, T data)
        {
            this.callback = callback;
            this.data = data;
        }
    }
}
