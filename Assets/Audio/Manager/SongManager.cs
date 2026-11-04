using System;
using System.Collections;
using UnityEngine;

//TODO: Add support for looped audio (Easy) or looping a segment of the audio after initial playback(Hard?)
/// <summary>
/// Keeps track of the beat/time in the current song.
/// Loop and beat numbers are zero indexed (The first beat/loop is beat/loop number 0).
/// Assumes song starts on the first beat (Trim audio so first beat is at t = 0).
/// </summary>
public class SongManager : MonoBehaviour
{
    [SerializeField] public AudioSource audioSource;
    
    [Tooltip("Number of beats to wait before first displayed beat reaches the center")]
    [SerializeField] int pickupBeats;

    [Tooltip("Beats per minute. A 'beat' should be the subdivision in the key signature. (E.g. in 3/8 a beat should be an eight note)")]
    [SerializeField] public double bpm;

    [Tooltip("Should be the numerator in the key signature")]
    [SerializeField] public int beatsPerMeasure;

    [Tooltip("Delay in seconds after Start() for audio to start playing. Should be at least 0.5s to pad software delays")]
    [SerializeField] public double startDelay = 1.0; //in seconds
    [SerializeField] public double resumeDelay = 1.0;

    double _timePerBeat;
    public double timePerBeat => _timePerBeat;

    double _startTime;
    /// <summary>
    /// Actual dsp time when the song starts
    /// </summary>
    public double startTime => _startTime;

    double pauseTime = double.NegativeInfinity; //Use as floor for the timeInSong, negative so beats don't populate on start
    double _pauseDspTime;
    float _pauseSongTime;

    double _pauseDuration = 0;
    bool paused = false;

    /// <summary>
    /// Time in seconds since song started (includes time in previous loops)
    /// </summary>
    public double timeInSong => paused ? pauseTime : Math.Max(AudioSettings.dspTime - _pauseDuration - _startTime, pauseTime);

    public double timeInLoop => timeInSong - Math.Floor(loopNumber) * beatsPerLoop * timePerBeat;

  
    /// <summary>
    /// Beats since song started (including previous loops). Zero indexed: First beat is beat 0
    /// </summary>
    public double beatInSong => timeInSong / _timePerBeat - pickupBeats;
    
    public double beatsPerLoop;

    /// <summary>
    /// Beats since the start of the current loop. Zero indexed.
    /// </summary>
    public double beatInLoop => beatInSong - (Math.Floor(loopNumber) * beatsPerLoop);

    /// <summary>
    /// Number of times the audio clip has looped. Zero indexed: Initial play is loop 0, first repeat is loop 1.
    /// </summary>
    public double loopNumber => beatInSong / beatsPerLoop;

    /// <summary>
    /// Calculate song position in measures.
    /// </summary>
    public double measureInSong => beatInSong / beatsPerMeasure;

    
    void Awake()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("Audio source not assigned in Inspector. Searching for one using GetComponent");
            audioSource = GetComponent<AudioSource>();
        }
    }

    //Starts audio on start after scheduled time
    void Start()
    {
        _timePerBeat = 60.0 / bpm;

        _startTime = startDelay + AudioSettings.dspTime;
        audioSource.PlayScheduled(_startTime);
    }

    public void OnPause()
    {
        if (paused) return;
        pauseTime = timeInSong;
        _pauseDspTime = AudioSettings.dspTime;
        _pauseSongTime = audioSource.time;
        audioSource.Stop();
        paused = true;
    }

    public void OnResume()
    {
        if (!paused) return;
        double scheduledResumeTime = AudioSettings.dspTime + resumeDelay;
        _pauseDuration += scheduledResumeTime - _pauseDspTime;
        audioSource.time = (float)Math.Max(pauseTime, 0) % audioSource.clip.length;
        audioSource.PlayScheduled(scheduledResumeTime + (pauseTime < 0 ? -pauseTime : 0)); //Add back time if paused before song started
        paused = false;
    }


    //Set variables continously for debugging purposes.
    double _timeInSong;
    double _beatInSong;   
    double _beatInLoop;
    double _loopNumber;
    double _measureInSong;
    /*
    void Update()
    {
        _beatInSong = beatInSong;
        _timeInSong = timeInSong;
        _measureInSong = measureInSong;
        if (audio.loop)
        {
            _loopNumber = loopNumber;
            _beatInLoop = beatInLoop;
        }
    }
    */
}
