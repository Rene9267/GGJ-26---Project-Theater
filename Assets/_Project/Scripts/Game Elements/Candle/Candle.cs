using System;
using System.Collections;
using UnityEngine;

public class Candle : MonoBehaviour
{
    #region  Variables

    public event Action OnDarkEffect;

    [Header("Interaction")]
    [SerializeField] private CandleInteractionArea _interactionArea;
    [SerializeField] private Animation _animation;

    
    private bool _isDark = false;
    private float _darkGuestRunTimer;
    private Coroutine _darkCoroutine;

private readonly string _candleOff = "AC_FlameOff";
    private readonly string _cancdleOn = "AC_FlameOn";

    #endregion

    void OnValidate()
    {
        if (_interactionArea == null)
        {
            Debug.LogWarning("[Candle]: riferimento mancante al CandleInteractionArea");
        }
    }

    void OnDisable()
    {
        if (_interactionArea != null)
        {
            _interactionArea.OnCompleteInteract -= HandleCompleteInteraction;
            _interactionArea.OnInteract -= HandleStartInteraction;
        }
    }

    public void TurnOff(float timeToRaiseTheDarkness)
    {
        _animation.Play(_candleOff);
        _isDark = true;

        _darkGuestRunTimer = timeToRaiseTheDarkness;

        _darkCoroutine ??= StartCoroutine(DarkIsComing());

        if (_interactionArea == null)
            Debug.LogWarning($"[Candle - {this.gameObject}]: {_interactionArea} risulta null");

        _interactionArea.SetUpInteractionArea();

        _interactionArea.OnCompleteInteract -= HandleCompleteInteraction;
        _interactionArea.OnCompleteInteract += HandleCompleteInteraction;

        _interactionArea.OnInteract -= HandleStartInteraction;
        _interactionArea.OnInteract += HandleStartInteraction;

    }


    private void HandleStartInteraction()
    {
        if (_darkCoroutine != null) StopCoroutine(_darkCoroutine);
        _interactionArea.OnInteract -= HandleStartInteraction;
    }

    private void HandleCompleteInteraction()
    {
        _isDark = false;
        _animation.Play(_cancdleOn);
        _interactionArea.OnCompleteInteract -= HandleCompleteInteraction;
    }

    private IEnumerator DarkIsComing()
    {
        float tmpDarkTimer = _darkGuestRunTimer;
        while (_isDark)
        {
            tmpDarkTimer -= Time.deltaTime;
            if (tmpDarkTimer <= 0)
            {
                tmpDarkTimer = _darkGuestRunTimer;
                OnDarkEffect?.Invoke();
            }
                yield return null;
        }
    }
}
