using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utility class containing mathematical functions for procedural generation.
/// All methods are static and can be called without instantiating the class.
/// </summary>
public static class Utils
{
    /// <summary>
    /// Generates fractal Brownian Motion (fBM) noise - a fundamental technique in procedural generation.
    /// fBM creates natural-looking patterns by combining multiple octaves of Perlin noise at different
    /// frequencies and amplitudes. This produces terrain with both large-scale features and fine details.
    /// 
    /// How it works:
    /// 1. Start with base frequency and amplitude
    /// 2. For each octave: sample noise, add to total, double frequency, reduce amplitude
    /// 3. Each octave adds smaller details at higher frequencies
    /// 4. Normalize result to maintain consistent output range
    /// 
    /// Parameters:
    /// </summary>
    /// <param name="x">X coordinate in noise space</param>
    /// <param name="y">Y coordinate in noise space</param>
    /// <param name="oct">Number of octaves (more = more detail, slower computation)</param>
    /// <param name="persistence">How much each octave contributes (0-1, lower = smoother)</param>
    /// <returns>Normalized noise value typically in 0-1 range</returns>
    public static float fBM(float x, float y, int oct, float persistence)
    {
        float total = 0f;       // Accumulated noise value
        float frequency = 1f;   // Current octave frequency (doubles each octave)
        float amplitude = 1f;   // Current octave amplitude (reduces by persistence each octave)
        float maxValue = 0f;    // Sum of all amplitudes for normalization

        // Generate each octave of noise
        for (int i = 0; i < oct; i++)
        {
            // Sample Perlin noise at current frequency and scale by current amplitude
            total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
            
            // Track maximum possible value for normalization
            maxValue += amplitude;
            
            // Prepare for next octave: reduce amplitude, increase frequency
            amplitude *= persistence; // Smaller contribution from higher frequencies
            frequency *= 2;          // Double the frequency for finer detail
        }

        // Normalize the result to maintain consistent output range
        return total / maxValue;
    }
}
