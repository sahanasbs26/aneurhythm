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
    [SerializeField] protected Quaternion _initialRotation;
    public Quaternion initialRotation => _initialRotation;
    [SerializeField] protected Vector3 _initialPosition;
    public Vector3 initialPosition => _initialPosition;
    [SerializeField] protected Pose[] _poses;
    [SerializeField] ArrayManager arrayManager;

    protected int _poseIndex = 0;
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
    protected List<GameplayTile> neighborTiles;

    void Awake()
    {
        if (!gameObject.TryGetComponent<Grid>(out grid))
        {
            Debug.LogError("TileGroup not attached to a grid object");
        }

    }

    /// <summary>
    /// Must be called after all tiles in the TileGroups of connectedTileGroups have been initialized 
    /// </summary>
    public void InitializeGroup()
    {
        //Set connecting tiles within group
        neighborTiles = new List<GameplayTile>();
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
        ChangeStitchPose(_poseIndex);
        SetVisualPosition();
    }

    /// <summary>
    /// Initialize child tiles and compile them into a list
    /// </summary>
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
                //Vector3 pos = tile.position;
                //Debug.Log("Found tile at pos: " + pos);
            }
            _tiles.AddRange(levelTiles);
        }
    }

    [ContextMenu("Initialize")]
    void InitializeTilesFromEditor()
    {
        InitializeTiles();
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        #endif
    }


    /// <summary>
    /// Changes simulated pose and connections between blocks without yet setting the transform's position
    /// </summary>
    /// <param name="newIndex"></param>
    protected virtual void ChangeStitchPose(int newIndex)
    {
        _poseIndex = newIndex;

        StitchNeighbors();
    }

    /// <summary>
    /// Sets transform's position and rotation to its simulated position (based off current pose).
    /// </summary>
    public void SetVisualPosition()
    {
        transform.position = position;
        transform.rotation = rotation;
    }

    protected void StitchNeighbors()
    {
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
    [SerializeField] Vector3 _rotation;
    [SerializeField] int _beat;

    public Vector3 position => _position;
    public Quaternion rotation => Quaternion.Euler(_rotation);
    public int beat => _beat;
}
