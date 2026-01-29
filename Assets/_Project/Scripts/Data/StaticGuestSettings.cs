using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StaticGuestSettings", menuName = "Scriptable Objects/StaticGuestSettings")]
public class StaticGuestSettings : ScriptableObject
{
    public List<Color> BodyColor = new();
}
