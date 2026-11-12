using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Tooltip("If true, will control player 2, otherwise controls player 1")]
    //stupid but it works
    [SerializeField] bool isPlayer2 = false;
    public const float defaultMoveTime = 0.1f;

    [SerializeField] Vector3Int gridPosition = Vector3Int.zero;
    [SerializeField] Vector3 worldOffset;
    [SerializeField] ArrayManager arrayManager;

    //Handles interpolation coroutine
    Coroutine moveCoroutine = null;
    Vector3 currentDestination;

    void Start()
    {
        transform.position = WorldUtils.ArrayToWorldCoordinates(gridPosition) + worldOffset;
    }

    void MovePlayer(Vector3 arrayCoordinates)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            transform.position = currentDestination;
        }
        moveCoroutine = StartCoroutine(LerpToPosition(WorldUtils.ArrayToWorldCoordinates(arrayCoordinates) + worldOffset));
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

    public void OnMove(InputValue value)
    {
        Vector2 rawInput = value.Get<Vector2>();
        rawInput.x *= -1; //Invert so "D" or "Right" is positive
        Vector3Int direction = DirectionMaskUtils.Vector3Floor(rawInput);
        Debug.Log(direction);
        Vector3Int target = arrayManager.FindMoveableTarget(gridPosition, direction, DirectionMask.U);
        if (gridPosition != target)
        {
            gridPosition = target;
            Debug.Log("Moving to array" + gridPosition + " world" + WorldUtils.ArrayToWorldCoordinates(gridPosition));
            MovePlayer(gridPosition);
        }
    }
}
