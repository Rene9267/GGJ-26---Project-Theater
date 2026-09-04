using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour
{
    #region Parameters

    public static AudioController Instance { get; private set; }

    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private List<AudioSource> _musicAudioSource;

    private const string MUSIC_PARAM = "MusicParam";
    private const string FX_PARAM = "FXParam";
    private const string UI_PARAM = "UIParam";
    private const string RIVERBER_PARAM = "RiverberParam";

    #endregion

    #region Unity Standard Methods

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnValidate()
    {
        if (mainMixer == null)
            DevLog.LogWarning("Riferimento al mixer assente", this);
    }

    void Start()
    {
        ApplyAllVolumes();

        if (PauseController.Instance != null)
        {
            PauseController.Instance.OnPauseToggled += SetPauseState;
        }
    }

    private void OnDestroy()
    {
        if (PauseController.Instance != null)
        {
            PauseController.Instance.OnPauseToggled -= SetPauseState;
        }
    }

    #endregion

    #region Class Methods

    public void ApplyAllVolumes()
    {
        var settings = GameSettings.Instance;

        SetVolume(MUSIC_PARAM, settings.MusicVolume);
        SetVolume(FX_PARAM, settings.FXVolume);
        SetVolume(UI_PARAM, settings.FXVolume);
        SetVolume(RIVERBER_PARAM, settings.MusicVolume);
    }

    public void SetVolume(string parameterName, float sliderValue)
    {
        DevLog.Log($"Volume set to {sliderValue} for {parameterName}", this);
        float normalizedValue = Mathf.InverseLerp(0, 100, sliderValue);
        float dB = normalizedValue > 0.0001f ? Mathf.Log10(normalizedValue) * 20 : -80f;
        mainMixer.SetFloat(parameterName, dB);
    }

    public void SetPauseState(bool isPaused)
    {
        if (isPaused)
        {
            mainMixer.SetFloat(FX_PARAM, -30f);
            if (_musicAudioSource != null && _musicAudioSource.Count > 0)
                foreach (var source in _musicAudioSource)
                {
                    source.Pause();
                }
        }
        else
        {
            SetVolume(FX_PARAM, GameSettings.Instance.FXVolume);
            if (_musicAudioSource != null && _musicAudioSource.Count > 0)
                foreach (var source in _musicAudioSource)
                {
                    source.UnPause();
                }
        }
    }

    #endregion

}