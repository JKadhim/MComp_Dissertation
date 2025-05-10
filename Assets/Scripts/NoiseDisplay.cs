using UnityEngine;

// Handles the display of map data, including textures and meshes.
public class NoiseDisplay : MonoBehaviour
{
    // Renderer for displaying textures on a plane.
    public Renderer textureRender;

    // Mesh components for displaying 3D terrain.
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    // Displays a texture on the texture renderer.
    public void DrawTexture(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogWarning("DrawTexture called with a null texture.");
            return;
        }

        if (textureRender == null)
        {
            Debug.LogWarning("Texture renderer is not assigned.");
            return;
        }

        // Assign the texture to the material and adjust the scale to match the texture dimensions.
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    // Displays a mesh using the provided mesh data.
    public void DrawMesh(Mesh meshData)
    {
        if (meshData == null)
        {
            Debug.LogWarning("DrawMesh called with null meshData.");
            return;
        }

        // Generate and assign the mesh to the mesh filter.
        meshFilter.sharedMesh = meshData.CreateMesh();
    }
}
