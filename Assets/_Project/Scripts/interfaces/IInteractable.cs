
using System;
using UnityEngine;

public enum InteractType
{
    MessageSender,
    MessageReciver,
    Candle,
    TakeGuest,
    DrobGuest,
    None
}
public interface IInteractable
{
    public InteractType InteactableType { get; }

    void Interact();

    void CompleteInteraction();

    public Color MyInteractionColor { get;}
}
