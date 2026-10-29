using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    ObjectPool<Beat> beatObjectPool;
    Queue<BeatData> activeBeatObjects;

    [Tooltip("Positions of the spawned beats in terms of the beat in the song. Will loop at the end. Assumes beats are listed in chronological order")]
    [SerializeField] BeatData[] presetBeatData;
    [SerializeField] public List<BeatData> pregeneratedBeatData;

    void Awake()
    {
        songManager = GetComponent<SongManager>();

        beatObjectPool = new ObjectPool<Beat>(CreateBeat, OnGetFromPool, OnReleaseFromPool, OnDestroyPooledObject, true, poolDefaultCapacity, maxPoolCapacity);
        pregeneratedBeatData = new List<BeatData>();
    }

    void Start()
    {
        InitializeBeatData();
        pregeneratedLoops = 0;
    }

    void Update()
    {
        //If beatData is empty (e.g. at the start of a song loop), reinitialize
        if (pregeneratedBeatData != null && Math.Ceiling(songManager.loopNumber) > pregeneratedLoops && (songManager.beatsPerLoop - songManager.beatInLoop) * beatSpacing < screenHalfWidth + beatSpacing)
        {
            Debug.Log("Regenerated beat data");
            InitializeBeatData();
            pregeneratedLoops = (int)Math.Ceiling(songManager.loopNumber);
        }

        //Update positions of beatData
        if (pregeneratedBeatData != null)
        {        
            foreach (BeatData beatData in pregeneratedBeatData)
            {
                beatData.beatsUntilPlay = beatData.positionInBeats - (float)songManager.beatInSong;

                //Spawn beats that should appear on screen
                if (!beatData.hasSpawned && beatData.beatsUntilPlay > 0 && beatData.beatsUntilPlay * beatSpacing <= (screenHalfWidth + beatSpacing)) //Spawn beats offscreen
                {
                    Debug.Log("Spawned " + (beatData.isPlayer2 ? "P2" : "P1") + " beat: " + beatData.beatsUntilPlay);
                    beatData.hasSpawned = true;
                    SpawnBeat(beatData);
                }

                //Despawning is handled by the beat itself.
            }
            pregeneratedBeatData.RemoveAll(beat => beat.beatsUntilPlay <= 0);
        }
    }

    Beat SpawnBeat(BeatData beatData)
    {
        Beat beat = beatObjectPool.Get();
        beat.data = beatData;
        beat.SetPos();
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

    /// <summary>
    /// Fills beatData with loops of the presetBeatData up to the number of beats in the song loop;
    ///! Might result in weirdness if position/beatsPerLoop is not an integer
    /// </summary>
    public void InitializeBeatData()
    {
        //Fills list of beatdata with repetitions of the presetBeatData for one loop
        for (float i = 0; i < songManager.beatsPerLoop; i += presetBeatData[presetBeatData.Length - 1].positionInBeats)
        {
            foreach(BeatData beatData in presetBeatData)
            {
                pregeneratedBeatData.Add(new BeatData((float)(Math.Ceiling(songManager.loopNumber) * songManager.beatsPerLoop) + beatData.positionInBeats + i, beatData.isPlayer2));
            }
        }
    }
}
