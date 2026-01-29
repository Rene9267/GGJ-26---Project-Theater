using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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
    [SerializeField] private GuestsSettings _settings;
    [SerializeField] private FollowerGuestInteractionArea _guestInteractionArea;
    [SerializeField] private List<FollowerGuestExitArea> _guestInteractionExitAreas = new();
    [SerializeField] private Transform _guestSpwanTransform;


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
        // Disiscrizione corretta dall'area di presa
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
                exitArea.OnCompleteInteract -= HandlePlayerDropGuest;
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

            exitArea.OnCompleteInteract += HandlePlayerDropGuest;
            exitArea.OnStartInteract += HandlePlayerStartDroppingGuest;

            _guestInteractionExitAreasDic.Add(ExitInfo.color, exitArea);
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
            Debug.LogWarning("[Guest Controller]: Pool vuoto, impossibile spawnare.");
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

    private void HandlePlayerDropGuest(Color colorID)
    {
        if (colorID == null)
        {
#if UNITY_EDITOR
            Debug.LogError("[Guest Controller]: Manca il Colore");
#endif
            return;
        }

        int guestDroppedCount = 0;

        if (_activeGuests.ContainsKey(colorID))
        {
            guestDroppedCount = _activeGuests[colorID].Count;
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

        OnGuestDropped?.Invoke(guestDroppedCount);
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
}