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
    [NonSerialized] public RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
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
