using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CrowdSettings", menuName = "Scriptable Objects/CrowdSettings")]
public class CrowdSettings : ScriptableObject
{
    public List<GameObject> SpawnableGuest;

    [Header("Guest Spawn Settings")]
    public float Radius = 1f;
    public float SecurityRadiusCheck = 1f;

    [Tooltip("Indica il numero minimo e massimo di elementi che possono essere spawnati")]
    public Vector2 CrowdSize;

    [Header("Sicurezza")]
    public LayerMask ObstacleLayer;
    public int MaxAttemptsPerPawn = 10;
    
    [Header("Message Area Settings")]
    public float RotationSpeed = 100f;
    public bool Clockwise = true;
}
