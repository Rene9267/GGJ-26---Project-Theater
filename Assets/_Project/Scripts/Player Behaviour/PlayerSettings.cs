using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Scriptable Objects/PlayerSettings")]
public class PlayerSettings : ScriptableObject
{
    [Header("Movement Settings")]
    [Tooltip("Base movement speed of the player")]
    public float MoveSpeed = 5f;
    [Tooltip("Multiplier applied to MoveSpeed when sprinting")]
    public float SprintMultiplier = 1.5f;
}
