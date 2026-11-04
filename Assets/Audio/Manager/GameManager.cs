using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{  
    [SerializeField] public PlayerInput playerInput;
    public void SwitchToCalibrationControls()
    {
        playerInput.SwitchCurrentActionMap("Calibration");
    }
    public void SwitchToGameplayControls()
    {
        playerInput.SwitchCurrentActionMap("TwoPlayer");
    }

    public void SwitchToUiControls()
    {
        playerInput.SwitchCurrentActionMap("UI");
    }
}
