using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CandleSettings", menuName = "Scriptable Objects/CandleSettings")]
public class CandleSettings : ScriptableObject
{
    [Header("Game Loop Settings")]
    public float TimeToRiseTheDarkness = 5f;
}
