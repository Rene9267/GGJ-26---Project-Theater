using System;
using UnityEngine;

public class FollowerGuestExitArea : MonoBehaviour, IInteractable
{
    [Header("UI Icon")]
    [SerializeField] private InteractionDynamicIcon _directionIcon;
    [SerializeField] private DirectionIcon _areaIcon;

    public event Action OnPlayerEntered;
    public event Action OnPlayerExited;
    public event Action<Color> OnCompleteInteract;
    public event Action<Color> OnStartInteract;

    public InteractType InteactableType { get; set; }

    public Color MyInteractionColor { get; private set; }

    void OnEnable()
    {
        _directionIcon.gameObject.SetActive(false);
    }

    public void SetUpInteractionArea(Color color, Sprite directionIcon, Sprite baseIcon)
    {
        MyInteractionColor = color;
        _directionIcon.ChangableImage.sprite = directionIcon;
        _areaIcon.ChangableImage.sprite = baseIcon;

        InteactableType = InteractType.DrobGuest;
    }

    public void EnableDirectionIcon()
    {
        _directionIcon.gameObject.SetActive(true);
        _areaIcon.gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerController>(out var player))
        {
            Debug.Log($"[FollowerGuestInteractionArea]: Qualcuno è entrato{other.name}");
            if (player.GuestFamilyColor == MyInteractionColor)
            {
                OnPlayerEntered?.Invoke();
                player.OnInteractionAreaEnter(this);
                _directionIcon.SetActiveInteractionIcon(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerController>(out var player))
        {
            player.OnInteractionAreaExit();
            OnPlayerExited?.Invoke();
            _directionIcon.SetActiveInteractionIcon(false);
        }
    }

    public void Interact()
    {
        OnStartInteract?.Invoke(MyInteractionColor);
    }

    public void CompleteInteraction()
    {
        OnCompleteInteract?.Invoke(MyInteractionColor);
        _directionIcon.gameObject.SetActive(false);
    }

    public void DisableDirectionIcon()
    {
        _directionIcon.gameObject.SetActive(false);
    }
}
