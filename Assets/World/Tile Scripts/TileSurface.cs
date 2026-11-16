using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Contains an orientation (represented by a normal vector/normal cardinal) and ports
/// Multiple are held by a tile
/// Player can move between surfaces with matching ports. Disregards surface orientation in favor of port orientations & heights.
/// Ideally, surface & ports rotate with the physical object.
/// </summary>
/// 
///
[Serializable]
public class TileSurface : MonoBehaviour
{
    [SerializeField] TileSurfaceData data;
    public Dictionary<DirectionMask, TilePort>.ValueCollection ports => _portLookup.Values;
    public DirectionMask orientation => data.orientation;
    public TileMount mount => data.mount;

    [SerializeField] Dictionary<DirectionMask, TilePort> _portLookup;

    [SerializeField] GameplayTile _parent;
    public GameplayTile parent => _parent;

    void OnDrawGizmos()
    {
        DrawConnectedPorts();
        // if (ports != null)
        // {
        //     foreach (TilePort port in ports)
        //     {
        //         if (port.neighbor != null)
        //         {
        //             Vector3 boundry = (Vector3)port.neighbor.parentSurface.parent.arrayPosition + (Vector3)DirectionMaskUtils.MaskToVector(port.neighbor.orientation) * 0.5f + new Vector3(0, 0, 1) * port.neighbor.height;
        //             Vector3 closeBoundry = (Vector3)parent.arrayPosition - (Vector3)DirectionMaskUtils.MaskToVector(port.orientation) * 0f + new Vector3(0, 0, 1) * port.height;
        //             Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f);
        //             Gizmos.DrawSphere(WorldUtils.ArrayToWorldCoordinates(0.5f * (boundry+closeBoundry)) + offset, 0.2f);
        //             Vector3 position = port.neighbor.parentSurface.parent.arrayPosition + port.neighbor.heightOffset * (Vector3)(DirectionMaskUtils.MaskToVector(port.neighbor.parentSurface.orientation) + DirectionMaskUtils.MaskToVector(port.neighbor.orientation));
        //             Gizmos.DrawLine(WorldUtils.ArrayToWorldCoordinates(parent.arrayPosition + port.heightOffset * (Vector3)(DirectionMaskUtils.MaskToVector(orientation) + DirectionMaskUtils.MaskToVector(port.orientation)) + offset), WorldUtils.ArrayToWorldCoordinates(position + offset));
        //         }
        //     }
        // }      
    }

    public void DrawConnectedPorts()
    {
        if (ports == null) return;
        foreach (TilePort port in ports)
        {
            if (port.neighbor != null)
            {
                Gizmos.color = Color.green;
                Vector3 center = WorldUtils.ArrayToWorldCoordinates((Vector3)parent.arrayPosition + new Vector3(0.5f, 0.5f, 0.5f));
                Vector3 direction = WorldUtils.ArrayToWorldCoordinates(DirectionMaskUtils.MaskToVector(port.orientation));
                float length = 0.5f;
                Gizmos.DrawRay(center, direction * length);
            }
        }
    }

    public void Initialize(TileSurfaceData data, GameplayTile parent)
    {
        this.data = data;
        _parent = parent;
        _portLookup = new Dictionary<DirectionMask, TilePort>();
        foreach (TilePortData portData in data.ports)
        {
            TilePort port = new TilePort(portData, this);
            //Debug.Log("Adding portData: " + portData.orientation + ", " + portData.height);
            _portLookup.Add(port.orientation, port);
        }
        //Debug.Log("Finished constructing surface");
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    /// <summary>
    /// If this surface has a port whose orientation matches the provided DirectionMask, returns true & sets the output parameter. 
    /// </summary>
    /// <param name="mask"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    public bool TryGetPort(DirectionMask mask, out TilePort port)
    {
        if (_portLookup == null) Debug.LogError("Port lookup dictionary is null");
        return _portLookup.TryGetValue(mask, out port);
    }

    /// <summary>
    /// Sets neighbor ports between this and the provided surface that is in direction relativeDirection relative to this surface.
    /// Returns number of neighboring ports found.
    /// Overrides existing neighbors on this surface and the provided surfaces.
    /// *Does not check adjacency
    /// </summary>
    /// <param name="surface"></param>
    public int SetNeighbors(TileSurface surface, DirectionMask relativeDirection)
    {
        int numNeighbors = 0;
        foreach (TilePort port in ports)
        {
            if ((port.orientation & relativeDirection) != 0 && surface.TryGetPort(DirectionMaskUtils.ReverseMask(port.orientation), out TilePort neighbor))
            {
                Debug.Log("Checking neighbor " + neighbor.orientation + " from tile" + parent.arrayPosition + ", " + orientation);
               if(port.TrySetNeighbor(neighbor)) numNeighbors++;
            }
        }
        Debug.Log("Found neighbors from " + parent.arrayPosition + " facing " + orientation + ": " + numNeighbors);
        return numNeighbors;
    }

    public static implicit operator string(TileSurface surface)
    {
        return $"TileSurface(@{surface.parent.arrayPosition},{surface.orientation})";
    }
}

/// <summary>
/// Contains an orientation (represented by a normal vector/normal cardinal) and ports
/// Multiple are held by a tile
/// Player can move between surfaces with matching ports. Disregards surface orientation in favor of port orientations & heights.
/// Ideally, surface & ports rotate with the physical object.
/// </summary>
/// 
[Serializable]
public struct TileSurfaceData
{
    [Tooltip("Free movement between opposite-facing same-height ports")]
    [SerializeField] List<TilePortData> _ports;
    public List<TilePortData> ports => _ports;

    [Tooltip("Determines how the player stands on the surface")]
    [SerializeField] DirectionMask _orientation;

    public DirectionMask orientation => _orientation;

    [Tooltip("Determines the visualized position of the player on the surface")]
    [SerializeField] TileMount _mount;
    public TileMount mount => _mount;

    public void OnValidate()
    {
        if (!DirectionMaskUtils.ExactlyOneBit(_orientation))
        {
            Debug.LogError("Error in TileSurface: orientation must include exactly one direction");
        } else
        {
            _mount.SetAngleForSurface(orientation);
            Debug.Log("Set mount rotation: " + _mount.rotation);
        }
    }
    
}

/// <summary>
/// Defines orientation of the player when in the block in WORLD SPACE.
/// Offsets and angles should therefore be safe to directly apply to player.
/// !IN WORLD SPACE
/// </summary>
///
[Tooltip("Position of player when in tile IN WORLD SPACE")]
[Serializable]
public struct TileMount
{
    public static float TILE_HALF_WIDTH = 0.5f;

    [SerializeField] Quaternion _rotation;

    /// <summary>
    /// Angle in WORLD SPACE to rotate the player when standing.
    /// Should be able to easily combine with the player's own rotation
    /// !ANGLE IN WORLD SPACE
    /// </summary>
    public Quaternion rotation => _rotation; 


    [Tooltip("IN WORLD SPACE")]
    [SerializeField] Vector3 _offset;
    /// <summary>
    /// Position in array space to offset the player when standing
    /// !OFFSET IN WORLD SPACE
    /// </summary>
    public Vector3 offset => _offset;

    public void SetAngleForSurface(DirectionMask surface)
    {
        _rotation = Quaternion.LookRotation(Vector3.right, WorldUtils.ArrayToWorldCoordinates(DirectionMaskUtils.MaskToVector(surface)));
        Debug.Log("Set rotation: " + rotation);
    }

    public void SetOuterSurfaceMount(DirectionMask surface, float offsetAmount)
    {
        SetAngleForSurface(surface);
        switch(surface)
        {
            case DirectionMask.U:
                _offset = offsetAmount * Vector3.up;
                break;
            case DirectionMask.D:
                _offset = offsetAmount * Vector3.down;
                break;
            case DirectionMask.N:
                _offset = offsetAmount * WorldUtils.ArrayToWorldCoordinates(DirectionMaskUtils.MaskToVector(DirectionMask.N));
                break;
            case DirectionMask.S:
                _offset = offsetAmount * WorldUtils.ArrayToWorldCoordinates(DirectionMaskUtils.MaskToVector(DirectionMask.S));
                break;
            case DirectionMask.W:
                _offset = offsetAmount * WorldUtils.ArrayToWorldCoordinates(DirectionMaskUtils.MaskToVector(DirectionMask.W));
                break;
            case DirectionMask.E:
                _offset = offsetAmount * WorldUtils.ArrayToWorldCoordinates(DirectionMaskUtils.MaskToVector(DirectionMask.E));
                break;
        }
    }
}
    