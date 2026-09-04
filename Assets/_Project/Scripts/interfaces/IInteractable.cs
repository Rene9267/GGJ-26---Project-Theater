
using System;
using UnityEngine;

public enum InteractType
{
    MessageSender,
    MessageReceiver,
    Candle,
    TakeGuest,
    DropGuest,
    None
}
public interface IInteractable
{
    public InteractType InteractableType { get; }

    void Interact();

    void CompleteInteraction();

    public Color MyInteractionColor { get;}
}
