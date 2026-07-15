using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    #region Variables
    //======================= VARIABLES ================================

    [Header("Settings")]
    [SerializeField] private OperaSettings _settings;

    [Header("Core References")]
    [SerializeField] private GuestController _guestController;
    [SerializeField] private CandleController _candleController;
    [SerializeField] private MessageController _messageController;
    [SerializeField] private CrowdSpawner _crowdSpawner;
    [SerializeField] private GlobalUIController _uiController;
    [SerializeField] private ScoreController _scoreController;
    [SerializeField] private ScoreCanvas _scoreCanvas;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Camera _cutSceneCamera;
    [SerializeField] private Transform _finalCameraPosition;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private MusiciansController MusiciansController;
    [SerializeField] private ActorController _actorController;
    [SerializeField] private CutSceneController _cutSceneController;
    [SerializeField] private King _king;
    [SerializeField] private WayPointNavigator _fakePlayer;

    [Header("Cutscenes")]
    [SerializeField] private PlayableDirector _endOperaCutscene;
    [SerializeField] private PlayableDirector _faceTheKingCutscene;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioSource _theaterBackground;

    [Header("Audio Clip")]
    [SerializeField] private AudioClip _kingTromb;
    [SerializeField] private AudioClip _rulloDiTamburi;
    [SerializeField] private AudioClip _kingApplause;
    [SerializeField] private AudioClip _kingBuu;
    [SerializeField] private AudioClip _preShow;
    [SerializeField] private AudioClip _act1;

    private bool _isAct1Notified, _isAct2Notified, _isAct3Notified;

    [Header("UI References")]
    private int _remainingGuests;
    private float _elapsedTime;
    private bool _isOperaRunning;
    private float _guestTimer, _messageTimer, _lightTimer;
    private readonly string _menuScene = "Scene_Menu";

    public static event Action<bool> OnEndingAnimation;

    [Header("Pause Settings")]
    [SerializeField] private GameObject _pauseMenuRoot;
    [SerializeField] private Animator _pauseAnimator;

    private static readonly int IsPauseTrigger = Animator.StringToHash("IsPause");
    private static readonly int IsEndPauseTrigger = Animator.StringToHash("IsEndPause");

    //=======================================================
    #endregion

    #region Unity Methods

    private void OnValidate()
    {
        if (_settings == null)
            DevLog.LogWarning("Opera Settings is not assigned in GameController.");
        if (_guestController == null)
            DevLog.LogWarning("GuestController is not assigned in GameController.");
        if (_candleController == null)
            DevLog.LogWarning("LightController is not assigned in GameController.");
        if (_messageController == null)
            DevLog.LogWarning("MessageController is not assigned in GameController.");
        if (_crowdSpawner == null)
            DevLog.LogWarning("CrowdSpawner is not assigned in GameController.");
        if (_uiController == null)
            DevLog.LogWarning($"[{this.gameObject}]: GlobalUIController is not assigned");
    }

    private void OnEnable()
    {
        _guestController.OnGuestDropped += HandleGuestDrop;
        _messageController.OnTaskFailed += HandleMessageFail;
        _candleController.OnDarkRise += HandleDarkRise;
        _guestController.OnTaskFailed += HandleGuestTaskFailed;
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    private void OnDisable()
    {
        _guestController.OnGuestDropped -= HandleGuestDrop;
        _messageController.OnTaskFailed -= HandleMessageFail;
        _candleController.OnDarkRise -= HandleDarkRise;
        _guestController.OnTaskFailed -= HandleGuestTaskFailed;
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;

        if (PauseController.Instance != null)
        {
            PauseController.Instance.OnPauseToggled -= HandlePause;
            PauseController.Instance.OnResumeRequested -= StartExitAnimation;
        }
    }

    private void Awake()
    {
        playerInput.enabled = false;

        if (playerInput.TryGetComponent<PlayerController>(out var pc)) pc.CanMove = false;

        _remainingGuests = _settings.InitialPublic;
        _uiController.SetPeopleNumber(_remainingGuests);

        if (_scoreController != null)
        {
            playerInput.TryGetComponent<PlayerController>(out var player);
            _scoreController.Connect(_guestController, _messageController, _candleController, player);
            _scoreController.ResetStats(_settings.InitialPublic);
        }
    }

    private void OnDestroy()
    {
        if (_scoreController != null) _scoreController.Disconnect();
    }

    private void Start()
    {
        if (PauseController.Instance != null)
        {
            PauseController.Instance.OnPauseToggled += HandlePause;
            PauseController.Instance.OnResumeRequested += StartExitAnimation;
        }
    }

    #region Class Methods

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        DevLog.Log($"La scena {scene.name} ├¿ completamente caricata!");
        CutsceneStartOpera();
    }

    private async void CutsceneStartOpera()
    {
        var crowds = _crowdSpawner.InitializeCrowds();
        _messageController.GetCrowds(crowds);

        FadeAudio(_theaterBackground, false, 0, 0).Forget();
        _theaterBackground.clip = _preShow;
        await FadeAudio(_theaterBackground, true, 1, 0.3f);

        await _cutSceneController.IntroCutscene();
        _uiController.StartUp();
        StartGameplay();
    }

    public async void StartGameplay()
    {
        FadeAudio(_theaterBackground, false, 0.5f, 0).Forget();
        
        await _fakePlayer.StartNavigation();
        _fakePlayer.gameObject.SetActive(false);

        // Riattiviamo l'input e il movimento invece di accendere bruscamente il GameObject
        playerInput.enabled = true;
        if (playerInput.TryGetComponent<PlayerController>(out var pc)) pc.CanMove = true;
        playerInput.gameObject.SetActive(true);
        
        _theaterBackground.clip = _act1;
        FadeAudio(_theaterBackground, true, 3, 1f).Forget();
        _actorController.StartAct();
        StartOpera();
    }

    private void HandleGuestTaskFailed(int lostCount)
    {
        ChangeTotlaGuest(-lostCount);
        DevLog.Log($"Task Guest Fallita! Persi {lostCount} spettatori.");
    }

    void Update()
    {
        if (!_isOperaRunning) return;

        _elapsedTime += Time.deltaTime;

        float progress = Mathf.Clamp01(_elapsedTime / _settings.OperaDurationSeconds);

        _uiController.UpdateTimerBar(progress);

        if (progress >= _settings.GuestActivationThreshold && !_isAct1Notified)
        {
            _isAct1Notified = true;
        }

        if (progress >= _settings.MessageActivationThreshold && !_isAct2Notified)
        {
            _isAct2Notified = true;
        }

        if (progress >= _settings.CandleActivationThreshold && !_isAct3Notified)
        {
            _isAct3Notified = true;
        }

        HandleSpawning(progress);

        if (progress >= 1f) EndOpera();
    }

    //=======================================================
    #endregion

    #region GameLoop
    //======================= GameLoop ================================

    private void HandleSpawning(float progress)
    {
        if (progress >= _settings.GuestActivationThreshold)
            UpdateSpawnTimer(ref _guestTimer, _settings.guestSpawnInterval.Evaluate(progress), SpawnGuest);

        if (progress >= _settings.MessageActivationThreshold)
        {
            UpdateSpawnTimer(ref _messageTimer, _settings.messageSpawnInterval.Evaluate(progress), SpawnMessage);
        }

        if (progress >= _settings.CandleActivationThreshold)
        {
            UpdateSpawnTimer(ref _lightTimer, _settings.lightSpawnInterval.Evaluate(progress), SpawnLight);
        }
    }

    private void UpdateSpawnTimer(ref float timer, float interval, System.Action spawnAction)
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            spawnAction?.Invoke();
            timer = interval;
        }
    }

    private void SpawnGuest() => _guestController.CreateGuestDirection();
    private void SpawnMessage() => _messageController.CreateCrowdLink();
    private void SpawnLight() => _candleController.TurnOffACandle();
    private void StartOpera() => _isOperaRunning = true;

    private IEnumerator MuoviCameraAZero(float durata)
    {
        _mainCamera.transform.GetLocalPositionAndRotation(out Vector3 posizioneIniziale, out Quaternion rotazioneIniziale);
        float tempoTrascorso = 0f;

        while (tempoTrascorso < durata)
        {
            tempoTrascorso += Time.deltaTime;
            float percentuale = tempoTrascorso / durata;
            percentuale = percentuale * percentuale * (3f - 2f * percentuale);

            _mainCamera.transform.SetLocalPositionAndRotation(Vector3.Lerp(posizioneIniziale, Vector3.zero, percentuale), Quaternion.Slerp(rotazioneIniziale, Quaternion.identity, percentuale));
            yield return null;
        }

        _mainCamera.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    private async void EndOpera()
    {
        StopAllGameplay();
        _actorController.StartBending();
        StartCoroutine(MuoviCameraAZero(1f));
        _uiController.FadeCanvas(true, 1).Forget();
        _uiController.GamePlayUI.SetActive(false);

        var finalStats = _scoreController != null ? _scoreController.GetFinalStats() : new SessionStats();

        await CutsceneMiscellaneous.PlayCutscene(_endOperaCutscene, this.gameObject);
        await CutsceneMiscellaneous.PlayCutscene(_faceTheKingCutscene, this.gameObject);
        await UniTask.Delay(2000);

        if (_remainingGuests > 20)
        {
            _king.EndRate(KingState.Happy);
            await UniTask.Delay(2000);
            _audioSource.PlayOneShot(_kingApplause);
            OnEndingAnimation?.Invoke(true);
        }
        else
        {
            _king.EndRate(KingState.Sad);
            await UniTask.Delay(2000);
            _audioSource.PlayOneShot(_kingBuu);
            OnEndingAnimation?.Invoke(false);
        }

        await UniTask.Delay(2000);

        // Svela la scena e mostra la UI punteggio
        await _uiController.FadeCanvas(false, 1);
        if (_scoreCanvas != null) _scoreCanvas.Show(finalStats);

        // Congela tutto (le animazioni della UI usano unscaledDeltaTime)
        Time.timeScale = 0f;
    }

    private void HandleGuestDrop(int guestDroppedDount)
    {
        if (guestDroppedDount > 0)
        {
            ChangeTotlaGuest(guestDroppedDount);
        }
    }

    private void HandleMessageFail()
    {
        ChangeTotlaGuest(-_settings.MessageFailTask);
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

    private void ChangeTotlaGuest(int value)
    {
        if (value != 0)
        {
            _remainingGuests += value;
            _uiController.SetPeopleNumber(_remainingGuests);
            DevLog.Log($"[{this.gameObject}]: Sto decrementando il valore degli spettatori di {value}, rimanenti: {_remainingGuests}");
            if (_remainingGuests <= 0)
            {
                EndOpera();
            }
        }
    }

    void HandleDarkRise()
    {
        ChangeTotlaGuest(-_settings.DarkIsRising);
        DevLog.Log($"[{this.gameObject}]: Sto decrementando il valore degli spettatori di {_settings.DarkIsRising}, rimanenti: {_remainingGuests}");
    }

    private void StopAllGameplay()
    {
        _isOperaRunning = false;
        playerInput.DeactivateInput();

        if (_guestController != null) _guestController.StopAllGuests();
        if (_messageController != null) _messageController.StopAllMessages();
        if (_candleController != null) _candleController.StopAllCandles();
        if (_actorController != null) _actorController.StopAct();
    }

    private void BroadcastEndingToStaticGuests(bool isHappy)
    {
        var staticGuests = FindObjectsByType<StaticGuest_Public_Controller>(FindObjectsSortMode.None);
        foreach (var guest in staticGuests)
            guest.SetEndingAnimation(isHappy);
    }
    #endregion

    #region Pause Logic
    //======================= PAUSE ================================

    private void HandlePause(bool isPaused)
    {
        DevLog.Log($"[GameController] Pausa toggled: {(isPaused ? "PAUSED" : "RESUMED")}");
        if (_pauseAnimator == null || _pauseMenuRoot == null) return;

        if (isPaused)
        {
            _pauseMenuRoot.SetActive(true);
            _pauseAnimator.ResetTrigger(IsEndPauseTrigger);
            _pauseAnimator.SetTrigger(IsPauseTrigger);
        }
    }

    private void StartExitAnimation()
    {
        if (_pauseAnimator != null && _pauseAnimator.isActiveAndEnabled)
        {
            DevLog.Log("[Main Game] Avvio Animazione IsEndPause");
            _pauseAnimator.ResetTrigger(IsPauseTrigger);
            _pauseAnimator.SetTrigger(IsEndPauseTrigger);
        }
        else
        {
            DevLog.Log("[Main Game] Animator non disponibile, sblocco immediato.");
            OnPauseOutAnimationComplete();
        }
    }

    public void OnPauseOutAnimationComplete()
    {
        if (PauseController.Instance != null)
        {
            PauseController.Instance.FinalizeResume();
        }

        if (_pauseMenuRoot != null)
        {
            _pauseMenuRoot.SetActive(false);
        }
    }

    public void Btn_ResumeGame()
    {
        if (PauseController.Instance != null && PauseController.Instance.IsPaused)
        {
            PauseController.Instance.RequestResume();
        }
    }

    public void Btn_ReturnToMenu()
    {
        if (PauseController.Instance != null)
        {
            PauseController.Instance.FinalizeResume();
        }
        SceneManager.LoadScene(_menuScene);
    }

    //==============================================================
    #endregion

    #endregion
}
