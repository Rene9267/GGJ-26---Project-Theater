using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;


struct ExitInfo
{
    public Color color;
    public Sprite direction;
    public Sprite background;
}

public class GuestController : MonoBehaviour
{
    [SerializeField] private GuestsSettings _settings;
    [SerializeField] private FollowerGuestInteractionArea _guestInteractionArea;
    [SerializeField] private List<FollowerGuestExitArea> _guestInteractionExitAreas = new();
    [SerializeField] private Transform _guestSpwanTransform;

    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip _spawnClip;
    [SerializeField] private AudioClip _arriveClip;

    [Header("Exit Physics")]
    [Tooltip("Il nome del layer creato per i guest che escono (es. 'ExitingGuest')")]
    [SerializeField] private string _exitLayerName = "ExitingGuest";
    private int _exitLayerIndex;
    private int _defaultGuestLayer;


    private Dictionary<Color, List<FollowerGuest>> _activeGuests = new Dictionary<Color, List<FollowerGuest>>();
    private Dictionary<Color, FollowerGuestExitArea> _guestInteractionExitAreasDic = new Dictionary<Color, FollowerGuestExitArea>();
    private List<GameObject> _guestPool = new();
    private List<Color> _availableGuestsColor;
    private Vector2 _crowdMiddlePoint;
    private bool _isSpawnAreaFree = true;
    private List<FollowerGuest> _lastSpawnedGuests;
    public event Action<int> OnGuestDropped;
    private CancellationTokenSource _cts;
    public event Action OnTaskFailed;


    void OnValidate()
    {
        if (_settings == null || _settings.GuestsPool == null || _settings.GuestsPool.Count <= 0)
        {
            Debug.LogWarning("[Guest Controller]: Impostazioni o Pool di elementi assente nell'Inspector.");
        }
    }

    void OnDisable()
    {
        if (_guestInteractionArea != null)
        {
            _guestInteractionArea.OnAreaExit -= HandlePlayerExitGrabArea;
            _guestInteractionArea.OnStartInteract -= HandlePlayerGrabGuests;
            _guestInteractionArea.OnInteract -= HandleplayerStartInteract;
            _guestInteractionArea.OnHurryUP -= HandleHurryUP;
        }

        foreach (FollowerGuestExitArea exitArea in _guestInteractionExitAreas)
        {
            if (exitArea != null)
            {
                exitArea.OnCompleteInteractWithExit -= HandlePlayerDropGuest;
                exitArea.OnStartInteract -= HandlePlayerStartDroppingGuest;
            }
        }
    }

    void Awake()
    {
        if (_settings == null) return;

        // Popoliamo il pool
        foreach (var guest in _settings.GuestsPool)
        {
            if (guest == null) continue;
            var tmpGuest = Instantiate(guest, _guestSpwanTransform);
            tmpGuest.SetActive(false);
            _guestPool.Add(tmpGuest);
        }

        _guestInteractionExitAreasDic ??= new();

        _availableGuestsColor = new List<Color>(_settings.GuestsColors);
    }

    void Start()
    {
        Queue<ExitInfo> shuffledQueue = new();
        List<ExitInfo> tmpList = new();
        for (int i = 0; i < _availableGuestsColor.Count; i++)
        {
            tmpList.Add(new ExitInfo
            {
                color = _availableGuestsColor[i],
                background = _settings.ExitAreaBaseImages[i],
                direction = _settings.ExitAreaDirectionImages[i]
            });
        }
        ShuffleAndEnqueue(tmpList, shuffledQueue);

        foreach (FollowerGuestExitArea exitArea in _guestInteractionExitAreas)
        {
            var ExitInfo = shuffledQueue.Dequeue();
            exitArea.SetUpInteractionArea(ExitInfo.color, ExitInfo.direction, ExitInfo.background);

            _guestInteractionArea.OnStartInteract -= HandlePlayerGrabGuests;
            _guestInteractionArea.OnInteract -= HandleplayerStartInteract;

            exitArea.OnCompleteInteractWithExit += HandlePlayerDropGuest;
            exitArea.OnStartInteract += HandlePlayerStartDroppingGuest;

            _guestInteractionExitAreasDic.Add(ExitInfo.color, exitArea);
        }

        _exitLayerIndex = LayerMask.NameToLayer(_exitLayerName);

        if (_settings.GuestsPool.Count > 0 && _settings.GuestsPool[0] != null)
        {
            _defaultGuestLayer = _settings.GuestsPool[0].layer;
        }
    }

    private void ShuffleAndEnqueue(List<ExitInfo> list, Queue<ExitInfo> queue)
    {
        System.Random rng = new();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        queue.Clear();
        foreach (var item in list)
        {
            queue.Enqueue(item);
        }
    }
    private async void HandleHurryUP()
    {
        if (_lastSpawnedGuests != null && _lastSpawnedGuests.Count > 0)
        {
            foreach (var obj in _lastSpawnedGuests)
            {
                obj.HurryUp();
                await UniTask.Delay(200);
            }
        }
    }
    [ContextMenu("Genera Direction Manuale")]
    public void CreateGuestDirection()
    {
        if (_guestInteractionArea == null || _availableGuestsColor == null || _availableGuestsColor.Count == 0)
        {
            DevLog.LogError("[Guest Controller]: Impossibile generare. Area mancante o Colori terminati.");
            return;
        }

        if (_isSpawnAreaFree == false) return;

        _cts = new CancellationTokenSource();
        _guestInteractionArea.OnAreaExit -= HandlePlayerExitGrabArea;
        _guestInteractionArea.OnStartInteract -= HandlePlayerGrabGuests;

        int randomGuestFamilyColor = Random.Range(0, _availableGuestsColor.Count);
        var tmpColor = _availableGuestsColor[randomGuestFamilyColor];

        if (!_activeGuests.ContainsKey(tmpColor))
        {
            _activeGuests.Add(tmpColor, new List<FollowerGuest>());
            SpawnGuests(tmpColor);
            _guestInteractionArea.ResetArea();
            _guestInteractionArea.SetUpInteractionArea(tmpColor, _settings.RotationSpeed, _settings.Clockwise);
            _availableGuestsColor.RemoveAt(randomGuestFamilyColor);
        }

        _guestInteractionArea.OnAreaExit += HandlePlayerExitGrabArea;
        _guestInteractionArea.OnStartInteract += HandlePlayerGrabGuests;
        _guestInteractionArea.OnInteract += HandleplayerStartInteract;
        _guestInteractionArea.OnHurryUP -= HandleHurryUP;
        _guestInteractionArea.OnHurryUP += HandleHurryUP;
        GuestArriveTimerStart(_settings.TimeToExit, _cts.Token);
    }

    private void SpawnGuests(Color color)
    {
        if (_guestPool.Count == 0)
        {
            DevLog.LogWarning("[Guest Controller]: Pool vuoto, impossibile spawnare.");
            return;
        }

        _crowdMiddlePoint = Vector2.zero;

        int randomGuestNumber = Random.Range((int)_settings.GuestSpawnRange.x, (int)_settings.GuestSpawnRange.y);
        int actualSpawnedCount = 0;

        for (int i = 0; i < randomGuestNumber; i++)
        {
            if (_guestPool.Count == 0) break;

            bool foundValidSpot = false;
            int attempts = 0;

            while (!foundValidSpot && attempts < _settings.MaxAttemptsPerPawn)
            {
                attempts++;
                Vector2 randomPoint2D = Random.insideUnitCircle * _settings.Radius;
                Vector3 spawnPosition = new Vector3(randomPoint2D.x, 0, randomPoint2D.y) + _guestInteractionArea.transform.position;

                if (!Physics.CheckSphere(spawnPosition, _settings.SecurityRadiusCheck, _settings.ObstacleLayer))
                {
                    int randomIndex = Random.Range(0, _guestPool.Count);
                    var tmpGuest = _guestPool[randomIndex];

                    tmpGuest.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
                    tmpGuest.SetActive(true);

                    var followerComp = tmpGuest.GetComponent<FollowerGuest>();
                    if (followerComp != null)
                    {
                        _activeGuests[color].Add(followerComp);
                        followerComp.SetMyColor(color);
                    }

                    _guestPool.RemoveAt(randomIndex);
                    _crowdMiddlePoint += new Vector2(spawnPosition.x, spawnPosition.z);
                    actualSpawnedCount++;
                    foundValidSpot = true;
                }
            }
        }

        _source.PlayOneShot(_spawnClip);

        if (actualSpawnedCount > 0)
        {
            _isSpawnAreaFree = false;
            _crowdMiddlePoint /= actualSpawnedCount;
            _lastSpawnedGuests = new List<FollowerGuest>(_activeGuests[color]);

            _guestInteractionArea.transform.position = new Vector3(_crowdMiddlePoint.x, _guestInteractionArea.transform.position.y, _crowdMiddlePoint.y);
        }
    }

    private void HandlePlayerGrabGuests()
    {
        if (_guestInteractionExitAreasDic.ContainsKey(_guestInteractionArea.MyInteractionColor))
        {
            DevLog.Log("Il mio colore è attivo");
            _guestInteractionExitAreasDic[_guestInteractionArea.MyInteractionColor].EnableDirectionIcon();
        }

    }

    private void HandleplayerStartInteract()
    {
        CompleteGuestTask();
    }

    private void HandlePlayerDropGuest(Color colorID, Transform alignPoint, Transform exitPoint)
    {
        if (colorID == Color.clear)
        {
            Debug.LogError("[Guest Controller]: Manca il Colore");
            return;
        }

        int guestDroppedCount = 0;

        if (_activeGuests.ContainsKey(colorID))
        {
            guestDroppedCount = _activeGuests[colorID].Count;

            List<FollowerGuest> guestsToExit = new List<FollowerGuest>(_activeGuests[colorID]);

            _activeGuests[colorID].Clear();
            _activeGuests.Remove(colorID);
            _availableGuestsColor.Add(colorID);

            ExitSequenceRoutine(guestsToExit, alignPoint, exitPoint).Forget();
        }

        _source.PlayOneShot(_arriveClip);
        OnGuestDropped?.Invoke(guestDroppedCount);
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private async UniTaskVoid ExitSequenceRoutine(List<FollowerGuest> guests, Transform alignPoint, Transform exitPoint)
    {
        if (guests == null || guests.Count == 0) return;

        foreach (var guest in guests)
        {
            guest.SetPlayer(null);

            if (_exitLayerIndex != -1)
            {
                SetLayerRecursively(guest.gameObject, _exitLayerIndex);
            }

            if (guest.TryGetComponent<FollowerGuestMovement>(out var movement))
            {
                movement.LocalOffset = Vector3.zero;
            }

            guest.SetUpTarget(alignPoint);
        }

        await WaitForGuestsToReachTarget(guests, alignPoint, 1.5f);

        foreach (var guest in guests)
        {
            guest.SetUpTarget(exitPoint);
        }

        await WaitForGuestsToReachTarget(guests, exitPoint, 1.0f);

        foreach (FollowerGuest guest in guests)
        {
            if (guest.TryGetComponent<FollowerGuestMovement>(out var movement))
            {
                movement.LocalOffset = new Vector3(0, 1.2f, -1.8f);
            }

            guest.gameObject.layer = _defaultGuestLayer;

            guest.SetUpTarget(null);
            guest.gameObject.SetActive(false);
            _guestPool.Add(guest.gameObject);
        }
    }

    private async UniTask WaitForGuestsToReachTarget(List<FollowerGuest> guests, Transform target, float threshold)
    {
        bool allArrived = false;
        // Timeout di sicurezza (es. 5 secondi) per evitare loop infiniti se si incastrano
        float timeout = 5f;
        float timer = 0f;

        while (!allArrived && timer < timeout)
        {
            timer += Time.deltaTime;
            allArrived = true;

            foreach (var guest in guests)
            {
                if (guest == null || !guest.gameObject.activeSelf) continue;

                float dist = Vector3.Distance(guest.transform.position, target.position);
                if (dist > threshold)
                {
                    allArrived = false;
                    break; // Basta che uno sia lontano per aspettare ancora
                }
            }
            await UniTask.Yield(); // Aspetta un frame
        }
    }

    private void HandlePlayerStartDroppingGuest(Color colorID)
    {
        if (colorID == null)
        {
            DevLog.LogError("[Guest Controller]: Manca il Colore");
            return;
        }

        foreach (FollowerGuest guest in _activeGuests[colorID])
        {
            guest.CompleteMessageTask();
        }
    }


    private void HandlePlayerExitGrabArea(PlayerController player)
    {
        if (player == null || _lastSpawnedGuests == null) return;

        var followTarget = player.gameObject.transform;

        int runAwayTimer = _settings.GuestRunAwayTimer;
        for (int i = 0; i < _lastSpawnedGuests.Count; i++)
        {
            if (_lastSpawnedGuests[i] == null) continue;
            _lastSpawnedGuests[i].OnRunAway -= HandleRunAway;
            _lastSpawnedGuests[i].OnRunAway += HandleRunAway;
            _lastSpawnedGuests[i].SetUpTarget(followTarget, runAwayTimer);
            _lastSpawnedGuests[i].SetPlayer(player);
            followTarget = _lastSpawnedGuests[i].transform;
        }

        _isSpawnAreaFree = true;
        _lastSpawnedGuests.Clear();
        _lastSpawnedGuests = null;
        _guestInteractionArea.ResetArea();
    }

    private async void GuestArriveTimerStart(int time, CancellationToken cts)
    {
        int halfTime = (int)(time * 0.5);
        try
        {
            await UniTask.Delay(halfTime, cancellationToken: cts);
            _guestInteractionArea.HurryUp();

            await UniTask.Delay(halfTime, cancellationToken: cts);
            TaskFailed();
        }
        catch (OperationCanceledException)
        {

        }
    }

    private void CompleteGuestTask()
    {
        if (_cts == null) return;

        try
        {
            _cts.Cancel();
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
        }

        foreach (var obj in _lastSpawnedGuests)
        {
            obj.HurryUpEnd();
        }
    }

    private void HandleRunAway(Color colorID)
    {
        // 1. Spegniamo l'icona dell'Exit Area corrispondente
        if (_guestInteractionExitAreasDic.ContainsKey(colorID))
        {
            _guestInteractionExitAreasDic[colorID].DisableDirectionIcon();
        }

        // 2. Logica esistente di pulizia
        if (_activeGuests.ContainsKey(colorID))
        {
            foreach (FollowerGuest guest in _activeGuests[colorID])
            {
                guest.SetUpTarget(null);
                guest.gameObject.SetActive(false);
                _guestPool.Add(guest.gameObject);
            }
            _availableGuestsColor.Add(colorID);
            _activeGuests[colorID].Clear();
            _activeGuests.Remove(colorID);
        }
    }

    private void TaskFailed()
    {
        OnTaskFailed?.Invoke();
        //ANIMAZIONE DI USCITA PERSONAGGI

        foreach (FollowerGuest guest in _lastSpawnedGuests)
        {
            guest.SetUpTarget(null);
            guest.gameObject.SetActive(false);
            _guestPool.Add(guest.gameObject);
        }

        _isSpawnAreaFree = true;
        var tmpColor = _guestInteractionArea.MyInteractionColor;
        _lastSpawnedGuests.Clear();
        _lastSpawnedGuests = null;
        _availableGuestsColor.Add(tmpColor);
        _activeGuests[tmpColor].Clear();
        _guestInteractionArea.ResetArea();
        _activeGuests.Remove(tmpColor);
    }

    public void StopAllGuests()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        if (_guestInteractionArea != null)
        {
            _guestInteractionArea.OnAreaExit -= HandlePlayerExitGrabArea;
            _guestInteractionArea.OnStartInteract -= HandlePlayerGrabGuests;
            _guestInteractionArea.OnInteract -= HandleplayerStartInteract;
            _guestInteractionArea.OnHurryUP -= HandleHurryUP;
        }

        foreach (var exitArea in _guestInteractionExitAreas)
        {
            exitArea.OnCompleteInteractWithExit -= HandlePlayerDropGuest;
            exitArea.OnStartInteract -= HandlePlayerStartDroppingGuest;
        }

        _lastSpawnedGuests?.Clear();
    }
}