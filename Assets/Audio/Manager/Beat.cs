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

    void Update()
    {
        //TODO: Interpolate between changes
        if (data != null && beatManager != null) SetPos();
        else
        {
            if (data == null)
            {
                Debug.LogError("Beat does not have data");
            } else if (beatManager == null)
            {
                Debug.LogError("Beat's manager is null");
            }
        }

        if (data.beatsUntilPlay <= 0)
        {
            //TODO: Animate natural depawning in Despawn() function itself
            Debug.Log("Despawned " + (data.isPlayer2 ? "P2" : "P1") + " beat: " + data.beatsUntilPlay);
            Despawn();
        }

        //! TODO: Handle successful beat presses
    }

    /// <summary>
    /// Uses updated data from BeatManager to set beat's position.
    /// TODO: Currently non-animated
    /// </summary>
    public void SetPos()
    {
        //TODO: Interpolate between position changes
        Vector3 currentPos = rectTransform.anchoredPosition;
        float x = data.beatsUntilPlay * beatManager.beatSpacing * (data.isPlayer2 ? -1 : 1);
        rectTransform.anchoredPosition = new Vector3(x, currentPos.y, currentPos.z);
    }

    /// <summary>
    /// Releases itself from object pool.
    /// TODO: Currently non-animated
    /// </summary>
    public void Despawn()
    {
        //TODO: Play despawn animation
        data.hasSpawned = false;
        objectPool.Release(this);
    }
}

/// <summary>
/// Holds minimal beat data for all beats to be constantly updated by beatManager
/// </summary>
[Serializable]
public class BeatData
{
    [SerializeField] public float positionInBeats;

    [NonSerialized] public float beatsUntilPlay; //Position from center along UI
    [SerializeField] public bool isPlayer2;

    [NonSerialized] public bool hasSpawned;
    public BeatData(float positionInBeats, bool isPlayer2)
    {
        this.positionInBeats = positionInBeats;
        beatsUntilPlay = 0;
        this.isPlayer2 = isPlayer2;
        hasSpawned = false;
    }
}
