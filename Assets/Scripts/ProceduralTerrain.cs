using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class ProceduralTerrain : MonoBehaviour
{
    // Constants
    const float scale = 1f; // Scale factor for terrain
    const float updateDistance = 10f; // Distance threshold for updating chunks
    const float updateDistanceSqr = updateDistance * updateDistance; // Squared distance for optimization

    // Public Fields
    public static float maxViewDistance;
    public LevelOfDetailData[] detailLevels;
    public Transform cameraTransform;
    public Material mapMaterial;
    [Range(1, 1000)]
    public int benchmarkIterations = 10;

    // Static Fields
    public static Vector2 cameraPosition;
    static TerrainGenerator terrainGenerator; 

    // Private Fields
    Vector2 lastCameraPosition;
    int tileSize;
    int visibleTiles;
    bool benchmarkCompleted = false; // Flag to check if the benchmark is completed

    Dictionary<Vector2, TerrainTile> tileDict = new Dictionary<Vector2, TerrainTile>(); // Dictionary to store terrain tiles
    static List<TerrainTile> priorTilesVisible = new List<TerrainTile>();

    // Initializes the terrain system by setting up the map generator, calculating tile size, and updating visible chunks.
    void Start()
    {
        terrainGenerator = FindFirstObjectByType<TerrainGenerator>();
        maxViewDistance = detailLevels[detailLevels.Length - 1].range;

        // Determine tile size based on the noise type
        tileSize = terrainGenerator.noiseType == TerrainGenerator.NoiseType.DiamondSquare
            ? TerrainGenerator.diamondSquareSize - 1
            : TerrainGenerator.terrainSize - 1;

        // Calculate the number of tiles visible within the view distance
        visibleTiles = Mathf.RoundToInt(maxViewDistance / tileSize);

        // Update visible tiles at the start
        UpdateVisibleTiles();

        // Benchmark the terrain generation
        //StartCoroutine(BenchmarkTerrainGenerationCoroutine(benchmarkIterations, 0.0f));
    }

    // Updates the cameraTransform's position and checks if the visible tiles need to be updated.
    void Update()
    {
        //if (!benchmarkCompleted) return; // Skip update if benchmark is completed

        var pos = cameraTransform.position / scale;
        cameraPosition = new Vector2(pos.x, pos.z);

        // Update tiles only if the cameraTransform has moved a significant distance
        if ((lastCameraPosition - cameraPosition).sqrMagnitude > updateDistanceSqr)
        {
            lastCameraPosition = cameraPosition;
            UpdateVisibleTiles();
        }
    }

    IEnumerator BenchmarkTerrainGenerationCoroutine(int iterations, float waitTime)
    {
        UnityEngine.Debug.Log($"Starting terrain generation benchmark for {iterations} iterations...");

        long totalMilliseconds = 0;

        for (int i = 0; i < iterations; i++)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Update visible tiles (this triggers terrain generation)
            UpdateVisibleTiles();

            stopwatch.Stop();
            totalMilliseconds += stopwatch.ElapsedMilliseconds;

            UnityEngine.Debug.Log($"Iteration {i + 1}: {stopwatch.ElapsedMilliseconds} ms");

            DeleteAllTerrainTiles();
            priorTilesVisible.Clear(); // Clear the list of visible tiles for the next iteration
            tileDict.Clear(); // Clear the tile dictionary for the next iteration
            //DeleteAllTerrainTiles(); // Delete all terrain tiles to avoid clutter

            // Wait for the specified time before the next iteration
            yield return new WaitForSeconds(waitTime);
        }

        float averageMilliseconds = totalMilliseconds / (float)iterations;
        UnityEngine.Debug.Log($"Average terrain generation time over {iterations} iterations: {averageMilliseconds} ms");
        UpdateVisibleTiles(); // Update visible tiles after the benchmark
        benchmarkCompleted = true; // Set the benchmark completed flag to true
    }

    void DeleteAllTerrainTiles()
    {
        GameObject[] terrainTiles = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (GameObject tile in terrainTiles)
        {
            if (tile.name == "Terrain Mesh")
            {
                GameObject.Destroy(tile);
            }
        }
    }

    // Updates the visibility of terrain tiles based on the cameraTransform's position.
    void UpdateVisibleTiles()
    {
        // Hide all tiles from the last update
        foreach (var tile in priorTilesVisible)
        {
            tile.SetVisible(false);
        }

        priorTilesVisible.Clear();

        // Calculate the current tile coordinates
        int currentTilePosX = Mathf.RoundToInt(cameraPosition.x / tileSize);
        int currentTilePosY = Mathf.RoundToInt(cameraPosition.y / tileSize);

        // Loop through visible chunks and update or create them
        for (int yOffset = -visibleTiles; yOffset <= visibleTiles; yOffset++)
        {
            for (int xOffset = -visibleTiles; xOffset <= visibleTiles; xOffset++)
            {
                Vector2 viewedTilePos = new Vector2(currentTilePosX + xOffset, currentTilePosY + yOffset);

                if (tileDict.ContainsKey(viewedTilePos))
                {
                    // Update existing tile
                    tileDict[viewedTilePos].UpdateTile();
                }
                else
                {
                    // Create a new tile if it doesn't exist
                    tileDict.Add(viewedTilePos, new TerrainTile(viewedTilePos, tileSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }

    public class TerrainTile
    {
        public GameObject meshObject; // GameObject representing the terrain tile
        Vector2 position; // Position of the tile
        Bounds bounds; // Bounds of the tile

        MeshRenderer meshRenderer; // Renderer for the tile's mesh
        MeshFilter meshFilter; // Filter for the tile's mesh

        LevelOfDetailData[] lodDetails; // LOD data for the tile
        LevelOfDetailMesh[] levelOfDetailMeshes; // LOD meshes for the tile

        float[,] heightMap; // Heightmap data for the tile
        bool mapReceived; // Flag to check if map data is received
        int priorIndex = -1; // Previously used LOD index

        // Initializes a new terrain tile at the specified coordinates.
        public TerrainTile(Vector2 position, int size, LevelOfDetailData[] lodDetails, Transform parent, Material material)
        {
            this.lodDetails = lodDetails;
            this.position = position * size;
            bounds = new Bounds(this.position, Vector2.one * size);
            Vector3 pos3D = new Vector3(this.position.x, 0, this.position.y);

            // Create and configure the tile's GameObject
            meshObject = new GameObject("Terrain Mesh");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = pos3D * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;

            SetVisible(false);

            // Initialize LOD meshes
            levelOfDetailMeshes = new LevelOfDetailMesh[lodDetails.Length];
            for (int i = 0; i < lodDetails.Length; i++)
            {
                levelOfDetailMeshes[i] = new LevelOfDetailMesh(lodDetails[i].lOD, UpdateTile);
            }

            // Request map data for the tile
            terrainGenerator.RequestTerrain(this.position, onMapDataReceived);
        }

        // Callback for when map data is received. Generates a texture and updates the tile.
        void onMapDataReceived(float[,] mapInfo)
        {
            if (meshRenderer == null)
            {
                UnityEngine.Debug.LogWarning("MeshRenderer has been destroyed. Skipping map data processing.");
                return;
            }

            this.heightMap = mapInfo;
            mapReceived = true;

            // Generate texture from heightmap and apply it to the material
            Texture2D texture = TextureGenerator.TextureFromNoiseMap(mapInfo);
            meshRenderer.material.mainTexture = texture;

            UpdateTile();
        }

        // Updates the tile's visibility and LOD based on the cameraTransform's position.
        public void UpdateTile()
        {
            if (!mapReceived) return;

            // Calculate distance from the cameraTransform to the tile
            float distFromEdge = Mathf.Sqrt(bounds.SqrDistance(cameraPosition));
            bool visible = distFromEdge <= maxViewDistance;

            if (visible)
            {
                if (terrainGenerator.noiseType == TerrainGenerator.NoiseType.DiamondSquare)
                {
                    // Always use the highest detail level for Diamond Square noise
                    if (levelOfDetailMeshes[0].recieved)
                    {
                        meshFilter.mesh = levelOfDetailMeshes[0].mesh;
                    }
                    else if (!levelOfDetailMeshes[0].requested)
                    {
                        levelOfDetailMeshes[0].RequestMesh(heightMap);
                    }
                }
                else
                {
                    // Standard LOD logic
                    int index = 0;
                    for (int i = 0; i < lodDetails.Length - 1; i++)
                    {
                        if (distFromEdge > lodDetails[i].range)
                        {
                            index = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (index != priorIndex)
                    {
                        LevelOfDetailMesh levelOfDetailMesh = levelOfDetailMeshes[index];
                        if (levelOfDetailMesh.recieved)
                        {
                            priorIndex = index;
                            meshFilter.mesh = levelOfDetailMesh.mesh;
                        }
                        else if (!levelOfDetailMesh.requested)
                        {
                            levelOfDetailMesh.RequestMesh(heightMap);
                        }
                    }
                }

                priorTilesVisible.Add(this);
            }

            SetVisible(visible);
        }

        // Sets the visibility of the tile.
        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        // Checks if the tile is currently visible.
        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    class LevelOfDetailMesh
    {
        public UnityEngine.Mesh mesh; // Mesh for the LOD
        public bool requested; // Flag to check if mesh data is requested
        public bool recieved; // Flag to check if mesh data is received
        int levelOfDetail; // LOD level

        System.Action updateCallback; // Callback to update the tile

        // Initializes a new LOD mesh with the specified level of detail and update callback.
        public LevelOfDetailMesh(int levelOfDetail, System.Action updateCallback)
        {
            this.levelOfDetail = levelOfDetail;
            this.updateCallback = updateCallback;
        }

        // Callback for when mesh data is received. Updates the mesh and triggers the update callback.
        void onMeshDataReceived(Mesh meshData)
        {
            mesh = meshData.CreateMesh();
            recieved = true;

            updateCallback();
        }

        // Requests mesh data for the LOD.
        public void RequestMesh(float[,] map)
        {
            requested = true;
            terrainGenerator.RequestMesh(onMeshDataReceived, map, levelOfDetail);
        }
    }

    [System.Serializable]
    public struct LevelOfDetailData
    {
        public int lOD; // LOD level
        public float range; // Range for this LOD
    }
}
