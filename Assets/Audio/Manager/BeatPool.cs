using System;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Should be part of the same object as its associated lane
/// </summary>
public class BeatPool : MonoBehaviour
{
    Lane lane;
    static int poolDefaultCapacity = 30;
    static int maxPoolCapacity = 100;

    [NonSerialized] public ObjectPool<Beat> pool;
    void Awake()
    {
        lane = GetComponent<Lane>();
        pool = new ObjectPool<Beat>(CreateBeat, OnGetFromPool, OnReleaseFromPool, OnDestroyPooledObject, true, poolDefaultCapacity, maxPoolCapacity);
    }

    public Beat Get()
    {
        return pool.Get();
    }

    public void Release(Beat beat)
    {
        pool.Release(beat);
    }

    /// <summary>
    /// Beat's data should be assigned after it is gotten from the pool
    /// </summary>
    Beat CreateBeat()
    {
        Beat beat = Instantiate(lane.beatPrefab, lane.parent);
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
        Destroy(beat.gameObject);
    }
}
