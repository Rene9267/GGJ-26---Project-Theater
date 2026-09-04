using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CandleInteractionArea : MonoBehaviour, IInteractable
{
    #region  Variables
    [Header("Interaction Area Warning UI")]
    [SerializeField] private Image _areaImage;
    //[SerializeField] private InteractionDynamicIcon _directionIcon;
    private Coroutine _rotationCoroutine;

    // ===== Event =====
    public event Action OnPlayerEntered;
    public event Action OnPlayerExited;
    public event Action OnCompleteInteract;
    public event Action OnInteract;

    // ===== ==== =====

    public InteractType InteractableType { get; private set; }
    public Color MyInteractionColor => Color.clear;

    #endregion

    #region SetUp
    void OnEnable()
    {
        //_directionIcon.gameObject.SetActive(false);
    }

    void OnValidate()
    {
        if (_areaImage == null)
        {
            DevLog.LogWarning($"[CandleInteractionArea - {this.gameObject}]: Area Image ref is missing");
        }
    }

    void OnDisable()
    {
        if (_rotationCoroutine != null)
        {
            StopCoroutine(_rotationCoroutine);
            _rotationCoroutine = null;
        }
    }

    void Awake()
    {
        InteractableType = InteractType.Candle;
    }
    
    public void SetUpInteractionArea()
    {
        InteractableType = InteractType.Candle;
        _areaImage.gameObject.SetActive(true);
        _rotationCoroutine ??= StartCoroutine(AreaImageRotate(100, true));
        //_directionIcon.gameObject.SetActive(true);
        //_directionIcon.SetActiveInteractionIcon(false);
    }

    #endregion

    #region Interaction

    private void OnTriggerEnter(Collider other)
    {
        DevLog.Log($"[{this.gameObject}]: Qualcuno è entrato : {other.name}");
        if (other.gameObject.TryGetComponent<PlayerController>(out var player))
        {
            DevLog.Log($"[{this.gameObject}]: Player entrato nella mia Interaction Area");

            OnPlayerEntered?.Invoke();
            //_directionIcon.SetActiveInteractionIcon(true);
            player.OnInteractionAreaEnter(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerController>(out var player))
        {
            player.OnInteractionAreaExit();
            OnPlayerExited?.Invoke();
            //_directionIcon.SetActiveInteractionIcon(false);
            DevLog.Log($"[{this.gameObject}]: Player uscito dalla mia interaction Area");
        }
    }

    public IEnumerator AreaImageRotate(float rotationSpeed, bool clockwise = true)
    {
        float direction = clockwise ? -1f : 1f;

        while (true)
        {
            _areaImage.transform.Rotate(0, 0, direction * rotationSpeed * Time.deltaTime, Space.Self);
            yield return null;
        }
    }

    public void Interact()
    {
        OnInteract?.Invoke();
    }

    public void CompleteInteraction()
    {
        _areaImage.gameObject.SetActive(false);
        OnCompleteInteract?.Invoke();
    }
    #endregion

}
