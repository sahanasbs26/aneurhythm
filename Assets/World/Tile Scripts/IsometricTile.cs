using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameplayTile : MonoBehaviour
{
    public Vector3Int arrayPosition;
    [SerializeField] TileRules _rules;
    public TileRules rules => _rules;

    List<TileSurface> _surfaces;

    /// <summary>
    /// List of surfaces the player can stand on. Orientation is normal from the surface.
    /// </summary>
    public List<TileSurface> surfaces => _surfaces;
    Dictionary<DirectionMask, TileSurface> _surfaceLookup;

    void Start()
    {
        //Initialize();
    }

    public void Initialize()
    {
        TileSurface[] surfaces = transform.GetComponents<TileSurface>();
        foreach (TileSurface surface in surfaces)
        {
            DestroyImmediate(surface);
        }
        _surfaces = new List<TileSurface>();
        _surfaceLookup = new Dictionary<DirectionMask, TileSurface>();
        foreach (TileSurfaceData surfaceData in _rules.surfaces)
        {
            TileSurface surface = transform.AddComponent<TileSurface>();
            surface.Initialize(surfaceData, this);
            _surfaces.Add(surface);
            _surfaceLookup.Add(surface.orientation, surface);
        }
    }

    /// <summary>
    /// Translates from the tile's transform.position to a position in the array.
    /// Returns the transformed position.
    /// </summary>
    /// <returns></returns>
    public Vector3Int SetArrayPosition()
    {
        Vector3 rawPos = transform.position;
        arrayPosition = DirectionMaskUtils.Vector3Floor(WorldUtils.WorldToArrayCoordinates(rawPos));
        return arrayPosition;
    } 

        public TileSurface GetSurface(DirectionMask mask)
    {
        return _surfaceLookup[mask];
    }

    public bool TryGetSurface(DirectionMask mask, out TileSurface surface)
    {
        return _surfaceLookup.TryGetValue(mask, out surface);
    }
}
