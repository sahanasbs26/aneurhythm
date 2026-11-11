using UnityEngine;
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
            foreach (GameplayTile tile in tiles)
            {
                Vector3 rawPos = WorldUtils.WorldToArrayCoordinates(tile.transform.position);
                Vector3Int pos = DirectionMaskUtils.Vector3Floor(rawPos);
                Debug.Log("Found tile at pos: " + pos);
                array[pos.x, pos.y, pos.z] = tile;                
            }
        }
    }

    /// <summary>
    /// Determines whether player can move in a direction based on their position and surrounding tiles.
    /// </summary>
    /// <param name="direction">Direction the player will move in, in array space</param>
    /// <param name="currentPos">Position of the player, in array space</param>
    /// <param name="groundDirection">The direction of the plane the player is standing on, e.g. "U" if the player is standing on top of a block</param>
    /// <returns></returns>
    public bool CanMove(Vector3Int direction, Vector3Int currentPos, DirectionMask groundDirection)
    {
        DirectionMask directionMask = DirectionMaskUtils.VectorToMaskStrict(direction);
        GameplayTile current = array[currentPos.x, currentPos.y, currentPos.z];
        bool canExit = current == null || current.rules.CanExitTowards(directionMask);
        Vector3Int targetPos = currentPos + direction;
        GameplayTile target = array[targetPos.x, targetPos.y, targetPos.z];

        GameplayTile groundingTarget;
        if (target == null)
        {
            //If moving to an empty tile, check for a standable block in the direction the player wants to move in
            Vector3Int groundingTargetPos = targetPos - DirectionMaskUtils.MaskToVector(groundDirection);
            groundingTarget = array[groundingTargetPos.x, groundingTargetPos.y, groundingTargetPos.z];
            return canExit && groundingTarget != null && (groundingTarget.rules.surfaceableDirections & groundDirection) != 0;
        } 
        else if (
            //If moving onto a "wall" tile, ensure that the tile's valid standable planes match the player's current plane
            target != null && target.rules.requiredSurfaceDirection != 0 &&
            (groundDirection & DirectionMaskUtils.ReverseMask(target.rules.requiredSurfaceDirection)) == 0)
        {
            return false;
        } 
        else
        {
            //If standable planes match/don't apply, check enter/exit conditions
            return canExit && target != null && target.rules.CanEnterFrom(directionMask);
        }
    }
}
