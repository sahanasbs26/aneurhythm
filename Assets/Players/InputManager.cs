using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Switches between input maps and delegates raw input calls to their respective functions & classes.
/// </summary>
public class InputManager : MonoBehaviour
{  
    [SerializeField] public PlayerInput playerInput;
    [SerializeField] PlayerManager playerManager;
    [SerializeField] BeatManager beatManager;
    public CalibrationBeatManager calibrationManager;

    public const string calibrationInputMapName = "Calibration";
    public const string twoPlayerInputMapName = "TwoPlayer";
    public const string uiInputMapName = "UI";

    public void Start()
    {
        playerInput.actions.FindActionMap(calibrationInputMapName).Disable();
        playerInput.actions.FindActionMap(twoPlayerInputMapName).Disable();
        playerInput.actions.FindActionMap(uiInputMapName).Disable();
        
        playerInput.SwitchCurrentActionMap(twoPlayerInputMapName);

        
    }

    public void SwitchToCalibrationControls()
    {
        playerInput.SwitchCurrentActionMap(calibrationInputMapName);
        Debug.Log("Switched to map: " + playerInput.currentActionMap.name);
    }
    public void SwitchToGameplayControls()
    {
        playerInput.SwitchCurrentActionMap(twoPlayerInputMapName);
    }

    public void SwitchToUiControls()
    {
        playerInput.SwitchCurrentActionMap(uiInputMapName);
    }

    void OnMovePlayer1(InputValue value)
    {
        if (playerInput.currentActionMap.name == twoPlayerInputMapName)
            playerManager.OnMove(value, PlayerManager.PlayerID.P1);
    }

    void OnMovePlayer2(InputValue value)
    {
        if (playerInput.currentActionMap.name == twoPlayerInputMapName)
            playerManager.OnMove(value, PlayerManager.PlayerID.P2);
    }

    void OnHit(InputValue value)
    {
        calibrationManager.OnHit(value);
    }

    void OnMoveDelay(InputValue value)
    {
        calibrationManager.OnMoveDelay(value);
    }

    void OnToggleHideBeat(InputValue value)
    {
        calibrationManager.OnToggleHideBeat(value);
    }
}
