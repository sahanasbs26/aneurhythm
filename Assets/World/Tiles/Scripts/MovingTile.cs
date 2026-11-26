using System.Collections;
using UnityEngine;

public class MovingTile : TileGroup
{
    [SerializeField] BeatManager beatManager;
    [SerializeField] SongManager songManager;

    [SerializeField] int patternLengthBeats;
    [SerializeField] float lookaheadBeats;

    [Tooltip("Time before pose change finishes where player can hop to the new connections")]
    [SerializeField] float stitchAnticipationTime;
    int nextBeat;
    int patternIndex;
    int patternLoop;
    Pose[] beatPattern => _poses;

    // Update is called once per frame
    void Update()
    {
        double delayBeats = CalibrationBeatManager.audioDelay / songManager.timePerBeat;
        double now = songManager.beatInSong;
        float timeUntilNextBeat = (float)(nextBeat - now + delayBeats);

        //Spawn the next beat in the pattern if it should be close to the edge of the screen
        if (timeUntilNextBeat <= lookaheadBeats)
        {
            ChangePose(patternIndex, timeUntilNextBeat * (float)songManager.timePerBeat);
            if (++patternIndex == beatPattern.Length)
            {
                patternLoop++;
                patternIndex = 0;
            }
            nextBeat = patternLoop * patternLengthBeats + beatPattern[patternIndex].beat;
        }
    }

    Coroutine lerpCoroutine = null;
    Vector3 currentDestination;
    Quaternion currentDestinationRotation;
    IEnumerator LerpToPose(int newIndex, float duration)
    {
        Pose pose = _poses[newIndex];
        currentDestination = pose.position;
        currentDestinationRotation = pose.rotation;

        Vector3 initialPos = transform.position;
        Quaternion initialRotation = transform.rotation;

        float elapsedTime = 0;
        bool hasStitched = false;

        while (elapsedTime < duration)
        {
            if ((duration - elapsedTime) < stitchAnticipationTime && !hasStitched)
            {
                //Debug.Log($"Changed stich at {duration - elapsedTime}");
                ChangeStitchPose(newIndex);
                hasStitched = true;
            }
            float t = elapsedTime / duration;
            transform.position = Vector3.Lerp(initialPos, pose.position, t);
            transform.rotation = Quaternion.Lerp(initialRotation, pose.rotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        SetVisualPosition();
        lerpCoroutine = null;
        yield return null;
    }

    protected void ChangePose(int newIndex, float duration)
    {
        if (lerpCoroutine != null)
        {
            StopCoroutine(lerpCoroutine);
            transform.position = currentDestination;
            transform.rotation = currentDestinationRotation;
        } 
        lerpCoroutine = StartCoroutine(LerpToPose(newIndex, duration));
    }
}
