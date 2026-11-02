using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using TMPro;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.UI;

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

    //Holds UI element (Input Field TMPro) for manual delay input
    [SerializeField] TMP_InputField delayField; 

    //Indicator that moves as a function of delay to represent the position where the beat should hit
    [SerializeField] RectTransform centerIndicator;

    //How long beat highlight lasts after hitting
    [SerializeField] float indicatorBeatLifetime = 1f;
    //Prefab of the spawned highlights after hitting
    [SerializeField] Beat indicatorBeatPrefab;
    [SerializeField] Beat beatPrefab;

    //How much to change the delay when pressing left or right in seconds
    [SerializeField] float delayChangePerMovement = 0.5f / 1000f;
    //How much to change the delay when clicking the +/- button in seconds
    [SerializeField] float delayChangePerButton = 2.0f / 1000f;
    float delayChange = 0;
 

    [Header("Beat Pattern Properties")]
    [Tooltip("In seconds; grace period after beat hits before despawning beat")]
    public double missedBeatWindow = 0.2;
    [Tooltip("Positions of the spawned beats in terms of the beat in the song. Will loop at the end. Assumes beats are listed in chronological order.")]
    [SerializeField] BeatData[] beatPattern;
    [Tooltip("Length of the pattern in beats. If left unspecified, will assume the last beat is the length of the pattern")]
    [SerializeField] float patternLengthBeats;
    public static float audioDelay = 0f;
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

    bool hideBeat = false;

    ObjectPool<Beat> beatObjectPool;
    Queue<Beat> beatQueue;


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
        beatQueue = new Queue<Beat>();
    }

    void Start()
    {
        patternIndex = 0;
        patternLoop = 0;
        nextBeat = beatPattern[0].positionInBeats;
    }

    void FixedUpdate()
    {
        if (!Mathf.Approximately(delayChange, 0))
        {
            audioDelay += delayChange;
        }
    }

    void Update()
    {
        double now = songManager.beatInSong;
        double lookahead = 1f + screenHalfWidth / beatSpacing;

        SetBaselinePos();

        //Spawn the next beat in the pattern if it should be close to the edge of the screen
        while (nextBeat - now <= lookahead)
        {
            QueueBeat(nextBeat);
            if (++patternIndex == beatPattern.Length)
            {
                patternLoop++;
                patternIndex = 0;
            }
            nextBeat = patternLoop * patternLengthBeats + beatPattern[patternIndex].positionInBeats;
        }


        foreach(Beat beat in beatQueue)
        {
            beat.SetX((float)(beat.positionInBeats - now) * -beatSpacing); //spawn player1 beats from the left
        }

        //TODO: Have judgment class destroy missed beats
        //  After a waiting for the player to attempt to hit the beat, destroy
        //  For now, handle here
        while (beatQueue.Count > 0 && (now - beatQueue.Peek().positionInBeats) * beatSpacing > screenHalfWidth)
        {
            beatObjectPool.Release(beatQueue.Dequeue());
        }
    }

    /// <summary>
    /// Sets position of the center basline indicator bar
    /// </summary>
    void SetBaselinePos()
    {
        Vector2 currentPos = centerIndicator.anchoredPosition;
        double x = audioDelay / songManager.timePerBeat * beatSpacing;
        centerIndicator.anchoredPosition = new Vector2((float)x, currentPos.y);
        delayField.text = audioDelay.ToString("F3");
    }

    /// <summary>
    /// Called when delay input field is submitted
    /// </summary>
    /// <param name="text"></param>
    public void OnSubmitDelay(string text)
    {
        audioDelay = float.Parse(text);
        Debug.Log("Submitted Delay: " + text);
    }

    /// <summary>
    /// Toggles visibility of the moving beat
    /// </summary>
    public void OnToggleHideBeat(InputValue value)
    {
        hideBeat = !hideBeat;
        Debug.Log("hideBeat: " + hideBeat);
        beatQueue.Peek().gameObject.GetComponent<Image>().enabled = !hideBeat;
    }

    void OnHit(InputValue value)
    {
        float error;
        if (HitBeat(beatQueue, out error))
        {
            Debug.Log("Hit: " + (error * 1000) + "ms");
        } else
        {
            Debug.Log("Misstep: " + (error * 1000) + "ms");
        }
    }

    /// <summary>
    /// Called when the user presses left/right or a/d to change the delay (by moving the baseline indicator bar)
    /// </summary>
    /// <param name="value"></param>
    void OnMoveDelay(InputValue value)
    {
        delayChange = delayChangePerMovement * value.Get<Vector2>().x;
    }

    /// <summary>
    /// Called by the UI button to increase delay 
    /// </summary>
    public void OnDelayIncreaseClick()
    {
        audioDelay += delayChangePerButton;
    }

    /// <summary>
    /// Called by the UI button to decrease delay
    /// </summary>
    public void OnDelayDecreaseClick()
    {
        audioDelay -= delayChangePerButton;
    }
    
    /// <summary>
    /// Checks if the next beat in the queue has played within missedBeatWindow. 
    /// If it has played, despawn beat and return true. Otherwise return false.
    /// Does not check if queue is null, but checks if empty.
    /// </summary>
    /// <param name="queue"> Queue of beats to check </param>
    /// <param name="error"> Holds error in seconds, compensating for audioDelay. </param>
    /// <returns> Whether or not hit was successful. If queue is empty, returns false with an error of 0 </returns>
    bool HitBeat(Queue<Beat> queue, out float error)
    {
        if (queue.Count <= 0) { error = 0; return false; }

        //positive = late, negative = early
        error = (float)((songManager.beatInSong - queue.Peek().positionInBeats) * songManager.timePerBeat) - audioDelay;
        
        Beat calibrationBeat = Instantiate(indicatorBeatPrefab, parentPanel);
        calibrationBeat.rectTransform.anchoredPosition = queue.Peek().rectTransform.anchoredPosition;
        Destroy(calibrationBeat.gameObject, indicatorBeatLifetime);

        if (Mathf.Abs(error) <= missedBeatWindow)
        {
            //beatObjectPool.Release(queue.Dequeue());
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
    void QueueBeat(double beatPosition)
    {
        Beat beat = SpawnBeat(beatPosition);
        beat.gameObject.GetComponent<Image>().enabled = !hideBeat;
        beatQueue.Enqueue(beat);
    }

    /// <summary>
    /// Retrieve a beat from the pool with the specified position and player.
    /// </summary>
    Beat SpawnBeat(double position)
    {
        Beat beat = beatObjectPool.Get();
        beat.rectTransform.anchoredPosition = new Vector2 (0, beatPositionY);
        beat.positionInBeats = position;
        beat.isPlayer2 = false;
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
