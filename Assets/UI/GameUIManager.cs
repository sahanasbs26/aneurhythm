using System.Collections;
using UnityEngine;


/// <summary>
/// Should be attached to the main canvas of a gameplay scene
/// </summary>
public class GameUIManager : MonoBehaviour
{
    [SerializeField] RectTransform pauseMenu;
    [SerializeField] GameObject calibrationPrefab;
    [SerializeField] RectTransform calibrationParent;
    [SerializeField] SongManager songManager;
    BeatManager beatManager;
    InputManager gameManager;

    GameObject calibrationUI;

    void Awake()
    {
        gameManager = GetComponent<InputManager>();
        pauseMenu.gameObject.SetActive(false);
        beatManager = songManager.gameObject.GetComponent<BeatManager>();
    }


    /// <summary>
    /// Instantiates the calibration menu prefab as a child of the provided parent
    /// </summary>
    public void ToggleCalibration()
    {
        if (calibrationUI == null) {
            calibrationUI = Instantiate(calibrationPrefab, calibrationParent);
            gameManager.SwitchToCalibrationControls();
            pauseMenu.gameObject.SetActive(false);
        } else
        {
            gameManager.SwitchToUiControls(); //! Assumes player exits into another menu
            Destroy(calibrationUI);
        }
    }

    public void TogglePauseMenu()
    {
        if (!pauseMenu.gameObject.activeInHierarchy)
        {
            OnPause();
        } else
        {
            OnResume();
        }  
    }

    void OnPause()
    {
        pauseMenu.gameObject.SetActive(true);
        songManager.OnPause();
        gameManager.SwitchToUiControls();
        if (calibrationUI != null) ToggleCalibration();
    }

    void OnResume()
    {
        pauseMenu.gameObject.SetActive(false);
        songManager.OnResume();
        gameManager.SwitchToGameplayControls();

    }

}
