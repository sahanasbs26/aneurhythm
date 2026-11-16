using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Contains position, mounting, and exit/enter/surface rules of a tile.
/// Other tiles should inherit from this one.
/// All coordinates are relative to "array" space, as (x, y, z) = (N, E, U) = (forward, right, up).
/// "Surfacing" means a player can walk on the periphery of the tile (e.g. on top).
/// "Entering" means a player can walk into this tile position.
/// </summary>
[CreateAssetMenu(fileName = "TileRule", menuName = "Scriptable Objects/TileRule")]
public class TileRules : ScriptableObject
{

    [Tooltip("Standable surfaces")]
    [SerializeField] TileSurfaceData[] _surfaces;

    /// <summary>
    /// List of surfaces the player can stand on. Orientation is normal from the surface.
    /// </summary>
    public TileSurfaceData[] surfaces => _surfaces;

    void OnValidate()
    {
        for (int i = 0; i < _surfaces.Length; i++)
        {
            surfaces[i].OnValidate();
            foreach (TilePortData port in surfaces[i].ports)
            {
                port.OnValidate();
            }
        }
        //TODO: Validate surface orientation & ports
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
}

//TODO: Check destructiveness of maskToVector/vectorToMask functions
public class DirectionMaskUtils
{
    public static Dictionary<DirectionMask, int> bitPositions = new Dictionary<DirectionMask, int>
    {
        {DirectionMask.N, 0},
        {DirectionMask.E, 1}, 
        {DirectionMask.S, 2}, 
        {DirectionMask.W, 3},

        {DirectionMask.U, 4}, 
        {DirectionMask.D, 5}, 

        {DirectionMask.UN, 6},
        {DirectionMask.UE, 7}, 
        {DirectionMask.US, 8}, 
        {DirectionMask.UW, 9},

        {DirectionMask.DN, 10},
        {DirectionMask.DE, 11}, 
        {DirectionMask.DS, 12}, 
        {DirectionMask.DW, 13},

        {DirectionMask.NE, 14},
        {DirectionMask.SE, 15}, 
        {DirectionMask.SW, 16}, 
        {DirectionMask.NW, 17},
    };

    public static int NUM_DIRECTIONS = 18;

    public static bool ExactlyOneBit(DirectionMask direction)
    {
        return (direction > 0) && (direction & (direction - 1)) == 0;
    }

    /// <summary>
    /// Returns a DirectionMask in all the general directions of the given vector.
    /// E.g. a vector in +x will return a direciton mask in N, UN, SN directions
    /// Pass an integer for best results. 
    /// </summary>
    /// <param name="direction">Vector in ARRAY SPACE</param>
    /// <returns></returns>
    public static DirectionMask VectorToMaskLenient(Vector3 direction)
    {
        DirectionMask mask = 0;
        if (direction.x > 0)
        {
            mask |= DirectionMask.N | DirectionMask.DN | DirectionMask.UN | DirectionMask.NE | DirectionMask.NW;
        } else if (direction.x < 0)
        {
            mask |= DirectionMask.S | DirectionMask.DS | DirectionMask.US | DirectionMask.SE | DirectionMask.SW;
        }

        if (direction.y > 0)
        {
            mask |= DirectionMask.E | DirectionMask.DE | DirectionMask.UE | DirectionMask.NE | DirectionMask.SE;
        } else if (direction.y < 0)
        {
            mask |= DirectionMask.W | DirectionMask.DW | DirectionMask.UW | DirectionMask.NW | DirectionMask.SW;
        }

        if (direction.z > 0)
        {
            mask |= DirectionMask.U | DirectionMask.UN | DirectionMask.UE | DirectionMask.US | DirectionMask.UW;
        } else if (direction.z < 0)
        {
            mask |= DirectionMask.D | DirectionMask.DN | DirectionMask.DE | DirectionMask.DS | DirectionMask.DW;
        }

        return mask;
    }

    /// <summary>
    /// Returns a DirectionMask in the directions of the given vector points
    /// E.g. a vector in +x will return a mask in N direction. A vector in +x and +y will return N|E;
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static DirectionMask VectorToMaskStrict(Vector3 direction)
    {
        if (direction == Vector3.zero) return 0;

        DirectionMask mask = 0;
        switch( Mathf.Atan2(direction.y, direction.x))
        {
            case >= Mathf.PI * 7f/8:
                mask |= DirectionMask.W;
                break;
            case >= Mathf.PI * 5f/8:
                mask |= DirectionMask.NW;
                break;
            case >= Mathf.PI * 3f/8:
                mask |= DirectionMask.N;
                break;
            case >= Mathf.PI / 8:
                mask |= DirectionMask.NE;
                break;
            case >= -Mathf.PI / 8:
                mask |= DirectionMask.E;
                break;
            case >= -Mathf.PI * 3f/8:
                mask |= DirectionMask.SE;
                break;
            case >= -Mathf.PI * 5f/8:
                mask |= DirectionMask.S;
                break;
            case >= -Mathf.PI * 7f/8:
                mask |= DirectionMask.SW;
                break;
            default:
                mask |= DirectionMask.W;
                break; 
        }

        switch(Mathf.Atan2(direction.z, direction.x))
        {
            case >= Mathf.PI * 7f/8:
                mask |= DirectionMask.W;
                break;
            case >= Mathf.PI * 5f/8:
                mask |= DirectionMask.UW;
                break;
            case >= Mathf.PI * 3f/8:
                mask |= DirectionMask.U;
                break;
            case >= Mathf.PI / 8:
                mask |= DirectionMask.UE;
                break;
            case >= -Mathf.PI / 8:
                mask |= DirectionMask.E;
                break;
            case >= -Mathf.PI * 3f/8:
                mask |= DirectionMask.DE;
                break;
            case >= -Mathf.PI * 5f/8:
                mask |= DirectionMask.D;
                break;
            case >= -Mathf.PI * 7f/8:
                mask |= DirectionMask.DW;
                break;
            default:
                mask |= DirectionMask.W;
                break; 
        }

        switch(Mathf.Atan2(direction.z, direction.y))
        {
            case >= Mathf.PI * 7f/8:
                mask |= DirectionMask.S;
                break;
            case >= Mathf.PI * 5f/8:
                mask |= DirectionMask.US;
                break;
            case >= Mathf.PI * 3f/8:
                mask |= DirectionMask.U;
                break;
            case >= Mathf.PI / 8:
                mask |= DirectionMask.UN;
                break;
            case >= -Mathf.PI / 8:
                mask |= DirectionMask.N;
                break;
            case >= -Mathf.PI * 3f/8:
                mask |= DirectionMask.DN;
                break;
            case >= -Mathf.PI * 5f/8:
                mask |= DirectionMask.D;
                break;
            case >= -Mathf.PI * 7f/8:
                mask |= DirectionMask.DS;
                break;
            default:
                mask |= DirectionMask.S;
                break; 
        }

        return mask;
    }
    
    /// <summary>
    /// Returns vector in directions of the direction mask.
    /// Values in any direction are constrained to 1.
    /// Not normalized
    /// (x, y, z) = (N, E, U)
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static Vector3Int MaskToVector(DirectionMask direction)
    {
        Vector3Int vect = Vector3Int.zero;
        if (((DirectionMask.N | DirectionMask.UN | DirectionMask.DN | DirectionMask.NE | DirectionMask.NW) & direction) != 0)
            vect += new Vector3Int(1, 0, 0);
        if (((DirectionMask.S | DirectionMask.US | DirectionMask.DS | DirectionMask.SE | DirectionMask.SW) & direction) != 0)
            vect += new Vector3Int(-1, 0, 0);

        if (((DirectionMask.E | DirectionMask.UE | DirectionMask.DE | DirectionMask.NE | DirectionMask.SE) & direction) != 0)
            vect += new Vector3Int(0, 1, 0);
        if (((DirectionMask.W | DirectionMask.UW | DirectionMask.DW | DirectionMask.NW | DirectionMask.SW) & direction) != 0)
            vect += new Vector3Int(0, -1, 0);

        if (((DirectionMask.U | DirectionMask.UN | DirectionMask.UE | DirectionMask.US | DirectionMask.UW) & direction) != 0)
            vect += new Vector3Int(0, 0, 1);
        if (((DirectionMask.D | DirectionMask.DN | DirectionMask.DE | DirectionMask.DS | DirectionMask.DW) & direction) != 0)
            vect += new Vector3Int(0, 0, -1);
        return vect;
    }

    public static Vector3Int Vector3Floor(Vector3 v)
    {
        return new Vector3Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
    }

    /// <summary>
    /// Swaps the bits at the two given positions. 
    /// !Does not bounds check.
    /// </summary>
    /// <param name="num"></param>
    /// <param name="pos1"></param>
    /// <param name="pos2"></param>
    /// <returns></returns>
    public static int SwapBits(int num, int pos1, int pos2)
    {
        int bit1 = (num >> pos1) & 1;
        int bit2 = (num >> pos2) & 1;

        //flip bits if different
        if ((bit1 ^ bit2) != 0) num ^= (1 << pos1) | (1 << pos2);

        return num;
    }

    public static DirectionMask SwapBits(DirectionMask mask, int pos1, int pos2)
    {
        int num = (int)mask;
        int bit1 = (num >> pos1) & 1;
        int bit2 = (num >> pos2) & 1;

        //flip bits if different
        if ((bit1 ^ bit2) != 0) num ^= (1 << pos1) | (1 << pos2);

        return (DirectionMask)num;
    }

    /// <summary>
    /// Reverses the directions of the mask.
    /// </summary>
    /// <param name="directions"></param>
    /// <returns></returns>
    public static DirectionMask ReverseMask(DirectionMask direction)
    {
        //theres probably a smarter way to do this but i eepy
        DirectionMask mask = direction;
        mask = SwapBits(mask, bitPositions[DirectionMask.N], bitPositions[DirectionMask.S]);
        mask = SwapBits(mask, bitPositions[DirectionMask.W], bitPositions[DirectionMask.E]);
        mask = SwapBits(mask, bitPositions[DirectionMask.U], bitPositions[DirectionMask.D]);

        mask = SwapBits(mask, bitPositions[DirectionMask.UN], bitPositions[DirectionMask.DS]);
        mask = SwapBits(mask, bitPositions[DirectionMask.UE], bitPositions[DirectionMask.DW]);
        mask = SwapBits(mask, bitPositions[DirectionMask.US], bitPositions[DirectionMask.DN]);
        mask = SwapBits(mask, bitPositions[DirectionMask.UW], bitPositions[DirectionMask.DE]);

        mask = SwapBits(mask, bitPositions[DirectionMask.NE], bitPositions[DirectionMask.SW]);
        mask = SwapBits(mask, bitPositions[DirectionMask.SE], bitPositions[DirectionMask.NW]);
        return mask;
    }

    /// <summary>
    /// Returns the cardinal directions of a ground plane. E.g. when direction is Up or Down, returns N|E|S|W.
    /// </summary>
    /// <param name="direction">Mask with a single direction representing the ground plane</param>
    /// <returns></returns>
    public static DirectionMask CardinalsSurroundingAndBelow(DirectionMask direction) => direction switch
    {
        DirectionMask.U or DirectionMask.D => DirectionMask.N | DirectionMask.E | DirectionMask.S | DirectionMask.W | DirectionMask.DN | DirectionMask.DE | DirectionMask.DS | DirectionMask.DW,
        DirectionMask.N or DirectionMask.S => DirectionMask.U | DirectionMask.E | DirectionMask.D | DirectionMask.W,
        DirectionMask.E or DirectionMask.W => DirectionMask.U | DirectionMask.N | DirectionMask.D | DirectionMask.S,
        _ => 0
    };
}

/// <summary>
/// Binary mask corresponding to different cardinal directions.
/// !Don't convert to signed due to arithmetic bit shifting.
/// </summary>
[Flags]
public enum DirectionMask : uint //? maybe unnecessary to use short...
{
    N = 1 << 0,
    E = 1 << 1,
    S = 1 << 2,
    W = 1 << 3,

    U = 1 << 4,
    D = 1 << 5,

    UN = 1 << 6,
    UE = 1 << 7,
    US = 1 << 8,
    UW = 1 << 9,

    DN = 1 << 10,
    DE = 1 << 11,
    DS = 1 << 12,
    DW = 1 << 13,

    NE = 1 << 14,
    SE = 1 << 15,
    SW = 1 << 16,
    NW = 1 << 17,
}
