using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FollowerGuestInteractionArea : MonoBehaviour, IInteractable
{
    [Header("UI Icon")]
    [SerializeField] private GameObject _directionIcon;
    [SerializeField] private GameObject _interactionIcon;
    [SerializeField] private Image _areaImage;
    [SerializeField] private GameObject _HurryUpIcon;

    public InteractType InteactableType { get; set; }
    public Color MyInteractionColor { get; private set; }
    public event Action OnPlayerEntered;
    public event Action OnPlayerExited;
    public event Action<PlayerController> OnAreaExit;
    public event Action OnInteract;
    public event Action OnHurryUP;
    public event Action OnStartInteract;

    private PlayerController _playerElement;
    private Coroutine _rotationCoroutine;
    private bool isGrabbed = false;

    private void OnValidate()
    {
        if (_areaImage == null)
        {
            DevLog.LogWarning("Area Image ref is missing");
        }
    }

    public void SetUpInteractionArea(Color color, float rotationSpeed, bool clockwise = true)
    {
        InteactableType = InteractType.TakeGuest;
        MyInteractionColor = color;
        _areaImage.color = MyInteractionColor;
        _areaImage.gameObject.SetActive(true);

        _rotationCoroutine ??= StartCoroutine(AreaImageRotate(rotationSpeed, clockwise));

        _directionIcon.SetActive(true);
        //DEBUG
        _directionIcon.GetComponent<MeshRenderer>().material.color = color;
    }

    public void ResetArea()
    {
        if (_rotationCoroutine != null)
        {
            StopCoroutine(_rotationCoroutine);
            _rotationCoroutine = null;
        }

        MyInteractionColor = Color.clear;
        _areaImage.color = MyInteractionColor;
        _areaImage.gameObject.SetActive(false);
        _directionIcon.SetActive(false);
        _HurryUpIcon.SetActive(false);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerController>(out var player))
        {
            DevLog.Log($"[FollowerGuestInteractionArea]: Qualcuno è entrato{other.name}");
            if (player.GuestFamilyColor == Color.clear)
            {
                OnPlayerEntered?.Invoke();
                player.OnInteractionAreaEnter(this);
                DevLog.Log("[FollowerGuestInteractionArea]: Player Entrato in me");
                _playerElement = player;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerController>(out var player))
        {
            if (isGrabbed)
            {
                AreaExitAndFollow();
                isGrabbed = false;
            }

            player.OnInteractionAreaExit();
            OnPlayerExited?.Invoke();
            _playerElement = null;
            DevLog.Log("[FollowerGuestInteractionArea]: Player Uscito da me");
        }
    }

    public IEnumerator AreaImageRotate(float rotationSpeed, bool clockwise = true)
    {
        float direction = clockwise ? -1f : 1f;

        while (true)
        {
            _areaImage.transform.Rotate(0, 0, direction * rotationSpeed * Time.deltaTime, Space.Self);
            yield return null;
        }
    }

    public void Interact()
    {
       OnInteract?.Invoke();
    }

    public void HurryUp()
    {
        OnHurryUP?.Invoke();
    }

    private void AreaExitAndFollow()
    {
        OnAreaExit?.Invoke(_playerElement);
        _directionIcon.SetActive(false);
    }

    public void CompleteInteraction()
    {
        //animazioni varie
        if (_playerElement == null)
        {
            DevLog.LogError("[FollowerGuestInteractionArea]: La posizione del player è assente");
            return;
        }

        isGrabbed = true;
        _areaImage.gameObject.SetActive(false);
        if (_rotationCoroutine != null)
        {
            StopCoroutine(_rotationCoroutine);
            _rotationCoroutine = null;
        }

        OnStartInteract?.Invoke();
    }
}
