using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

/// <summary>
/// Simulates isometric world in an array
/// </summary>
public class ArrayManager : MonoBehaviour
{
    public Grid grid;
    public GridData gridData;

    [SerializeField]
    Tilemap[] tilemapLevels;

    [SerializeField] PlayerController player1;
    [SerializeField] PlayerController player2;

    GameplayTile[,,] array;

    void Awake()
    {
        if (gridData == null)
        {
            Debug.LogError("Assigned grid does not have a GridData attached. Make sure grid was created using Tools->Generate Grid");
            return;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Initialize();
    }
    /// <summary>
    /// Call after level has been completedd to populate array
    /// </summary>
    void Initialize()
    {
        Vector3Int size = gridData.bounds.size;
        array = new GameplayTile[size.x, size.y, size.z];
        tilemapLevels = grid.GetComponentsInChildren<Tilemap>(); 
        foreach (Tilemap tilemap in tilemapLevels)
        {
            GameplayTile[] tiles = tilemap.GetComponentsInChildren<GameplayTile>();
            Debug.Log("Found tilemap: " + tilemap.name + " with " + tiles.Length + " tiles");
            foreach (GameplayTile tile in tiles)
            {
                Vector3 rawPos = WorldUtils.WorldToArrayCoordinates(tile.transform.position);
                Vector3Int pos = DirectionMaskUtils.Vector3Floor(rawPos);
                Debug.Log("Found tile at pos: " + pos);
                array[pos.x, pos.y, pos.z] = tile;                
            }
        }
    }


    //Determine potential movement targets as all faces in general direction of movement & current tile's exit
    //In a priority order, check whether the player can move in those directions & move if able
    public bool FindMoveableTarget(Vector3Int currentPos, Vector3 directionVector, DirectionMask surface, out Vector3Int? outTargetPos, out TileSurface? outTileSurface)
    {
        //Default to null if no target is found or on error
        outTargetPos = null;
        outTileSurface = null;

        DirectionMask direction = DirectionMaskUtils.VectorToMaskLenient(directionVector);

        //Search for a valid current tile; accepts tiles the player is standing on if their current pos is air==null
        GameplayTile current = array[currentPos.x, currentPos.y, currentPos.z];
        if (current == null) {
            Vector3Int groundPos = currentPos - DirectionMaskUtils.MaskToVector(surface);
            current = array[groundPos.x, groundPos.y, groundPos.z];
        }
        if (current == null)
        {
            Debug.LogError("Couldn't find current tile at current{array" + currentPos + ", world" + WorldUtils.ArrayToWorldCoordinates(currentPos) + "}");
            return false;
        }

        //Retrieve and check that the surface
        TileSurface currentSurface;
        if (!current.rules.TryGetSurface(surface, out currentSurface))
        {
            Debug.LogError("Couldn't find surface(" + surface + ") of current tile at current{array" + currentPos + ", world" + WorldUtils.ArrayToWorldCoordinates(currentPos) + "}");
            return false;
        }

        //Gather all directions with matching move & port exit directions
        DirectionMask possibleDirections = 0;
        foreach(TilePort port in currentSurface.ports)
        {
            possibleDirections |= port.orientation & direction;
        }

        //! Assumes DirectionMasks are ordered in priority
        foreach(DirectionMask dir in Enum.GetValues(typeof(DirectionMask)))
        {
            if ((possibleDirections & dir) == 0) continue;
            Vector3Int targetPos = currentPos + DirectionMaskUtils.MaskToVector(dir);
            GameplayTile target = array[targetPos.x, targetPos.y, targetPos.z];
            //TODO: Handle being above the target block
            // if (target == null)
            // {
            //     Vector3Int groundPos = targetPos - DirectionMaskUtils.MaskToVector(surface);
            //     target = array[groundPos.x, groundPos.y, groundPos.z];
            // }
            if (target == null)
            {
                Debug.Log("No tile found at " + targetPos + " (world" + WorldUtils.ArrayToWorldCoordinates(targetPos) + ") looking in direction(" + dir + ")");
                continue;
            }
            foreach (TileSurface targetSurface in target.rules.surfaces)
            {
                TilePort targetPort;
                TilePort currentPort = currentSurface.GetPort(dir); //Current port should have been checked when accumulating possibleDirections
                if (targetSurface.TryGetPort(DirectionMaskUtils.ReverseMask(dir), out targetPort) && targetPort.height == currentPort.height)
                {
                    outTargetPos = targetPos;
                    outTileSurface = targetSurface;

                    Debug.Log("Found target at " + targetPos + "(world("+ WorldUtils.ArrayToWorldCoordinates(targetPos) + ") with surface orientation " + targetSurface.orientation);
                    return true;
                }
            }
        }
        Debug.Log("No ports found");
        return false;
    }
}
