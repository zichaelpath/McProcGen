using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Unity Editor for the CustomTerrain class.
/// This creates a custom inspector interface with buttons to execute different terrain generation methods.
/// Place this script in an "Editor" folder to ensure it only compiles for the Unity Editor.
/// </summary>
[CustomEditor(typeof(CustomTerrain))]
public class CustomTerrainEditor : Editor
{
    /// <summary>
/// Overrides the default Unity inspector GUI to create custom buttons for terrain generation.
/// This method is called every time the inspector is drawn in the Unity Editor.
/// </summary>
    public override void OnInspectorGUI()
    {
        // Draw the default inspector properties first
        base.OnInspectorGUI();

        // Get reference to the CustomTerrain component this editor is targeting
        CustomTerrain terrain = (CustomTerrain)target;

        // Create a series of buttons that trigger different terrain generation methods
        // Each button calls a corresponding method on the CustomTerrain component
        
        // Reset the terrain to a flat surface (all heights = 0)
        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrain();
        }

        // Generate completely random terrain heights
        if (GUILayout.Button("Random Terrain"))
        {
            terrain.RandomTerrain();
        }

        // Generate terrain using basic Perlin noise
        if (GUILayout.Button("Simple Perlin"))
        {
            terrain.SimplePerlin();
        }

        // Generate terrain using fractal Brownian motion (fBM) - enhanced Perlin noise
        if (GUILayout.Button("Premium Perlin With Ads"))
        {
            terrain.Perlin();
        }

        // Add a new Perlin noise layer to the parameters list
        if (GUILayout.Button("Add Perlin"))
        {
            terrain.AddPerlin();
        }

        // Remove Perlin noise layers marked for deletion
        if (GUILayout.Button("Remove Perlin"))
        {
            terrain.RemovePerlin();
        }

        // Generate terrain using multiple Perlin noise layers combined
        if (GUILayout.Button("Premium Perlin No Ads"))
        {
            terrain.MultiplePerlin();
        }

        // Generate terrain using Voronoi diagrams - creates mountain peaks and valleys
        if (GUILayout.Button("Voronoi"))
        {
            terrain.Voronoi();
        }
    }
}
