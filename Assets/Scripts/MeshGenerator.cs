using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail, TerrainGenerator.NoiseType noiseType)
    {
        AnimationCurve newHeightCurve = new AnimationCurve(heightCurve.keys);

        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        if (noiseType == TerrainGenerator.NoiseType.DiamondSquare)
        {
            width = heightMap.GetLength(0) - 1;
            height = heightMap.GetLength(1) - 1;
        }

        float topLeftX = (width - 1) / 2f;
        float topLeftZ = (height - 1) / 2f;

        // Prevent LOD if noise type is "diamond square"
        int detailIncrement = (noiseType == TerrainGenerator.NoiseType.DiamondSquare) ? 1 : (levelOfDetail == 0 ? 1 : levelOfDetail * 2);
        int verticesPerLine = (width - 1) / detailIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        for (int y = 0; y < height; y += detailIncrement)
        {
            for (int x = 0; x < width; x += detailIncrement)
            {
                float heightValue = newHeightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;

                meshData.vertices[vertexIndex] = new Vector3(topLeftX - x, heightValue, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(0, heightValue / heightMultiplier);

                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices { get; private set; }
    public int[] triangles { get; private set; }
    public Vector2[] uvs { get; private set; }

    private int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = c;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = a;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };
        mesh.RecalculateNormals();
        return mesh;
    }
}
