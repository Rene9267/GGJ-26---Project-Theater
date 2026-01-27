using System.Collections.Generic;
using UnityEngine;


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
            _guestInteractionArea.OnInteract -= HandlePlayerGrabGuest;
        }
        foreach (FollowerGuestExitArea exitArea in _guestInteractionExitAreas)
        {
            if (exitArea != null)
                exitArea.OnInteract -= HandlePlayerDropGuest;
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
        Queue<Color> shuffledQueue = new();
        ShuffleAndEnqueue(_availableGuestsColor, shuffledQueue);
        
        foreach (FollowerGuestExitArea exitArea in _guestInteractionExitAreas)
        {
            Color tmpColor = shuffledQueue.Dequeue();
            exitArea.SetUpInteractionArea(tmpColor);
            exitArea.OnInteract += HandlePlayerDropGuest;
            Debug.Log($"Creato elemento del dictionary: {tmpColor}, {exitArea}");
            _guestInteractionExitAreasDic.Add(tmpColor, exitArea);
        }
    }

    public void ShuffleAndEnqueue(List<Color> list, Queue<Color> queue)
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
        foreach (Color item in list)
        {
            queue.Enqueue(item);
        }
    }

    [ContextMenu("Genera Direction Manuale")]
    public void CreateGuestDirection()
    {
        if (_guestInteractionArea == null || _availableGuestsColor == null || _availableGuestsColor.Count == 0)
        {
            Debug.LogError("[Guest Controller]: Impossibile generare. Area mancante o Colori terminati.");
            return;
        }

        if (_isSpawnAreaFree == false) return;

        _guestInteractionArea.OnInteract -= HandlePlayerGrabGuest;

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

        _guestInteractionArea.OnInteract += HandlePlayerGrabGuest;
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

    private void HandlePlayerDropGuest(Color colorID)
    {
        if (colorID == null)
        {
#if UNITY_EDITOR
            Debug.LogError("[Guest Controller]: Manca il Colore");
#endif
            return;
        }

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

    private void HandlePlayerGrabGuest(Transform starterElement)
    {
        if (starterElement == null || _lastSpawnedGuests == null) return;

        var followTarget = starterElement;
        foreach (var guest in _lastSpawnedGuests)
        {
            if (guest == null) continue;
            guest.SetUpTarget(followTarget);
            followTarget = guest.transform;
        }

        if (_guestInteractionExitAreasDic.ContainsKey(_guestInteractionArea.MyInteractionColor))
        {
            Debug.Log("Il mio colore è attivo");
            _guestInteractionExitAreasDic[_guestInteractionArea.MyInteractionColor].EnableDirectionIcon();
        }

        _isSpawnAreaFree = true;
        _lastSpawnedGuests.Clear();
        _lastSpawnedGuests = null;
        _guestInteractionArea.ResetArea();

    }
}