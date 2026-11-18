using System;
using Unity.VisualScripting;
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
    [SerializeField] TilePortData data;

    public float _height;
    public float height => _height;
    public float heightOffset => data.heightOffset;
    public DirectionMask orientation => data.orientation;
    public DirectionMask gravityPlane => _parentSurface.gravityPlane;

    [SerializeReference] TilePort _neighbor;
    public TilePort neighbor => _neighbor;

    [SerializeField] TileSurface _parentSurface;
    public TileSurface parentSurface => _parentSurface;
    public void SetParent(TileSurface surface) { _parentSurface = surface;}

    public TilePort(TilePortData data, TileSurface parent)
    {
        this.data = data;
        _height = parent.parent.arrayPosition.z + data.heightOffset;
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
        if (Mathf.Approximately(height, port.height) && ((DirectionMaskUtils.ReverseMask(orientation) & port.orientation) != 0) && (gravityPlane & port.gravityPlane) != 0)
        {
            Debug.Log($"Found match from {(string)this}@{(string)parentSurface} to {(string)port}@{(string)port.parentSurface}");
            _neighbor = port;
            port._neighbor = this;
            return true;
        }
        return false;
    }

    public static implicit operator string(TilePort port)
    {
        return $"TilePort({port.orientation},H={port.height})";
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
    [SerializeField] DirectionMask _orientation;
    public DirectionMask orientation => _orientation;

    public void OnValidate()
    {    
        if (!DirectionMaskUtils.ExactlyOneBit(_orientation))
        {
            _orientation = DirectionMask.N;
            Debug.LogError("Error in TilePort: orientation must include exactly one direction");
        }
    }
}
