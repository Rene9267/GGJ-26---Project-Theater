using System.Collections.Generic;
using UnityEngine;

public class Crowd : MonoBehaviour
{
    [Header("UI Icon")]
    [SerializeField] private List<GameObject> _messageIcons;
    [SerializeField] private GameObject _reciverIcon;
    [SerializeField] private GameObject _interactionIcon;

    [Header("Interaction")]
    [SerializeField] private CrowdInteractionArea _crowdInteractionArea;

    [Header("Core References")]
    [SerializeField] private CrowdSettings _crowdSettings;
    [SerializeField] private Transform _guestFather;

    private bool IsReciverOrSender = false;
    private readonly List<GameObject> _crowdMembers = new();
    private Coroutine _uiRotationCoroutine;

    public bool GetMessageStatus => IsReciverOrSender;
    public void SetMessageStatus(bool state)
    {
        IsReciverOrSender = state;
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
    }

    public void EnableSender(Color CircleColor)
    {
        _crowdInteractionArea.gameObject.SetActive(true);
        _crowdInteractionArea.SetImageColor(CircleColor);

        if (_uiRotationCoroutine != null)
        {
            StopCoroutine( _uiRotationCoroutine );
            _uiRotationCoroutine = null;
        }

        
        _uiRotationCoroutine = StartCoroutine(_crowdInteractionArea.AreaImageRotate(_crowdSettings.RotationSpeed, _crowdSettings.Clockwise));
        int randomIconIndex = Random.Range(0, _messageIcons.Count);
        //_messageIcons[randomIconIndex].SetActive(true);
    }

    public void EnableReciver(Color CircleColor)
    {
        _crowdInteractionArea.gameObject.SetActive(true);
        _crowdInteractionArea.SetImageColor(CircleColor);

        if (_uiRotationCoroutine != null)
        {
            StopCoroutine(_uiRotationCoroutine);
            _uiRotationCoroutine = null;
        }


        _uiRotationCoroutine = StartCoroutine(_crowdInteractionArea.AreaImageRotate(_crowdSettings.RotationSpeed, _crowdSettings.Clockwise));
        //_reciverIcon.SetActive(true);
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
