using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

/// <summary>
/// Controls visual beat icons
/// Should be attached to same gameObject as SongManager
/// ?Handling of interactions & hence carrying of beatData might be better suited to be handeled by SongManager
/// </summary>
public class CalibrationBeatManager : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("The visual spacing between one beat")]
    [SerializeField] public float beatSpacing = 100;

    [Tooltip("In the parent canvas's coord space; the width from the center to the edge of the screen")]
    [SerializeField] public float screenHalfWidth = 500;

    [Tooltip("In the parent canvas's coord space")]
    [SerializeField] public float beatPositionY = 0;
    [SerializeField] public Transform parentPanel;
    [SerializeField] public Beat beatPrefab;


    [Header("Beat Pattern Properties")]
    [Tooltip("In seconds; grace period after beat hits before despawning beat")]
    public double missedBeatWindow = 0.2;
    [Tooltip("Positions of the spawned beats in terms of the beat in the song. Will loop at the end. Assumes beats are listed in chronological order.")]
    [SerializeField] BeatData[] beatPattern;
    [Tooltip("Length of the pattern in beats. If left unspecified, will assume the last beat is the length of the pattern")]
    [SerializeField] float patternLengthBeats;
    [SerializeField] double audioDelay = 0.2;
    [SerializeField] double videoDelay = 0.2;
    SongManager songManager;
    // Position of the next beat to spawn
    double nextBeat;
    // Index in the beat pattern of the next beat to spawn
    int patternIndex;
    // How many times the beat patter nhas looped
    int patternLoop;

    [Header("Object Pooling Settings")]
    [SerializeField] private int poolDefaultCapacity = 30;
    [SerializeField] private int maxPoolCapacity = 100;

    ObjectPool<Beat> beatObjectPool;
    Queue<Beat> player1BeatObjects;
    Queue<Beat> player2BeatObjects;
    Queue<Beat>[] beatQueues;


    void Awake()
    {
        if (beatPattern == null || beatPattern.Length == 0) 
        { 
            Debug.LogError("Error: beatPattern is empty or unspecified");
            enabled = false; 
        }
        if (patternLengthBeats < 0.05)
        {
            Debug.LogError("Error: patternLengthBeats is too small (<0.05)");
            enabled = false;
        }
        if (beatPattern[beatPattern.Length - 1].positionInBeats < patternLengthBeats)
        {
            Debug.LogWarning("Warning: provided patternLengthBeats is not equal to the final beat of beatPattern");
        }

        songManager = GetComponent<SongManager>();

        beatObjectPool = new ObjectPool<Beat>(CreateBeat, OnGetFromPool, OnReleaseFromPool, OnDestroyPooledObject, true, poolDefaultCapacity, maxPoolCapacity);
        player1BeatObjects = new Queue<Beat>();
        player2BeatObjects = new Queue<Beat>();
        beatQueues = new Queue<Beat>[] {player1BeatObjects, player2BeatObjects};
    }

    void Start()
    {
        patternIndex = 0;
        patternLoop = 0;
        nextBeat = beatPattern[patternIndex % beatPattern.Length].positionInBeats;
    }

    void Update()
    {
        double now = songManager.beatInSong;
        double lookahead = 1f + screenHalfWidth / beatSpacing;

        //Spawn the next beat in the pattern if it should be close to the edge of the screen
        while (nextBeat - now <= lookahead)
        {
            QueueBeat(nextBeat, beatPattern[patternIndex].isPlayer2);
            if (++patternIndex == beatPattern.Length)
            {
                patternLoop++;
                patternIndex = 0;
            }
            nextBeat = patternLoop * patternLengthBeats + beatPattern[patternIndex].positionInBeats;
        }

        foreach (Queue<Beat> queue in beatQueues)
        {    
            //Update positions of spawned beats
            foreach(Beat beat in queue)
            {
                beat.SetX((float)(beat.positionInBeats + audioDelay - now) * beatSpacing * (beat.isPlayer2 ? 1 : -1)); //spawn player1 beats from the left
            }

            //TODO: Have judgment class destroy missed beats
            //  After a waiting for the player to attempt to hit the beat, destroy
            //  For now, handle here
            while (queue.Count > 0 && (queue.Peek().positionInBeats - now + audioDelay) < -missedBeatWindow * 2 / songManager.timePerBeat)
            {
                beatObjectPool.Release(queue.Dequeue());
                Debug.Log("Missed");
            }
        }

    }

    void OnMovePlayer1(InputValue value)
    {
        double error;
        if (HitBeat(player1BeatObjects, out error))
        {
            Debug.Log("Hit: P1");
        } else
        {
            Debug.Log("Misstep: P1");
        }
        Debug.Log("Error (ms): " + error * 1000);
    }

    void OnMovePlayer2(InputValue value)
    {
        if (HitBeat(player2BeatObjects))
        {
            Debug.Log("Hit: P2");
        } else
        {
            Debug.Log("Misstep: P2");
        }
    }
    
    /// <summary>
    /// Checks if the next beat in the queue has played within missedBeatWindow. 
    /// If it has played, despawn beat and return true. Otherwise return false.
    /// Does not check if queue is null, but checks if empty.
    /// </summary>
    /// <param name="queue"> Queue of beats to check </param>
    /// <param name="error"> Holds error, compensating for audioDelay. </param>
    /// <returns> Whether or not hit was successful </returns>
    bool HitBeat(Queue<Beat> queue, out double error)
    {
        //positive = late, negative = early
        error = songManager.beatInSong - queue.Peek().positionInBeats - audioDelay;
        if (queue.Count > 0 && Math.Abs(error) * songManager.timePerBeat <= missedBeatWindow)
        {
            beatObjectPool.Release(queue.Dequeue());
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if the next beat in the queue has played within missedBeatWindow. 
    /// If it has played, despawn beat and return true. Otherwise return false.
    /// Does not check if queue is null, but checks if empty.
    /// </summary>
    /// <param name="queue"> Queue of beats to check </param>
    /// <returns> Whether or not hit was successful </returns>
    bool HitBeat(Queue<Beat> queue)
    {
        //positive = late, negative = early
        double error = songManager.beatInSong - queue.Peek().positionInBeats - audioDelay;
        if (queue.Count > 0 && Math.Abs(error) * songManager.timePerBeat <= missedBeatWindow)
        {
            beatObjectPool.Release(queue.Dequeue());
            return true;
        }
        return false;
    }
    

    /// <summary>
    /// Spawns and adds a beat object to the provided player's queues.
    /// </summary>
    void QueueBeat(double beatPosition, bool isPlayer2)
    {
        Queue<Beat> queue = isPlayer2 ? player2BeatObjects : player1BeatObjects;
        queue.Enqueue(SpawnBeat(beatPosition, isPlayer2));
    }

    /// <summary>
    /// Retrieve a beat from the pool with the specified position and player.
    /// </summary>
    Beat SpawnBeat(double position, bool isPlayer2)
    {
        Beat beat = beatObjectPool.Get();
        beat.rectTransform.anchoredPosition = new Vector2 (0, beatPositionY);
        beat.positionInBeats = position;
        beat.isPlayer2 = isPlayer2;
        return beat;
    }

    /// <summary>
    /// Beat's data should be assigned after it is gotten from the pool
    /// </summary>
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
        Destroy(beat.gameObject);
    }
}
