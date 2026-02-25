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
    [SerializeField] private AudioSource _smokeParticlesAudioSource;
    [SerializeField] private AudioClip _spawnClip;
    [SerializeField] private AudioClip _arriveClip;

    [Header("VFX")]
    [Tooltip("Inserisci qui il Particle System per l'effetto di completamento (nella scena o prefab)")]
    [SerializeField] private ParticleSystem _completionParticles;

    private Dictionary<Color, FollowerGuestExitArea> _guestInteractionExitAreasDic = new();
    private List<FollowerGuestTutorial> _spawnedGuests = new List<FollowerGuestTutorial>();
    private UniTaskCompletionSource _tutorialCompletionSource;

    private int _exitLayerIndex;
    private int _defaultGuestLayer;
    private Color _currentSpawnColor;

    private void Start()
    {
        _exitLayerIndex = LayerMask.NameToLayer("ExitingGuest");

        // Leggiamo il layer direttamente dal primo prefab nei Settings
        if (_settings != null && _settings.GuestsPool.Count > 0 && _settings.GuestsPool[0] != null)
        {
            _defaultGuestLayer = _settings.GuestsPool[0].layer;
        }

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
    }

    public async UniTask StartGuestTutorial()
    {
        _tutorialCompletionSource = new UniTaskCompletionSource();

        _currentSpawnColor = _settings.GuestsColors[Random.Range(0, _settings.GuestsColors.Count)];

        _guestInteractionArea.ResetArea();
        _guestInteractionArea.SetUpInteractionArea(_currentSpawnColor, _settings.RotationSpeed, _settings.Clockwise);
        _guestInteractionArea.OnStartInteract += HandlePlayerGrabGuests;
        _guestInteractionArea.OnAreaExit += HandlePlayerExitGrabArea;

        SpawnGuests();

        await _tutorialCompletionSource.Task;
    }

    private void SpawnGuests()
    {
        if (_settings.GuestsPool == null || _settings.GuestsPool.Count == 0)
        {
            DevLog.LogError("[Tutorial_GuestController]: GuestsPool è vuoto nei Settings!");
            return;
        }

        int guestCount = Random.Range((int)_settings.GuestSpawnRange.x, (int)_settings.GuestSpawnRange.y);
        Vector2 crowdMiddlePoint = Vector2.zero;

        for (int i = 0; i < guestCount; i++)
        {
            Vector2 randomPoint2D = Random.insideUnitCircle * _settings.Radius;
            Vector3 spawnPosition = new Vector3(randomPoint2D.x, 0, randomPoint2D.y) + _guestInteractionArea.transform.position;

            // Spawna direttamente dai Settings
            GameObject guestPrefab = _settings.GuestsPool[Random.Range(0, _settings.GuestsPool.Count)];
            GameObject guestObj = Instantiate(guestPrefab, spawnPosition, Quaternion.identity, _guestSpwanTransform);
            guestObj.SetActive(true);

            // Cerca il TUO script custom che hai messo sui prefab del tutorial
            if (guestObj.TryGetComponent<FollowerGuestTutorial>(out var followerComp))
            {
                _spawnedGuests.Add(followerComp);
            }
            else
            {
                DevLog.LogWarning($"Il prefab {guestObj.name} non ha il componente FollowerGuestTutorial!");
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
            guest.SetUpTarget(followTarget);
            guest.SetPlayer(player);
            followTarget = guest.transform;
        }

        _guestInteractionArea.ResetArea();
    }

    private async void HandlePlayerDropGuest(Color colorID, Transform alignPoint, Transform exitPoint)
    {
        if (colorID != _currentSpawnColor) return;

        if (_source != null && _arriveClip != null)
        {
            _source.PlayOneShot(_arriveClip);
        }

        // Calcolo del punto medio per il particellare
        Vector3 middlePoint = Vector3.zero;
        if (_spawnedGuests.Count > 0)
        {
            foreach (var guest in _spawnedGuests)
            {
                middlePoint += guest.transform.position;
            }
            middlePoint /= _spawnedGuests.Count;

            // Spostiamo e facciamo partire il particellare
            if (_completionParticles != null)
            {
                _completionParticles.transform.position = middlePoint;
                _completionParticles.gameObject.SetActive(true);
                _smokeParticlesAudioSource.Play();
                await UniTask.Delay(200);
                _completionParticles.Play();
            }

            // Disattiviamo e distruggiamo i guest istantaneamente
            foreach (var guest in _spawnedGuests)
            {
                if (guest != null)
                {
                    guest.gameObject.SetActive(false);
                    Destroy(guest.gameObject);
                }
            }
            _spawnedGuests.Clear();
        }

        // Pulizia Eventi
        foreach (var exitArea in _guestInteractionExitAreas)
        {
            exitArea.OnCompleteInteractWithExit -= HandlePlayerDropGuest;
        }
        _guestInteractionArea.OnStartInteract -= HandlePlayerGrabGuests;
        _guestInteractionArea.OnAreaExit -= HandlePlayerExitGrabArea;

        _tutorialCompletionSource.TrySetResult();
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