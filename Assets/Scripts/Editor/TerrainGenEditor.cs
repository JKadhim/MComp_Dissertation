using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGenEditor : Editor
{
    #region Serialized Properties

    // General properties
    SerializedProperty renderMode;
    SerializedProperty noiseType;

    // Perlin noise properties
    SerializedProperty perlinScale;
    SerializedProperty perlinOctaves;
    SerializedProperty perlinPersistence;
    SerializedProperty perlinLacunarity;
    SerializedProperty perlinOffset;

    // Diamond square properties
    SerializedProperty diamondSquareRoughness;

    // Voronoi noise properties
    SerializedProperty voronoiSeedCount;

    // Cellular automata properties
    SerializedProperty cellularSteps;
    SerializedProperty cellularBlurPasses;
    SerializedProperty cellularAliveChance;

    // Map generation properties
    SerializedProperty randomSeed;

    // Falloff map properties
    SerializedProperty applyFalloff;
    SerializedProperty falloffSteepness;
    SerializedProperty falloffShift;

    // Mesh generation properties
    SerializedProperty terrainHeightMultiplier;
    SerializedProperty terrainHeightCurve;
    SerializedProperty editorLevelOfDetail;
    SerializedProperty normalizationMode;

    private void OnEnable()
    {
        // General properties
        renderMode = serializedObject.FindProperty("renderMode");
        noiseType = serializedObject.FindProperty("noiseType");
        randomSeed = serializedObject.FindProperty("randomSeed");

        // Perlin noise properties
        perlinScale = serializedObject.FindProperty("perlinScale");
        perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        perlinPersistence = serializedObject.FindProperty("perlinPersistence");
        perlinLacunarity = serializedObject.FindProperty("perlinLacunarity");
        perlinOffset = serializedObject.FindProperty("perlinOffset");

        // Diamond square properties
        diamondSquareRoughness = serializedObject.FindProperty("diamondSquareRoughness");

        // Voronoi noise properties
        voronoiSeedCount = serializedObject.FindProperty("voronoiSeedCount");

        // Cellular automata properties
        cellularSteps = serializedObject.FindProperty("cellularSteps");
        cellularBlurPasses = serializedObject.FindProperty("cellularBlurPasses");
        cellularAliveChance = serializedObject.FindProperty("cellularAliveChance");

        // Falloff map properties
        applyFalloff = serializedObject.FindProperty("applyFalloff");
        falloffSteepness = serializedObject.FindProperty("falloffSteepness");
        falloffShift = serializedObject.FindProperty("falloffShift");

        // Mesh generation properties
        terrainHeightMultiplier = serializedObject.FindProperty("terrainHeightMultiplier");
        terrainHeightCurve = serializedObject.FindProperty("terrainHeightCurve");
        editorLevelOfDetail = serializedObject.FindProperty("editorLevelOfDetail");
        normalizationMode = serializedObject.FindProperty("normalizationMode");
    }

    public override void OnInspectorGUI()
    {
        // Update the serialized object
        serializedObject.Update();

        RenderGeneralSettings();
        RenderNoiseSettings();
        DrawMeshSettings();

        // Add a button to generate the map
        if (GUILayout.Button("Generate Map"))
        {
            TerrainGenerator terrainGenerator = (TerrainGenerator)target;
            terrainGenerator.GenerateTerrainInEditor();
        }

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }

    #endregion

    #region Custom Methods

    private void RenderGeneralSettings()
    {
        EditorGUILayout.PropertyField(renderMode);
        EditorGUILayout.PropertyField(noiseType);

        TerrainGenerator.NoiseType selectedNoiseType = (TerrainGenerator.NoiseType)noiseType.enumValueIndex;

        if (selectedNoiseType != TerrainGenerator.NoiseType.DiamondSquare)
        {
            EditorGUILayout.PropertyField(randomSeed);
            EditorGUILayout.PropertyField(applyFalloff);
        }

        if (applyFalloff.boolValue || selectedNoiseType == TerrainGenerator.NoiseType.FalloffMap)
        {
            EditorGUILayout.LabelField("Falloff Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(falloffSteepness);
            EditorGUILayout.PropertyField(falloffShift);
        }
    }

    private void RenderNoiseSettings()
    {
        TerrainGenerator.NoiseType selectedNoiseType = (TerrainGenerator.NoiseType)noiseType.enumValueIndex;

        switch (selectedNoiseType)
        {
            case TerrainGenerator.NoiseType.Perlin:
                EditorGUILayout.LabelField("Perlin Noise Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(perlinScale);
                EditorGUILayout.PropertyField(perlinOctaves);
                EditorGUILayout.PropertyField(perlinPersistence);
                EditorGUILayout.PropertyField(perlinLacunarity);
                EditorGUILayout.PropertyField(perlinOffset);
                break;

            case TerrainGenerator.NoiseType.DiamondSquare:
                EditorGUILayout.LabelField("Diamond Square Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(diamondSquareRoughness);
                break;

            case TerrainGenerator.NoiseType.Voronoi:
                EditorGUILayout.LabelField("Voronoi Noise Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(voronoiSeedCount);
                break;

            case TerrainGenerator.NoiseType.CellularAutomata:
                EditorGUILayout.LabelField("Cellular Noise Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(cellularAliveChance);
                EditorGUILayout.PropertyField(cellularSteps);
                EditorGUILayout.PropertyField(cellularBlurPasses);
                break;
        }
    }

    private void DrawMeshSettings()
    {
        TerrainGenerator.RenderMode selectedDrawMode = (TerrainGenerator.RenderMode)renderMode.enumValueIndex;

        if (selectedDrawMode == TerrainGenerator.RenderMode.TerrainMesh)
        {
            EditorGUILayout.LabelField("Mesh Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(terrainHeightMultiplier);
            EditorGUILayout.PropertyField(terrainHeightCurve);
            EditorGUILayout.PropertyField(normalizationMode);
        }
    }

    #endregion
}
