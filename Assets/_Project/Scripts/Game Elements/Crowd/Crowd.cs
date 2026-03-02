using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Cysharp.Threading.Tasks;
using System.Threading;

public class Crowd : MonoBehaviour
{
    #region  Parameters
    [Header("Interaction")]
    public CrowdInteractionArea _crowdInteractionArea;

    [Header("Core References")]
    [SerializeField] private CrowdSettings _crowdSettings;
    [SerializeField] private Transform _guestFather;
    [SerializeField] private Animation _myAnimation;

    public AudioClip TaskFailedClip;
    public AudioClip TaskSuccessClip;
    public AudioSource MessageSource;

    private bool IsReciverOrSender = false;
    private readonly List<StaticGuest_Controller> _crowdMembers = new();
    private Coroutine _uiRotationCoroutine;
    private Vector2 _crowdMiddlePoint;
    public event Action<Color> OnInteracionComplete;
    public event Action<Color> OnTaskFailed;

    private Color _myColor;
    private CancellationTokenSource _cts;
    private readonly string _letterSpawnName = "AC_LetterSpawn";
    private readonly string _letterTaskFail = "AC_MessageFail";
    private readonly string _letterTaskGet = "AC_GetMessage";
    private Crowd _myReciver;

    #endregion

    #region Unity Methods
    void OnDisable()
    {
        if (_crowdInteractionArea != null)
        {
            _crowdInteractionArea.OnCompleteInteract -= HandleInteraction;
            _crowdInteractionArea.OnMessageTake -= CompleteMessageTask;
        }
        if (_crowdMembers != null)
        {
            foreach (var obj in _crowdMembers)
            {
                obj.StopHurry();
            }
        }
    }

    #endregion

    #region Class Methods
    private void HandleInteraction()
    {
        _myAnimation.Play(_letterTaskGet);
        MessageSource.PlayOneShot(TaskSuccessClip);
        _crowdInteractionArea.gameObject.SetActive(false);
        _crowdInteractionArea.IconController.gameObject.SetActive(false);

        OnInteracionComplete?.Invoke(_myColor);
    }

    public void ResetCrowd()
    {
        if (_crowdInteractionArea != null)
        {
            _crowdInteractionArea.OnCompleteInteract -= HandleInteraction;
            _crowdInteractionArea.OnMessageTake -= CompleteMessageTask;
            _crowdInteractionArea.OnHurryUp -= HandleHurryup;
        }

        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        if (_uiRotationCoroutine != null)
        {
            StopCoroutine(_uiRotationCoroutine);
            _uiRotationCoroutine = null;
        }

        // --- INIZIO MODIFICHE: Interruzione animazioni e suoni in corso ---
        if (_myAnimation != null) _myAnimation.Stop();
        if (MessageSource != null) MessageSource.Stop();
        // --- FINE MODIFICHE ---

        _myColor = Color.clear;
        IsReciverOrSender = false;
        _myReciver = null;

        if (_crowdInteractionArea != null)
        {
            _crowdInteractionArea.ResetArea();
            _crowdInteractionArea.gameObject.SetActive(false);
        }

        foreach (var obj in _crowdMembers)
        {
            obj.StopHurry();
        }
    }

    public void SpawnGuests()
    {
        if (_crowdSettings.SpawnableGuest == null || _crowdSettings.SpawnableGuest.Count == 0)
        {

            DevLog.LogError("No spawnable guests available in CrowdSettings.");
            return;
        }

        int crowdSize = Random.Range((int)_crowdSettings.CrowdSize.x, (int)_crowdSettings.CrowdSize.y);

        int actualCrowd = 0;

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
                    Quaternion pawnRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                    GameObject guest = Instantiate(_crowdSettings.SpawnableGuest[randomIndex], spawnPosition, pawnRotation, _guestFather);

                    _crowdMiddlePoint += new Vector2(spawnPosition.x, spawnPosition.z);
                    actualCrowd++;
                    _crowdMembers.Add(guest.GetComponent<StaticGuest_Controller>());
                    foundValidSpot = true;
                }
            }
            if (!foundValidSpot)
            {
                DevLog.LogWarning($"Impossibile trovare un posto per lo spettatore {i} dopo {_crowdSettings.MaxAttemptsPerPawn} tentativi.");
            }
        }

        _crowdMiddlePoint /= actualCrowd;
        _crowdInteractionArea.transform.position = new Vector3(_crowdMiddlePoint.x, _crowdInteractionArea.transform.position.y, _crowdMiddlePoint.y);
    }

    public void EnableSender(Color CircleColor, Crowd myReciver)
    {
        _cts = new CancellationTokenSource();
        _myReciver = myReciver;

        _myAnimation.Play(_letterSpawnName);

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
        _crowdInteractionArea.OnCompleteInteract += HandleInteraction;

        _crowdInteractionArea.OnMessageTake -= CompleteMessageTask;
        _crowdInteractionArea.OnMessageTake += CompleteMessageTask;

        _crowdInteractionArea.OnHurryUp -= HandleHurryup;
        _crowdInteractionArea.OnHurryUp += HandleHurryup;

        MessageTimeStart(_crowdSettings.MessageTimeSetting, _cts.Token);
    }

    private void HandleHurryup()
    {
        foreach (var obj in _crowdMembers)
        {
            obj.HurryUp();
        }
    }

    public void EnableReciver(Color CircleColor)
    {
        _crowdInteractionArea.gameObject.SetActive(false);

        _myColor = CircleColor;

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
        MessageSource.PlayOneShot(TaskFailedClip);
        _myAnimation.Play(_letterTaskFail);

        OnTaskFailed?.Invoke(_myColor);
        _myReciver = null;
        StopInteraction();
    }

    public void ActivateReciverIcon(Crowd Sender)
    {
        _crowdInteractionArea.gameObject.SetActive(true);

        _crowdInteractionArea.SetUPInteractionArea(_myColor, InteractType.MessageReciver);

        if (_uiRotationCoroutine != null)
        {
            StopCoroutine(_uiRotationCoroutine);
            _uiRotationCoroutine = null;
        }
        _uiRotationCoroutine = StartCoroutine(_crowdInteractionArea.AreaImageRotate(_crowdSettings.RotationSpeed, _crowdSettings.Clockwise));

        _crowdInteractionArea.IconController.SelectReciverIcon(Sender._crowdInteractionArea.IconController.iconIndex);
        _myAnimation.Play(_letterSpawnName);
    }

    private void CompleteMessageTask()
    {
        if (_cts == null) return;

        _myAnimation.Play(_letterTaskGet);

        _crowdInteractionArea.gameObject.SetActive(false);

        _myReciver.ActivateReciverIcon(this);
        foreach (var obj in _crowdMembers)
        {
            obj.StopHurry();
        }

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
        _crowdInteractionArea.IconController.gameObject.SetActive(false);
    }

    #endregion

    #region Gizmos
    //===== DEBUG =====
    private void OnDrawGizmos()
    {
        if (_crowdSettings == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _crowdSettings.Radius);
    }
    #endregion
}