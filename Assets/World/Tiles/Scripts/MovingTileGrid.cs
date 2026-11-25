using UnityEngine;

/// <summary>
/// Controls a moving grid with local connections, and conditionally sets connections with outer tiles
/// At a set pattern of beats, snaps to set orientations, interpolating between them.
/// Orientations control position/rotation and outer connections
///     ? Setting outer connections:
///         1. Make ports only ever accessible (via level design) from one other port and conditionally set a canEnter bool.
///         2. Allow for multiple ports: change TileSurface to contain multiple ports in a single direction, with conditionals for each port  
///                 Requires rewrite FindMoveableTile to search through multiple ports in the same direction,
///                 and adding a modifiable conditional to either the surface or to the ports.
///                     Conditional must allow only one port to be accessed at a time, conditional accesses the MovingTileGrid management script, 
///                         e.g.: subscribes a local enable function to an event raised by movingTileGrid
///         3. MovingTileGrid stores tilesurface/port data for each orientation and swaps between datasets on movement
///             Requires custom initialization script
///                 script can create a data object for current orientation, designer can move object & create a new orientation, orientation data is assigned in a list
///                 performance?
/// ? Player movement on rotating tile
///     1. Keep WASD/inputs absolute so player has to press a different directions to move "forwards" as platform rotates
///         Make sure movement script accounts for general directions
///         Understandable but may be initially unintuitive
///     2. Local player movement: Change movement system to forward/backward, rotate right/left
/// </summary>
public class MovingTileGrid : MonoBehaviour
{
    [SerializeField] SongManager songManager;
    [SerializeField] BeatManager beatManager;

    //Contain positional states and the beats in a set pattern, and ports to outer tiles

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public struct MovingTileOrientation
{
    [SerializeField] Vector3 position;
    [SerializeField] Quaternion rotation;

    [SerializeField] MovingTileOuterConnection[] connections;
}

public struct MovingTileOuterConnection
{
    [SerializeField] GameplayTile local;
    [SerializeField] GameplayTile[] outer;
}
