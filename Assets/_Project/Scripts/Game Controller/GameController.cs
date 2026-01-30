using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
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
    [SerializeField] private Animation _myAnimation;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Camera _cutSceneCamera;
    [SerializeField] private Transform _finalCameraPosition;
    [SerializeField] public PlayerInput playerInput;
    [SerializeField] public MusiciansController MusiciansController;
    [SerializeField] public ActorController _actorController;
    [SerializeField] private King _king;


    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioSource _theaterBackground;

    [Header("Audio Clip")]
    [SerializeField] private AudioClip _kingTromb;
    [SerializeField] private AudioClip _rulloDiTamburi;

    [SerializeField] private AudioClip _act1;
    [SerializeField] private AudioClip _act2;
    [SerializeField] private AudioClip _act3;

    private bool _isAct1Notified, _isAct2Notified, _isAct3Notified;

    [Header("UI References")]
    private int _remainingGuests;
    private float _elapsedTime;
    private bool _isOperaRunning;
    private float _guestTimer, _messageTimer, _lightTimer;
    private readonly string _cameraAnimation = "AC_CameraStartMove";
    private readonly string _menuScene = "Scene_Menu";

    //=======================================================
    #endregion

    #region Unity Methods
    //======================= Unity Methods ================================
    private void OnValidate()
    {
        if (_settings == null)
            Debug.LogWarning("Opera Settings is not assigned in GameController.");
        if (_guestController == null)
            Debug.LogWarning("GuestController is not assigned in GameController.");
        if (_candleController == null)
            Debug.LogWarning("LightController is not assigned in GameController.");
        if (_messageController == null)
            Debug.LogWarning("MessageController is not assigned in GameController.");
        if (_crowdSpawner == null)
            Debug.LogWarning("CrowdSpawner is not assigned in GameController.");
        if (_uiController == null)
            DevLog.LogWarning($"[{this.gameObject}]: GlobalUIController is not assigned");
    }

    private void OnEnable()
    {
        _guestController.OnGuestDropped += HandleGuestDrop;
        _messageController.OnTaskFailed += HandleMessageFail;
        _candleController.OnDarkRise += HandleDarkRise;
    }

    private void OnDisable()
    {
        _guestController.OnGuestDropped -= HandleGuestDrop;
        _messageController.OnTaskFailed -= HandleMessageFail;
        _candleController.OnDarkRise += HandleDarkRise;
    }

    private void Awake()
    {
        playerInput.DeactivateInput();
        _remainingGuests = _settings.InitialPublic;
        _uiController.SetPeopleNumber(_remainingGuests);
    }

    private void Start()
    {
        CutsceneStartOpera();
    }


    private async void CutsceneStartOpera()
    {
        var crowds = _crowdSpawner.InitializeCrowds();
        _messageController.GetCrowds(crowds);

        _myAnimation.Play(_cameraAnimation);
        _uiController.StartUp();
        await UniTask.Delay(2000);
    }

    public async void StartGameplay()
    {
        _ = FadeAudio(_theaterBackground, false, 0);
        _mainCamera.gameObject.SetActive(true);
        playerInput.ActivateInput();
        await UniTask.Delay(1000);
        if (_act1 != null)
            _theaterBackground.clip = _act1;
        _ = FadeAudio(_theaterBackground, true, 3);
        _actorController.StartAct();
        StartOpera();
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
            _ = FadeAudio(_theaterBackground, false, 1);
            if (_act2 != null)
                _theaterBackground.clip = _act2;
            _ = FadeAudio(_theaterBackground, true, 1);
            _isAct2Notified = true;
        }

        if (progress >= _settings.CandleActivationThreshold && !_isAct3Notified)
        {
            _ = FadeAudio(_theaterBackground, false, 1);
            if (_act3 != null)
                _theaterBackground.clip = _act3;
            _ = FadeAudio(_theaterBackground, true, 1);
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


    private void FaceTheKing()
    {
        _cutSceneCamera.transform.SetPositionAndRotation(_finalCameraPosition.position, _finalCameraPosition.rotation);
        _cutSceneCamera.fieldOfView = 45;
    }

    private async void EndOpera()
    {
        _isOperaRunning = false;
        playerInput.DeactivateInput();
        _mainCamera.gameObject.SetActive(false);
        await _uiController.FadeCanvas(false, 1);

        await UniTask.Delay(500);

        FaceTheKing();
        await UniTask.Delay(1200);
        await _uiController.FadeCanvas(true, 1);

        _audioSource.PlayOneShot(_kingTromb);
        await UniTask.Delay(5000);

        if (_rulloDiTamburi != null) ;
        _audioSource.clip = _rulloDiTamburi;
        _audioSource.Play();

        await UniTask.Delay(2000);

        if (_remainingGuests > (int)(_settings.InitialPublic * 0.5))
        {
            _king.EndRate(KingState.Happy);
        }
        else
        {
            _king.EndRate(KingState.Ok);
        }

        await UniTask.Delay(3000);
        await _uiController.FadeCanvas(false, 1);

        await UniTask.Delay(500);

        SceneManager.LoadScene(_menuScene);
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
        ChangeTotlaGuest(_settings.MessageFailTask);
    }


    public async UniTask FadeAudio(AudioSource source, bool isFadeIn, float duration)
    {
        if (source == null) return;

        float startVolume = isFadeIn ? 0f : source.volume;
        float endVolume = isFadeIn ? 1f : 0f;
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
            if (_remainingGuests <= 0)
            {
                EndOpera();
            }
        }
    }

    void HandleDarkRise()
    {
        ChangeTotlaGuest(_settings.DarkIsRising);
        DevLog.Log($"[{this.gameObject}]: Sto decrementando il valore degli spettatori di {1}, rimanenti: {_remainingGuests}");
    }

    #endregion
}