using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Scriptable Objects/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    [Header("Movement Settings")]
    [Tooltip("Base movement speed of the player")]
    public float MoveSpeed = 5f;
    [Tooltip("Multiplier applied to MoveSpeed when sprinting")]
    public float SprintMultiplier = 1.5f;
    [Tooltip("Rotation speed of the player in degrees per second")]
    public float RotationSpeed = 10f;


    [Header("Interaction Settings")]
    public string StunGuestTag = "Guest";
}
