using UnityEngine;

public class GameController : MonoBehaviour
{
    #region Variables
    //======================= VARIABLES ================================

    [Header("Settings")]
    [SerializeField] private OperaSettings _settings;

    [Header("Core References")]
    [SerializeField] private GuestController _guestController;
    [SerializeField] private LightController _lightController;
    [SerializeField] private MessageController _messageController;
    [SerializeField] private CrowdSpawner _crowdSpawner;

    private int _remainingGuests;
    private float _elapsedTime;
    private bool _isOperaRunning;
    private float _guestTimer, _messageTimer, _lightTimer;
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
        if (_lightController == null)
            Debug.LogWarning("LightController is not assigned in GameController.");
        if (_messageController == null)
            Debug.LogWarning("MessageController is not assigned in GameController.");
        if (_crowdSpawner == null)
            Debug.LogWarning("CrowdSpawner is not assigned in GameController.");
    }

    private void OnEnable()
    {
        _guestController.OnGuestDropped += HandleGuestDrop;
        _messageController.OnTaskFailed += HandleMessageFail;
    }

    private void OnDisable()
    {
        _guestController.OnGuestDropped -= HandleGuestDrop;
        _messageController.OnTaskFailed -= HandleMessageFail;
    }

    private void Awake()
    {
        _remainingGuests = _settings.InitialPublic;
    }

    private void Start()
    {
        var crowds = _crowdSpawner.InitializeCrowds();
        _messageController.GetCrowds(crowds);

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
    private void SpawnLight() => Debug.Log("Spawning Light...");
    private void StartOpera() => _isOperaRunning = true;
    private void EndOpera()
    {
        _isOperaRunning = false;
        Debug.Log("Opera Finita!");
    }

    private void HandleGuestDrop(int guestDroppedDount)
    {
        if(guestDroppedDount>0)
        {
            _remainingGuests += guestDroppedDount;
        }
    }

    private void HandleMessageFail()
    {
        DecreaseTotalGuest(5);
    }

    private void DecreaseTotalGuest(int decreaseValue)
    {
        if(decreaseValue > 0)
        {
            _remainingGuests -= decreaseValue;
            if(_remainingGuests<= 0)
            {
                EndOpera();
            }
        }
    }

    #endregion

}