using System;
using System.Collections;
using UnityEngine;

public class Candle : MonoBehaviour
{
    #region  Variables

    public event Action OnDarkEffect;

    [Header("Interaction")]
    [SerializeField] private CandleInteractionArea _interactionArea;

    [Header("GamePlay")]
    [SerializeField] private GameObject _candleFire;
    [SerializeField] private GameObject _candleFireUI;
    
    private bool _isDark = false;
    private float _darkGuestRunTimer;
    private Coroutine _darkCoroutine;

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
            _interactionArea.OnInteract -= HandleInteraction;
    }

    public void TurnOff(float timeToRaiseTheDarkness)
    {
        _candleFire.SetActive(false);
        _candleFireUI.SetActive(false);
        _isDark = true;

        _darkGuestRunTimer = timeToRaiseTheDarkness;

        _darkCoroutine ??= StartCoroutine(DarkIsComing());

        if (_interactionArea == null)
            Debug.LogWarning($"[Candle - {this.gameObject}]: {_interactionArea} risulta null");

        _interactionArea.SetUpInteractionArea();

        _interactionArea.OnInteract -= HandleInteraction;
        _interactionArea.OnInteract += HandleInteraction;
    }

    private void HandleInteraction()
    {
        _isDark = false;

        if (_darkCoroutine != null) StopCoroutine(_darkCoroutine);

        _interactionArea.OnInteract -= HandleInteraction;
        _candleFire.SetActive(true);
        _candleFireUI.SetActive(true);
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
