using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomTerrain))]
public class CustomTerrainEditor : Editor
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Targets an instance of Custom Terrain
        CustomTerrain terrain = (CustomTerrain)target;

        // Creates buttons that call their associated method
        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrain();
        }

        if (GUILayout.Button("Random Terrain"))
        {
            terrain.RandomTerrain();
        }

        if (GUILayout.Button("Simple Perlin"))
        {
            terrain.SimplePerlin();
        }
    }
}
