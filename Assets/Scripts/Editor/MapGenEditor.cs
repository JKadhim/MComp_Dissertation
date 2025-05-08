using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGenEditor : Editor
{
    SerializedProperty drawMode;
    SerializedProperty noiseType;

    // Perlin noise properties
    SerializedProperty noiseScale;
    SerializedProperty octaves;
    SerializedProperty persistence;
    SerializedProperty lacunarity;
    SerializedProperty offset;

    // Diamond square properties
    SerializedProperty roughness;

    // Voronoi noise properties
    SerializedProperty seedCount;

    // Cellular automata properties
    SerializedProperty steps;
    SerializedProperty blurPasses;

    // Map generation properties
    SerializedProperty seed;

    // Falloff map properties
    SerializedProperty useFalloff;
    SerializedProperty falloffSteepness;
    SerializedProperty falloffShift;

    // Mesh generation properties
    SerializedProperty meshHeightMultiplier;
    SerializedProperty meshCurve;
    SerializedProperty lOD;


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
        useFalloff = serializedObject.FindProperty("useFalloff");
        falloffSteepness = serializedObject.FindProperty("falloffSteepness");
        falloffShift = serializedObject.FindProperty("falloffShift");
        meshHeightMultiplier = serializedObject.FindProperty("meshHeightMultiplier");
        meshCurve = serializedObject.FindProperty("meshCurve");
        lOD = serializedObject.FindProperty("lOD");
        seedCount = serializedObject.FindProperty("seedCount");
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

        MapGenerator.DrawMode selectedDrawMode = (MapGenerator.DrawMode)drawMode.enumValueIndex;

        if (selectedNoiseType != MapGenerator.NoiseType.Falloff) 
        {
            EditorGUILayout.PropertyField(seed);
            EditorGUILayout.PropertyField(useFalloff);
        }

        if (useFalloff.boolValue || selectedNoiseType == MapGenerator.NoiseType.Falloff)
        {
            EditorGUILayout.TextField("Falloff Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(falloffSteepness);
            EditorGUILayout.PropertyField(falloffShift);
        }

        switch (selectedNoiseType)
        {
            case MapGenerator.NoiseType.PerlinNoise:
                EditorGUILayout.TextField("Perlin Noise Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(noiseScale);
                EditorGUILayout.PropertyField(octaves);
                EditorGUILayout.PropertyField(persistence);
                EditorGUILayout.PropertyField(lacunarity);
                EditorGUILayout.PropertyField(offset);
                break;

            case MapGenerator.NoiseType.DiamondSquareNoise:
                EditorGUILayout.TextField("Diamond Square Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(roughness);
                break;

            case MapGenerator.NoiseType.VoronoiNoise:
                EditorGUILayout.TextField("Voronoi Noise Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(seedCount);
                break;

            case MapGenerator.NoiseType.CellularNoise:
                EditorGUILayout.TextField("Cellular Noise Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(steps);
                EditorGUILayout.PropertyField(blurPasses);
                break;
        }

        switch (selectedDrawMode)
        {
            case MapGenerator.DrawMode.NoiseMap:
                break;
            case MapGenerator.DrawMode.Mesh:
                EditorGUILayout.TextField("Mesh Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(meshHeightMultiplier);
                EditorGUILayout.PropertyField(meshCurve);
                EditorGUILayout.PropertyField(lOD);
                break;
        }

        // Add a button to generate the map
        if (GUILayout.Button("Generate Map"))
        {
            MapGenerator mapGenerator = (MapGenerator)target;
            mapGenerator.GenerateMap();
        }

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}
