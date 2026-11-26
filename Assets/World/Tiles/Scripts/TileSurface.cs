using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    public TileRules rules => parentTile.rules;
    [SerializeField] private int surfaceIndexInRules;
    public TileSurfaceData data => rules.surfaces[surfaceIndexInRules];
    public Directions.Mask localOrientation => data.orientation;

    /// <summary>
    /// Calculates & retrieves the global direction of the "standing plane" of the surface. Opposite direction of gravity. Used to filter connecting surfaces.
    /// </summary>
    public Directions.Mask gravityPlane => parentTile.WorldDirection(data.gravityPlane);
    public Directions.Mask localGravityPlane => data.gravityPlane;
    public TileMount mount => data.mount;

    [SerializeField] Dictionary<Directions.Mask, TilePort> _portLookup;
    public Dictionary<Directions.Mask, TilePort>.ValueCollection ports => _portLookup.Values;

    [SerializeField] GameplayTile _parentTile;
    public GameplayTile parentTile => _parentTile;

    /// <summary>
    /// Position of tile in WORLD SPACE
    /// </summary>
    public Vector3 position => _parentTile.position;

    /// <summary>
    /// Rotation of tile in WORLD SPACE
    /// </summary>
    public Quaternion rotation => _parentTile.rotation;

    /// <summary>
    /// Invoked whenever 
    /// </summary>
    public UnityEvent<TilePort> onEnter;

    void OnDrawGizmos()
    {
        DrawConnectedPorts();   
    }

    /// <summary>
    /// Can be overriden to add runtime enter conditions 
    /// </summary>
    /// <param name="direction">Direction of the player/port entering to this surface</param>
    /// <returns></returns>
    public virtual bool CanEnter(Directions.Mask direction)
    {
        return true;
    }

    /// <summary>
    /// Invokes onEnter event, passing as a parameter the port on this surface the player is entering.
    /// E.g. if the player moves north onto this surface, they are entering the southern port of the surface.
    /// </summary>
    public virtual void OnEnter(TilePort entryPort)
    {
        onEnter.Invoke(entryPort);
    }

    public void DrawConnectedPorts()
    {
        if (ports == null) return;
        foreach (TilePort port in ports)
        {
            if (port.neighbor != null)
            {
                Gizmos.color = Color.green;
                Vector3 direction = WorldUtils.ArrayToWorldCoordinates(Directions.MaskToVector(port.orientation));
                float length = 0.5f;
                Gizmos.DrawRay(position, direction * length);
            }
        }
    }

    public void Initialize(int surfaceIndex, GameplayTile parent)
    {
        _parentTile = parent;
        surfaceIndexInRules = surfaceIndex;

        _portLookup = new Dictionary<Directions.Mask, TilePort>();
        foreach (TilePortData portData in data.ports)
        {
            TilePort port = new TilePort(portData, this);
            //Debug.Log("Adding portData: " + portData.localOrientation + ", " + portData.height);
            _portLookup.Add(port.localOrientation, port);
        }
        //Debug.Log("Finished constructing surface");
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    /// <summary>
    /// Attempts to retrieve a port matching the given world orientation.
    /// </summary>
    /// <param name="worldMask">Direction mask relative to the world</param>
    /// <param name="port"></param>
    /// <returns></returns>
    public bool TryGetPort(Directions.Mask worldMask, out TilePort port)
    {
        if (_portLookup == null) Debug.LogError("Port lookup dictionary is null");
        Directions.Mask inLocal = Directions.VectorToClosestMask(WorldUtils.WorldToArrayCoordinates(Quaternion.Inverse(rotation) * WorldUtils.ArrayToWorldCoordinates(Directions.MaskToVector(worldMask))));
        return _portLookup.TryGetValue(inLocal, out port);
    }

    /// <summary>
    /// Sets neighbor ports between this and the provided surface that is in direction relativeDirection relative to this surface.
    /// Returns number of neighboring ports found.
    /// Overrides existing neighbors on this surface and the provided surfaces.
    /// *Does not check adjacency
    /// </summary>
    /// <param name="surface"></param>
    /// <param name="relativeDirection">Relative direction of tiles in world (not local)</param>
    public int SetNeighbors(TileSurface surface, Directions.Mask relativeDirection)
    {
        int numNeighbors = 0;
        foreach (TilePort port in ports)
        {
            if ((port.orientation & relativeDirection) != 0 && surface.TryGetPort(Directions.ReverseMask(port.orientation), out TilePort neighbor))
            {
                //Debug.Log("Checking neighbor " + neighbor.orientation + " from tile" + parentTile.transform.position + ", " + localOrientation);
               if(port.TrySetNeighbor(neighbor)) numNeighbors++;
            }
        }
        //Debug.Log("Found neighbors from " + parentTile.transform.position + " facing " + localOrientation + ": " + numNeighbors);
        return numNeighbors;
    }

    public static implicit operator string(TileSurface surface)
    {
        return $"TileSurface(@{surface.parentTile.transform.position},Local={surface.localOrientation},World={surface.parentTile.WorldDirection(surface.localOrientation)})";
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
    [SerializeField] Directions.Mask _orientation;
    public Directions.Mask orientation => _orientation;

    [Tooltip("Use to filter which ports the surface can connect to. Can have multiple planes")]
    [SerializeField] Directions.Mask _gravityPlane;
    public Directions.Mask gravityPlane => _gravityPlane;

    [Tooltip("Determines the visualized position of the player on the surface")]
    [SerializeField] TileMount _mount;
    public TileMount mount => _mount;

    public void OnValidate()
    {
        if (!Directions.ExactlyOneBit(_orientation))
        {
            Debug.LogError("Error in TileSurface: orientation must include exactly one direction");
        } else
        {
            _mount.SetAngleForSurface(_orientation);
            Debug.Log($"Set mount rotation for {_orientation}: " + _mount.rotation);
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

    public void SetAngleForSurface(Directions.Mask surface)
    {
        _rotation = Quaternion.LookRotation(Vector3.right, WorldUtils.ArrayToWorldCoordinates((Vector3)Directions.singleMaskToVector[surface]));
        Debug.Log($"Set rotation for {surface}, {Directions.singleMaskToVector[surface]}, {WorldUtils.ArrayToWorldCoordinates(Directions.singleMaskToVector[surface])}: " + rotation);
    }
}
    