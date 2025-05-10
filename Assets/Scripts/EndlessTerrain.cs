using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class EndlessTerrain : MonoBehaviour
{
    const float scale = 1f;

    public static float maxViewDistance;
    public LevelOfDetailData[] detailLevels;

    const float updateDistance = 10f;
    const float updateDistanceSqr = updateDistance * updateDistance;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    Vector2 lastViewerPosition;

    static MapGenerator mapGenerator;

    int chunkSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> chunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        mapGenerator = FindFirstObjectByType<MapGenerator>();
        maxViewDistance = detailLevels[detailLevels.Length - 1].range;
        chunkSize = MapGenerator.mapSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();
    }

    void Update()
    {
        var pos = viewer.position / scale;
        viewerPosition = new Vector2(pos.x, pos.z);
        if ((lastViewerPosition - viewerPosition).sqrMagnitude > updateDistanceSqr)
        {
            lastViewerPosition = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }

        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (chunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    chunkDictionary[viewedChunkCoord].UpdateChunk();                  
                }
                else
                {
                    chunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }

    public class TerrainChunk
    {
        public GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        LevelOfDetailData[] detailLevels;
        LevelOfDetailMesh[] levelOfDetailMeshes;

        float[,] mapInfo;
        bool mapReceived;
        int priorIndex = -1;


        public TerrainChunk(Vector2 coord, int size, LevelOfDetailData[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Mesh");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;

            SetVisible(false);

            levelOfDetailMeshes = new LevelOfDetailMesh[detailLevels.Length];

            for (int i = 0; i < detailLevels.Length; i++)
            {
                levelOfDetailMeshes[i] = new LevelOfDetailMesh(detailLevels[i].lOD, UpdateChunk);                
            }

            mapGenerator.RequestMap(position, onMapDataReceived);
        }

        void onMapDataReceived(float[,] mapInfo)
        {
            this.mapInfo = mapInfo;
            mapReceived = true;

            Texture2D texture = TextureGenerator.TextureFromHeightMap(mapInfo);
            meshRenderer.material.mainTexture = texture;

            UpdateChunk();
        }

        public void UpdateChunk()
        {
            if (!mapReceived)
            {
                return;
            }
            float distFromEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = distFromEdge <= maxViewDistance;

            if (visible) 
            { 
                int index = 0;

                for (int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if (distFromEdge > detailLevels[i].range)
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
                        levelOfDetailMesh.RequestMesh(mapInfo);
                    }
                }

                terrainChunksVisibleLastUpdate.Add(this);
            }

            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    class LevelOfDetailMesh
    {
        public Mesh mesh;
        public bool requested;
        public bool recieved;
        int levelOfDetail;

        System.Action updateCallback;

        public LevelOfDetailMesh(int levelOfDetail, System.Action updateCallback)
        {
            this.levelOfDetail = levelOfDetail;
            this.updateCallback = updateCallback;
        }

        void onMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            recieved = true;

            updateCallback();
        }

        public void RequestMesh(float[,] map)
        {
            requested = true;
            mapGenerator.RequestMesh(onMeshDataReceived, map, levelOfDetail);
        }
    }

    [System.Serializable]
    public struct LevelOfDetailData
    {
        public int lOD;
        public float range;
    }
}
