using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles lose & win conditions, and delegates calls to players between the two PlayerControllerObjects.
/// </summary>
/// TODO: Add win state check, add fail state event
/// TODO: Add pause/resume/restart/exit functionality as needed
public class PlayerManager : MonoBehaviour
{
    [SerializeField] BeatManager beatManager;
    [SerializeField] public ArrayManager arrayManager;
    [SerializeField] PlayerController player1;
    [SerializeField] PlayerController player2;
    [SerializeField] int startingLives = 3;
    int lives;

    public enum PlayerID {P1, P2};

    void Awake()
    {
        if (!isValid()) return;
    }

    void Start()
    {
        lives = startingLives;
    }

    //TODO: Add win state check
    public void OnMove(InputValue value, PlayerID player)
    {
        float error;
        if (beatManager.HitBeat(player, out error))
        {
            (player == PlayerID.P1 ? player1 : player2).OnMove(value);
        } else
        {
            Debug.Log($"{player.HumanName()} Misstep: {error*1000}ms");
            lives--;
        }
    }

    public void OnRestart()
    {
        //TODO: Move players to starting tile
        lives = startingLives;
        player1.OnStart();
        player2.OnStart();
    }

    bool isValid()
    {
        bool isError = false;
        if (arrayManager == null)
        {
            Debug.LogError($"Error in {(string)this}: arrayManager is not assigned");
            isError = true;
        }
        if (beatManager == null)
        {
            Debug.LogError($"Error in {(string)this}: beatManager is not assigned");
            isError = true;
        }
        if (player1 == null)
        {
            Debug.LogError($"Error in {(string)this}: player1 is not assigned");
            isError = true;
        }
        if (player2 == null)
        {
            Debug.LogError($"Error in {(string)this}: player2 is not assigned");
            isError = true;
        }
        return isError;
    }

    public static implicit operator string(PlayerManager playerManager)
    {
        return $"PlayerManager({playerManager.gameObject.name})";
    }
}
