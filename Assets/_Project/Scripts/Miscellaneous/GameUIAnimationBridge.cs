using UnityEngine;

public class GameUIAnimationBridge : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameController _gameController;

    private void Awake()
    {
        if (_gameController == null)
        {
            _gameController = GetComponentInParent<GameController>();
        }
    }

    /// <summary>
    /// Questo è il metodo da selezionare nell'Animation Event sulla Timeline / Animation Window per la scena Main.
    /// </summary>
    public void TriggerPauseOutComplete()
    {
        if (_gameController != null)
        {
            _gameController.OnPauseOutAnimationComplete();
        }
        else
        {
            Debug.LogWarning($"[Bridge] GameController non trovato per l'oggetto {gameObject.name}");
        }
    }
}