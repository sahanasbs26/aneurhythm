using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] Vector3 rotation = new Vector3(35, 45, 0);
    [SerializeField] Vector3 focusPosition = Vector3.zero;
    [SerializeField] float cameraDistance = 10.0f; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Quaternion rotationTransform = Quaternion.Euler(rotation);
        transform.localRotation = Quaternion.Euler(rotation);
        transform.localPosition = focusPosition - rotationTransform * Vector3.forward * cameraDistance;
    }
}
