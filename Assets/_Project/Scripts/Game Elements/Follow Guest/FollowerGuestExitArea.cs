using System;
using UnityEngine;

public class FollowerGuestExitArea : MonoBehaviour, IInteractable
{
    [Header("UI Icon")]
    [SerializeField] private GameObject _directionIcon;
    [SerializeField] private GameObject _interactionIcon;
    [SerializeField] private GameObject _ExitPlane;

    public event Action OnPlayerEntered;
    public event Action OnPlayerExited;
    public event Action<Color> OnInteract;

    public InteractType InteactableType { get; set; }

    public Color MyInteractionColor { get; private set; }

    public void SetUpInteractionArea(Color color)
    {
        MyInteractionColor = color;
        _ExitPlane.GetComponent<MeshRenderer>().material.color = MyInteractionColor;
        _directionIcon.GetComponent<MeshRenderer>().material.color = MyInteractionColor;
        InteactableType = InteractType.DrobGuest;
    }

    public void EnableDirectionIcon()
    {
        _directionIcon.SetActive(true);
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
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerController>(out var player))
        {
            player.OnInteractionAreaExit();
            OnPlayerExited?.Invoke();
        }
    }

    public void Interact()
    {
        OnInteract?.Invoke(MyInteractionColor);
        _directionIcon.SetActive(false);
    }
}
