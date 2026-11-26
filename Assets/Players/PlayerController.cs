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
    [SerializeField] Vector3 baseOffset = new(0.5f, 0, 0.5f);
    Vector3 totalOffset;
    [SerializeReference] GameplayTile startingTile;
    TileSurface currentSurface;
    [SerializeField] PlayerManager playerManager;

    //Handles interpolation coroutine
    Coroutine moveCoroutine = null;
    TileSurface currentDestination;

    Quaternion moveDirectionRotation = Quaternion.identity;

    void Awake()
    {
        if (!IsValid()) return;
    }

    void Update()
    {
        basePosition = currentSurface.position;
        totalOffset = baseOffset + currentSurface.mount.offset;
        Vector3 visualPosition = currentSurface.transform.position + totalOffset + new Vector3(0.5f, 0.5f, 0.5f);
        transform.position = visualPosition;
    }

    public void Initialize()
    {
        currentSurface = startingTile.surfaces[0];
        basePosition = currentSurface.position;
        totalOffset = baseOffset + currentSurface.mount.offset;
        transform.position = basePosition + totalOffset;
        transform.rotation = currentSurface.mount.rotation;
    }

    /// <summary>
    /// Moves player to specified position in array coordinates
    /// </summary>
    /// <param name="arrayCoordinates"></param>
    void MovePlayer(TileSurface surface)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            transform.position = WorldSurfacePosition(currentDestination);
            transform.rotation = WorldSurfaceRotation(currentDestination);
        }
        moveCoroutine = StartCoroutine(LerpToSurface(surface));
    }

    /// <summary>
    /// Interpolates movement to a new position and rotation. Call as a coroutine.
    /// </summary>
    IEnumerator LerpToSurface(TileSurface surface, float moveDuration = defaultMoveTime)
    {
        float elapsedTime = 0;
        currentDestination = surface;

        Vector3 initialPos = transform.position;
        Vector3 worldCoordinates = WorldSurfacePosition(surface);

        //Save initial rotation for linear interpolation
        Quaternion initialRotation = transform.rotation;
        Quaternion rotation = WorldSurfaceRotation(surface);

        //Set initial position for a spherical interpolation (interpolates between two vectors treated as rays along a sphere; must therefore adjust for center pos)
        Vector3 center = (worldCoordinates + initialPos) * 0.5f;
        center -= Vector3.up * (worldCoordinates - initialPos).magnitude / 2.0f;
        Vector3 initialRelCenter = initialPos - center;
        Vector3 finalRelCenter = worldCoordinates - center;

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
        Vector3Int direction = Vector3Int.FloorToInt(rawInput);
        if (ArrayManager.FindMoveableTarget(currentSurface, direction, out TileSurface targetSurface))
        {
            currentSurface = targetSurface;
            basePosition = targetSurface.position;
            totalOffset = baseOffset + targetSurface.mount.offset;
            //Debug.Log($"Moving to array {gridPosition} world{WorldUtils.ArrayToWorldCoordinates(gridPosition)} with offset {currentSurface.mount.offset} and rotation {currentSurface.mount.rotation}");
            
            moveDirectionRotation = Quaternion.LookRotation(WorldUtils.ArrayToWorldCoordinates((Vector3)rawInput));
            MovePlayer(targetSurface); 
        }
    }

    //Offset of player on its tile
    Vector3 LocalOffset(TileSurface s) => s.mount.offset + baseOffset + new Vector3(0.5f, 0.5f, 0.5f);
    Quaternion LocalRotation(TileSurface s) => s.mount.rotation * moveDirectionRotation; 
    Vector3 WorldSurfacePosition(TileSurface s) => s.transform.position + s.transform.rotation * LocalOffset(s);
    Quaternion WorldSurfaceRotation(TileSurface s) => s.transform.rotation * LocalRotation(s);

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
        if (startingTile == null)
        {
            Debug.LogError($"Error in {(string)this}: startingTile is not assigned");
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
        if (startingTile != null && basePosition != startingTile.position)
        {
            basePosition = startingTile.position;
            totalOffset = baseOffset + startingTile.surfaces[0].mount.offset;
            transform.position = basePosition + totalOffset;
            transform.rotation = startingTile.surfaces[0].mount.rotation;
        }
    }

    public static implicit operator string(PlayerController player)
    {
        return $"PlayerController(Name=\"{player.name}\")";
    }
}
