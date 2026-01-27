using UnityEngine;

[CreateAssetMenu(fileName = "OperaSettings", menuName = "Scriptable Objects/OperaSettings")]
public class OperaSettings : ScriptableObject
{
    [Header("Opera Starting Info")]
    public int InitialPublic = 200;
    public float OperaDurationSeconds = 120f;

    [Header("Feature Thresholds (0 to 1)")]
    [Range(0, 1)] public float GuestActivationThreshold = 0f;
    [Range(0, 1)] public float MessageActivationThreshold = 0.3f;
    [Range(0, 1)] public float CandleActivationThreshold = 0.6f;


    [Header("Spawn Intervals (Animation Curves)")]
    [Tooltip("X = Normalized Time (0-1), Y = Seconds between spawns")]
    public AnimationCurve guestSpawnInterval;
    public AnimationCurve messageSpawnInterval;
    public AnimationCurve lightSpawnInterval;
}
