using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public class CrowdInteractionArea : MonoBehaviour, IInteractable
{

    [Header("UI Icon")]
    [SerializeField] private List<Sprite> _messageIcons;
    [SerializeField] private InteractionDynamicIcon interactionDynamicIcon;
    [SerializeField] private DirectionIcon _hurryUpIcon;
    [SerializeField] private Sprite _reciverIcon;
    [SerializeField] private Sprite _reciverIconBackground;
    [SerializeField] private Sprite _sanderIconBackground;
    [SerializeField] private Image _areaImage;

    public InteractType InteactableType { get; private set; }

    public event Action OnPlayerEntered;
    public event Action OnPlayerExited;
    public event Action OnInteract;
    public event Action OnMessageTake;


    public Color MyInteractionColor { get; private set; }

    private void OnValidate()
    {
        if (_areaImage == null)
        {
            Debug.LogWarning("Area Image ref is missing");
        }
    }

    void Awake()
    {
        _areaImage.gameObject.SetActive(false);
        interactionDynamicIcon.gameObject.SetActive(false);
        if (_hurryUpIcon != null)
            _hurryUpIcon.gameObject.SetActive(false);
    }

    public void ResetArea()
    {
        InteactableType = InteractType.None;
        MyInteractionColor = Color.clear;

        interactionDynamicIcon.SetActiveInteractionIcon(false);
        interactionDynamicIcon.gameObject.SetActive(false);

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
        if (InteactableType == InteractType.MessageSender)
        {
            int randomIconIndex = Random.Range(0, _messageIcons.Count);
            interactionDynamicIcon.ChangableImage.sprite = _messageIcons[randomIconIndex];
            interactionDynamicIcon.BackgroundImage.sprite = _sanderIconBackground;
            interactionDynamicIcon.gameObject.SetActive(true);
            interactionDynamicIcon.SetActiveInteractionIcon(false);
        }
        else if (InteactableType == InteractType.MessageReciver)
        {
            interactionDynamicIcon.ChangableImage.sprite = _reciverIcon;
            interactionDynamicIcon.BackgroundImage.sprite = _reciverIconBackground;
            interactionDynamicIcon.gameObject.SetActive(true);
            interactionDynamicIcon.SetActiveInteractionIcon(false);
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

    public void HurryUp()
    {
        if (_hurryUpIcon != null)
            _hurryUpIcon.gameObject.SetActive(true);
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
                interactionDynamicIcon.SetActiveInteractionIcon(true);
                DevLog.Log("[Crowd Interaction Area]: Player Entrato in me");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerController>(out var player))
        {
            player.OnInteractionAreaExit();
            OnPlayerExited?.Invoke();
            interactionDynamicIcon.SetActiveInteractionIcon(false);
            DevLog.Log("[Crowd Interaction Area]: Player Uscito da me");
        }
    }

    public void Interact()
    {
        //animazioni varie

        if (InteactableType == InteractType.MessageSender)
        {
            if(_hurryUpIcon!= null)
            _hurryUpIcon.gameObject.SetActive(false);
            OnMessageTake?.Invoke();
        }
        else if (InteactableType == InteractType.MessageReciver)
        {
            OnInteract?.Invoke();
        }

        interactionDynamicIcon.SetActiveInteractionIcon(false);
        interactionDynamicIcon.gameObject.SetActive(false);
    }
}
