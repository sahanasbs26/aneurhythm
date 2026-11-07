using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Tooltip("If true, will control player 2, otherwise controls player 1")]
    //stupid but it works
    [SerializeField] bool isPlayer2 = false;

    public static float isometricToWorldScaleFactor = 1; 
    public const float defaultMoveTime = 0.1f;

    [SerializeField] Vector3 gridPosition = Vector3.zero;

    //Handles interpolation coroutine
    Coroutine moveCoroutine = null;
    Vector3 currentDestination;

    void Start()
    {
        transform.position = IsometricArrayToWorldCoordinates(gridPosition);
    }


    /// <summary>
    /// Translates a from a array position (poisition in (x, y, z) where z is height) to a world space position.
    /// * Uses a WASD = NE,NW,SW,SE control scheme
    /// </summary>
    public static Vector3 IsometricArrayToWorldCoordinates(Vector3 arrayCoordinates)
    {
        return new Vector3(arrayCoordinates.y, arrayCoordinates.z, -arrayCoordinates.x) * isometricToWorldScaleFactor;
    }

    /// <summary>
    /// Translates a from a world position to a isometric grid position.
    /// * Assumes a WASD = NE,NW,SW,SE control scheme
    /// </summary>
    public static Vector3 WorldToIsometricArrayCoordinates(Vector3 worldCoordinates)
    {
        return new Vector3(-worldCoordinates.z, worldCoordinates.x, worldCoordinates.y) * isometricToWorldScaleFactor;
    }


    void MovePlayer(Vector3 arrayCoordinates)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            transform.position = currentDestination;
        }
        moveCoroutine = StartCoroutine(LerpToPosition(IsometricArrayToWorldCoordinates(arrayCoordinates)));
    }

    IEnumerator LerpToPosition(Vector3 worldCoordinates, float moveDuration = defaultMoveTime)
    {
        float elapsedTime = 0;
        currentDestination = worldCoordinates;

        Vector3 initialPos = transform.position;
        Vector3 center = (worldCoordinates + initialPos) * 0.5f;
        center -= Vector3.up * (worldCoordinates - initialPos).magnitude / 2.0f;
        Vector3 initialRelCenter = initialPos - center;
        Vector3 finalRelCenter = worldCoordinates - center;
        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            transform.position = Vector3.Slerp(initialRelCenter, finalRelCenter, t);
            transform.position += center;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = worldCoordinates;
        moveCoroutine = null;
    }



    void OnMove(InputValue value)
    {
        Vector2 inputDirection = value.Get<Vector2>();
        gridPosition += (Vector3)inputDirection;
        MovePlayer(gridPosition);
    }

    void OnMovePlayer1(InputValue value)
    {
        if (isPlayer2) return;
        OnMove(value);
    }

    void OnMovePlayer2(InputValue value)
    {
        if (!isPlayer2) return;
        OnMove(value);
    }
}
