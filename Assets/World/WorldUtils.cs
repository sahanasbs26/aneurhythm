using UnityEngine;

public class WorldUtils
{

    public const float isometricToWorldScaleFactor = 1; 
    /// <summary>
    /// Translates a from a array position (poisition in (x, y, z) where z is height) to a world space position.
    /// Works with rotation vectors.
    /// * Uses a WASD = NE,NW,SW,SE control scheme
    /// </summary>
    public static Vector3 ArrayToWorldCoordinates(Vector3 arrayCoordinates, float scaleFactor = isometricToWorldScaleFactor)
    {
        return new Vector3(arrayCoordinates.y, arrayCoordinates.z, -arrayCoordinates.x);
    }

    /// <summary>
    /// Translates a from a world position to a array position.
    /// Works with rotation vectors.
    /// * Assumes a WASD = NE,NW,SW,SE control scheme
    /// </summary>
    public static Vector3 WorldToArrayCoordinates(Vector3 worldCoordinates, float scaleFactor = isometricToWorldScaleFactor)
    {
        return new Vector3(-worldCoordinates.z, worldCoordinates.x, worldCoordinates.y);
    }
    
}
