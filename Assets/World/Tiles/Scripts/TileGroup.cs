using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Should be attached to a Grid gameObject.
/// TODO:
/// Give poses to tilegroup
///     * postiions and rotations, as a list to iterate through
///     * record initial position, set to different pose
/// Record positions in tiles
///     * Record initial position
///     * On pose change, record new position and rotation
///     * (On pose change will also restitch outer connections)
///     * Refactor transform.position to use new position/rotation
/// Rewrite arrayManager to use tilegroups
///     * Change stitch function to accept groups of tiles to search through
///         *  Take small group to search thru all tiles, search thru a second super group
///         * Search thru all groups from smallest to largest? 
/// </summary>
public class TileGroup : MonoBehaviour
{
    [SerializeField] Quaternion _initialRotation;
    public Quaternion initialRotation => _initialRotation;
    [SerializeField] Vector3 _initialPosition;
    public Vector3 initialPosition => _initialPosition;
    [SerializeField] Pose[] _poses;
    [SerializeField] ArrayManager arrayManager;

    int _poseIndex = 0;
    public int poseIndex => _poseIndex;
    public Pose pose => _poses[_poseIndex];
    public Vector3 position => pose.position;
    public Quaternion rotation => pose.rotation;

    public Grid grid;

    [SerializeField, HideInInspector]
    Tilemap[] tilemapLevels;

    List<GameplayTile> _tiles;
    public List<GameplayTile> tiles => _tiles;

    [SerializeField] List<TileGroup> connectedTileGroups; 
    List<GameplayTile> neighborTiles;

    void Awake()
    {
        if (!gameObject.TryGetComponent<Grid>(out grid))
        {
            Debug.LogError("TileGroup not attached to a grid object");
        }

        neighborTiles = new List<GameplayTile>();
        if (_tiles == null)
        {
            _tiles = new List<GameplayTile>();
            foreach(Tilemap tilemap in gameObject.GetComponentsInChildren<Tilemap>())
            {
                _tiles.AddRange(tilemap.GetComponentsInChildren<GameplayTile>());
            }
        }

    }

    /// <summary>
    /// Call after level has been completedd to populate array
    /// </summary>
    public void InitializeGroup()
    {
        //Set connecting tiles within group
        ArrayManager.SetTileNeighbors(_tiles, _tiles);
        if (connectedTileGroups != null)
        {
            foreach(TileGroup group in connectedTileGroups)
            {
                neighborTiles.AddRange(group.tiles);
            }
        }
        _poseIndex = 0;
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
        ChangePose(_poseIndex);
    }

    public void InitializeTiles()
    {
        _tiles = new List<GameplayTile>();
        tilemapLevels = grid.GetComponentsInChildren<Tilemap>(); 
        foreach (Tilemap tilemap in tilemapLevels)
        {
            GameplayTile[] levelTiles = tilemap.GetComponentsInChildren<GameplayTile>();
            Debug.Log("Found tilemap: " + tilemap.name + " with " + levelTiles.Length + " tiles");
            foreach (GameplayTile tile in levelTiles)
            {
                tile.Initialize(this);
                Vector3 pos = tile.position;
                Debug.Log("Found tile at pos: " + pos);
            }
            _tiles.AddRange(levelTiles);
        }
    }

    [ContextMenu("Initialize")]
    void InitializeFromEditor()
    {
        InitializeTiles();
        InitializeGroup();
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        #endif
    }

    void ChangePose(int newIndex)
    {
        _poseIndex = newIndex;

        //TODO: Add interpolation
        transform.position = position;
        transform.rotation = rotation;

        if (connectedTileGroups != null && neighborTiles != null)
        {
            ArrayManager.SetTileNeighbors(_tiles, neighborTiles);
        }
    }
}

[Serializable]
public struct Pose
{
    [SerializeField] Vector3 _position;
    [SerializeField] Quaternion _rotation;

    public Vector3 position => _position;
    public Quaternion rotation => _rotation;
}
