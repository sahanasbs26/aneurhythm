using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Simulates isometric world in an array
/// </summary>
public class ArrayManager : MonoBehaviour
{
    public Grid grid;
    public GridData gridData;
    public TileGroup tileGroup;

    public static readonly float EPSILON_ROUNDED_SQR_MAGNITUDE = 0.01f; 


    void Awake()
    {
        if (gridData == null)
        {
            Debug.LogError("Assigned grid does not have a GridData attached. Make sure grid was created using Tools->Generate Grid");
            return;
        }
    }

    /// <summary>
    /// Logs a warning if the vector is far from an integer position
    /// </summary>
    /// <param name="pos"></param>
    /// <returns>Rounded vector</returns>
    public static Vector3Int RoundAndWarn(Vector3 pos)
    {
        Vector3Int roundedPos = Vector3Int.RoundToInt(pos - new Vector3(0.5f, 0.5f, 0.5f));
        if ((roundedPos - pos).sqrMagnitude >= EPSILON_ROUNDED_SQR_MAGNITUDE)
        {
            Debug.LogWarning($"Warning in ArrayManager Initialization: exact tile position deviates from a grid position by more than {EPSILON_ROUNDED_SQR_MAGNITUDE}");
        }
        return roundedPos;
    }

    /// <summary>
    /// Sets the neighbors of all adjacent matching ports.
    /// ?Check performance 
    /// </summary>
    public static void SetTileNeighbors(List<GameplayTile> originTiles, List<GameplayTile> neighborTiles)
    {
        Dictionary<Vector3Int, GameplayTile> originMap = new Dictionary<Vector3Int, GameplayTile>();
        Dictionary<Vector3Int, GameplayTile> neighborMap = new Dictionary<Vector3Int, GameplayTile>();
        foreach (GameplayTile tile in originTiles)
        {
            originMap.Add(RoundAndWarn(tile.position), tile);
        }
        foreach (GameplayTile tile in neighborTiles)
        {
            neighborMap.Add(RoundAndWarn(tile.position), tile);
        }

        UnlinkTiles(originTiles, neighborTiles);

        Debug.Log("Setting tile neighbors");
        foreach((Vector3Int pos, GameplayTile tile) in originMap)
        {
            HashSet<(Vector3Int, Directions.Mask)> targetPositions = PossibleNeighborPositions(pos, tile);

            //Scan every surface and target surface for matching ports; only searches ports with matching relativeDirection
            foreach ((Vector3Int targetPos, Directions.Mask relativeDirection) in targetPositions)
            {
                if (!neighborMap.TryGetValue(targetPos, out GameplayTile target)) continue;
                AssignNeighbors(tile, target, relativeDirection);
                Debug.Log("\tTarget found @" + targetPos);
  
            }
        }
    }

    /// <summary>
    /// Unlinks ports in origin that connect to ports in neighbor
    /// </summary>
    /// <param name="originTiles"></param>
    /// <param name="neighborTiles"></param>
    public static void UnlinkTiles(List<GameplayTile> originTiles, List<GameplayTile> neighborTiles)
    {
        foreach (GameplayTile tile in originTiles)
        {
            foreach (TileSurface surface in tile.surfaces)
            {
                foreach (TilePort port in surface.ports)
                {
                    if (port.neighbor != null && neighborTiles.Contains(port.neighbor.parentSurface.parentTile))
                    {
                        port.Unlink();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Retrieves a set of positions adjacent to the tile and in the direction of the tile's ports 
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="roundedPos"></param>
    /// <returns></returns>
    public static HashSet<(Vector3Int,Directions.Mask)> PossibleNeighborPositions(Vector3Int roundedPos, GameplayTile tile)
    {
        HashSet<(Vector3Int, Directions.Mask)> targets = new HashSet<(Vector3Int, Directions.Mask)>();
        
        //Retrieve "possible" neighboring tile positions based on tiles in direction of port orientations
        foreach(TileSurface surface in tile.surfaces)
        {
            Debug.Log("\tScanning surfaces of tile @ " + tile.transform.position);
            foreach(TilePort port in surface.ports)
            {   
                //Find target position based off port orientation & add to set of possible positions
                //? Currently only searches in single relative direction
                Directions.Mask worldOrientation = port.orientation;
                Vector3Int targetDirection = WorldUtils.ArrayToWorldCoordinates(Directions.MaskToVector(worldOrientation));
                Vector3Int targetPos = roundedPos + targetDirection;

                targets.Add((targetPos, worldOrientation));
                //Debug.Log($"\t\tFrom {(string)surface} found port: {(string)port}, Target={targetPos}");
            }
        }
        return targets;
    }

    public static void AssignNeighbors(GameplayTile tile, GameplayTile target, Directions.Mask relativeDirection)
    {
        foreach (TileSurface surface in tile.surfaces)
        {
            foreach(TileSurface targetSurface in target.surfaces)
            {
                surface.SetNeighbors(targetSurface, relativeDirection);
            }
        }
    }

    //Determine potential movement targets as all faces in general direction of movement & current tile's exit
    //In a priority order, check whether the player can move in those directions & move if able
    public static bool FindMoveableTarget(TileSurface surface, Vector3 directionVector, out TileSurface outTileSurface)
    {
        Directions.Mask directionCone = Directions.GeneralDirectionToMask(directionVector);
        foreach(Directions.Mask dir in Directions.ALL_DIRECTIONS) {
            if (
                (directionCone & dir) == 0 ||                  
                !surface.TryGetPort(dir, out TilePort port) ||
                port.neighbor == null ||                       
                !port.neighbor.parentSurface.CanEnter(dir)
                ) continue;
            outTileSurface = port.neighbor.parentSurface;
            return true;
        }
        outTileSurface = null;
        return false;
    }

    /// <summary>
    /// Adds a tilemap level at a specified height
    /// </summary>
    public static void AddTileMapLevel(Transform parent, int height)
    {
        GameObject level = new GameObject("Level " + height);
        Tilemap tilemap = level.AddComponent<Tilemap>();

        tilemap.orientation = Tilemap.Orientation.XZ;
        level.AddComponent<TilemapRenderer>();
        level.transform.position = Vector3.up * height;

        level.transform.SetParent(parent, false);
    }
}
