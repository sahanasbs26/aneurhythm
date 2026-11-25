using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains position, mounting, and exit/enter/surface rules of a tile.
/// Other tiles should inherit from this one.
/// All coordinates are relative to cartesian coordinate space, as (x, y, z) = (E, N, U) = (forward, right, up).
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
        if (_surfaces == null)
        {
            Debug.LogError("Error in TileRules: List of surfaces is empty");
            return;
        } 
        for (int surfaceIndex = 0; surfaceIndex < _surfaces.Length; surfaceIndex++)
        {
            surfaces[surfaceIndex].OnValidate();
            for (int portIndex = 0; portIndex < surfaces[surfaceIndex].ports.Count; portIndex++)
            {
                surfaces[surfaceIndex].ports[portIndex].OnValidate();
            }
        }
    }
}

//TODO: Check destructiveness of maskToVector/vectorToMask functions

public class Directions
{

    /// <summary>
    /// Binary mask corresponding to different cardinal directions.
    /// !Don't convert to signed due to arithmetic bit shifting.
    /// </summary>
    [Flags]
    public enum Mask : uint //? maybe unnecessary to use short...
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

        UNE = 1 << 18,
        USE = 1 << 19,
        USW = 1 << 20,
        UNW = 1 << 21,

        DNE = 1 << 22,
        DSE = 1 << 23,
        DSW = 1 << 24,
        DNW = 1 << 25,
    }
    public static Dictionary<Mask, int> bitPositions = new Dictionary<Mask, int>
    {
        {Mask.N, 0},
        {Mask.E, 1}, 
        {Mask.S, 2}, 
        {Mask.W, 3},

        {Mask.U, 4}, 
        {Mask.D, 5}, 

        {Mask.UN, 6},
        {Mask.UE, 7}, 
        {Mask.US, 8}, 
        {Mask.UW, 9},

        {Mask.DN, 10},
        {Mask.DE, 11}, 
        {Mask.DS, 12}, 
        {Mask.DW, 13},

        {Mask.NE, 14},
        {Mask.SE, 15}, 
        {Mask.SW, 16}, 
        {Mask.NW, 17},

        {Mask.UNE, 18},
        {Mask.USE, 19}, 
        {Mask.USW, 20}, 
        {Mask.UNW, 21},

        {Mask.DNE, 22},
        {Mask.DSE, 23}, 
        {Mask.DSW, 24}, 
        {Mask.DNW, 25},
    };

    /// <summary>
    /// Maps a single direction to a vector of +/- 1s. Not necessarily normalized.
    /// </summary>
    public static Dictionary<Mask, Vector3Int> singleMaskToVector = new Dictionary<Mask, Vector3Int>
    {
        {Mask.N, new Vector3Int(0, 1, 0)},
        {Mask.E, new Vector3Int(1, 0, 0)}, 
        {Mask.S, new Vector3Int(0, -1, 0)}, 
        {Mask.W, new Vector3Int(-1, 0, 0)},

        {Mask.U, new Vector3Int(0, 0, 1)}, 
        {Mask.D, new Vector3Int(0, 0, -1)}, 

        {Mask.UN, new Vector3Int(0, 1, 1)},
        {Mask.UE, new Vector3Int(1, 0, 1)}, 
        {Mask.US, new Vector3Int(0, -1, 1)}, 
        {Mask.UW, new Vector3Int(-1, 0, 1)},

        {Mask.DN, new Vector3Int(0, 1, -1)},
        {Mask.DE, new Vector3Int(1, 0, -1)},
        {Mask.DS, new Vector3Int(0, -1, -1)},
        {Mask.DW, new Vector3Int(-1, 0, -1)},

        {Mask.NE, new Vector3Int(1, 1, 0)},
        {Mask.SE, new Vector3Int(1, -1, 0)},
        {Mask.SW, new Vector3Int(-1, -1, 0)},
        {Mask.NW, new Vector3Int(-1, 1, 0)},

        {Mask.UNE, new Vector3Int(1, 1, 1)},
        {Mask.USE, new Vector3Int(1, -1, 1)},
        {Mask.USW, new Vector3Int(-1, -1, 1)},
        {Mask.UNW, new Vector3Int(-1, 1, 1)},

        {Mask.DNE, new Vector3Int(1, 1, -1)},
        {Mask.DSE, new Vector3Int(1, -1, -1)},
        {Mask.DSW, new Vector3Int(-1, -1, -1)},
        {Mask.DNW, new Vector3Int(-1, 1, -1)},
    };

    public static readonly int NUM_DIRECTIONS = 26;

    public static readonly Mask[] ALL_DIRECTIONS = new Mask[]
    {
        Mask.N,Mask.E,Mask.S,Mask.W,Mask.U,Mask.D,
        Mask.UN,Mask.UE,Mask.US,Mask.UW,
        Mask.DN,Mask.DE,Mask.DS,Mask.DW,
        Mask.NE,Mask.SE,Mask.SW,Mask.NW,
        Mask.UNE,Mask.USE,Mask.USW,Mask.UNW,
        Mask.DNE,Mask.DSE,Mask.DSW,Mask.DNW
    };

    public static bool ExactlyOneBit(Mask direction)
    {
        uint d = (uint)direction;
        return (d != 0) && (d & (d - 1)) == 0;
    }

    /// <summary>
    /// Returns a DirectionMask in all the general directions of the given vector.
    /// E.g. a vector in +x will return a direciton mask in N, UN, DN, NE, etc. directions
    /// Pass an integer for best results. 
    /// </summary>
    /// <param name="direction">Vector in ARRAY SPACE</param>
    /// <returns></returns>
    public static Mask GeneralDirectionToMask(Vector3 direction)
    {
        Mask mask = 0;
        if (direction.y > 0)
        {
            mask |= Mask.N | Mask.DN | Mask.UN | Mask.NE | Mask.NW | Mask.UNE | Mask.UNW | Mask.DNE | Mask.DNW;
        } else if (direction.y < 0)
        {
            mask |= Mask.S | Mask.DS | Mask.US | Mask.SE | Mask.SW | Mask.USE | Mask.USW | Mask.DSE | Mask.DSW;
        }

        if (direction.x > 0)
        {
            mask |= Mask.E | Mask.DE | Mask.UE | Mask.NE | Mask.SE | Mask.USE | Mask.UNE | Mask.DSE | Mask.DNE;
        } else if (direction.x < 0)
        {
            mask |= Mask.W | Mask.DW | Mask.UW | Mask.NW | Mask.SW | Mask.USW | Mask.UNW | Mask.DSW | Mask.DNW;
        }

        if (direction.z > 0)
        {
            mask |= Mask.U | Mask.UN | Mask.UE | Mask.US | Mask.UW | Mask.UNE | Mask.UNW | Mask.USE | Mask.USW;
        } else if (direction.z < 0)
        {
            mask |= Mask.D | Mask.DN | Mask.DE | Mask.DS | Mask.DW | Mask.DNE | Mask.DNW | Mask.DSE | Mask.DSW;
        }

        return mask;
    }

    /// <summary>
    /// Returns a direction mask in the single direction closest to the given vector in array space
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static Mask VectorToClosestMask(Vector3 direction)
    {
        if (direction == Vector3.zero) return 0;

        float minDistance = float.MaxValue;
        Mask closest = 0;
        foreach(Mask mask in ALL_DIRECTIONS)
        {
            float distance = (direction.normalized - ((Vector3)singleMaskToVector[mask]).normalized).magnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = mask;
            }
        }
        return closest;
    }
    
    /// <summary>
    /// Returns an array space vector in directions of the direction mask.
    /// Values in any direction are constrained to 1.
    /// Not normalized
    /// (x, y, z) = (E, N, U)
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static Vector3Int MaskToVector(Mask direction)
    {
        if (!ExactlyOneBit(direction))
        {
            Debug.LogWarning("Warning from MaskToVector: Passed mask with multiple directions");
        }
        Vector3Int v = Vector3Int.zero;
        foreach(Mask mask in ALL_DIRECTIONS)
        {
            if ((direction & mask) != 0)
            {
                v += singleMaskToVector[mask];
            }
        }

        //Normalize each component to +/- 1
        return new Vector3Int(Mathf.Clamp(v.x, -1, 1), Mathf.Clamp(v.y, -1, 1), Mathf.Clamp(v.z, -1, 1));
    }

    /// <summary>
    /// Swaps the bits at the two given positions. 
    /// !Does not bounds check.
    /// </summary>
    /// <param name="num"></param>
    /// <param name="pos1"></param>
    /// <param name="pos2"></param>
    /// <returns></returns>
    public static uint SwapBits(uint num, int pos1, int pos2)
    {
        uint bit1 = (num >> pos1) & 1;
        uint bit2 = (num >> pos2) & 1;

        //flip bits if different
        if ((bit1 ^ bit2) != 0) num ^= (uint)(1 << pos1) | (uint)(1 << pos2);

        return num;
    }

    public static Mask SwapBits(Mask mask, int pos1, int pos2)
    {
        int num = (int)mask;
        int bit1 = (num >> pos1) & 1;
        int bit2 = (num >> pos2) & 1;

        //flip bits if different
        if ((bit1 ^ bit2) != 0) num ^= (1 << pos1) | (1 << pos2);

        return (Mask)num;
    }

    /// <summary>
    /// Reverses the directions of the mask.
    /// </summary>
    /// <param name="directions"></param>
    /// <returns></returns>
    public static Mask ReverseMask(Mask direction)
    {
        //theres probably a smarter way to do this but i eepy
        Mask mask = direction;
        mask = SwapBits(mask, bitPositions[Mask.N], bitPositions[Mask.S]);
        mask = SwapBits(mask, bitPositions[Mask.W], bitPositions[Mask.E]);
        mask = SwapBits(mask, bitPositions[Mask.U], bitPositions[Mask.D]);

        mask = SwapBits(mask, bitPositions[Mask.UN], bitPositions[Mask.DS]);
        mask = SwapBits(mask, bitPositions[Mask.UE], bitPositions[Mask.DW]);
        mask = SwapBits(mask, bitPositions[Mask.US], bitPositions[Mask.DN]);
        mask = SwapBits(mask, bitPositions[Mask.UW], bitPositions[Mask.DE]);

        mask = SwapBits(mask, bitPositions[Mask.NE], bitPositions[Mask.SW]);
        mask = SwapBits(mask, bitPositions[Mask.SE], bitPositions[Mask.NW]);

        mask = SwapBits(mask, bitPositions[Mask.UNE], bitPositions[Mask.DSW]);
        mask = SwapBits(mask, bitPositions[Mask.USE], bitPositions[Mask.DNW]);
        mask = SwapBits(mask, bitPositions[Mask.USW], bitPositions[Mask.DNE]);
        mask = SwapBits(mask, bitPositions[Mask.UNW], bitPositions[Mask.DSE]);
        return mask;
    }
}
