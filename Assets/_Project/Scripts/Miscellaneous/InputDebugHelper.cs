using UnityEngine;
using UnityEngine.InputSystem;

public class InputDebugHelper : MonoBehaviour
{
    void Update()
    {
        // Controlla se il TimeScale è bloccato a zero
        if (Time.timeScale == 0 && !(PauseController.Instance != null && PauseController.Instance.IsPaused))
        {
            Debug.LogWarning("<color=red>[DEBUG]</color> Il TimeScale è a 0 ma NON siamo in pausa! Qualcosa ha bloccato il gioco.");
        }

        // Verifica se il tasto ESC viene premuto a livello di sistema (ignorando il PlayerInput)
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("<color=cyan>[DEBUG]</color> Tasto ESC premuto fisicamente sulla tastiera.");
        }
    }
}