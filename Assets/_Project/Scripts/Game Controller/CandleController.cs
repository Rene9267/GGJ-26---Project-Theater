using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CandleController : MonoBehaviour
{
    #region  Variables

    public event Action OnDarkRise;


    [Header("Settings References")]
    [SerializeField] private CandleSettings _settings;
    [SerializeField]private List<Candle> _allSceneCandle;


    private List<Candle> _availableCandle;
    private List<Candle> _darkCandle;
    #endregion

    #region SetUp

    void Awake()
    {
        _availableCandle = new List<Candle>(_allSceneCandle);
        _darkCandle = new();
    }

    void OnEnable()
    {
        foreach (var candle in _availableCandle)
        {
            candle.OnDarkEffect += HandleDarkRaise;
        }
    }

    void OnDisable()
    {
        foreach (var candle in _availableCandle)
        {
            candle.OnDarkEffect -= HandleDarkRaise;
        }
        foreach (var candle in _darkCandle)
        {
            candle.OnDarkEffect -= HandleDarkRaise;
        }
    }
    #endregion

    public void TurnOffACandle()
    {
        if (_availableCandle.Count <= 0)
        {
            DevLog.LogWarning($"[{this.gameObject}]: Nessuna candela disponibile per essre spenta");
            return;
        }

        int randomCandleIndex = Random.Range(0, _availableCandle.Count);
        var tmpCandle = _availableCandle[randomCandleIndex];

        if (tmpCandle == null)
        {
            DevLog.LogWarning($"[{this.gameObject}]: La candela {tmpCandle}, all'indice {randomCandleIndex} risulta essere Null ");
            return;
        }

        tmpCandle.TurnOff(_settings.TimeToRiseTheDarkness);
        _darkCandle.Add(tmpCandle);
    }

    private void HandleDarkRaise()
    {
        OnDarkRise?.Invoke();
    }
}
