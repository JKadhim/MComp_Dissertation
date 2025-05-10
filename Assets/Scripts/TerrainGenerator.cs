using Unity.Mathematics;
using UnityEngine;
using System.Threading;
using System;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour
{
    public enum RenderMode
    {
        HeightMap,
        TerrainMesh
    }

    public enum NoiseType
    {
        Perlin,
        DiamondSquare,
        Voronoi,
        CellularAutomata,
        FalloffMap
    }

    public RenderMode renderMode;
    public NoiseType noiseType;

    public PerlinNoise.NormalizationMode normalizationMode;

    // Terrain data
    float[,] terrainMap;
    public int randomSeed;

    public static int terrainSize = 241;
    public static int diamondSquareSize = 257; // Diamond square size

    // Perlin noise parameters
    [Min(0.01f)]
    public float perlinScale;
    public Vector2 perlinOffset;
    public int perlinOctaves;
    [Range(0, 1)]
    public float perlinPersistence;
    public float perlinLacunarity;

    // Diamond square parameters
    public float diamondSquareRoughness = 2.0f;

    // Voronoi noise parameters
    [Min(5)]
    public int voronoiSeedCount = 10;

    // Cellular automata parameters
    [Range(1, 15)]
    public int cellularSteps;
    [Range(0, 10)]
    public int cellularBlurPasses;
    [Range(0.1f, 0.4f)]
    public float cellularAliveChance;

    // Falloff map parameters
    public bool applyFalloff;
    float[,] falloffMap;
    [Range(1, 10)]
    public float falloffSteepness;
    [Range(0.5f, 10f)]
    public float falloffShift;

    // Mesh generation parameters
    public float terrainHeightMultiplier = 10;
    public AnimationCurve terrainHeightCurve;
    [Range(1, 6)]
    public int editorLevelOfDetail = 1;

    Queue<ThreadedData<float[,]>> terrainDataQueue = new Queue<ThreadedData<float[,]>>();
    Queue<ThreadedData<MeshData>> meshDataQueue = new Queue<ThreadedData<MeshData>>();

    // Generates the terrain in the Unity Editor based on the selected render mode.
    public void GenerateTerrainInEditor()
    {
        terrainMap = GenerateTerrain(Vector2.zero);
        NoiseDisplay display = FindFirstObjectByType<NoiseDisplay>();

        switch (renderMode)
        {
            case RenderMode.HeightMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(terrainMap));
                break;
            case RenderMode.TerrainMesh:
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(terrainMap, terrainHeightMultiplier, terrainHeightCurve, editorLevelOfDetail, noiseType));
                break;
        }
    }

    // Requests terrain generation on a separate thread and invokes the callback when done.
    public void RequestTerrain(Vector2 center, Action<float[,]> callback)
    {
        ThreadStart threadStart = delegate
        {
            TerrainThread(center, callback);
        };

        new Thread(threadStart).Start();
    }

    // Generates the terrain data on a separate thread and enqueues the result.
    void TerrainThread(Vector2 center, Action<float[,]> callback)
    {
        float[,] terrainData = GenerateTerrain(center);
        lock (terrainDataQueue)
        {
            terrainDataQueue.Enqueue(new ThreadedData<float[,]>(callback, terrainData));
        }
    }

    // Requests mesh generation on a separate thread and invokes the callback when done.
    public void RequestMesh(Action<MeshData> callback, float[,] terrainData, int levelOfDetail)
    {
        ThreadStart threadStart = delegate
        {
            MeshThread(callback, terrainData, levelOfDetail);
        };
        new Thread(threadStart).Start();
    }

    // Generates the mesh data on a separate thread and enqueues the result.
    void MeshThread(Action<MeshData> callback, float[,] terrainData, int levelOfDetail)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(terrainData, terrainHeightMultiplier, terrainHeightCurve, levelOfDetail, noiseType);
        lock (meshDataQueue)
        {
            meshDataQueue.Enqueue(new ThreadedData<MeshData>(callback, meshData));
        }
    }

    // Initializes the scene by deactivating the "NoisePlane" and "EditorMesh" GameObjects if they exist.
    private void Start()
    {
        GameObject noisePlane = GameObject.Find("NoisePlane");
        GameObject editorMesh = GameObject.Find("EditorMesh");

        if (noisePlane != null)
        {
            if (noisePlane.activeSelf)
            {
                noisePlane.SetActive(false);
            }
            else
            {
                Debug.Log("GameObject 'NoisePlane' is already inactive.");
            }
        }
        else
        {
            Debug.LogWarning("GameObject 'NoisePlane' not found in the scene.");
        }

        if (editorMesh != null)
        {
            if (editorMesh.activeSelf)
            {
                editorMesh.SetActive(false);
            }
            else
            {
                Debug.Log("GameObject 'EditorMesh' is already inactive.");
            }
        }
        else
        {
            Debug.LogWarning("GameObject 'EditorMesh' not found in the scene.");
        }
    }

    // Processes the queued terrain and mesh data callbacks on the main thread.
    public void Update()
    {
        lock (terrainDataQueue)
        {
            if (terrainDataQueue.Count > 0)
            {
                for (int i = 0; i < terrainDataQueue.Count; i++)
                {
                    ThreadedData<float[,]> threadData = terrainDataQueue.Dequeue();
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
                    ThreadedData<MeshData> threadData = meshDataQueue.Dequeue();
                    threadData.callback(threadData.data);
                }
            }
        }
    }

    // Generates the terrain based on the selected noise algorithm and parameters.
    float[,] GenerateTerrain(Vector2 center)
    {
        switch (noiseType)
        {
            case NoiseType.Perlin:
                terrainMap = PerlinNoise.GenerateNoiseMap(terrainSize, randomSeed, perlinScale, perlinOctaves, perlinPersistence, perlinLacunarity, center + perlinOffset, normalizationMode);
                break;
            case NoiseType.DiamondSquare:
                terrainMap = DiamondSquare.GenerateNoiseMap(diamondSquareSize, diamondSquareRoughness, randomSeed);
                break;
            case NoiseType.Voronoi:
                terrainMap = VoronoiNoise.GenerateNoiseMap(randomSeed, terrainSize, voronoiSeedCount);
                break;
            case NoiseType.CellularAutomata:
                terrainMap = CellularAutomata.GenerateNoiseMap(terrainSize, randomSeed, cellularSteps, cellularBlurPasses, cellularAliveChance);
                break;
            case NoiseType.FalloffMap:
                terrainMap = MapFalloff.GenerateFalloff(terrainSize, falloffSteepness, falloffShift);
                break;
        }

        return terrainMap;
    }

#if UNITY_EDITOR

    // Validates and clamps certain parameters in the Unity Editor.
    private void OnValidate()
    {
        if (perlinLacunarity < 1)
        {
            perlinLacunarity = 1;
        }
        if (perlinOctaves < 1)
        {
            perlinOctaves = 1;
        }

        falloffMap = MapFalloff.GenerateFalloff(terrainSize, falloffSteepness, falloffShift);
    }

#endif

    // Struct to hold thread data and its associated callback.
    struct ThreadedData<T>
    {
        public readonly Action<T> callback;
        public readonly T data;

        public ThreadedData(Action<T> callback, T data)
        {
            this.callback = callback;
            this.data = data;
        }
    }
}


