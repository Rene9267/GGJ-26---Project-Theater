using UnityEngine;
using System;

public class OptionsController : MonoBehaviour
{
    #region Variables

    //Ascoltati da START MENU CONTROLLER
    public event Action OnFXVolumeChange;
    public event Action OnMusicVolumeChange;

    [SerializeField] private SelectionButton _languageSelector;
    [SerializeField] private SelectionButton _windowModeSelector;
    [SerializeField] private SliderController _mainVolumeSlider;
    [SerializeField] private SliderController _fxVolumeSlider;

    #endregion


    #region Unity Standard Methods

    void OnValidate()
    {
        if (_mainVolumeSlider == null)
            DevLog.LogError($"[{this}]: Main Volume Slider null", this);
        if (_fxVolumeSlider == null)
            DevLog.LogError($"[{this}]: FX Volume Slider null", this);
        if (_languageSelector == null)
            DevLog.LogError($"[{this}]: Language selector null", this);
        if (_windowModeSelector == null)
            DevLog.LogError($"[{this}]: Window selector null", this);
    }

    void Start()
    {
        ReadSavedValues();
    }

    #endregion


    #region Class Methods

    public void HandleOptionsSave()
    {
        if (SystemSavingController.Instance == null)
        {
            DevLog.LogError($"[{this}]Saves assenti", this);
            return;
        }
        GameSettings.Instance.Language = _languageSelector.CurrentListIndex;
        GameSettings.Instance.WindowModeIndex = (FullScreenMode)_windowModeSelector.CurrentListIndex;
        SystemSavingController.Instance.SaveSettings();
    }

    private void ReadSavedValues()
    {
        if (GameSettings.Instance == null)
        {
            DevLog.LogError($"[{this}]Saves assenti", this);
            return;
        }

        _mainVolumeSlider.SetUpSliderValue(GameSettings.Instance.MusicVolume);
        _fxVolumeSlider.SetUpSliderValue(GameSettings.Instance.FXVolume);
        _languageSelector.SelectPrecise(GameSettings.Instance.Language);
        _windowModeSelector.SelectPrecise((int)GameSettings.Instance.WindowModeIndex);
    }

    public void OnMusicSliderChange()
    {
        GameSettings.Instance.MusicVolume = _mainVolumeSlider.SliderValue;
        OnMusicVolumeChange?.Invoke();
    }

    public void OnFXSliderChange()
    {
        GameSettings.Instance.FXVolume = _fxVolumeSlider.SliderValue;
        OnFXVolumeChange?.Invoke();
    }


    #endregion

}
