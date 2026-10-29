using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Controls visual beat icons
/// Should be attached to same gameObject as SongManager
/// ?Handling of interactions & hence carrying of beatData might be better suited to be handeled by SongManager
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
    [SerializeField] private int maxPoolCapacity = 100;

    SongManager songManager;

    int pregeneratedLoops;

    double nextBeat;
    double missedBeatWindow = 0.2;
    int patternIndex;

    [SerializeField] double front;

    ObjectPool<Beat> beatObjectPool;
    Queue<Beat> activeBeatObjects;

    [Tooltip("Positions of the spawned beats in terms of the beat in the song. Will loop at the end. Assumes beats are listed in chronological order")]
    [SerializeField] BeatData[] beatPattern;
    [SerializeField] public List<BeatData> pregeneratedBeatData;

    void Awake()
    {
        songManager = GetComponent<SongManager>();

        beatObjectPool = new ObjectPool<Beat>(CreateBeat, OnGetFromPool, OnReleaseFromPool, OnDestroyPooledObject, true, poolDefaultCapacity, maxPoolCapacity);
        pregeneratedBeatData = new List<BeatData>();
        activeBeatObjects = new Queue<Beat>();
    }

    void Start()
    {
        patternIndex = 0;
        nextBeat = beatPattern[patternIndex % beatPattern.Length].positionInBeats;
        pregeneratedLoops = 0;
    }

    void Update()
    {
        double now = songManager.beatInSong;
        double lookahead = 1f + screenHalfWidth / beatSpacing;

        //Spawn the next beat in the song (enumerating through the presetBeatData)
        //point towards queue + beat distance of next preset beat, once within the lookahead, spawn beat and advance the preset index
        while (nextBeat - now <= lookahead)
        {
            activeBeatObjects.Enqueue(SpawnBeat(new BeatData(nextBeat, beatPattern[patternIndex].isPlayer2)));
            if (patternIndex == beatPattern.Length - 1)
            {
                nextBeat += beatPattern[0].positionInBeats;
            } else
            {
                nextBeat += beatPattern[patternIndex + 1].positionInBeats - beatPattern[patternIndex].positionInBeats;
            }
            patternIndex = (patternIndex + 1) % beatPattern.Length;
        }
        //Get beat from the pool and add beat to queue

        //Update positions of spawned beats
        //enumerate through queue
        foreach(Beat beat in activeBeatObjects)
        {
            beat.SetX((float)(beat.data.positionInBeats - now) * beatSpacing * (beat.data.isPlayer2 ? 1 : -1));
        }

        //TODO: Have judgment class destroy missed beats
        //  After a waiting for the player to attempt to hit the beat, destroy
        //  For now, handle here
        while (activeBeatObjects.Count > 0 && (activeBeatObjects.Peek().data.positionInBeats - now) < -missedBeatWindow)
        {
            beatObjectPool.Release(activeBeatObjects.Dequeue());
        }

    }

    Beat SpawnBeat(BeatData beatData)
    {
        Beat beat = beatObjectPool.Get();
        beat.data = beatData;
        return beat;
    }

    /// <summary>
    /// Beat's data should be assigned after it is gotten from the pool
    /// </summary>
    Beat CreateBeat()
    {
        Beat beat = Instantiate(beatPrefab, parentPanel);
        beat.beatManager = this;
        beat.songManager = songManager;
        beat.objectPool = beatObjectPool;
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
