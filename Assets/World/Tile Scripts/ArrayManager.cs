using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
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

    [SerializeField] PlayerController player1;
    [SerializeField] PlayerController player2;

    //Only used during initialization; not saved
    Dictionary<Vector3Int, GameplayTile> tiles;


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
        //Initialize();
    }
    /// <summary>
    /// Call after level has been completedd to populate array
    /// </summary>
    [ContextMenu("Initialize")]
    void Initialize()
    {
        //Add all tiles to array 
        tiles = new Dictionary<Vector3Int, GameplayTile>();
        tilemapLevels = grid.GetComponentsInChildren<Tilemap>(); 
        foreach (Tilemap tilemap in tilemapLevels)
        {
            GameplayTile[] levelTiles = tilemap.GetComponentsInChildren<GameplayTile>();
            Debug.Log("Found tilemap: " + tilemap.name + " with " + levelTiles.Length + " tiles");
            foreach (GameplayTile tile in levelTiles)
            {
                Vector3Int pos = tile.SetArrayPosition();
                tile.Initialize();
                Debug.Log("Found tile at pos: " + pos);
                if (!tiles.TryAdd(pos, tile))
                {
                    Debug.LogError($"Error during map initialization: Multiple tiles found at position {pos}");
                }
            }
        }
        SetTileNeighbors();

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
        #endif
    }

    /// <summary>
    /// Sets the neighbors of all adjacent matching ports.
    /// ?Check performance 
    /// </summary>
    public void SetTileNeighbors()
    {
        Debug.Log("Setting tile neighbors");
        foreach((Vector3Int pos, GameplayTile tile) in tiles)
        {
            HashSet<(Vector3Int, DirectionMask)> targetPositions = new HashSet<(Vector3Int, DirectionMask)>();

            //Get retrieve "possible" tile positions based on tiles in direction of port orienations
            foreach(TileSurface surface in tile.surfaces)
            {
                Debug.Log("\tScanning surfaces of tile @ " + tile.arrayPosition);
                foreach(TilePort port in surface.ports)
                {
                    Vector3Int targetPos = pos + DirectionMaskUtils.MaskToVector(port.orientation);

                    //* VectorToMaskLenient: Currently set to add any port that is in general direction of port orientation rather than specific direction
                    // (e.g. N would connect to NE, NW, UN, DN)
                    targetPositions.Add((targetPos, DirectionMaskUtils.VectorToMaskLenient(targetPos - pos)));
                    Debug.Log("\t\tFrom " + surface.parent.arrayPosition + ", " + surface.orientation + " found port: " + port.orientation + ", H=" + port.height + ", Target="+targetPos);
                }
            }

            //Scan every surface and target surface for matching ports; only searches ports with matching relativeDirection
            foreach ((Vector3Int targetPos, DirectionMask relativeDirection) in targetPositions)
            {
                if (!tiles.TryGetValue(targetPos, out GameplayTile target)) continue;

                Debug.Log("\tTarget found @" + targetPos);
                foreach (TileSurface surface in tile.surfaces)
                {
                    foreach(TileSurface targetSurface in target.surfaces)
                    {
                        surface.SetNeighbors(targetSurface, relativeDirection);
                    }
                }
            }
        }
    }

    //Determine potential movement targets as all faces in general direction of movement & current tile's exit
    //In a priority order, check whether the player can move in those directions & move if able
    public bool FindMoveableTarget(TileSurface surface, Vector3 directionVector, out TileSurface outTileSurface)
    {
        DirectionMask directionCone = DirectionMaskUtils.VectorToMaskLenient(directionVector);
        foreach(DirectionMask dir in Enum.GetValues(typeof(DirectionMask))) {
            if ((directionCone & dir) == 0)                  continue;
            if (!surface.TryGetPort(dir, out TilePort port)) continue;
            if (port.neighbor == null)                       continue;
            outTileSurface = port.neighbor.parentSurface;
            return true;
        }
        outTileSurface = null;
        return false;
    }
}
