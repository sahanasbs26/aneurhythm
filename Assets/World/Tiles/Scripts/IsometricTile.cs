using System.Collections.Generic;
using UnityEngine;

public class GameplayTile : MonoBehaviour
{
    [SerializeField] TileRules _rules;
    public TileRules rules => _rules;

    [SerializeField] List<TileSurface> _surfaces;

    /// <summary>
    /// List of surfaces the player can stand on. Orientation is normal from the surface.
    /// </summary>
    public List<TileSurface> surfaces => _surfaces;
    Dictionary<Directions.Mask, TileSurface> _surfaceLookup;

    //! Add tileGroup must be initialized from ArrayManager/whatever grid the moving tile is attached to
    [SerializeField] TileGroup _tileGroup;
    public TileGroup tileGroup => _tileGroup;
    
    [SerializeField] Vector3 initialPosition;
    [SerializeField] Quaternion initialRotation;
    [SerializeField] Vector3 localPosition;
    [SerializeField] Quaternion localRotation;

    public Vector3 position => _tileGroup.position + _tileGroup.rotation * localPosition;
    public Quaternion rotation => _tileGroup.rotation * localRotation;

    void Start()
    {
        //Initialize();
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(position, new Vector3(1, 1, 1));
    }

    /// <summary>
    /// Returns the closest direction mask in world space of a rotated tile's local single bit direction mask.
    /// For convertions to a world space vector, still must convert to array vector then world space vector.
    /// </summary>
    /// <param name="mask"></param>
    /// <returns></returns>
    public Directions.Mask WorldDirection(Directions.Mask mask)
    {
        Directions.Mask cumulative = 0;
        foreach (Directions.Mask dir in Directions.ALL_DIRECTIONS)
        {
            if ((dir & mask) != 0) cumulative |= SingleBitWorldDirection(dir);
        }
        return cumulative;
    }

    /// <summary>
    /// Rotates the given mask based on the tile's rotation and returns the closest direction.
    /// </summary>
    /// <param name="mask"></param>
    /// <returns></returns>
    public Directions.Mask SingleBitWorldDirection(Directions.Mask mask)
    {
        if (!Directions.ExactlyOneBit(mask))
        {
            Debug.LogError("Error in SingleBitWorldDirection: Mask does not have exactly one bit");
            return 0;
        }

        return Directions.VectorToClosestMask(WorldUtils.WorldToArrayCoordinates(rotation * WorldUtils.ArrayToWorldCoordinates(Directions.MaskToVector(mask))));
    }

    /// <summary>
    /// Initializes surfaces & ports of the tiles based on this object's TileRules.
    /// Calls AddSurface to create a surface on the object; override with an AddComponent of the specific type of tilesurface if necessary.
    /// Automatically calls the Initialize function on the added surface.
    /// </summary>
    public void Initialize(TileGroup tileGroup)
    {
        _tileGroup = tileGroup;
        //! Uses initial visual position as initial simulated position & rotation
        initialPosition = transform.position + new Vector3(0.5f, 0.5f, 0.5f);
        initialRotation = transform.rotation;

        localPosition = Quaternion.Inverse(tileGroup.initialRotation) * (initialPosition - tileGroup.initialPosition);
        localRotation = Quaternion.Inverse(tileGroup.initialRotation) * initialRotation; 

        TileSurface[] surfaces = gameObject.GetComponents<TileSurface>();
        foreach (TileSurface surface in surfaces)
        {
            DestroyImmediate(surface);
        }
        _surfaces = new List<TileSurface>();
        _surfaceLookup = new Dictionary<Directions.Mask, TileSurface>();
        for (int i = 0; i < _rules.surfaces.Length; i++)
        {
            TileSurface surface = AddSurface();
            //surface.name = "TileSurface" + surface.orientation;
            surface.Initialize(i, this);
            _surfaces.Add(surface);
            _surfaceLookup.Add(surface.localOrientation, surface);
        }

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }


    public virtual TileSurface AddSurface()
    {
        return gameObject.AddComponent<TileSurface>(); 
    }

        public TileSurface GetSurface(Directions.Mask mask)
    {
        return _surfaceLookup[mask];
    }

    public bool TryGetSurface(Directions.Mask mask, out TileSurface surface)
    {
        return _surfaceLookup.TryGetValue(mask, out surface);
    }
}
