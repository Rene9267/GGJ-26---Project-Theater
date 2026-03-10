using System;
using UnityEngine;

public class PauseController : MonoBehaviour
{
    public static PauseController Instance { get; private set; }
    public event Action<bool> OnPauseToggled;
    public event Action OnResumeRequested;
    public bool IsPaused { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this.gameObject); return; }
        Instance = this;
        Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        if (IsPaused)
            RequestResume();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        OnPauseToggled?.Invoke(true);
        DevLog.Log("<color=yellow>[PauseController]</color> Stato: PAUSA");
    }

    public void RequestResume()
    {
        DevLog.Log("<color=orange>[PauseController]</color> Richiesta ripresa inviata alla UI...");
        OnResumeRequested?.Invoke();
    }

    public void FinalizeResume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        OnPauseToggled?.Invoke(false);
        DevLog.Log("<color=green>[PauseController]</color> Stato: GIOCO RIPRESO");
    }

    private void OnDestroy()
    {
        if (Instance == this) { Time.timeScale = 1f; Instance = null; }
    }
}