using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/GameSettings")]
public class GameSettings : ScriptableObject
{

    public static GameSettings Instance;

    public float MusicVolume = 1f;
    public float FXVolume = 1f;
    public int Language = 0;
    public FullScreenMode WindowModeIndex = FullScreenMode.ExclusiveFullScreen;

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        if (Instance == this)
            Instance = null;
    }
}
