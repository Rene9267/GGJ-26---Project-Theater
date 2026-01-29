using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Cysharp.Threading.Tasks.Internal;
using Cysharp.Threading.Tasks;
using System.Threading;

public class Crowd : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private CrowdInteractionArea _crowdInteractionArea;

    [Header("Core References")]
    [SerializeField] private CrowdSettings _crowdSettings;
    [SerializeField] private Transform _guestFather;
    private bool IsReciverOrSender = false;
    private readonly List<GameObject> _crowdMembers = new();
    private Coroutine _uiRotationCoroutine;
    private Vector2 _crowdMiddlePoint;
    public event Action<Color> OnInteracionComplete;
    public event Action<Color> OnTaskFailed;

    private Color _myColor;
    private CancellationTokenSource _cts;

    void OnDisable()
    {
        if (_crowdInteractionArea != null)
        {
            _crowdInteractionArea.OnCompleteInteract -= HandleInteraction;
            _crowdInteractionArea.OnMessageTake -= CompleteMessageTask;
        }
    }

    private void HandleInteraction()
    {
        OnInteracionComplete?.Invoke(_myColor);
    }

    public void ResetCrowd()
    {
        _crowdInteractionArea.OnCompleteInteract -= HandleInteraction;
        _crowdInteractionArea.OnMessageTake -= CompleteMessageTask;
        if (_uiRotationCoroutine != null)
        {
            StopCoroutine(_uiRotationCoroutine);
            _uiRotationCoroutine = null;
        }

        _myColor = Color.clear;
        IsReciverOrSender = false;

        if (_crowdInteractionArea != null)
        {
            _crowdInteractionArea.ResetArea(); // Reset interno dell'area
            _crowdInteractionArea.gameObject.SetActive(false); // Spegne l'oggetto
        }
    }

    public void SpawnGuests()
    {
        if (_crowdSettings.SpawnableGuest == null || _crowdSettings.SpawnableGuest.Count == 0)
        {

#if UNITY_EDITOR
            Debug.LogError("No spawnable guests available in CrowdSettings.");
#endif
            return;
        }

        int crowdSize = Random.Range((int)_crowdSettings.CrowdSize.x, (int)_crowdSettings.CrowdSize.y);

        for (int i = 0; i < crowdSize; i++)
        {
            bool foundValidSpot = false;
            int attempts = 0;

            while (!foundValidSpot && attempts < _crowdSettings.MaxAttemptsPerPawn)
            {
                attempts++;
                //Posizione casuale all'interno del cerchio
                Vector2 randomPoint2D = Random.insideUnitCircle * _crowdSettings.Radius;
                Vector3 spawnPosition = new Vector3(randomPoint2D.x, 0, randomPoint2D.y) + _guestFather.position;

                if (!Physics.CheckSphere(spawnPosition, _crowdSettings.SecurityRadiusCheck, _crowdSettings.ObstacleLayer))
                {
                    //Seleziona un guest casuale dalla lista
                    int randomIndex = Random.Range(0, _crowdSettings.SpawnableGuest.Count);

                    //Istanzia il guest
                    GameObject guest = Instantiate(_crowdSettings.SpawnableGuest[randomIndex], spawnPosition, Quaternion.identity, _guestFather);

                    _crowdMiddlePoint += new Vector2(spawnPosition.x, spawnPosition.z);

                    _crowdMembers.Add(guest);
                    foundValidSpot = true;
                }
            }
            if (!foundValidSpot)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Impossibile trovare un posto per lo spettatore {i} dopo {_crowdSettings.MaxAttemptsPerPawn} tentativi.");
#endif
            }
        }

        _crowdMiddlePoint /= crowdSize;
        _crowdInteractionArea.transform.position = new Vector3(_crowdMiddlePoint.x, _crowdInteractionArea.transform.position.y, _crowdMiddlePoint.y);
    }

    public void EnableSender(Color CircleColor)
    {
        _cts = new CancellationTokenSource();

        _crowdInteractionArea.gameObject.SetActive(true);
        _crowdInteractionArea.SetUPInteractionArea(CircleColor, InteractType.MessageSender);
        _myColor = CircleColor;

        if (_uiRotationCoroutine != null)
        {
            StopCoroutine(_uiRotationCoroutine);
            _uiRotationCoroutine = null;
        }

        _uiRotationCoroutine = StartCoroutine(_crowdInteractionArea.AreaImageRotate(_crowdSettings.RotationSpeed, _crowdSettings.Clockwise));
        _crowdInteractionArea.OnCompleteInteract -= HandleInteraction;
        _crowdInteractionArea.OnMessageTake -= CompleteMessageTask;
        _crowdInteractionArea.OnCompleteInteract += HandleInteraction;
        _crowdInteractionArea.OnMessageTake += CompleteMessageTask;

        MessageTimeStart(5000, _cts.Token);
    }

    public void EnableReciver(Color CircleColor)
    {
        _crowdInteractionArea.gameObject.SetActive(true);
        _crowdInteractionArea.SetUPInteractionArea(CircleColor, InteractType.MessageReciver);
        _myColor = CircleColor;
        if (_uiRotationCoroutine != null)
        {
            StopCoroutine(_uiRotationCoroutine);
            _uiRotationCoroutine = null;
        }

        _uiRotationCoroutine = StartCoroutine(_crowdInteractionArea.AreaImageRotate(_crowdSettings.RotationSpeed, _crowdSettings.Clockwise));
        _crowdInteractionArea.OnCompleteInteract -= HandleInteraction;
        _crowdInteractionArea.OnCompleteInteract += HandleInteraction;
    }

    private async void MessageTimeStart(int time, CancellationToken cts)
    {
        int halfTime = (int)(time * 0.5);
        try
        {
            await UniTask.Delay(halfTime, cancellationToken: cts);
            _crowdInteractionArea.HurryUp();

            await UniTask.Delay(halfTime, cancellationToken: cts);
            TaskFailed();
        }
        catch (OperationCanceledException)
        {

        }
    }

    private void TaskFailed()
    {
        OnTaskFailed?.Invoke(_myColor);
        StopInteraction();
    }
    
    private void CompleteMessageTask()
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
    }

    public void StopInteraction()
    {
        if (_uiRotationCoroutine != null)
        {
            StopCoroutine(_uiRotationCoroutine);
            _uiRotationCoroutine = null;
        }

        _crowdInteractionArea.gameObject.SetActive(false);
    }


    //===== DEBUG =====
    private void OnDrawGizmos()
    {
        if (_crowdSettings == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _crowdSettings.Radius);
    }
}
