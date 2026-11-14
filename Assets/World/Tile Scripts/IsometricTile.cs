using UnityEngine;

public class GameplayTile : MonoBehaviour
{
    [SerializeField] TileRules _rules;
    public Vector3Int arrayPosition;
    public TileRules rules => _rules;

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
}
