using System.Reflection.Emit;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls visual beat icons
/// Should be attached to same gameObject as SongManager
/// ?Handling of interactions & hence carrying of beatData might be better suited to be handeled by SongManager
/// </summary>
public class BeatManager : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("The visual spacing between one beat as a fraction of the distance between its starting and ending points")]
    [SerializeField] public float beatSpacingFraction = 0.2f;
    [SerializeField] protected Lane player1;
    [SerializeField] protected Lane player2;
    protected Lane[] lanes;


    [Header("Beat Pattern Properties")]
    [Tooltip("In seconds; period after beat hits before despawning beat")]
    public double beatDespawnWindow = 0.2;
    [Tooltip("In seconds; window to hit the beat")]
    public double beatHitWindow = 0.15;
    [Tooltip("Positions of the spawned beats in terms of the beat in the song. Will loop at the end. Assumes beats are listed in chronological order.")]
    [SerializeField] BeatData[] beatPattern;
    [Tooltip("Length of the pattern in beats. If left unspecified, will assume the last beat is the length of the pattern")]
    [SerializeField] float patternLengthBeats;
    protected SongManager songManager;
    // Position of the next beat to spawn
    double nextBeat;
    // Index in the beat pattern of the next beat to spawn
    int patternIndex;
    // How many times the beat patter nhas looped
    int patternLoop;

    void Awake()
    {
        if (beatPattern == null || beatPattern.Length == 0) 
        { 
            Debug.LogError("Error: beatPattern is empty or unspecified");
            enabled = false; 
            return;
        }
        if (patternLengthBeats < 0.05)
        {
            Debug.LogError("Error: patternLengthBeats is too small (<0.05)");
            enabled = false;
            return;
        }
        if (beatPattern[beatPattern.Length - 1].positionInBeats > patternLengthBeats)
        {
            Debug.LogWarning("Warning: provided patternLengthBeats is not equal to the final beat of beatPattern");
            return;
        }
        songManager = GetComponent<SongManager>();
        lanes = new Lane[] {player1, player2};
        foreach (Lane lane in lanes) lane.beatSpacingFraction = beatSpacingFraction;
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
        double lookahead = 1 / beatSpacingFraction + beatSpacingFraction;

        //Spawn the next beat in the pattern if it should be close to the edge of the screen
        while (nextBeat - now <= lookahead)
        {
            Lane player = beatPattern[patternIndex].isPlayer2 ? player2 : player1;
            player.Enqueue(nextBeat);
            if (++patternIndex == beatPattern.Length)
            {
                patternLoop++;
                patternIndex = 0;
            }
            nextBeat = patternLoop * patternLengthBeats + beatPattern[patternIndex].positionInBeats;
        }

        foreach (Lane lane in lanes)
        {    
            //Update positions of spawned beats
            lane.UpdatePos(now);

            //TODO: Have judgment class destroy missed beats
            //  After a waiting for the player to attempt to hit the beat, destroy
            //  For now, handle here
            while (!lane.IsEmpty() && BeatError(lane.Peek().positionInBeats) > beatDespawnWindow)
            {
                lane.Dequeue();
                Debug.Log("Missed");
            }
        }

        OnUpdate();
    }
    
    /// <summary>
    /// To be overridden by a child class, executed at the end of Update() in BeatManager()
    /// </summary>
    protected virtual void OnUpdate() {}

    protected void OnMovePlayer1(InputValue value)
    {
        float error;
        if (HitBeat(player1, out error))
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
        float error;
        if (HitBeat(player2, out error))
        {
            Debug.Log("Hit: P2");
        } else
        {
            Debug.Log("Misstep: P2");
        }
        Debug.Log("Error (ms): " + error * 1000);
    }
    
    /// <summary>
    /// Checks if the next beat in the queue has played within missedBeatWindow. 
    /// If it has played, despawn beat and return true. Otherwise return false.
    /// Does not check if queue is null, but checks if empty.
    /// </summary>
    /// <param name="lane"> Lane of beats to check </param>
    /// <returns> Whether or not hit was successful </returns>
    bool HitBeat(Lane lane, out float error)
    {
        if (lane.IsEmpty()) {error = 0; return false;};
        error = NearestBeatError(lane);
        //positive = late, negative = early
        if (Mathf.Abs(error) <= beatHitWindow)
        {
            lane.Dequeue();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns the error of the nearest beat in the lane in seconds.
    /// Caller should check if the lane is null.
    /// </summary>
    /// <param name="lane">Searches beats in this lane</param>
    /// <returns>Error in seconds</returns>
    public float NearestBeatError(Lane lane)
    {
        return BeatError(lane.Nearest(songManager.beatInSong).positionInBeats);
    }

    /// <summary>
    /// Returns the error of the specified position in seconds.
    /// Accounts for audio delay
    /// </summary>
    /// <param name="beatPosition">Position to calculate error of</param>
    /// <returns>Error in seconds</returns>
    public float BeatError(double beatPosition)
    {
        return (float)((songManager.beatInSong - beatPosition) * songManager.timePerBeat - CalibrationBeatManager.audioDelay);
    }
}