using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

struct ExitInfo
{
    public Color color;
    public Sprite direction;
    public Sprite background;
}

public class GuestController : MonoBehaviour
{
    #region Parameters
    [SerializeField] private GuestsSettings _settings;
    [SerializeField] private FollowerGuestInteractionArea _guestInteractionArea;
    [SerializeField] private List<FollowerGuestExitArea> _guestInteractionExitAreas = new();
    [SerializeField] private Transform _guestSpwanTransform;

    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip _spawnClip;
    [SerializeField] private AudioClip _arriveClip;

    [Header("Run Away / Bad Exit Settings")]
    [Tooltip("Il punto davanti alla porta generica/ingresso dove si allineano prima di sparire")]
    [SerializeField] private Transform _runAwayAlignPoint;

    [Tooltip("Il punto finale fuori scena (dietro la porta generica)")]
    [SerializeField] private Transform _runAwayExitPoint;

    [Tooltip("Il punto fisso fuori mappa (es. la porta) da cui partono i guest")]
    [SerializeField] private Transform _entranceDoorPoint;

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
    private int _totalGuestsSpawned;
    public int TotalGuestsSpawned => _totalGuestsSpawned;
    public event Action<int> OnGuestDropped;
    private CancellationTokenSource _cts;
    public event Action<int> OnTaskFailed;

    #endregion

    #region Unity Methods
    void OnValidate()
    {
        if (_settings == null || _settings.GuestsPool == null || _settings.GuestsPool.Count <= 0)
        {
            DevLog.LogWarning("[Guest Controller]: Impostazioni o Pool di elementi assente nell'Inspector.");
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
        DevLog.LogWarning($"[GuestController]: Layer di uscita '{_exitLayerName}' ha indice {_exitLayerIndex}. Assicurati che sia corretto e che il layer esista.");
        
        if (_settings.GuestsPool.Count > 0 && _settings.GuestsPool[0] != null)
        {
            _defaultGuestLayer = _settings.GuestsPool[0].layer;
        }
    }

    #endregion  

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
        if (_guestPool.Count == 0) return;

        _crowdMiddlePoint = Vector2.zero;
        int randomGuestNumber = Random.Range((int)_settings.GuestSpawnRange.x, (int)_settings.GuestSpawnRange.y);
        int actualSpawnedCount = 0;

        List<FollowerGuest> guestsToEnter = new List<FollowerGuest>();
        List<Vector3> targetRandomPositions = new List<Vector3>();

        for (int i = 0; i < randomGuestNumber; i++)
        {
            if (_guestPool.Count == 0) break;

            bool foundValidSpot = false;
            int attempts = 0;
            float minDistanceBetweenGuests = 1.2f;

            while (!foundValidSpot && attempts < _settings.MaxAttemptsPerPawn)
            {
                attempts++;
                Vector2 randomPoint2D = Random.insideUnitCircle * _settings.Radius;
                Vector3 finalRandomPos = new Vector3(randomPoint2D.x, 0, randomPoint2D.y) + _guestInteractionArea.transform.position;

                bool isTooCloseToOthers = false;
                foreach (var existingPos in targetRandomPositions)
                {
                    if (Vector3.Distance(finalRandomPos, existingPos) < minDistanceBetweenGuests)
                    {
                        isTooCloseToOthers = true;
                        break;
                    }
                }

                if (!isTooCloseToOthers && !Physics.CheckSphere(finalRandomPos, _settings.SecurityRadiusCheck, _settings.ObstacleLayer))
                {
                    int randomIndex = Random.Range(0, _guestPool.Count);
                    var tmpGuest = _guestPool[randomIndex];

                    tmpGuest.transform.SetPositionAndRotation(_entranceDoorPoint.position, Quaternion.identity);
                    tmpGuest.SetActive(true);

                    var followerComp = tmpGuest.GetComponent<FollowerGuest>();
                    if (followerComp != null)
                    {
                        _activeGuests[color].Add(followerComp);
                        followerComp.SetMyColor(color);
                        guestsToEnter.Add(followerComp);

                        targetRandomPositions.Add(finalRandomPos);
                    }

                    _guestPool.RemoveAt(randomIndex);
                    _crowdMiddlePoint += new Vector2(finalRandomPos.x, finalRandomPos.z);
                    actualSpawnedCount++;
                    foundValidSpot = true;
                }
            }
        }

        _source.PlayOneShot(_spawnClip);

        if (actualSpawnedCount > 0)
        {
            _totalGuestsSpawned += actualSpawnedCount;
            _isSpawnAreaFree = false;
            _crowdMiddlePoint /= actualSpawnedCount;
            _lastSpawnedGuests = new List<FollowerGuest>(_activeGuests[color]);

            _guestInteractionArea.transform.position = new Vector3(_crowdMiddlePoint.x, _guestInteractionArea.transform.position.y, _crowdMiddlePoint.y);

            EnterSequenceRoutine(guestsToEnter, targetRandomPositions, color).Forget();
        }
    }

    private async UniTaskVoid EnterSequenceRoutine(List<FollowerGuest> guests, List<Vector3> targetPositions, Color color)
    {
        List<Transform> temporaryTargets = new List<Transform>();

        for (int i = 0; i < guests.Count; i++)
        {
            if (_exitLayerIndex != -1)
            {
                SetLayerRecursively(guests[i].gameObject, _exitLayerIndex);
            }

            if (guests[i].TryGetComponent<FollowerGuestMovement>(out var movement))
            {
                movement.SetExitMode(true);
                movement.LocalOffset = Vector3.zero;
            }

            GameObject tempTarget = new GameObject($"TempTarget_Enter_{i}");
            tempTarget.transform.position = targetPositions[i];
            temporaryTargets.Add(tempTarget.transform);

            guests[i].SetUpTarget(tempTarget.transform);
        }

        await WaitForGuestsToReachTarget(guests, temporaryTargets, 1.0f);

        for (int i = 0; i < guests.Count; i++)
        {
            guests[i].transform.rotation = Quaternion.Euler(0, guests[i].transform.rotation.eulerAngles.y, 0);

            guests[i].SetUpTarget(null);
            Destroy(temporaryTargets[i].gameObject);


            if (guests[i].TryGetComponent<FollowerGuestMovement>(out var movement))
            {
                movement.SetExitMode(false);
                movement.LocalOffset = new Vector3(0, 1.2f, -1.8f);
            }

            SetLayerRecursively(guests[i].gameObject, _defaultGuestLayer);
        }

        _guestInteractionArea.ResetArea();
        _guestInteractionArea.SetUpInteractionArea(color, _settings.RotationSpeed, _settings.Clockwise);
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
            DevLog.LogError("[Guest Controller]: Manca il Colore");
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
                movement.SetExitMode(true);
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
                movement.SetExitMode(false);
                movement.LocalOffset = new Vector3(0, 1.2f, -1.8f);
            }

            SetLayerRecursively(guest.gameObject, _defaultGuestLayer);

            guest.SetUpTarget(null);
            guest.gameObject.SetActive(false);
            _guestPool.Add(guest.gameObject);
        }
    }

    private async UniTask WaitForGuestsToReachTarget(List<FollowerGuest> guests, List<Transform> targets, float threshold)
    {
        bool allArrived = false;
        float timeout = 8f;
        float timer = 0f;

        while (!allArrived && timer < timeout)
        {
            timer += Time.deltaTime;
            allArrived = true;

            for (int i = 0; i < guests.Count; i++)
            {
                if (guests[i] == null || !guests[i].gameObject.activeSelf) continue;

                float dist = Vector3.Distance(guests[i].transform.position, targets[i].position);
                if (dist > threshold)
                {
                    allArrived = false;
                    break;
                }
            }
            await UniTask.Yield();
        }
    }

    private async UniTask WaitForGuestsToReachTarget(List<FollowerGuest> guests, Transform singleTarget, float threshold)
    {
        bool allArrived = false;
        float timeout = 10f;
        float timer = 0f;

        while (!allArrived && timer < timeout)
        {
            timer += Time.deltaTime;
            allArrived = true;

            for (int i = 0; i < guests.Count; i++)
            {
                if (guests[i] == null || !guests[i].gameObject.activeSelf) continue;

                float dist = Vector3.Distance(guests[i].transform.position, singleTarget.position);
                if (dist > threshold)
                {
                    allArrived = false;
                    break;
                }
            }
            await UniTask.Yield();
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
        if (_cts != null)
        {
            try
            {
                _cts.Cancel();
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
            }
        }

        if (_lastSpawnedGuests != null)
        {
            foreach (var obj in _lastSpawnedGuests)
            {
                if (obj != null)
                {
                    obj.HurryUpEnd();
                }
            }
        }
    }

    private void HandleRunAway(Color colorID)
    {
        if (_guestInteractionExitAreasDic.ContainsKey(colorID))
        {
            _guestInteractionExitAreasDic[colorID].DisableDirectionIcon();
        }

        if (_runAwayAlignPoint == null || _runAwayExitPoint == null)
        {
            DevLog.LogError("[GuestController] Mancano i punti _runAwayAlignPoint o _runAwayExitPoint nell'inspector!");
            return;
        }

        if (_activeGuests.ContainsKey(colorID))
        {
            List<FollowerGuest> guestsToExit = new List<FollowerGuest>(_activeGuests[colorID]);

            _activeGuests[colorID].Clear();
            _activeGuests.Remove(colorID);
            _availableGuestsColor.Add(colorID);

            ExitSequenceRoutine(guestsToExit, _runAwayAlignPoint, _runAwayExitPoint).Forget();
        }
    }

    private void TaskFailed()
    {
        int lostGuestsCount = _lastSpawnedGuests != null ? _lastSpawnedGuests.Count : 0;
        OnTaskFailed?.Invoke(lostGuestsCount);

        if (_lastSpawnedGuests != null && _lastSpawnedGuests.Count > 0)
        {
            List<FollowerGuest> guestsToExit = new List<FollowerGuest>(_lastSpawnedGuests);
            ExitSequenceRoutine(guestsToExit, _runAwayAlignPoint, _runAwayExitPoint).Forget();
        }

        _isSpawnAreaFree = true;
        var tmpColor = _guestInteractionArea.MyInteractionColor;

        if (_lastSpawnedGuests != null)
        {
            _lastSpawnedGuests.Clear();
            _lastSpawnedGuests = null;
        }

        _availableGuestsColor.Add(tmpColor);

        if (_activeGuests.ContainsKey(tmpColor))
        {
            _activeGuests[tmpColor].Clear();
            _activeGuests.Remove(tmpColor);
        }

        _guestInteractionArea.ResetArea();
    }

    public void StopAllGuests()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        // Modifica: invece di nascondere i GameObject, li mettiamo in idle
        foreach (var obj in _activeGuests)
        {
            foreach (var ele in obj.Value)
            {
                ele.SetIdleState();
            }
        }

        if (_guestInteractionArea != null)
        {
            _guestInteractionArea.ResetArea();
            _guestInteractionArea.OnAreaExit -= HandlePlayerExitGrabArea;
            _guestInteractionArea.OnStartInteract -= HandlePlayerGrabGuests;
            _guestInteractionArea.OnInteract -= HandleplayerStartInteract;
            _guestInteractionArea.OnHurryUP -= HandleHurryUP;
        }

        foreach (var exitArea in _guestInteractionExitAreas)
        {
            exitArea.DisableDirectionIcon();
            exitArea.OnCompleteInteractWithExit -= HandlePlayerDropGuest;
            exitArea.OnStartInteract -= HandlePlayerStartDroppingGuest;
        }

        _lastSpawnedGuests?.Clear();
    }
}