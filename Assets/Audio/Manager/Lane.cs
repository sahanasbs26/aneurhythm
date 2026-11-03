using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

/// <summary>
/// Contains functions to spawn, despawn, and update positions of beats.
/// Should be a child of the SongManager prefab.
/// GameObject should also have a BeatPool attached. 
/// </summary>
public class Lane : MonoBehaviour
{
    public Beat beatPrefab;
    public RectTransform parent;
    
    //!Set via BeatManager
    [NonSerialized] public float beatSpacingFraction;
    [SerializeField] Vector2 startPos;
    [SerializeField] Vector2 endPos;
    Queue<Beat> queue;
    BeatPool pool;

    bool showBeat = true;

    void Awake()
    {
        if (beatPrefab == null) { Debug.LogError("Error in Lane: beatPrefab not assigned"); return; }
        if (parent == null) { Debug.LogError("Error in Lane: parent not assigned"); return; }
        if (startPos == null) { Debug.LogError("Error in Lane: startPos not assigned"); return; }
        if (endPos == null) { Debug.LogError("Error in Lane: endPos not assigned"); return; }
        queue = new Queue<Beat>();
        pool = GetComponent<BeatPool>();
    }

    public bool IsEmpty()
    {
        return queue == null || queue.Count == 0;
    }

    public Beat Peek()
    {
        if (IsEmpty()) return null;
        return queue.Peek();
    }

    public float GetRawSpacing()
    {
        return beatSpacingFraction * (endPos - startPos).magnitude;
    }

    public void SetBeatVisibility(bool showBeat)
    {
        this.showBeat = showBeat;
        foreach (Beat beat in queue)
        {
            beat.GetComponent<Image>().enabled = showBeat;
        }
    }

    /// <summary>
    /// Returns the closest beat, including negative positions.
    /// Returns null if queue is empty.
    /// </summary>
    /// <param name="currentBeat">Position in song in beats</param>
    public Beat Nearest(double currentBeat)
    {
        if (IsEmpty()) return null;
        Beat nearest = queue.Peek();
        double closestDistance = nearest.positionInBeats - currentBeat;
        foreach (Beat beat in queue)
        {
            double distance = beat.positionInBeats - currentBeat;
            if (Math.Abs(distance) < Math.Abs(closestDistance))
            {
                nearest = beat;
                closestDistance = distance;
            }
        }
        return nearest;
    }

    /// <summary>
    /// Moves beat along its track from startPos to endPos.
    /// </summary>
    /// <param name="currentBeat">Current time in beats</param>
    public void UpdatePos(double currentBeat)
    {
        foreach (Beat beat in queue)
        {
            beat.rectTransform.anchoredPosition = 
                endPos + 
                (startPos - endPos) * 
                ((float)(beat.positionInBeats + CalibrationBeatManager.audioDelay - currentBeat) * beatSpacingFraction);
        }
    }

    /// <summary>
    /// Spawns and adds a beat object to the provided player's queues.
    /// </summary>
    public void Enqueue(double position)
    {
        Beat beat = pool.Get();
        beat.positionInBeats = position;
        beat.gameObject.GetComponent<Image>().enabled = showBeat;
        queue.Enqueue(beat);
    }

    public void Dequeue()
    {
        if (IsEmpty()) return;
        pool.Release(queue.Dequeue());
    }
}