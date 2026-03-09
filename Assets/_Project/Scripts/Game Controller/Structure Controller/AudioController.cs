using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour
{
    #region Variables

    public static AudioController Instance { get; private set; }

    [SerializeField] private AudioMixer mainMixer;

    #endregion

    #region Unity Standard Methods

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnValidate()
    {
        if (mainMixer == null)
            DevLog.LogError("Riferimento al mixer assente", this);
    }

    void Start()
    {
        ApplyAllVolumes();
    }

    #endregion

    #region Class Methods

    public void ApplyAllVolumes()
    {
        var settings = GameSettings.Instance;

        SetVolume("MusicParam", settings.MusicVolume);
        SetVolume("FXParam", settings.FXVolume);
    }

    public void SetVolume(string parameterName, float sliderValue)
    {
        DevLog.Log($"Volume set to {sliderValue} for {parameterName}", this);
        float normalizedValue = Mathf.InverseLerp(0, 100, sliderValue);
        float dB = normalizedValue > 0 ? Mathf.Log10(normalizedValue) * 20 : -80f;
        mainMixer.SetFloat(parameterName, dB);
    }

    #endregion

}