
using System;
using UnityEngine;

public enum InteractType
{
    MessageSender,
    MessageReciver,
    Light,
    TakeGuest,
    DrobGuest,
    None
}
public interface IInteractable
{
    public InteractType InteactableType { get; }

    void Interact();

    public Color MyInteractionColor { get;}
}
