using UnityEngine;

public class GameplayTile : MonoBehaviour
{
    [SerializeField] TileRules _rules;
    public TileRules rules => _rules;
}
