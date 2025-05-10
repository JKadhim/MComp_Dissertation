using UnityEngine;

public static class TerrainMeshGenerator
{
    // Generates a terrain mesh based on the provided height map, height multiplier, height curve, level of detail, and noise type.
    public static Mesh GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail, TerrainGenerator.NoiseType noiseType)
    {
        AnimationCurve adjustedHeightCurve = new AnimationCurve(heightCurve.keys);

        int meshWidth = heightMap.GetLength(0);
        int meshHeight = heightMap.GetLength(1);

        // Adjust dimensions for DiamondSquare noise type
        if (noiseType == TerrainGenerator.NoiseType.DiamondSquare)
        {
            meshWidth = heightMap.GetLength(0) - 1;
            meshHeight = heightMap.GetLength(1) - 1;
        }

        float topLeftX = (meshWidth - 1) / 2f;
        float topLeftZ = (meshHeight - 1) / 2f;

        // Determine the step size based on the level of detail and noise type
        int stepSize = (noiseType == TerrainGenerator.NoiseType.DiamondSquare) ? 1 : (levelOfDetail == 0 ? 1 : levelOfDetail * 2);
        int verticesPerLine = (meshWidth - 1) / stepSize + 1;

        Mesh mesh = new Mesh(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        // Loop through the height map to generate vertices and triangles
        for (int y = 0; y < meshHeight; y += stepSize)
        {
            for (int x = 0; x < meshWidth; x += stepSize)
            {
                float vertexHeight = adjustedHeightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;

                mesh.vertices[vertexIndex] = new Vector3(topLeftX - x, vertexHeight, topLeftZ - y);
                mesh.uvs[vertexIndex] = new Vector2(0, vertexHeight / heightMultiplier);

                // Add triangles for the current vertex if it's not on the edge
                if (x < meshWidth - 1 && y < meshHeight - 1)
                {
                    mesh.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    mesh.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        return mesh;
    }
}

public class Mesh
{
    public Vector3[] vertices { get; private set; } // Array of vertices for the mesh
    public Vector2[] uvs { get; private set; }     // Array of UV coordinates for the mesh
    public int[] triangles { get; private set; }   // Array of triangle indices for the mesh

    private int triangleIndex; // Tracks the current index for adding triangles

    // Initializes the mesh data with the specified width and height
    public Mesh(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    // Creates and returns a Unity Mesh object from the mesh data
    public UnityEngine.Mesh CreateMesh()
    {
        UnityEngine.Mesh mesh = new UnityEngine.Mesh
        {
            vertices = vertices,
            uv = uvs,
            triangles = triangles
        };
        mesh.RecalculateNormals(); // Recalculates normals for proper lighting
        return mesh;
    }

    // Adds a triangle to the mesh using the specified vertex indices
    public void AddTriangle(int vertexA, int vertexB, int vertexC)
    {
        triangles[triangleIndex] = vertexC;
        triangles[triangleIndex + 1] = vertexB;
        triangles[triangleIndex + 2] = vertexA;
        triangleIndex += 3;
    }
}
