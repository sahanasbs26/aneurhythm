using System;
using UnityEngine;

/// <summary>
/// Contains position, mounting, and exit/enter/surface rules of a tile.
/// Other tiles should inherit from this one.
/// All coordinates are relative to "array" space, as (x, y, z) = (forward, right, up).
/// "Surfacing" means a player can walk on the periphery of the tile (e.g. on top).
/// "Entering" means a player can walk into this tile position.
/// </summary>
[CreateAssetMenu(fileName = "TileRule", menuName = "Scriptable Objects/TileRule")]
public class TileRules : ScriptableObject
{
    [Tooltip("Faces a player can stand on")]
    [SerializeField] DirectionMask _surfaceableDirections;
    [SerializeField] DirectionMask _exitDirections;
    [SerializeField] DirectionMask _enterDirections;

    [Tooltip("For \"wall\" tiles. Specifies a surface necessary to enter the tile")]
    [SerializeField] DirectionMask _requiredSurfaceDirection; 
    [SerializeField] MountPosition _mountPosition;

    /// <summary>
    /// Directions relative to this block where player can stand.
    /// Up means the player can stand on top of the block, 
    /// North means the player can stand on the northern side of the block.
    /// *Does not have a custom mask for surfaceable exits: Assumes player can exit towards any other enterable/surfaceble direction.
    /// *Therefore, a surfaceable direction should always appear as a flat surface.
    /// To define rules on a surface, add an enterable "wall" tile 
    /// </summary>
    public DirectionMask surfaceableDirections => _surfaceableDirections;

    /// <summary>
    /// For "wall" tiles or equivalent.
    /// Directions relative to this tile where a surfacable tile is needed to enter this block.
    /// E.g. For a vertical wall, a surface below the wall is needed and therefore a downward DirectionMask.
    /// </summary>
    public DirectionMask requiredSurfaceDirection => _requiredSurfaceDirection; 

    /// <summary>
    /// Directions player can exit this tile from, relative to this block.
    /// </summary>
    public DirectionMask exitDirections => _exitDirections;

    /// <summary>
    /// Directions player can enter this block from, relative to this block
    /// </summary>
    public DirectionMask enterDirections => _enterDirections;

    /// <summary>
    /// Contains offset and base angle of the player when on this tile.
    /// </summary>
    public MountPosition mountPosition => _mountPosition;

    public bool CanEnterFrom(DirectionMask directions) => (_enterDirections & directions) != 0;
    public bool RequiresSurfaceIn(DirectionMask directions) => (_requiredSurfaceDirection & directions) != 0;
    public bool CanSurfaceOn(DirectionMask faceDirections) => (_surfaceableDirections & faceDirections) != 0;
    public bool CanExitTowards(DirectionMask directions) => (_exitDirections & directions) != 0;

}

/// <summary>
/// Binary mask corresponding to different cardinal directions.
/// !Don't convert to signed due to arithmetic bit shifting.
/// </summary>
[Flags]
public enum DirectionMask : ushort //? maybe unnecessary to use short...
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
}

/// <summary>
/// Defines orientation of the player when in the block in WORLD SPACE.
/// Offsets and angles should therefore be safe to directly apply to player.
/// !IN WORLD SPACE
/// </summary>
///
[Tooltip("Position of player when in tile IN WORLD SPACE")]
[Serializable]
public struct MountPosition
{
    /// <summary>
    /// Angle in WORLD SPACE to rotate the player when standing.
    /// Should be able to easily combine with the player's own rotation
    /// !ANGLE IN WORLD SPACE
    /// </summary>
    [Tooltip("IN WORLD SPACE")]
    public Quaternion angle;

    /// <summary>
    /// Position in array space to offset the player when standing
    /// !OFFSET IN WORLD SPACE
    /// </summary>
    [Tooltip("IN WORLD SPACE")]
    public Vector3 offset;

    /// <summary>
    /// Calculates angle vector and offset position for a slope.
    /// Offset is relative to the height of the block
    /// </summary>
    /// <param name="direction">Cardinal direction IN ARRAY SPACE players moves in when going up the stairs.</param>
    /// <param name="slopeAngle">In degrees; angle of the slope to position the player.</param>
    public void SetSlopeMount(Vector3 direction, float slopeAngle)
    {
        direction = WorldUtils.ArrayToWorldCoordinates(direction.normalized);
        offset = Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Vector3.up;
        angle = Quaternion.LookRotation(direction + offset);
    }
}
    