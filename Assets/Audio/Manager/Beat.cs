using System;
using UnityEngine;
using UnityEngine.Pool;
/// <summary>
/// 
/// </summary>
[Serializable]
public class Beat : MonoBehaviour
{
    public double positionInBeats;
    public bool isPlayer2;
    [NonSerialized] public RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Uses updated data from BeatManager to set beat's position.
    /// TODO: Currently non-animated
    /// </summary>
    public void SetX(float x)
    {
        //TODO: Interpolate between position changes
        Vector2 currentPos = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = new Vector2(x, currentPos.y);
    }
}

/// <summary>
/// Holds minimal beat data for all beats to be constantly updated by beatManager
/// </summary>
[Serializable]
public struct BeatData
{
    public double positionInBeats;
    public bool isPlayer2;

    public BeatData(double positionInBeats, bool isPlayer2)
    {
        this.positionInBeats = positionInBeats;
        this.isPlayer2 = isPlayer2;
    }
}
