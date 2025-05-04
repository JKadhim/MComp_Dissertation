using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGenEditor : Editor
{
    SerializedProperty drawMode;
    SerializedProperty noiseType;
    SerializedProperty noiseScale;
    SerializedProperty octaves;
    SerializedProperty persistence;
    SerializedProperty lacunarity;
    SerializedProperty offset;
    SerializedProperty roughness;
    SerializedProperty steps;
    SerializedProperty seed;
    SerializedProperty blurPasses;

    private void OnEnable()
    {
        // Link serialized properties to the fields in MapGenerator
        drawMode = serializedObject.FindProperty("drawMode");
        noiseType = serializedObject.FindProperty("noiseType");
        noiseScale = serializedObject.FindProperty("noiseScale");
        octaves = serializedObject.FindProperty("octaves");
        persistence = serializedObject.FindProperty("persistence");
        lacunarity = serializedObject.FindProperty("lacunarity");
        offset = serializedObject.FindProperty("offset");
        roughness = serializedObject.FindProperty("roughness");
        steps = serializedObject.FindProperty("steps");
        seed = serializedObject.FindProperty("seed");
        blurPasses = serializedObject.FindProperty("blurPasses");
    }

    public override void OnInspectorGUI()
    {
        // Update the serialized object
        serializedObject.Update();

        EditorGUILayout.PropertyField(drawMode);

        // Draw the noiseType dropdown
        EditorGUILayout.PropertyField(noiseType);

        // Conditionally display fields based on the selected noiseType
        MapGenerator.NoiseType selectedNoiseType = (MapGenerator.NoiseType)noiseType.enumValueIndex;

        EditorGUILayout.PropertyField(seed);

        switch (selectedNoiseType)
        {
            case MapGenerator.NoiseType.PerlinNoise:
                EditorGUILayout.PropertyField(noiseScale);
                EditorGUILayout.PropertyField(octaves);
                EditorGUILayout.PropertyField(persistence);
                EditorGUILayout.PropertyField(lacunarity);
                EditorGUILayout.PropertyField(offset);
                break;

            case MapGenerator.NoiseType.DiamondSquareNoise:
                EditorGUILayout.PropertyField(roughness);
                break;

            case MapGenerator.NoiseType.VoronoiNoise:
                // No additional fields for VoronoiNoise
                break;

            case MapGenerator.NoiseType.CellularNoise:
                EditorGUILayout.PropertyField(steps);
                EditorGUILayout.PropertyField(blurPasses);
                break;
        }

        // Add a button to generate the map
        if (GUILayout.Button("Generate Map"))
        {
            MapGenerator mapGenerator = (MapGenerator)target;
            mapGenerator.Generate();
        }

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}
