using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    public Terrain terrain; // The terrain component of your class
    public TerrainData terrainData; // The data FROM the terrain component of your class

    public bool resetTerrain; // toggle to decide whether we reset the terrain at each instance

    public int hmr; // Shorthand for "Height Map Resolution

    public float perlinXScale = 0.01f; // How many waves you get in the x direction for perlin noise
    public float perlinYScale = 0.01f; // How many waves you get in the y (or z) direction for perlin noise
    public int offsetX = 0; // Location in the world in the x-direction
    public int offsetY = 0; // Location in the world in the y-direction (or z)
    public float perlinHeightScale = 0.5f; // How high we are limiting the height of the total generation

    void OnEnable()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        hmr = terrainData.heightmapResolution;
    }

    /// <summary>
    /// This method either:
    /// a) returns a new 2 dimensional floating point array of the length and width of the heightmap 
    /// or
    /// b) returns the current heightmap data from the terrain
    /// "resetTerrain" is the toggle that controls how this method works
    /// </summary>
    /// <returns></returns>
    float[,] GetHeights()
    {
        if (resetTerrain)
        {
            return new float[hmr, hmr];
        }
        else
        {
            return terrainData.GetHeights(0, 0, hmr, hmr);
        }
    }

    /// <summary>
    /// Gets the heightmap data from the above method
    /// loops through ALL the points on the terrain
    /// for each point (x, y) ( or (x, z) in engine) it sets a random height range between 0 and 1
    /// finally it sets the heights on the terrain
    /// </summary>
    public void RandomTerrain()
    {
        float[,] heightMap = GetHeights();

        for (int x = 0; x < hmr; x++)
        {
            for (int y = 0; y < hmr; y++)
            {
                heightMap[x, y] += UnityEngine.Random.Range(0f, 1f);
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void SimplePerlin()
    {
        float [,] heightMap = GetHeights();

        for (int x = 0; x < hmr; x++)
        {
            for (int y = 0; y < hmr; y++)
            {
                heightMap[x, y] += Mathf.PerlinNoise((x + offsetX) * perlinXScale, (y + offsetY) * perlinYScale) * perlinHeightScale;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Fully resets the terrain to where EVERY point on the terrain will simply be 0
    /// does this exactly the same way as the GetHeights method does only this time
    /// it immediately sets the heights in the method itself
    /// </summary>
    public void ResetTerrain()
    {
        float [,] heightmap = new float[hmr, hmr];
        terrainData.SetHeights(0, 0, heightmap);
    }
}
