using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Custom terrain generation system that provides various procedural terrain generation methods.
/// This class works with Unity's built-in Terrain system and provides multiple algorithms
/// for creating realistic and varied terrain heightmaps including Perlin noise, fBM, and Voronoi.
/// 
/// Key Features:
/// - Multiple Perlin noise layers with customizable parameters
/// - Fractal Brownian Motion (fBM) for more natural terrain
/// - Voronoi diagrams for mountain peaks and valleys
/// - Additive terrain generation (can build upon existing terrain)
/// - Real-time terrain modification in the Unity Editor
 /// </summary>
[ExecuteInEditMode] // Allows the script to run in Edit Mode, not just Play Mode
public class CustomTerrain : MonoBehaviour
{
    [Header("Terrain References")]
    public Terrain terrain; // Reference to the Unity Terrain component
    public TerrainData terrainData; // Reference to the TerrainData asset containing heightmap and texture data

    [Header("General Settings")]
    public bool resetTerrain; // If true, clears existing terrain before applying new generation

    public int hmr; // Height Map Resolution - cached value for performance
    
    public int seed = 12345; // Random seed for consistent terrain generation

    /// <summary>
    /// Serializable class that contains all parameters needed for a single Perlin noise layer.
    /// Multiple instances of this class can be combined to create complex, layered terrain.
    /// This allows for fine-tuned control over each noise layer's contribution to the final terrain.
    /// </summary>
    [System.Serializable]
    public class PerlinParameters
    {
        [Header("Scale Settings")]
        public float perlinXScale = 0.01f; // Frequency of noise in X direction (higher = more waves)
        public float perlinYScale = 0.01f; // Frequency of noise in Z direction (higher = more waves)
        
        [Header("Position Settings")]
        public int offsetX = 0; // World space offset in X direction (for sampling different noise areas)
        public int offsetY = 0; // World space offset in Z direction (for sampling different noise areas)
        
        [Header("Height Settings")]
        public float perlinHeightScale = 0.5f; // Maximum height contribution of this noise layer
        
        [Header("fBM Settings")]
        public int octaves = 2; // Number of noise octaves to combine (more = more detail)
        public float persistence = 0.2f; // How much each octave contributes (0-1, lower = smoother)
        
        [Header("Management")]
        public bool remove = false; // Mark this layer for removal
    }

    [Header("Multi-Layer Perlin Settings")]
    public List<PerlinParameters> perlinParameters = new List<PerlinParameters>()
    {
        new PerlinParameters() // Start with one default layer
    };
    
    [Header("Single Perlin Settings (Legacy)")]
    // These are used by SimplePerlin() and Perlin() methods for backward compatibility
    public float perlinXScale = 0.01f; // Noise frequency in X direction
    public float perlinYScale = 0.01f; // Noise frequency in Z direction
    public int offsetX = 0; // World position offset X
    public int offsetY = 0; // World position offset Z
    public float perlinHeightScale = 0.5f; // Height multiplier

    public int octaves = 2; // Number of fBM octaves
    public float persistence = 0.2f; // fBM persistence value

    [Header("Voronoi Settings")]
    public float voronoiFallOff = 0.2f; // Linear distance falloff rate
    public float voronoiDropOff = 0.2f; // Power/exponential falloff rate
    public float voronoiMinHeight = 0.1f; // Minimum peak height
    public float voronoiMaxHeight = 0.7f; // Maximum peak height
    public int peaks = 5; // Number of Voronoi peaks to generate

    /// <summary>
    /// Enum defining different Voronoi distance calculation methods.
    /// Each produces different terrain characteristics.
    /// </summary>
    public enum VoronoiType 
    { 
        Linear,    // Simple linear distance falloff
        Power,     // Power-based falloff for steeper slopes
        SinPow,    // Sine-power combination for ridged terrain
        Combined   // Combination of linear and power methods
    }

    public VoronoiType voronoi = VoronoiType.Linear;

    /// <summary>
    /// Unity lifecycle method called when the component is enabled.
    /// Initializes component references and caches frequently used values for performance.
    /// </summary>
    void OnEnable()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        hmr = terrainData.heightmapResolution; // Cache for performance
    }

    /// <summary>
    /// Returns a 2D float array representing the terrain heightmap.
    /// The behavior depends on the 'resetTerrain' toggle:
    /// - If resetTerrain is true: Returns a blank heightmap (all heights = 0)
    /// - If resetTerrain is false: Returns the current terrain heights for additive generation
    /// 
    /// This allows for both complete terrain replacement and layered terrain building.
    /// </summary>
    /// <returns>2D float array with dimensions [hmr, hmr] containing height values (0-1 range)</returns>
    float[,] GetHeights()
    {
        if (resetTerrain)
        {
            return new float[hmr, hmr]; // Create blank heightmap
        }
        else
        {
            return terrainData.GetHeights(0, 0, hmr, hmr); // Get existing heights
        }
    }

    /// <summary>
    /// Generates completely random terrain by assigning random height values to each point.
    /// This creates very chaotic, unrealistic terrain but is useful for testing or as a base layer.
    /// 
    /// Process:
    /// 1. Get the current heightmap (blank or existing based on resetTerrain setting)
    /// 2. For each point in the heightmap, add a random value between 0 and 1
    /// 3. Apply the modified heightmap back to the terrain
    /// 
    /// Note: Uses += so it can be additive if resetTerrain is false
    /// </summary>
    public void RandomTerrain()
    {
        float[,] heightMap = GetHeights();

        // Iterate through every point in the heightmap
        for (int x = 0; x < hmr; x++)
        {
            for (int y = 0; y < hmr; y++)
            {
                // Add random height between 0 and 1 to each point
                heightMap[x, y] += UnityEngine.Random.Range(0f, 1f);
            }
        }

        // Apply the modified heightmap to the terrain
        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Generates terrain using basic Perlin noise.
    /// Perlin noise creates smooth, natural-looking random patterns ideal for terrain generation.
    /// 
    /// Process:
    /// 1. Sample Perlin noise at each heightmap coordinate
    /// 2. Apply scaling factors for frequency (perlinXScale, perlinYScale) and amplitude (perlinHeightScale)
    /// 3. Use offsets and seed for positioning and reproducibility
    /// 
    /// This creates smooth rolling hills and valleys with a single noise layer.
    /// </summary>
    public void SimplePerlin()
    {
        float [,] heightMap = GetHeights();

        for (int x = 0; x < hmr; x++)
        {
            for (int y = 0; y < hmr; y++)
            {
                // Sample Perlin noise with position, offset, seed, and scale
                float noiseValue = Mathf.PerlinNoise(
                    (x + offsetX + seed) * perlinXScale, 
                    (y + offsetY + seed) * perlinYScale
                );
                
                // Apply height scaling and add to existing terrain
                heightMap[x, y] += noiseValue * perlinHeightScale;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Generates terrain using fractal Brownian Motion (fBM) - an enhanced version of Perlin noise.
    /// fBM combines multiple octaves of noise at different frequencies and amplitudes to create
    /// more detailed and natural-looking terrain with features at multiple scales.
    /// 
    /// This method uses the Utils.fBM function which:
    /// - Combines multiple noise octaves
    /// - Each octave has double the frequency and reduced amplitude
    /// - Creates terrain with both large features (mountains) and small details (surface roughness)
    /// </summary>
    public void Perlin()
    {
        float [,] heightMap = GetHeights();

        for (int x = 0; x < hmr; x++)
        {
            for (int y = 0; y < hmr; y++)
            {
                // Generate fBM noise value using utility function
                float fBMValue = Utils.fBM(
                    (x + offsetX) * perlinXScale,
                    (y + offsetY) * perlinYScale,
                    octaves,
                    persistence
                );
                
                // Apply height scaling and add to existing terrain
                heightMap[x, y] += fBMValue * perlinHeightScale;
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Generates terrain using multiple layers of fBM noise with individual parameters.
    /// This is the most advanced noise generation method, allowing for complex terrain
    /// by combining multiple noise layers with different:
    /// - Scales (frequency)
    /// - Heights (amplitude) 
    /// - Positions (offsets)
    /// - Detail levels (octaves)
    /// - Smoothness (persistence)
    /// 
    /// Example use cases:
    /// - Large scale mountains + medium hills + small surface detail
    /// - Different terrain types in different areas using offsets
    /// - Layered erosion effects
    /// </summary>
    public void MultiplePerlin()
    {
        float [,] heightMap = GetHeights();

        for (int x = 0; x < hmr; x++)
        {
            for (int y = 0; y < hmr; y++)
            {
                // Apply each Perlin layer to this coordinate
                foreach (PerlinParameters p in perlinParameters)
                {
                    // Generate fBM for this layer with its specific parameters
                    float layerValue = Utils.fBM(
                        (x + p.offsetX) * p.perlinXScale,
                        (y + p.offsetY) * p.perlinYScale,
                        p.octaves,
                        p.persistence
                    );
                    
                    // Add this layer's contribution to the total height
                    heightMap[x, y] += layerValue * p.perlinHeightScale;
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Adds a new Perlin noise layer to the parameters list.
    /// The new layer will have default parameters and can be customized in the inspector.
    /// This allows for building up complex terrain with multiple noise layers.
    /// </summary>
    public void AddPerlin()
    {
        perlinParameters.Add(new PerlinParameters());
    }

    /// <summary>
    /// Removes all Perlin noise layers that have been marked for removal.
    /// Iterates through the parameters list and removes any layer where 'remove' is set to true.
    /// This provides a safe way to delete layers without disrupting the list during iteration.
    /// 
    /// Note: Removal happens in forward order, so indices may shift during removal.
    /// This could potentially skip elements if multiple consecutive items are marked for removal.
    /// </summary>
    public void RemovePerlin()
    {
        // Iterate through list to find items marked for removal
        for (int i = 0; i < perlinParameters.Count; i++)
        {
            if (perlinParameters[i].remove)
            {
                perlinParameters.RemoveAt(i);
                i--; // Adjust index since list has shifted
            }
        }
    }

    /// <summary>
    /// Generates terrain using Voronoi diagrams to create mountain peaks and valleys.
    /// Voronoi patterns are based on distance to randomly placed "seed" points (peaks).
    /// Each point on the terrain belongs to the region of the nearest peak and gets a height
    /// based on its distance from that peak using various falloff functions.
    /// 
    /// Process:
    /// 1. Generate random peak locations and heights
    /// 2. For each point, calculate distance to nearest peak
    /// 3. Apply distance-based height falloff using selected algorithm
    /// 4. Only increase height (never decrease existing terrain)
    /// 
    /// Different VoronoiType options create different terrain characteristics:
    /// - Linear: Smooth cones
    /// - Power: Sharper peaks with exponential falloff
    /// - SinPow: Ridge-like terrain with sine wave modulation
    /// - Combined: Mix of linear and power methods
    /// </summary>
    public void Voronoi()
    {
        float[,] heightMap = GetHeights();

        // Generate the specified number of random peaks
        for (int i = 0; i < peaks; i++)
        {
            // Create random peak with position (x,z) and height (y)
            Vector3 peak = new Vector3(
                Random.Range(0, hmr), // X position within heightmap
                Random.Range(voronoiMinHeight, voronoiMaxHeight), // Peak height
                Random.Range(0, hmr) // Z position within heightmap
            );

            // Skip this peak if there's already higher terrain at this location
            if (heightMap[(int)peak.x, (int)peak.z] >= peak.y)
                continue;

            // Set the peak height
            heightMap[(int)peak.x, (int)peak.z] = peak.y;
            Vector2 peakLocation = new Vector2(peak.x, peak.z);

            // Calculate maximum possible distance for normalization
            float maxDistance = Vector2.Distance(
                new Vector2(0, 0),
                new Vector2(hmr, hmr)
            );

            // Apply this peak's influence to all points on the terrain
            for (int y = 0; y < hmr; y++)
            {
                for (int x = 0; x < hmr; x++)
                {
                    // Calculate normalized distance from this point to the peak
                    float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDistance;
                    float h; // Height at this point based on distance

                    // Apply different falloff algorithms based on selected type
                    if (voronoi == VoronoiType.Combined)
                    {
                        // Combines exponential and linear falloff for complex terrain
                        h = peak.y - Mathf.Pow(distanceToPeak, voronoiDropOff)
                                    - distanceToPeak * voronoiFallOff;  
                    }
                    else if (voronoi == VoronoiType.Power)
                    {
                        // Exponential falloff creates sharper peaks
                        h = peak.y - Mathf.Pow(distanceToPeak, voronoiDropOff) * voronoiFallOff;
                    }
                    else if (voronoi == VoronoiType.SinPow)
                    {
                        // Power falloff with sine wave modulation creates ridged terrain
                        h = peak.y - Mathf.Pow(distanceToPeak * 3, voronoiFallOff)
                                    - Mathf.Sin(distanceToPeak * 2 * Mathf.PI) / voronoiDropOff;
                    }
                    else // Linear
                    {
                        // Simple linear falloff creates smooth cone shapes
                        h = peak.y - distanceToPeak * voronoiFallOff;
                    }

                    // Only raise terrain, never lower it (allows for layering)
                    if (heightMap[x,y] < h)
                    {
                        heightMap[x,y] = h;
                    }
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Completely resets the terrain to a flat surface with all heights set to 0.
    /// This creates a blank slate for terrain generation by:
    /// 1. Creating a new heightmap array filled with zeros
    /// 2. Applying it directly to the terrain data
    /// 
    /// This is useful for:
    /// - Starting fresh terrain generation
    /// - Clearing unwanted terrain modifications
    /// - Resetting after experimentation with different algorithms
    /// 
    /// Note: This ignores the 'resetTerrain' toggle and always creates a flat surface.
    /// </summary>
    public void ResetTerrain()
    {
        // Create completely flat heightmap (all values default to 0)
        float [,] heightmap = new float[hmr, hmr];
        
        // Apply the flat heightmap to the terrain immediately
        terrainData.SetHeights(0, 0, heightmap);
    }
}
