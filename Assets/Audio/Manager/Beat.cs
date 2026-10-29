using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 
/// </summary>
[Serializable]
public class Beat : MonoBehaviour
{
    public BeatData data;
    public SongManager songManager;
    public BeatManager beatManager;
    
    [NonSerialized] public ObjectPool<Beat> objectPool;

    RectTransform rectTransform;

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
        Vector3 currentPos = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = new Vector3(x, currentPos.y, currentPos.z);
    }
}

/// <summary>
/// Holds minimal beat data for all beats to be constantly updated by beatManager
/// </summary>
[Serializable]
public class BeatData
{
    [SerializeField] public double positionInBeats;
    [SerializeField] public bool isPlayer2;

    public BeatData(double positionInBeats, bool isPlayer2)
    {
        this.positionInBeats = positionInBeats;
        this.isPlayer2 = isPlayer2;
    }
}
