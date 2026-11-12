using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{  
    [SerializeField] public PlayerInput playerInput;
    [SerializeField] PlayerController player1;
    [SerializeField] PlayerController player2;
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

    void OnMovePlayer1(InputValue value)
    {
        player1.OnMove(value);
    }

    void OnMovePlayer2(InputValue value)
    {
        player2.OnMove(value);
    }
}
