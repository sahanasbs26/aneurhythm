using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

//TODO: Add support for looped audio (Easy) or looping a segment of the audio after initial playback(Hard?)
/// <summary>
/// Keeps track of the beat/time in the current song.
/// Loop and beat numbers are zero indexed (The first beat/loop is beat/loop number 0).
/// Assumes song starts on the first beat (Trim audio so first beat is at t = 0).
/// </summary>
public class SongManager : MonoBehaviour
{
    [SerializeField] public AudioSource audio;
    
    [Tooltip("Number of beats to wait before first displayed beat reaches the center")]
    [SerializeField] int pickupBeats;

    [Tooltip("Beats per minute. A 'beat' should be the subdivision in the key signature. (E.g. in 3/8 a beat should be an eight note)")]
    [SerializeField] public float bpm;

    [Tooltip("Should be the numerator in the key signature")]
    [SerializeField] public int beatsPerMeasure;

    [Tooltip("Delay in seconds after Start() for audio to start playing. Should be at least 0.5s to pad software delays")]
    [SerializeField] public double startDelay = 1f; //in seconds

    [Tooltip("Delay before the first beat of the song plays. Use to account for pickups/intros. Not made to support looped audio.")]
    [SerializeField] public double firstBeatOffset;

    double timePerBeat;

    //Actual dsp time when the song starts
    double _startTime;
    public double startTime => _startTime;
    double _timeInSong;
    public double timeInSong => _timeInSong;
    double _beatInSong; //Zero indexed: First beat is beat 0
    public double beatInSong => _beatInSong;
    
    public double beatsPerLoop;
    double _beatInLoop; //Zero indexed
    public double beatInLoop => _beatInLoop;
    double _loopNumber; //Zero indexed
    public double loopNumber => _loopNumber;
    
    void Awake()
    {
        if (audio == null)
        {
            Debug.LogWarning("Audio source not assigned in Inspector. Searching for one using GetComponent");
            audio = GetComponent<AudioSource>();
        }
    }

    //Song is expected to start on the first beat (pickups are fine).
    void Start()
    {
        timePerBeat = 60f / bpm;

        _startTime = startDelay + AudioSettings.dspTime;
        audio.PlayScheduled(_startTime);
    }

    //Calculate position in time/beat, which includes pickups.
    //Position in measures should be calculated on demand.
    void Update()
    {
        _timeInSong = AudioSettings.dspTime - _startTime - (pickupBeats * timePerBeat);
        _beatInSong = _timeInSong / timePerBeat;
        if (audio.loop)
        {
            _loopNumber = _beatInSong / beatsPerLoop;
            _beatInLoop = _beatInSong - (Math.Floor(_loopNumber) * beatsPerLoop);
        }
    }

    /// <summary>
    /// Calculate song position in measures.
    /// Pickup notes are treated as the first measure = measure 0
    /// </summary>
    /// <returns>0 indexed fractional position in measures</returns>
    public double MeasureInSong()
    {
        if (_beatInSong <= pickupBeats - 1)
        {
            //Start partway through measure on a pickup
            return 1f - (pickupBeats - _beatInSong)/beatsPerMeasure;
        }
        else
        {
            //Account for added measure from pickup
            return (_beatInSong - pickupBeats) / beatsPerMeasure + (pickupBeats > 0 ? 1 : 0);
        }
    }
}
