using System;
using System.Collections;
using UnityEngine;

public class Candle : MonoBehaviour
{
    #region  Variables

    public event Action OnDarkEffect;
    public event Action<Candle> OnTurnOn;
    public event Action<Candle> OnDarkStarted;


    [Header("Interaction")]
    [SerializeField] private CandleInteractionArea _interactionArea;
    [SerializeField] private Animation _animation;

    [Header("Audio")]
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip _candleOn;
    [SerializeField] private AudioClip _candleOffClip;


    private bool _isDark = false;
    private float _darkGuestRunTimer;
    private Coroutine _darkCoroutine;

    private readonly string _candleOff = "Anim_Candle_FlameOff";
    private readonly string _cancdleOn = "Anim_Candle_FlameOn";

    #endregion

    void OnValidate()
    {
        if (_interactionArea == null)
        {
            DevLog.LogWarning("[Candle]: riferimento mancante al CandleInteractionArea");
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

        _source.PlayOneShot(_candleOffClip);

        _isDark = true;
        OnDarkStarted?.Invoke(this);

        _darkGuestRunTimer = timeToRaiseTheDarkness;

        _darkCoroutine ??= StartCoroutine(DarkIsComing());

        if (_interactionArea == null)
            DevLog.LogWarning($"[Candle - {this.gameObject}]: {_interactionArea} risulta null");

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
        _source.PlayOneShot(_candleOn);
        OnTurnOn?.Invoke(this);
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
