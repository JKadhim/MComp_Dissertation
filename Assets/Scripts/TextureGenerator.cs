using UnityEngine;

public static class TextureGenerator
{
    // Creates a texture from a given color map.
    public static Texture2D TextureFromColorMap(Color[] pixelColors, int textureWidth, int textureHeight)
    {
        Texture2D generatedTexture = new Texture2D(textureWidth, textureHeight);

        // Set texture properties for better control over rendering.
        generatedTexture.filterMode = FilterMode.Point;
        generatedTexture.wrapMode = TextureWrapMode.Clamp;

        // Apply the color map to the texture.
        generatedTexture.SetPixels(pixelColors);
        generatedTexture.Apply();
        return generatedTexture;
    }

    // Creates a texture from a height map by converting height values to grayscale colors.
    public static Texture2D TextureFromNoiseMap(float[,] elevationMap)
    {
        int mapWidth = elevationMap.GetLength(0);
        int mapHeight = elevationMap.GetLength(1);

        Color[] grayscaleColors = new Color[mapWidth * mapHeight];

        // Map height values to grayscale colors.
        for (int row = 0; row < mapHeight; row++)
        {
            int rowOffset = row * mapWidth;
            for (int col = 0; col < mapWidth; col++)
            {
                float normalizedHeight = elevationMap[col, row];
                grayscaleColors[rowOffset + col] = Color.Lerp(Color.black, Color.white, normalizedHeight);
            }
        }

        // Use the color map to generate the texture.
        return TextureFromColorMap(grayscaleColors, mapWidth, mapHeight);
    }
}
