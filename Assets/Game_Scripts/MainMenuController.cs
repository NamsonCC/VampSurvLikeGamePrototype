using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public delegate void RestartGameDelegate();

    public static RestartGameDelegate restartGameDelegate;


    public delegate void ResplutionChangedDelegate(int x, int y);

    public static ResplutionChangedDelegate resplutionChangedDelegate;

    public static MainMenuController Instance;

    public float musicVolume = 1.0f;
    public int qualityLevel = 2; // Varsayýlan grafik kalitesi

    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown orientationDropdown;
    private Resolution[] resolutions;

    public Slider volumeSlider;

    [SerializeField] private GameObject mainMenuBase;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject inGameMenu;
    [SerializeField] private GameObject inGameMenuMain;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject loadingTextObject;


    private void Awake()
    {
        Time.timeScale = 0;

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        restartGameDelegate += RestartDefaultMainMenu;
        mainMenuBase.SetActive(true);
        SceneManager.sceneLoaded += OnSceneLoaded;

        InitializeQualityDropdown();
        InitializeVolumeSlider();
        InitializeResolutionDropdown();

#region Old 
        /* if (Application.isMobilePlatform)
         {
             int width = Screen.currentResolution.width;
             int height = Screen.currentResolution.height;
             resolutions[0] = new Resolution
             {
                 width = Screen.currentResolution.width,
                 height = Screen.currentResolution.height,
                 refreshRateRatio = Screen.currentResolution.refreshRateRatio
             };
         }
         else
         {
             resolutions = Screen.resolutions;
             Debug.Log("[PC] Desteklenen Çözünürlükler:");
         }

         resolutionDropdown.ClearOptions();

         var options = new System.Collections.Generic.List<string>();
         int currentResolutionIndex = 0;

         for (int i = 0; i < resolutions.Length; i++)
         {
             string option = resolutions[i].width + "x" + resolutions[i].height;
             options.Add(option);

             if (resolutions[i].width == Screen.currentResolution.width &&
                 resolutions[i].height == Screen.currentResolution.height)
             {
                 currentResolutionIndex = i;
             }
         }

         resolutionDropdown.AddOptions(options);
         resolutionDropdown.value = currentResolutionIndex;
         resolutionDropdown.RefreshShownValue();
         resolutionDropdown.onValueChanged.AddListener(SetResolution);*/
        #endregion
        if (Application.isMobilePlatform)
        {
            InitializeMobileSettings();
        }
        else
        {
            DisableOrientationSettings();
        }
    }
    private void RestartDefaultMainMenu()
    {
        mainMenuBase.SetActive(true);
        mainMenu.SetActive(true);
        inGameMenu.SetActive(false);
        inGameMenuMain.SetActive(false);

    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        loadingTextObject.gameObject.SetActive(false);
        mainMenu.SetActive(true);
    }
    private void InitializeQualityDropdown()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
        qualityDropdown.onValueChanged.AddListener(SetQuality);
    }
    private void InitializeVolumeSlider()
    {
        volumeSlider.value = AudioListener.volume;
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }
    private void InitializeResolutionDropdown()
    {
        resolutions = Application.isMobilePlatform ? new Resolution[] { Screen.currentResolution } : Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width}x{resolutions[i].height}";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    private void InitializeMobileSettings()
    {
        // Dropdown seçeneklerini ayarla
        orientationDropdown.ClearOptions();
        orientationDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "Portrait",
            "Landscape Left",
            "Landscape Right",
            "Portrait Upside Down"
        });

        // Mevcut yönlendirmeyi Dropdown'da seçili hale getir
        SetDropdownValueFromCurrentOrientation();

        // Dropdown deðiþtiðinde yönlendirmeyi ayarla
        orientationDropdown.onValueChanged.AddListener(OnOrientationChanged);

    }
    private void DisableOrientationSettings()
    {
        orientationDropdown.gameObject.SetActive(false);
    }


    private void SetDropdownValueFromCurrentOrientation()
    {
        switch (Screen.orientation)
        {
            case ScreenOrientation.Portrait:
                orientationDropdown.value = 0;
                break;
            case ScreenOrientation.LandscapeLeft:
                orientationDropdown.value = 1;
                break;
            case ScreenOrientation.LandscapeRight:
                orientationDropdown.value = 2;
                break;
            case ScreenOrientation.PortraitUpsideDown:
                orientationDropdown.value = 3;
                break;
            default:
                orientationDropdown.value = 0; // Varsayýlan olarak Portrait
                break;
        }
        orientationDropdown.RefreshShownValue();
    }

    private void OnOrientationChanged(int index)
    {

        switch (index)
        {
            case 0:
                Screen.orientation = ScreenOrientation.Portrait;
                break;
            case 1:
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                break;
            case 2:
                Screen.orientation = ScreenOrientation.LandscapeRight;
                break;
            case 3:
                Screen.orientation = ScreenOrientation.PortraitUpsideDown;
                break;
        }
        Debug.Log($"Orientation set to: {orientationDropdown.options[index].text}");
    }

    private void OnAutoRotateChanged(bool isAutoRotate)
    {
        if (isAutoRotate)
        {
            // Otomatik döndürme açýk
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Debug.Log("Auto Rotation enabled");
        }
        else
        {
            // Otomatik döndürme kapalý
            Screen.orientation = ScreenOrientation.Portrait; // Varsayýlan olarak Portrait
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Debug.Log("Auto Rotation disabled");
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        resplutionChangedDelegate?.Invoke(resolution.width, resolution.height);
        Debug.Log("SetResolution called");
    }
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        Debug.Log($"Volume set to: {volume}");
    }
    public void SetQuality(int qualityIndex)
    {
        qualityLevel = qualityIndex;
        QualitySettings.SetQualityLevel(qualityIndex);
        Debug.Log("SetQuality called");
    }

    public void GetBackToMenu()
    {
        if (mainMenuBase.activeSelf)
        {
            mainMenu.SetActive(true);
        }
        else
        {
            inGameMenu.SetActive(true);
            inGameMenuMain.SetActive(true);
        }
        settingsMenu.SetActive(false);
    }

    public void StartGame()
    {
        Time.timeScale = 1f; 
        loadingTextObject.gameObject.SetActive(true);
        inGameMenu.SetActive(true);
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void QuitFromGame()
    {
        Application.Quit();
    }
}
