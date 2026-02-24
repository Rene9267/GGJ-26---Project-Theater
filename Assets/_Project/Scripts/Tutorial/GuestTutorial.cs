using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

struct TutorialExitInfo
{
    public Color color;
    public Sprite direction;
    public Sprite background;
}

public class GuestTutorial : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private GuestsSettings _settings;
    [SerializeField] private FollowerGuestInteractionArea _guestInteractionArea;
    [SerializeField] private List<FollowerGuestExitArea> _guestInteractionExitAreas = new();
    [SerializeField] private Transform _guestSpwanTransform;

    [Header("Audio")]
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip _spawnClip;
    [SerializeField] private AudioClip _arriveClip;

    private Dictionary<Color, FollowerGuestExitArea> _guestInteractionExitAreasDic = new();
    private List<FollowerGuest> _spawnedGuests = new List<FollowerGuest>();
    private UniTaskCompletionSource _tutorialCompletionSource;

    private int _exitLayerIndex;
    private int _defaultGuestLayer;
    private Color _currentSpawnColor;

    private void Start()
    {
        _exitLayerIndex = LayerMask.NameToLayer("ExitingGuest");

        if (_settings.GuestsPool.Count > 0 && _settings.GuestsPool[0] != null)
        {
            _defaultGuestLayer = _settings.GuestsPool[0].layer;
        }
    }

    public async UniTask StartGuestTutorial()
    {
        _tutorialCompletionSource = new UniTaskCompletionSource();

        // 1. Setup di tutte le aree di uscita (Logica presa dal GuestController originale)
        List<TutorialExitInfo> tmpList = new();
        for (int i = 0; i < _settings.GuestsColors.Count; i++)
        {
            tmpList.Add(new TutorialExitInfo
            {
                color = _settings.GuestsColors[i],
                background = _settings.ExitAreaBaseImages[i],
                direction = _settings.ExitAreaDirectionImages[i]
            });
        }

        // Mischia gli elementi per assegnarli in modo randomico
        System.Random rng = new System.Random();
        int n = tmpList.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = tmpList[k];
            tmpList[k] = tmpList[n];
            tmpList[n] = value;
        }

        Queue<TutorialExitInfo> shuffledQueue = new Queue<TutorialExitInfo>(tmpList);

        foreach (FollowerGuestExitArea exitArea in _guestInteractionExitAreas)
        {
            if (shuffledQueue.Count > 0)
            {
                var exitInfo = shuffledQueue.Dequeue();
                exitArea.SetUpInteractionArea(exitInfo.color, exitInfo.direction, exitInfo.background);
                exitArea.OnCompleteInteractWithExit += HandlePlayerDropGuest;
                _guestInteractionExitAreasDic.Add(exitInfo.color, exitArea);
            }
        }

        // 2. Scegliamo un colore randomico tra quelli disponibili in _settings per questo spawn
        _currentSpawnColor = _settings.GuestsColors[Random.Range(0, _settings.GuestsColors.Count)];

        // 3. Setup dell'Area di Interazione (Spawn)
        _guestInteractionArea.ResetArea();
        _guestInteractionArea.SetUpInteractionArea(_currentSpawnColor, _settings.RotationSpeed, _settings.Clockwise);
        _guestInteractionArea.OnStartInteract += HandlePlayerGrabGuests;
        _guestInteractionArea.OnAreaExit += HandlePlayerExitGrabArea;

        // 4. Spawna i Guest senza timer
        SpawnGuests();

        // 5. Attendi che il giocatore completi il task
        await _tutorialCompletionSource.Task;
    }

    private void SpawnGuests()
    {
        int guestCount = Random.Range((int)_settings.GuestSpawnRange.x, (int)_settings.GuestSpawnRange.y);
        Vector2 crowdMiddlePoint = Vector2.zero;

        for (int i = 0; i < guestCount; i++)
        {
            Vector2 randomPoint2D = Random.insideUnitCircle * _settings.Radius;
            Vector3 spawnPosition = new Vector3(randomPoint2D.x, 0, randomPoint2D.y) + _guestInteractionArea.transform.position;

            GameObject guestPrefab = _settings.GuestsPool[Random.Range(0, _settings.GuestsPool.Count)];
            GameObject guestObj = Instantiate(guestPrefab, spawnPosition, Quaternion.identity, _guestSpwanTransform);
            guestObj.SetActive(true);

            if (guestObj.TryGetComponent<FollowerGuest>(out var followerComp))
            {
                followerComp.SetMyColor(_currentSpawnColor);
                _spawnedGuests.Add(followerComp);
            }

            crowdMiddlePoint += new Vector2(spawnPosition.x, spawnPosition.z);
        }

        if (_spawnedGuests.Count > 0)
        {
            crowdMiddlePoint /= _spawnedGuests.Count;
            _guestInteractionArea.transform.position = new Vector3(crowdMiddlePoint.x, _guestInteractionArea.transform.position.y, crowdMiddlePoint.y);
        }

        if (_source != null && _spawnClip != null)
        {
            _source.PlayOneShot(_spawnClip);
        }
    }

    private void HandlePlayerGrabGuests()
    {
        if (_guestInteractionExitAreasDic.ContainsKey(_currentSpawnColor))
        {
            _guestInteractionExitAreasDic[_currentSpawnColor].EnableDirectionIcon();
        }
    }

    private void HandlePlayerExitGrabArea(PlayerController player)
    {
        if (player == null) return;

        Transform followTarget = player.transform;

        foreach (var guest in _spawnedGuests)
        {
            // Impostiamo un timer altissimo (es. 9999 secondi) per simulare l'assenza di timer nel tutorial
            guest.SetUpTarget(followTarget, 9999);
            guest.SetPlayer(player);
            followTarget = guest.transform;
        }

        _guestInteractionArea.ResetArea();
    }

    private void HandlePlayerDropGuest(Color colorID, Transform alignPoint, Transform exitPoint)
    {
        // Controlla che il colore droppato sia quello che abbiamo spawnato
        if (colorID != _currentSpawnColor) return;

        if (_source != null && _arriveClip != null)
        {
            _source.PlayOneShot(_arriveClip);
        }

        ExitSequenceRoutine(_spawnedGuests, alignPoint, exitPoint).Forget();

        // Pulizia eventi
        foreach (var exitArea in _guestInteractionExitAreas)
        {
            exitArea.OnCompleteInteractWithExit -= HandlePlayerDropGuest;
        }
        _guestInteractionArea.OnStartInteract -= HandlePlayerGrabGuests;
        _guestInteractionArea.OnAreaExit -= HandlePlayerExitGrabArea;

        // Segnaliamo il completamento del task al Tutorial_GameController
        _tutorialCompletionSource.TrySetResult();
    }

    private async UniTaskVoid ExitSequenceRoutine(List<FollowerGuest> guests, Transform alignPoint, Transform exitPoint)
    {
        foreach (var guest in guests)
        {
            guest.SetPlayer(null);
            SetLayerRecursively(guest.gameObject, _exitLayerIndex);

            if (guest.TryGetComponent<FollowerGuestMovement>(out var movement))
            {
                movement.SetExitMode(true);
                movement.LocalOffset = Vector3.zero;
            }

            guest.SetUpTarget(alignPoint);
        }

        // Attendiamo che raggiungano il punto di allineamento
        await UniTask.Delay(1500);

        foreach (var guest in guests)
        {
            guest.SetUpTarget(exitPoint);
        }

        // Attendiamo che superino la porta
        await UniTask.Delay(1500);

        // Nel tutorial possiamo semplicemente distruggerli una volta usciti dallo schermo
        foreach (var guest in guests)
        {
            if (guest != null) Destroy(guest.gameObject);
        }

        guests.Clear();
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (newLayer == -1) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
