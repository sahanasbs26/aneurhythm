using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [Tooltip("If true, will control player 2, otherwise controls player 1")]
    //stupid but it works
    [SerializeField] bool isPlayer2 = false;
    public const float defaultMoveTime = 0.1f;

    [SerializeField] Vector3Int gridPosition = Vector3Int.zero;
    [SerializeField] Vector3 worldOffset;
    [SerializeField] ArrayManager arrayManager;
    [SerializeReference] TileSurface currentSurface;

    //Handles interpolation coroutine
    Coroutine moveCoroutine = null;
    Vector3 currentDestination;

    void Start()
    {
        transform.position = WorldUtils.ArrayToWorldCoordinates(gridPosition) + worldOffset;
    }

    /// <summary>
    /// Moves player to specified position in array coordinates
    /// </summary>
    /// <param name="arrayCoordinates"></param>
    void MovePlayer(Vector3 arrayCoordinates)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            transform.position = currentDestination;
        }
        moveCoroutine = StartCoroutine(LerpToPosition(WorldUtils.ArrayToWorldCoordinates(arrayCoordinates)));
    }

    /// <summary>
    /// Moves player to specified position in array coordinates
    /// </summary>
    /// <param name="arrayCoordinates"></param>
    void MovePlayer(Vector3 arrayCoordinates, Vector3 worldSpaceOffset, Quaternion rotation)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            transform.position = currentDestination;
        }
        transform.rotation = rotation;
        moveCoroutine = StartCoroutine(LerpToPosition(WorldUtils.ArrayToWorldCoordinates(arrayCoordinates) + worldSpaceOffset));
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
        if (currentSurface == null)
        {
            Debug.LogError("OnMove " + this + ": currentSurface is null");
            return;
        }
        Debug.Log("Position: " + currentSurface.parent.arrayPosition);
        Debug.Log("Ports: " + currentSurface.ports);
        //return;
        Vector2 rawInput = value.Get<Vector2>();
        rawInput.x *= -1; //Invert so "D" or "Right" is positive
        Vector3Int direction = DirectionMaskUtils.Vector3Floor(rawInput);
        Debug.Log(direction);
        if (arrayManager.FindMoveableTarget(currentSurface, direction, out TileSurface targetSurface))
        {
            gridPosition = targetSurface.parent.arrayPosition;
            currentSurface = targetSurface;
            Debug.Log($"Moving to array {gridPosition} world{WorldUtils.ArrayToWorldCoordinates(gridPosition)} with offset {currentSurface.mount.offset} and rotation {currentSurface.mount.rotation}");
            MovePlayer(gridPosition, currentSurface.mount.offset + worldOffset, currentSurface.mount.rotation); 
        }
    }

    public static implicit operator string(PlayerController player)
    {
        return $"PlayerController(Name=\"{player.name}\")";
    }
}
