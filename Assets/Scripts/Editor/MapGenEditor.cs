using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGenEditor : Editor
{
    #region Serialized Properties

    // General properties
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
    SerializedProperty aliveChance;

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
    SerializedProperty normalizeMode;

    #endregion

    #region Unity Methods

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
        seedCount = serializedObject.FindProperty("seedCount");
        normalizeMode = serializedObject.FindProperty("normalizationMode");
        aliveChance = serializedObject.FindProperty("aliveChance");
    }

    public override void OnInspectorGUI()
    {
        // Update the serialized object
        serializedObject.Update();

        DrawGeneralSettings();
        DrawNoiseSettings();
        DrawMeshSettings();

        // Add a button to generate the map
        if (GUILayout.Button("Generate Map"))
        {
            MapGenerator mapGenerator = (MapGenerator)target;
            mapGenerator.EditorMapGeneration();
        }

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }

    #endregion

    #region Custom Methods

    private void DrawGeneralSettings()
    {
        EditorGUILayout.PropertyField(drawMode);
        EditorGUILayout.PropertyField(noiseType);

        MapGenerator.NoiseType selectedNoiseType = (MapGenerator.NoiseType)noiseType.enumValueIndex;

        if (selectedNoiseType != MapGenerator.NoiseType.Falloff)
        {
            EditorGUILayout.PropertyField(seed);
            EditorGUILayout.PropertyField(useFalloff);
        }

        if (useFalloff.boolValue || selectedNoiseType == MapGenerator.NoiseType.Falloff)
        {
            EditorGUILayout.LabelField("Falloff Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(falloffSteepness);
            EditorGUILayout.PropertyField(falloffShift);
        }
    }

    private void DrawNoiseSettings()
    {
        MapGenerator.NoiseType selectedNoiseType = (MapGenerator.NoiseType)noiseType.enumValueIndex;

        switch (selectedNoiseType)
        {
            case MapGenerator.NoiseType.PerlinNoise:
                EditorGUILayout.LabelField("Perlin Noise Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(noiseScale);
                EditorGUILayout.PropertyField(octaves);
                EditorGUILayout.PropertyField(persistence);
                EditorGUILayout.PropertyField(lacunarity);
                EditorGUILayout.PropertyField(offset);
                break;

            case MapGenerator.NoiseType.DiamondSquareNoise:
                EditorGUILayout.LabelField("Diamond Square Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(roughness);
                break;

            case MapGenerator.NoiseType.VoronoiNoise:
                EditorGUILayout.LabelField("Voronoi Noise Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(seedCount);
                break;

            case MapGenerator.NoiseType.CellularNoise:
                EditorGUILayout.LabelField("Cellular Noise Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(aliveChance);
                EditorGUILayout.PropertyField(steps);
                EditorGUILayout.PropertyField(blurPasses);
                break;
        }
    }

    private void DrawMeshSettings()
    {
        MapGenerator.DrawMode selectedDrawMode = (MapGenerator.DrawMode)drawMode.enumValueIndex;

        if (selectedDrawMode == MapGenerator.DrawMode.Mesh)
        {
            EditorGUILayout.LabelField("Mesh Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(meshHeightMultiplier);
            EditorGUILayout.PropertyField(meshCurve);
            EditorGUILayout.PropertyField(normalizeMode);
        }
    }

    #endregion
}
