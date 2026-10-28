using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Controls visual beat icons
/// Should be attached to same gameObject as SongManager
/// </summary>
public class BeatManager : MonoBehaviour
{
    [Tooltip("The visual spacing between one beat")]
    [SerializeField] public float beatSpacing = 100;
    [SerializeField] public float screenHalfWidth = 500;
    [SerializeField] public Beat beatPrefab;
    [SerializeField] public Transform parentPanel;
    [SerializeField] public float beatPositionY = 0;

    [SerializeField] private int poolDefaultCapacity = 30;
    [SerializeField] private int maxPoolCapacity = 200;

    SongManager songManager;

    ObjectPool<Beat> beatObjectPool;
    List<BeatData> activeBeatObjects;

    void Awake()
    {
        songManager = GetComponent<SongManager>();
        activeBeatObjects = new List<BeatData>();

        beatObjectPool = new ObjectPool<Beat>(CreateBeat, OnGetFromPool, OnReleaseFromPool, OnDestroyPooledObject, true, poolDefaultCapacity, maxPoolCapacity);
    }

    Beat CreateBeat()
    {
        Beat beat = Instantiate(beatPrefab, parentPanel);
        return beat;
    }

    void OnGetFromPool(Beat beat)
    {
        beat.gameObject.SetActive(true);
    }

    void OnReleaseFromPool(Beat beat)
    {
        beat.gameObject.SetActive(false);
    }

    void OnDestroyPooledObject(Beat beat)
    {
        Destroy(beat);
    }
}
