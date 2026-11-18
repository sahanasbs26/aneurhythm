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
    [SerializeField] BeatManager beatManager;
    [SerializeField] InputManager inputManager;
    [SerializeField] PlayerManager playerManager;

    GameObject calibrationUI;

    void Awake()
    {
        inputManager = GetComponent<InputManager>();
        pauseMenu.gameObject.SetActive(false);
        beatManager = songManager.gameObject.GetComponent<BeatManager>();
    }


    /// <summary>
    /// Instantiates the calibration menu prefab as a child of the provided parent
    /// </summary>
    public void ToggleCalibration()
    {
        if (calibrationUI == null) {
            Debug.Log("Switching to calibrationUI");
            calibrationUI = Instantiate(calibrationPrefab, calibrationParent);
            inputManager.calibrationManager = calibrationUI.GetComponentInChildren<CalibrationBeatManager>();
            pauseMenu.gameObject.SetActive(false);
            Debug.Log("Inputmanager null?: " + inputManager == null);
            inputManager.SwitchToCalibrationControls();
        } else
        {
            inputManager.SwitchToUiControls(); //! Assumes player exits into another menu
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

    public void Restart()
    {

        pauseMenu.gameObject.SetActive(false);
        inputManager.SwitchToGameplayControls();
        songManager.OnRestart();
        playerManager.OnRestart();
    }

    void OnPause()
    {
        pauseMenu.gameObject.SetActive(true);
        songManager.OnPause();
        inputManager.SwitchToUiControls();
        if (calibrationUI != null) ToggleCalibration();
    }

    void OnResume()
    {
        pauseMenu.gameObject.SetActive(false);
        songManager.OnResume();
        inputManager.SwitchToGameplayControls();

    }

}
