using UnityEngine;
using UnityEngine.Events;

public class UIAnimationBridge : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Tutorial_GameController _tutorialController;

    private void Awake()
    {
        if (_tutorialController == null)
        {
            _tutorialController = GetComponentInParent<Tutorial_GameController>();
        }
    }

    /// <summary>
    /// Questo è il metodo da selezionare nell'Animation Event sulla Timeline.
    /// </summary>
    public void TriggerPauseOutComplete()
    {
        if (_tutorialController != null)
        {
            _tutorialController.OnPauseOutAnimationComplete();
        }
        else
        {
            Debug.LogWarning($"[Bridge] Tutorial_GameController non trovato per l'oggetto {gameObject.name}");
        }
    }
}