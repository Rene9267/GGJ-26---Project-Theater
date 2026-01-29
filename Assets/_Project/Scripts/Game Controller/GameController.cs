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
    [SerializeField] public PlayerInput playerInput;
    [SerializeField] public MusiciansController MusiciansController;
    [SerializeField] public ActorController _actorController;



    [Header("UI References")]

    private int _remainingGuests;
    private float _elapsedTime;
    private bool _isOperaRunning;
    private float _guestTimer, _messageTimer, _lightTimer;
    private readonly string _cameraAnimation = "AC_CameraStartMove";
    private readonly string _faceTheKing = "AC_faceTheKing";
    private readonly string _HappyKing = "AC_HappyKing";
    private readonly string _SadKing = "AC_SadKing";
    private readonly string _BadEnding = "AC_BadEnding";
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
        _mainCamera.gameObject.SetActive(true);
        playerInput.ActivateInput();
        await UniTask.Delay(1000);
        _actorController.StartAct();
        StartOpera();
    }

    void Update()
    {
        if (!_isOperaRunning) return;

        _elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(_elapsedTime / _settings.OperaDurationSeconds);

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

    private async void EndOpera()
    {
        _isOperaRunning = false;
        playerInput.DeactivateInput();
        _uiController.EndGame();
        await UniTask.Delay(1200);

        _myAnimation.Play(_faceTheKing);
        await UniTask.Delay(1200);
        _uiController.gameObject.SetActive(false);
        await UniTask.Delay(2000);

        if (_remainingGuests > (int)(_settings.InitialPublic*0.5))
        {
            _myAnimation.Play(_HappyKing);
        }
        else if(_remainingGuests > (int)(_settings.InitialPublic * 0.5) && _remainingGuests >0)
        {
            _myAnimation.Play(_SadKing);
        }
        else if (_remainingGuests <= 0)
        {
            _myAnimation.Play(_BadEnding);
        }

        await UniTask.Delay(5000);

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