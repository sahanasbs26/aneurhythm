using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] Vector3 rotation = new Vector3(35, 45, 0);
    [SerializeField] Vector3 focusPosition = Vector3.zero;
    [SerializeField] float cameraDistance = 10.0f;

    void OnValidate()
    {
        SetPosition();
    }

    [ContextMenu("Set Position")]
    void SetPosition()
    {
        Quaternion rotationTransform = Quaternion.Euler(rotation);
        transform.localRotation = Quaternion.Euler(rotation);
        transform.localPosition = focusPosition - rotationTransform * Vector3.forward * cameraDistance;
    }
}
