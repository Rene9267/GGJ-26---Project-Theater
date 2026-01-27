using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GuestsSettings", menuName = "Scriptable Objects/GuestsSettings")]
public class GuestsSettings : ScriptableObject
{
    [Header("Guest Spawn Settings")]
    [Tooltip("Indica il numero minimo e massimo di elementi che possono essere spawnati")]
    public Vector2 GuestSpawnRange = new Vector2(1, 4);
    public LayerMask ObstacleLayer;
    public int MaxAttemptsPerPawn = 10;
    public float Radius = 1f;
    public float SecurityRadiusCheck = 1f;


    [Header("Visual Settings")]
    [Tooltip("Quali Guest Sono messi nella Pool")]
    public List<GameObject> GuestsPool = new();

    [Tooltip("Elenco di colori delle famiglie dei Guests")]
    public List<Color> GuestsColors = new();


    [Header("Message Area Settings")]
    public float RotationSpeed = 100f;
    public bool Clockwise = true;
}
