using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    public Terrain terrain;
    public TerrainData terrainData;

    public bool resetTerrain;

    public int hmr;

    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int offsetX = 0;
    public int offsetY = 0;
    public float perlinHeightScale = 0.5f;

    void OnEnable()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        hmr = terrainData.heightmapResolution;
    }

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

    public void ResetTerrain()
    {
        float [,] heightmap = new float[hmr, hmr];
        terrainData.SetHeights(0, 0, heightmap);
    }
}
