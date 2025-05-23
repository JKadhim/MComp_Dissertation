using Unity.Mathematics;
using UnityEngine;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
    [Min(1)]
    public int perlinOctaves;
    [Range(0, 1)]
    public float perlinPersistence;
    [Min(1)]
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
    Queue<ThreadedData<Mesh>> meshDataQueue = new Queue<ThreadedData<Mesh>>();

    //Benchmark Data
    [Range(1, 1000)]
    public int benchmarkIterations = 1000;
    public GameObject performanceTracker;

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
                UnityEngine.Debug.Log("GameObject 'NoisePlane' is already inactive.");
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("GameObject 'NoisePlane' not found in the scene.");
        }

        if (editorMesh != null)
        {
            if (editorMesh.activeSelf)
            {
                editorMesh.SetActive(false);
            }
            else
            {
                UnityEngine.Debug.Log("GameObject 'EditorMesh' is already inactive.");
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("GameObject 'EditorMesh' not found in the scene.");
        }
        if (!performanceTracker.activeSelf)
        {
            performanceTracker.SetActive(true);
        }
        else
        {
            UnityEngine.Debug.Log("GameObject 'PerformanceTracker' is already active.");
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

    // Generates the terrain in the Unity Editor based on the selected render mode.
    public void GenerateTerrainInEditor()
    {
        terrainMap = GenerateTerrain(Vector2.zero);
        NoiseDisplay display = FindFirstObjectByType<NoiseDisplay>();

        switch (renderMode)
        {
            case RenderMode.HeightMap:
                display.DrawTexture(TextureGenerator.TextureFromNoiseMap(terrainMap));
                break;
            case RenderMode.TerrainMesh:
                display.DrawMesh(TerrainMeshGenerator.GenerateTerrainMesh(terrainMap, terrainHeightMultiplier, terrainHeightCurve, editorLevelOfDetail, noiseType));
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
    public void RequestMesh(Action<Mesh> callback, float[,] terrainData, int levelOfDetail)
    {
        ThreadStart threadStart = delegate
        {
            MeshThread(callback, terrainData, levelOfDetail);
        };
        new Thread(threadStart).Start();
    }

    // Generates the mesh data on a separate thread and enqueues the result.
    void MeshThread(Action<Mesh> callback, float[,] terrainData, int levelOfDetail)
    {
        Mesh meshData = TerrainMeshGenerator.GenerateTerrainMesh(terrainData, terrainHeightMultiplier, terrainHeightCurve, levelOfDetail, noiseType);
        lock (meshDataQueue)
        {
            meshDataQueue.Enqueue(new ThreadedData<Mesh>(callback, meshData));
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
                    ThreadedData<Mesh> threadData = meshDataQueue.Dequeue();
                    threadData.callback(threadData.data);
                }
            }
        }
    }

#if UNITY_EDITOR

    //pre-generates the falloff map in the editor
    private void OnValidate()
    {
        falloffMap = MapFalloff.GenerateFalloff(terrainSize, falloffSteepness, falloffShift);
    }

    public void BenchmarkGenerationTechniques()
    {
        UnityEngine.Debug.Log("Starting benchmark for terrain generation techniques...");

        foreach (NoiseType noise in Enum.GetValues(typeof(NoiseType)))
        {
            noiseType = noise; // Set the current noise type
            Stopwatch stopwatch = Stopwatch.StartNew();

            //tests 100 times and gets the average time
            for (int i = 0; i < benchmarkIterations; i++)
            {
                GenerateTerrain(Vector2.zero);
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log($"{noiseType} generation took {stopwatch.ElapsedMilliseconds/benchmarkIterations} ms on average");
        }

        UnityEngine.Debug.Log("Benchmark completed.");
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


