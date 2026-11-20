using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls movement of an individual player
/// Needs an attached PlayerManager object
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Tooltip("If true, will control player 2, otherwise controls player 1")]
    //stupid but it works
    [SerializeField] bool isPlayer2 = false;
    public const float defaultMoveTime = 0.1f;

    [SerializeField] Vector3 basePosition = Vector3Int.zero;
    [SerializeField] Vector3 baseOffset = new(0.5f, 1, 0.5f);
    Vector3 totalOffset;
    [SerializeReference] TileSurface startingSurface;
    TileSurface currentSurface;
    [SerializeField] PlayerManager playerManager;

    //Handles interpolation coroutine
    Coroutine moveCoroutine = null;
    Vector3 currentDestination;
    Quaternion currentRotationDestination;

    void Awake()
    {
        if (!IsValid()) return;
    }

    void Start()
    {
        OnStart();
    }

    public void OnStart()
    {
        currentSurface = startingSurface;
        MoveToStartingSurface();
    }

    /// <summary>
    /// Moves player to specified position in array coordinates
    /// </summary>
    /// <param name="arrayCoordinates"></param>
    void MovePlayer(Vector3 worldPosition, Quaternion rotation)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            transform.position = currentDestination;
            transform.rotation = currentRotationDestination;
        }
        moveCoroutine = StartCoroutine(LerpToPosition(worldPosition, rotation));
    }

    /// <summary>
    /// Interpolates movement to a new position and rotation. Call as a coroutine.
    /// </summary>
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

    /// <summary>
    /// Translates an InputValue to a player movement
    /// </summary>
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
        if (ArrayManager.FindMoveableTarget(currentSurface, direction, out TileSurface targetSurface))
        {
            currentSurface = targetSurface;
            basePosition = targetSurface.transform.position;
            totalOffset = baseOffset + targetSurface.mount.offset;
            //Debug.Log($"Moving to array {gridPosition} world{WorldUtils.ArrayToWorldCoordinates(gridPosition)} with offset {currentSurface.mount.offset} and rotation {currentSurface.mount.rotation}");
            
            Quaternion moveDirectionRotation = Quaternion.LookRotation(WorldUtils.ArrayToWorldCoordinates((Vector3)rawInput));
            MovePlayer(
                basePosition + totalOffset, 
                currentSurface.mount.rotation * moveDirectionRotation
                ); 
        }
    }

    /// <summary>
    /// Checks whether serialized fields are set properly
    /// </summary>
    bool IsValid()
    {
        bool isError = false;
        if (playerManager == null)
        {
            Debug.LogError($"Error in {(string)this}: playerManager is not assigned");
            isError = true;
        }
        if (startingSurface == null)
        {
            Debug.LogError($"Error in {(string)this}: startingSurface is not assigned");
            isError = true;
        }
        return isError;
    }

    void OnValidate()
    {
        MoveToStartingSurface();
    }

    void MoveToStartingSurface()
    {
        if (startingSurface != null && basePosition != startingSurface.transform.position)
        {
            basePosition = startingSurface.transform.position;
            totalOffset = baseOffset + startingSurface.mount.offset;
            transform.position = basePosition + totalOffset;
            transform.rotation = startingSurface.mount.rotation;
        }
    }

    public static implicit operator string(PlayerController player)
    {
        return $"PlayerController(Name=\"{player.name}\")";
    }
}
