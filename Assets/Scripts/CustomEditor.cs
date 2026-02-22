using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomTerrain))]
public class CustomTerrainEditor : Editor
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CustomTerrain terrain = (CustomTerrain)target;

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
