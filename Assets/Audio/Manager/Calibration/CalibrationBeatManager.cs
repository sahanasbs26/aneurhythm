
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls visual beat icons
/// Should be attached to same gameObject as SongManager
/// ?Handling of interactions & hence carrying of beatData might be better suited to be handeled by SongManager
/// </summary>
public class CalibrationBeatManager : BeatManager
{
    //Holds UI element (Input Field TMPro) for manual delay input
    [SerializeField] TMP_InputField delayField; 

    //Indicator that moves as a function of delay to represent the position where the beat should hit
    [SerializeField] RectTransform centerIndicator;

    //How long beat highlight lasts after hitting
    [SerializeField] float indicatorBeatLifetime = 1f;
    //Prefab of the spawned highlights after hitting
    [SerializeField] Beat indicatorBeatPrefab;

    [SerializeField] public static float audioDelay = 0;

    //How much to change the delay when pressing left or right in seconds
    [SerializeField] float delayChangePerMovement = 0.5f / 1000f;
    //How much to change the delay when clicking the +/- button in seconds
    [SerializeField] float delayChangePerButton = 2.0f / 1000f;
    float delayChange = 0;
    bool hideBeat = false;

    void FixedUpdate()
    {
        if (!Mathf.Approximately(delayChange, 0))
        {
            audioDelay += delayChange;
        }
    }

    protected override void OnUpdate()
    {
        SetBaselinePos();
    }

    /// <summary>
    /// Sets position of the center basline indicator bar
    /// </summary>
    void SetBaselinePos()
    {
        Vector2 currentPos = centerIndicator.anchoredPosition;
        double x = audioDelay / songManager.timePerBeat * player1.GetRawSpacing();
        centerIndicator.anchoredPosition = new Vector2((float)x, currentPos.y);
        delayField.text = audioDelay.ToString("F3");
    }

    void OnHit(InputValue value)
    {
        if(player1.IsEmpty()) return;
        Debug.Log("Error (ms): " + NearestBeatError(player1) * 1000);

        Beat calibrationBeat = Instantiate(indicatorBeatPrefab, player1.parent);
        calibrationBeat.rectTransform.anchoredPosition = 
            player1.Nearest(songManager.beatInSong).rectTransform.anchoredPosition;
        Destroy(calibrationBeat.gameObject, indicatorBeatLifetime);
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
        player1.SetBeatVisibility(!hideBeat);
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
}
