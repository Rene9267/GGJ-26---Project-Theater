using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CrowdInteractionArea : MonoBehaviour, IInteractable
{

    [Header("UI Icon")]
    [SerializeField] private List<GameObject> _messageIcons;
    private GameObject _actualMessageIcon;


    [SerializeField] private GameObject _reciverIcon;
    [SerializeField] private GameObject _interactionIcon;
    [SerializeField] private Image _areaImage;

    public InteractType InteactableType { get; set; }

    public event Action OnPlayerEntered;
    public event Action OnPlayerExited;
    public event Action OnInteract;

    public Color MyInteractionColor { get; private set; }


    private void OnValidate()
    {
        if (_areaImage == null)
        {
            Debug.LogWarning("Area Image ref is missing");
        }
    }
    
    public void ResetArea()
    {
        InteactableType = InteractType.None;
        MyInteractionColor = Color.clear;

        foreach (var icon in _messageIcons)
        {
            if (icon != null) icon.SetActive(false);
        }

        if (_reciverIcon != null)
        {
            _reciverIcon.SetActive(false);
        }

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

        if (InteactableType == InteractType.MessageSender)
        {
            int randomIconIndex = Random.Range(0, _messageIcons.Count);
            _actualMessageIcon = _messageIcons[randomIconIndex];
            _actualMessageIcon.SetActive(true);
        }
        else if (InteactableType == InteractType.MessageReciver)
        {
            _reciverIcon.SetActive(true);
            _actualMessageIcon = _reciverIcon;
        }

        //DEBUG
        _actualMessageIcon.GetComponent<MeshRenderer>().material.color = color;
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

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.TryGetComponent<PlayerController>(out var player))
        {
            Debug.Log($"[Crowd Interaction Area]: Qualcuno è entrato{other.name}");
            if (InteactableType == InteractType.MessageReciver && player.ActualMessage.MessageColor == MyInteractionColor
            || InteactableType == InteractType.MessageSender && player.ActualMessage.MessageColor == Color.clear)
            {
                OnPlayerEntered?.Invoke();
                player.OnInteractionAreaEnter(this);
                Debug.Log("[Crowd Interaction Area]: Player Entrato in me");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerController>(out var player))
        {
            player.OnInteractionAreaExit();
            OnPlayerExited?.Invoke();
            Debug.Log("[Crowd Interaction Area]: Player Uscito da me");
        }
    }

    public void Interact()
    {
        //animazioni varie

        if (InteactableType == InteractType.MessageSender)
        {
            _actualMessageIcon.SetActive(false);
        }
        else if (InteactableType == InteractType.MessageReciver)
        {
            OnInteract?.Invoke();
        }
    }
}
