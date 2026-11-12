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
    public Vector3Int FindMoveableTarget(Vector3Int currentPos, Vector3 directionVector, DirectionMask groundPlane)
    {
        DirectionMask direction = DirectionMaskUtils.VectorToMaskLenient(directionVector);
        GameplayTile current = array[currentPos.x, currentPos.y, currentPos.z];
        DirectionMask possibleDirections = (current == null) ? 
            DirectionMaskUtils.CardinalsSurroundingAndBelow(groundPlane) & direction : 
            current.rules.exitDirections & direction;

        foreach (DirectionMask mask in Enum.GetValues(typeof(DirectionMask)))
        {
            if ((possibleDirections & mask) == 0) continue;
            
            Debug.Log("Checking direction: " + mask);
            Vector3Int target = DirectionMaskUtils.MaskToVector(mask) + currentPos;
            Debug.Log("Checking Target" + target + " from current" + currentPos);
            if (CanMove(target, currentPos, direction, groundPlane))
            {
                Debug.Log("Found target: " + target);
                return target;
            }
        }
        return currentPos;
    }

    /// <summary>
    /// Determines whether player can move in a direction based on their position and surrounding tiles.
    /// </summary>
    /// <param name="direction">Direction the player will move in, in array space</param>
    /// <param name="currentPos">Position of the player, in array space</param>
    /// <param name="groundPlane">The direction of the plane the player is standing on, e.g. "U" if the player is standing on top of a block</param>
    /// <returns></returns>
    public bool CanMove(Vector3Int targetPos, Vector3Int currentPos, DirectionMask directionMask, DirectionMask groundPlane)
    {
        Vector3Int groundingTargetPos = targetPos - DirectionMaskUtils.MaskToVector(groundPlane);
        Debug.Log(groundingTargetPos + ", World: " + WorldUtils.ArrayToWorldCoordinates(groundingTargetPos));

        GameplayTile current = array[currentPos.x, currentPos.y, currentPos.z];
        GameplayTile target = array[targetPos.x, targetPos.y, targetPos.z];
        GameplayTile groundingTarget = array[groundingTargetPos.x, groundingTargetPos.y, groundingTargetPos.z];
        Debug.Log(groundingTarget);
        Debug.Log("Ground: Exists=" + (groundingTarget != null) + ", CanSurfaceOn=" + (groundingTarget != null && groundingTarget.rules.CanSurfaceOn(groundPlane)));

        //Surface is valid if:
        //  there is a ground available in the player's grounding direction and
        //  the target has no surface restrictions or the target's surface restrictions match the ground direction 
        bool validSurface = 
            groundingTarget != null && groundingTarget.rules.CanSurfaceOn(groundPlane) &&
            (target == null || !target.rules.RequiresSurface() || target.rules.RequiresSurfaceIn(groundPlane));

        bool canExit = current == null || current.rules.CanExitTowards(directionMask);

        bool canEnterTarget = validSurface || (target != null && target.rules.CanEnterFrom(DirectionMaskUtils.ReverseMask(directionMask)));

        Debug.Log("Surface " + validSurface + ", Exit " + canExit + ", Enter " + canEnterTarget);
        
        return canExit && canEnterTarget;
    }

    void OnMovePlayer1(InputValue input)
    {
        Debug.Log("Moved player");
        player1.OnMove(input);
    }

    void OnMovePlayer2(InputValue input)
    {
        player2.OnMove(input);
    }
}
