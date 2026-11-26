using System;
using UnityEngine;

/// <summary>
/// Contains an orientation and height
/// Held by a surface. Multiple may be held.
/// Player can move between ports that match in height and orientation. 
/// Disregard local positioning of ports on a tile (aside from height); assume opposite facing ports allow exchange between eachother
/// </summary>
[Serializable]
public class TilePort
{
    public static readonly float HEIGHT_DIFFERENCE_EPSILON = 0.05f;
    [SerializeField] TilePortData data;

    //Assume height is measured from tile center
    public float heightOffset => data.heightOffset;

    /// <summary>
    /// Calculates & returns direction of port in global space. Based off the local direction and the parent tile's rotation. 
    /// </summary>
    public Directions.Mask orientation => _parentSurface.parentTile.WorldDirection(localOrientation);
    public Directions.Mask localOrientation => data.orientation;
    public Directions.Mask gravityPlane => _parentSurface.gravityPlane;

    TilePort _neighbor;
    public TilePort neighbor => _neighbor;

    [SerializeField] TileSurface _parentSurface;
    public TileSurface parentSurface => _parentSurface;
    public void SetParent(TileSurface surface) { _parentSurface = surface;}

    public TilePort(TilePortData data, TileSurface parent)
    {
        this.data = data;
        _parentSurface = parent;
    }

    /// <summary>
    /// If the provided port matches in height & orientation, sets eachother as neighbors.
    /// Does not check for adjacency.
    /// *Overrides existing neighbors.
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    public bool TrySetNeighbor(TilePort port)
    {
        Vector3 g = WorldUtils.ArrayToWorldCoordinates((Vector3)Directions.MaskToVector(gravityPlane)).normalized;
        float h1 = Vector3.Dot(port.parentSurface.position, g) + port.data.heightOffset;
        float h0 = Vector3.Dot(parentSurface.position, g) + data.heightOffset;
        float heightDifference = h1 - h0;
        if (Mathf.Abs(heightDifference) < HEIGHT_DIFFERENCE_EPSILON && ((Directions.ReverseMask(orientation) & port.orientation) != 0) && (gravityPlane & port.gravityPlane) != 0)
        {
            Debug.Log($"Found match from {(string)this}@{(string)parentSurface} to {(string)port}@{(string)port.parentSurface}");
            //Debug.Log($"h1={h1}, h0={h0}");
            _neighbor = port;
            port._neighbor = this;
            return true;
        }
        return false;
    }

    public void Unlink()
    {
        if (_neighbor != null && _neighbor._neighbor == this)
        {
            _neighbor._neighbor = null;
        }
        _neighbor = null;
    }

    public static implicit operator string(TilePort port)
    {
        return $"TilePort(World={port.orientation},Local={port.localOrientation}, H={port.heightOffset})";
    }
}


/// <summary>
/// Contains an orientation and height
/// Held by a surface. Multiple may be held.
/// Player can move between ports that match in height and orientation. 
/// Disregard local positioning of ports on a tile (aside from height); assume opposite facing ports allow exchange between eachother
/// </summary>
[Serializable]
public struct TilePortData
{
    [Tooltip("Same-height ports allow movement between them")]
    [SerializeField] float _heightOffset;
    public float heightOffset => _heightOffset;

    [Tooltip("Opposite-facing ports allow movement between")]
    [SerializeField] Directions.Mask _orientation;
    public Directions.Mask orientation => _orientation;

    public void OnValidate()
    {    
        if (!Directions.ExactlyOneBit(_orientation))
        {
            _orientation = Directions.Mask.N;
            Debug.LogError("Error in TilePort: orientation must include exactly one direction");
        }
    }
}
