using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class CrowdInteractionArea : MonoBehaviour, IInteractable
{

    [Header("UI Icon")]
    [SerializeField] private Image _areaImage;
    public LetterIcon_Controller IconController;

    public InteractType InteactableType { get; private set; }
    public event Action OnHurryUp;
    public event Action OnPlayerEntered;
    public event Action OnPlayerExited;
    public event Action OnCompleteInteract;
    public event Action OnInteract;
    public event Action OnMessageTake;
    public Color MyInteractionColor { get; private set; }

    private void OnValidate()
    {
        if (_areaImage == null)
        {
            DevLog.LogWarning("Area Image ref is missing");
        }
    }

    private void OnDisable()
    {
        IconController.HideAllIcons();
    }

    void Awake()
    {
        _areaImage.gameObject.SetActive(false);
    }

    public void ResetArea()
    {
        InteactableType = InteractType.None;
        MyInteractionColor = Color.clear;

        if (_areaImage != null)
        {
            _areaImage.color = Color.white;
        }
    }

    public void SetUPInteractionArea(Color color, InteractType type)
    {
        InteactableType = type;
        _areaImage.color = color;
        MyInteractionColor = color;
        _areaImage.gameObject.SetActive(true);

        if (type == InteractType.MessageSender)
            IconController.SetUpIcon(InteactableType);
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

    public void HurryUp()
    {
        OnHurryUp?.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerController>(out var player))
        {
            DevLog.Log($"[Crowd Interaction Area]: Qualcuno è entrato{other.name}");
            if (InteactableType == InteractType.MessageReciver && player.ActualMessage.MessageColor == MyInteractionColor
            || InteactableType == InteractType.MessageSender && player.ActualMessage.MessageColor == Color.clear)
            {
                OnPlayerEntered?.Invoke();
                player.OnInteractionAreaEnter(this);
                DevLog.Log("[Crowd Interaction Area]: Player Entrato in me");

                if(InteactableType == InteractType.MessageSender)
                {
                    bool isHeart = false;
                    if(IconController.iconIndex == 1) isHeart = true;
                    player.SetHeadIcon(isHeart);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerController>(out var player))
        {
            player.OnInteractionAreaExit();
            OnPlayerExited?.Invoke();
            DevLog.Log("[Crowd Interaction Area]: Player Uscito da me");
        }
    }

    public void Interact()
    {
        if (InteactableType == InteractType.MessageSender)
        {
            OnMessageTake?.Invoke();
        }
    }

    public void CompleteInteraction()
    {
        if (InteactableType == InteractType.MessageReciver)
        {
            OnCompleteInteract?.Invoke();
        }
        if (InteactableType == InteractType.MessageSender)
        {
            IconController.gameObject.SetActive(false);
            this.gameObject.SetActive(false);
        }
    }
}
