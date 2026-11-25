using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] TileGroup[] tileGroups;
    [SerializeField] PlayerController[] players;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach(TileGroup tileGroup in tileGroups) tileGroup.InitializeTiles();
        foreach(TileGroup tileGroup in tileGroups) tileGroup.InitializeGroup();
        foreach(PlayerController player in players) player.Initialize();
    }
}
