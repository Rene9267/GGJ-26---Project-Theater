using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleCrowd : MonoBehaviour
{
    #region Parameters

    [Header("Interaction")]
    public CrowdInteractionArea _crowdInteractionArea;

    [Header("Core References")]
    [SerializeField] private CrowdSettings _crowdSettings;
    [SerializeField] private Transform _guestFather;
    [SerializeField] private Animation _myAnimation;
    [SerializeField] private SmokeController _smokeController;

    public AudioClip TaskSuccessClip;
    public AudioSource MessageSource;

    private readonly List<StaticGuest_Controller> _crowdMembers = new();
    private Coroutine _uiRotationCoroutine;
    private Vector2 _crowdMiddlePoint;
    public event Action OnInteracionComplete; 

    private Color _myColor;
    private readonly string _letterSpawnName = "AC_LetterSpawn";
    private readonly string _letterTaskGet = "AC_GetMessage";
    private SimpleCrowd _myReciver;

    #endregion

    #region Unity Methods
    void OnDisable()
    {
        if (_crowdInteractionArea != null)
        {
            _crowdInteractionArea.OnCompleteInteract -= HandleInteraction;
            _crowdInteractionArea.OnMessageTake -= CompleteMessageTask;
        }
    }

    void OnValidate()
    {
        if(_smokeController == null)
        {
            DevLog.LogError("SmokeController reference is missing in SimpleCrowd.");
        }
    }

    #endregion

    #region Class Methods
    public void SpawnGuests()
    {
        if (_crowdSettings.SpawnableGuest == null || _crowdSettings.SpawnableGuest.Count == 0) return;
        _smokeController.PlaySmokeEffect();

        int crowdSize = Random.Range((int)_crowdSettings.CrowdSize.x, (int)_crowdSettings.CrowdSize.y);
        int actualCrowd = 0;

        for (int i = 0; i < crowdSize; i++)
        {
            Vector2 randomPoint2D = Random.insideUnitCircle * _crowdSettings.Radius;
            Vector3 spawnPosition = new Vector3(randomPoint2D.x, 0, randomPoint2D.y) + _guestFather.position;

            if (!Physics.CheckSphere(spawnPosition, _crowdSettings.SecurityRadiusCheck, _crowdSettings.ObstacleLayer))
            {
                int randomIndex = Random.Range(0, _crowdSettings.SpawnableGuest.Count);
                Quaternion pawnRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                GameObject guest = Instantiate(_crowdSettings.SpawnableGuest[randomIndex], spawnPosition, pawnRotation, _guestFather);

                _crowdMiddlePoint += new Vector2(spawnPosition.x, spawnPosition.z);
                actualCrowd++;
                _crowdMembers.Add(guest.GetComponent<StaticGuest_Controller>());
            }
        }

        if (actualCrowd > 0)
        {
            _crowdMiddlePoint /= actualCrowd;
            _crowdInteractionArea.transform.position = new Vector3(_crowdMiddlePoint.x, _crowdInteractionArea.transform.position.y, _crowdMiddlePoint.y);
        }
    }

    public void EnableSender(Color circleColor, SimpleCrowd myReciver)
    {
        _myReciver = myReciver;
        _myAnimation.Play(_letterSpawnName);

        _crowdInteractionArea.gameObject.SetActive(true);
        _crowdInteractionArea.SetUPInteractionArea(circleColor, InteractType.MessageSender);
        _myColor = circleColor;

        if (_uiRotationCoroutine != null) StopCoroutine(_uiRotationCoroutine);
        _uiRotationCoroutine = StartCoroutine(_crowdInteractionArea.AreaImageRotate(_crowdSettings.RotationSpeed, _crowdSettings.Clockwise));

        _crowdInteractionArea.OnMessageTake -= CompleteMessageTask;
        _crowdInteractionArea.OnMessageTake += CompleteMessageTask;

    }

    private void CompleteMessageTask()
    {
        _myAnimation.Play(_letterTaskGet);
        _crowdInteractionArea.gameObject.SetActive(false); 

        _myReciver.ActivateReciverIcon(this);
    }

    public void EnableReciver(Color circleColor)
    {
        _crowdInteractionArea.gameObject.SetActive(true);
        _crowdInteractionArea.SetUPInteractionArea(circleColor, InteractType.MessageReciver);
        _myColor = circleColor;

        if (_uiRotationCoroutine != null) StopCoroutine(_uiRotationCoroutine);
        _uiRotationCoroutine = StartCoroutine(_crowdInteractionArea.AreaImageRotate(_crowdSettings.RotationSpeed, _crowdSettings.Clockwise));

        _crowdInteractionArea.OnCompleteInteract -= HandleInteraction;
        _crowdInteractionArea.OnCompleteInteract += HandleInteraction;
    }

    public void ActivateReciverIcon(SimpleCrowd sender)
    {
        _crowdInteractionArea.IconController.SelectReciverIcon(sender._crowdInteractionArea.IconController.iconIndex);
        _myAnimation.Play(_letterSpawnName);
    }

    private void HandleInteraction()
    {
        _myAnimation.Play(_letterTaskGet);
        if (MessageSource != null && TaskSuccessClip != null) MessageSource.PlayOneShot(TaskSuccessClip);

        _crowdInteractionArea.gameObject.SetActive(false);
        _crowdInteractionArea.IconController.gameObject.SetActive(false);

        OnInteracionComplete?.Invoke();
    }
    
    #endregion
}