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
    Quaternion currentRotationDestination;

    void Start()
    {
        transform.position = WorldUtils.ArrayToWorldCoordinates(gridPosition) + worldOffset;
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
            transform.rotation = currentRotationDestination;
        }
        moveCoroutine = StartCoroutine(LerpToPosition(WorldUtils.ArrayToWorldCoordinates(arrayCoordinates) + worldSpaceOffset, rotation));
    }

    IEnumerator LerpToPosition(Vector3 worldCoordinates, Quaternion rotation, float moveDuration = defaultMoveTime)
    {
        float elapsedTime = 0;
        currentDestination = worldCoordinates;
        currentRotationDestination = rotation;

        //Set initial position for a spherical interpolation (interpolates between two vectors treated as rays along a sphere; must therefore adjust for center pos)
        Vector3 initialPos = transform.position;
        Vector3 center = (worldCoordinates + initialPos) * 0.5f;
        center -= Vector3.up * (worldCoordinates - initialPos).magnitude / 2.0f;
        Vector3 initialRelCenter = initialPos - center;
        Vector3 finalRelCenter = worldCoordinates - center;

        //Save initial rotation for linear interpolation
        Quaternion initialRotation = transform.rotation;
        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;

            transform.position = center + Vector3.Slerp(initialRelCenter, finalRelCenter, t);

            transform.rotation = Quaternion.Lerp(initialRotation, rotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = worldCoordinates;
        transform.rotation = rotation;
        moveCoroutine = null;
    }

    public void OnMove(InputValue value)
    {
        if (currentSurface == null)
        {
            Debug.LogError("OnMove " + this + ": currentSurface is null");
            return;
        }
        //Debug.Log("Position: " + currentSurface.parent.arrayPosition);
        //Debug.Log("Ports: " + currentSurface.ports);
        //return;
        Vector2 rawInput = value.Get<Vector2>();
        rawInput.x *= -1; //Invert so "D" or "Right" is positive
        Vector3Int direction = DirectionMaskUtils.Vector3Floor(rawInput);
        if (arrayManager.FindMoveableTarget(currentSurface, direction, out TileSurface targetSurface))
        {
            gridPosition = targetSurface.parent.arrayPosition;
            currentSurface = targetSurface;
            //Debug.Log($"Moving to array {gridPosition} world{WorldUtils.ArrayToWorldCoordinates(gridPosition)} with offset {currentSurface.mount.offset} and rotation {currentSurface.mount.rotation}");
            
            Quaternion moveDirectionRotation = Quaternion.LookRotation(WorldUtils.ArrayToWorldCoordinates((Vector3)rawInput));
            MovePlayer(gridPosition, currentSurface.mount.offset + worldOffset, currentSurface.mount.rotation * moveDirectionRotation); 
        }
    }

    public static implicit operator string(PlayerController player)
    {
        return $"PlayerController(Name=\"{player.name}\")";
    }
}
