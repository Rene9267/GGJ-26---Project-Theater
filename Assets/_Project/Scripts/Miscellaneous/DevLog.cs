using System.Diagnostics;
using UnityEngine;

public static class DevLog 
{
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Log(string message)
    {
        UnityEngine.Debug.Log(message);
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(string message)
    {
        UnityEngine.Debug.LogWarning(message);
    }

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void LogError(string message)
    {
        UnityEngine.Debug.LogError(message);
    }
}
