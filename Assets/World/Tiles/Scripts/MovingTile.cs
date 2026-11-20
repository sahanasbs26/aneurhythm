using Unity.VisualScripting;

/// <summary>
/// 0
/// </summary>
public class MovingTile : GameplayTile
{
    public override TileSurface AddSurface()
    {
        return transform.AddComponent<TileSurface>(); 
    }
}
