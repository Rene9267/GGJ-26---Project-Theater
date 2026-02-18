using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    #region Variables
    [HideInInspector] public int SliderValue;

    [SerializeField] private TextMeshProUGUI _sliderValueTXT;
    [SerializeField] private Scrollbar _slider;

    #endregion

    #region Unity Standard Methods

    void OnValidate()
    {
        if(_sliderValueTXT == null)
            DevLog.LogError($"[{this}]: The slider Text is null", this);
        if(_slider == null)
            DevLog.LogError($"[{this}]: The slider is null", this);
    }

    void OnEnable()
    {
        _slider.onValueChanged.AddListener(OnSliderChange);
    }

    void OnDisable()
    {
        _slider.onValueChanged.RemoveAllListeners();
    }
    #endregion


    #region Class Methods

    public void OnSliderChange(Single value)
    {
        SliderValue = Mathf.RoundToInt(Mathf.Lerp(0, 100, _slider.value));
        _sliderValueTXT.text = SliderValue.ToString() + "%";
    }
    
    public void SetUpSliderValue(float newValue)
    {
        _slider.value = Mathf.InverseLerp(0, 100, newValue);
    }

    #endregion
}
