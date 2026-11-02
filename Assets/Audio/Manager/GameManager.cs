using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject calibrationPrefab;
    [SerializeField] RectTransform calibrationParent;
    GameObject calibrationUI;


    public void OnToggleCalibration()
    {
        if (calibrationUI == null) {
            calibrationUI = Instantiate(calibrationPrefab, calibrationParent);
        } else
        {
            Destroy(calibrationUI);
        }
    }
}
