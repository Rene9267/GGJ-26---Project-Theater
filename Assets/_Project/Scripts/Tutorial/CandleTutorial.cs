using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class CandleTutorial : MonoBehaviour
{
    #region Parameters
    public event Action OnCandleTutorialComplete;

    [SerializeField] private Candle _candle;

    #endregion

    #region Unity Methods   
    private void OnValidate()
    {
        if (_candle == null)
            DevLog.LogError($"[CandleTutorial - {this.gameObject}]: {_candle} risulta null");
    }

    #endregion

    #region Calss Methods

    public void StartTutorial()
    {
        _candle.TurnOff(999999f);
        _candle.OnTurnOn += HandleTurnOn;
    }

    private void HandleTurnOn(Candle candle)
    {
        _candle.OnTurnOn -= HandleTurnOn;
        OnCandleTutorialComplete?.Invoke();
    }

    #endregion


}
