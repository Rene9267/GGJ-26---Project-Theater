using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    #region Variables

    [Header("Controller References")]
    [SerializeField] private LanguageSelectorController _languageController;
    [SerializeField] private WindowController _windowController;
    [SerializeField] private AudioController _audioController;

    [Header("Canvasses Group References")]
    [SerializeField] private CanvasGroup _mainCanvasGroup;
    [SerializeField] private CanvasGroup _settingsCanvasGroup;
    [SerializeField] private CanvasGroup _creditsCanvasGroup;
    [SerializeField] private CanvasGroup _fadeCanvasGroup;
    [SerializeField] private CanvasGroup _commandsGroup;

    [Header("Button References")]
    [SerializeField] private GameObject _startButton;
    [SerializeField] private GameObject _settingsFirstButton;
    [SerializeField] private GameObject _commandsFirstButton;
    [SerializeField] private GameObject _creditsFirstButton;

    [Header("Effects References")]
    [SerializeField] private AudioSource _audio;
    [SerializeField] private Animator _animator;

    private OptionsController _optionsController;

    private int _animFadeInMainStart;
    private int _animFadeOutStart;
    private int _animFadeInSettings;
    private int _animFadeOutSettings;
    private int _animFICredits;
    private int _animFOCredits;
    private int _animFOCommands;
    private int _animFICommands;
    private int _animPlay;

    private readonly string _gamePlayScene = "Scene_Tutorial";
    #endregion

    #region Unity Standard Methods

    void OnValidate()
    {
        if (_mainCanvasGroup == null)
            DevLog.LogError("Riferimento al main canvas group assente", this);
        if (_settingsCanvasGroup == null)
            DevLog.LogError("Riferimento al settings canvas group assente", this);
        if (_creditsCanvasGroup == null)
            DevLog.LogError("Riferimento al credits canvas group assente", this);
        if (_fadeCanvasGroup == null)
            DevLog.LogError("Riferimento al fade canvas group assente", this);
        if (_commandsGroup == null)
            DevLog.LogError("Riferimento al commands canvas group assente", this);
        if (_languageController == null)
            DevLog.LogError("Riferimento al language controller assente", this);
        if (_windowController == null)
            DevLog.LogError("Riferimento al window controller assente", this);
        if (_audioController == null)
            DevLog.LogError("Riferimento al audio controller assente", this);
        if (_audio == null)
            DevLog.LogError("Riferimento all'audio source assente", this);
        if (_animator == null)
            DevLog.LogError("Riferimento all'animator assente", this);
    }

    void Awake()
    {
        //Canvas groups inizialization
        _commandsGroup.gameObject.SetActive(false);
        _commandsGroup.alpha = 0;
        _mainCanvasGroup.gameObject.SetActive(false);
        _mainCanvasGroup.alpha = 0;
        _settingsCanvasGroup.gameObject.SetActive(false);
        _settingsCanvasGroup.alpha = 0;
        _creditsCanvasGroup.gameObject.SetActive(false);
        _creditsCanvasGroup.alpha = 0;
        _fadeCanvasGroup.gameObject.SetActive(true);
        _fadeCanvasGroup.alpha = 1;


        _animFadeInMainStart = Animator.StringToHash("FadeInStart");
        _animFadeOutStart = Animator.StringToHash("FadeOutMain");
        _animFadeInSettings = Animator.StringToHash("FadeInSettings");
        _animFadeOutSettings = Animator.StringToHash("FadeOutSettings");
        _animFICredits = Animator.StringToHash("FadeInCredits");
        _animFOCredits = Animator.StringToHash("FadeOutCredits");
        _animFICommands = Animator.StringToHash("FICommands");
        _animFOCommands = Animator.StringToHash("FOCommands");
        _animPlay = Animator.StringToHash("Play");


        _settingsCanvasGroup.gameObject.TryGetComponent(out _optionsController);
        if(_optionsController == null)
            DevLog.LogError($"[{this}] Options controller non trovato nel canvas delle opzioni", this);
    }

    void Start()
    {
        StartGame();
    }

    void OnEnable()
    {
        if(_optionsController != null)
        {
            _optionsController.OnFXVolumeChange += ApplySettings;
            _optionsController.OnMusicVolumeChange += ApplySettings;
        }
    }

    void OnDisable()
    {
        if(_optionsController != null)
        {
            _optionsController.OnFXVolumeChange -= ApplySettings;
            _optionsController.OnMusicVolumeChange -= ApplySettings;
        }
    }

    #endregion

    #region Class Methods

    private async void StartGame()
    {
        await UniTask.Delay(500);
        _animator.SetTrigger(_animFadeInMainStart);
        _audio.Play();
    }

    public async void OnStartClick()
    {
        _animator.SetTrigger(_animPlay);
        await UniTask.Delay(500);
        _ = FadeAudio(_audio, false, 1, 0);
        await UniTask.Delay(1200);

        SceneManager.LoadScene(_gamePlayScene);
    }

    public async void OnExitClick()
    {
        await UniTask.Delay(1000);
        Application.Quit();
    }

    public void OnSettingShow()
    {
        _animator.SetTrigger(_animFadeInSettings);
    }

    public void OnSettingsHide()
    {
        ApplySettings();
        _animator.SetTrigger(_animFadeOutSettings);
    }

    public void ShowCredits()
    {
        ApplySettings();
        _animator.SetTrigger(_animFICredits);
    }

    public void HideCredits()
    {
        _animator.SetTrigger(_animFOCredits);
    }

    public void ShowCommands()
    {
        ApplySettings();
        _animator.SetTrigger(_animFICommands);
    }

    public void HideCommands()
    {
        _animator.SetTrigger(_animFOCommands);
    }

    public async void ShowTutorial()
    {
        await UniTask.Delay(500);
    }

    public async UniTask FadeAudio(AudioSource source, bool isFadeIn, float duration, float volume = 0)
    {
        if (source == null) return;

        float startVolume = isFadeIn ? 0f : source.volume;
        float endVolume = isFadeIn ? volume : 0f;
        float elapsed = 0f;

        if (isFadeIn && !source.isPlaying) source.Play();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            source.volume = Mathf.Lerp(startVolume, endVolume, progress);

            await UniTask.Yield();
        }

        source.volume = endVolume;

        if (!isFadeIn) source.Stop();
    }

    private void ApplySettings()
    {
        _languageController.ChangeLanguage(GameSettings.Instance.Language);
        _windowController.ApplyWindowSettings();
        _audioController.ApplyAllVolumes();
    }

    public void OpenMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_startButton);
    }

    public void OpenSettingsMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_settingsFirstButton);
    }

    public void OpenCommandsMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_commandsFirstButton);
    }

    public void OpenCreditsMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_creditsFirstButton);
    }
    #endregion
}
