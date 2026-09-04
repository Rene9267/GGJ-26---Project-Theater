using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class ScoreCanvas : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _rootPanel;
    [SerializeField] private Graphic _background;
    [SerializeField] private RectTransform _statPanel;

    [Header("Stat Texts")]
    [SerializeField] private TextMeshProUGUI _peopleGainedLabel;
    [SerializeField] private TextMeshProUGUI _peopleLostLabel;
    [SerializeField] private TextMeshProUGUI _satisfactionLabel;
    [SerializeField] private TextMeshProUGUI _peopleGainedText;
    [SerializeField] private TextMeshProUGUI _peopleLostText;
    [SerializeField] private TextMeshProUGUI _satisfactionText;
    [SerializeField] private TextMeshProUGUI _finalVoteText;

    private const float StatValueColumnGap = 16f;
    private const float FallbackLabelColumnWidth = 320f;

    [Header("Continue Button")]
    [SerializeField] private Button _continueButton;
    [SerializeField] private TextMeshProUGUI _continueButtonText;

    [Header("SFX")]
    [SerializeField] private AudioClip _panelDropClip;
    [SerializeField] private AudioClip _stampClip;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Blur Settings")]
    [SerializeField] private PostProcessVolume _blurVolume;
    [SerializeField] private float _blurAperture = 1.2f;
    [SerializeField] private float _blurFocusDistance = 0.1f;

    [Header("Animation Settings")]
    [SerializeField] private float _backgroundFadeDuration = 0.3f;
    [SerializeField] private float _panelSlideDuration = 0.6f;
    [SerializeField] private float _counterDuration = 0.8f;
    [SerializeField] private float _textRevealDelay = 1.0f;
    [SerializeField] private float _panelHiddenY = 1000f;
    [SerializeField] private float _panelTargetY = -25f;
    [SerializeField] private float _buttonFadeDuration = 0.4f;
    [SerializeField] private float _bumpDuration = 0.3f;
    [SerializeField] private float _bumpScale = 1.2f;

    [Header("Transition")]
    [SerializeField] private ScreenFade _screenFade;

    private SessionStats _currentStats;
    private readonly Dictionary<TextMeshProUGUI, float> _baseFontSizes = new();

    private void Awake()
    {
        PrepareStatTexts();
        AlignStatValues();
    }

    public void Show(SessionStats stats)
    {
        _currentStats = stats;
        ShowAsync(stats).Forget();
    }

    private async UniTaskVoid ShowAsync(SessionStats stats)
    {
        if (_rootPanel != null) _rootPanel.SetActive(true);

        if (_background != null)
        {
            Color bgColor = _background.color;
            bgColor.a = 0f;
            _background.color = bgColor;
        }
        _statPanel.anchoredPosition = new Vector2(_statPanel.anchoredPosition.x, _panelHiddenY);

        if (_continueButton != null)
            _continueButton.interactable = false;

        if (_continueButtonText != null)
        {
            Color btnColor = _continueButtonText.color;
            btnColor.a = 0f;
            _continueButtonText.color = btnColor;
        }

        _screenFade?.Hide();

        ClearTexts();

        await AnimateBlurIn();

        PlayDropSfx();

        await CanvasAnimator.MoveY(_statPanel, _panelHiddenY, _panelTargetY, _panelSlideDuration, CanvasAnimator.Easing.EaseInOut);

        var (gainedWord, lostWord, satisfactionWord) = await UniTask.WhenAll(
            LocalizationService.GetLocalizedStringAsync(LocalizationService.MainTable, stats.GainedWordKey),
            LocalizationService.GetLocalizedStringAsync(LocalizationService.MainTable, stats.LostWordKey),
            LocalizationService.GetLocalizedStringAsync(LocalizationService.MainTable, stats.GeneralSatisfactionWordKey)
        );

        AlignStatValues();

        await AnimateWordReveal(_peopleGainedText, gainedWord);
        await BumpScale(_peopleGainedText);
        await UniTask.Delay(Mathf.RoundToInt(_textRevealDelay * 1000), DelayType.UnscaledDeltaTime);

        await AnimateWordReveal(_peopleLostText, lostWord);
        await BumpScale(_peopleLostText);
        await UniTask.Delay(Mathf.RoundToInt(_textRevealDelay * 1000), DelayType.UnscaledDeltaTime);

        await AnimateWordReveal(_satisfactionText, satisfactionWord);
        await BumpScale(_satisfactionText);
        await UniTask.Delay(Mathf.RoundToInt(_textRevealDelay * 1000), DelayType.UnscaledDeltaTime);

        await AnimateGrade(_finalVoteText, stats.Grade);
        await UniTask.Delay(Mathf.RoundToInt(_textRevealDelay * 1000), DelayType.UnscaledDeltaTime);

        await FadeInButton();

        if (_continueButton != null)
            _continueButton.interactable = true;
    }

    private async UniTask AnimateWordReveal(TextMeshProUGUI text, string word)
    {
        if (text == null) return;

        text.gameObject.SetActive(true);
        text.rectTransform.localScale = Vector3.one;
        text.text = word;
        text.ForceMeshUpdate();

        float baseFontSize = GetBaseFontSize(text);
        float elapsed = 0f;
        float duration = _counterDuration * 0.5f;
        Color textColor = text.color;
        textColor.a = 0f;
        text.color = textColor;
        text.fontSize = baseFontSize * 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            textColor.a = t;
            text.color = textColor;
            text.fontSize = baseFontSize * Mathf.Lerp(0.5f, 1f, t);
            await UniTask.Yield();
        }

        textColor.a = 1f;
        text.color = textColor;
        text.fontSize = baseFontSize;
    }

    private async UniTask AnimateGrade(TextMeshProUGUI text, string grade)
    {
        if (text == null) return;

        text.text = grade;
        text.gameObject.SetActive(true);

        if (_sfxSource != null && _stampClip != null)
            _sfxSource.PlayOneShot(_stampClip);

        float elapsed = 0f;
        float duration = _counterDuration * 0.5f;
        Color textColor = text.color;
        textColor.a = 0f;
        text.color = textColor;
        text.rectTransform.localScale = Vector3.one * 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            textColor.a = t;
            text.color = textColor;
            text.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.2f, t);
            await UniTask.Yield();
        }

        elapsed = 0f;
        float settleDuration = 0.15f;
        while (elapsed < settleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / settleDuration);
            text.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.2f, 1f, t);
            await UniTask.Yield();
        }

        textColor.a = 1f;
        text.color = textColor;
        text.rectTransform.localScale = Vector3.one;
    }

    private async UniTask AnimateBlurIn()
    {
        if (_blurVolume == null || _blurVolume.profile == null) return;

        if (!_blurVolume.profile.TryGetSettings<DepthOfField>(out var dof)) return;

        float startAperture = dof.aperture.value;
        float startFocus = dof.focusDistance.value;

        await CanvasAnimator.AnimateFloat(t =>
        {
            dof.aperture.value = Mathf.Lerp(startAperture, _blurAperture, t);
            dof.focusDistance.value = Mathf.Lerp(startFocus, _blurFocusDistance, t);
        }, _backgroundFadeDuration, CanvasAnimator.Easing.SmoothStep);
    }

    private void PlayDropSfx()
    {
        if (_sfxSource != null && _panelDropClip != null)
            _sfxSource.PlayOneShot(_panelDropClip);
    }

    private async UniTask BumpScale(TextMeshProUGUI text)
    {
        if (text == null) return;

        float baseFontSize = GetBaseFontSize(text);
        float half = _bumpDuration * 0.5f;
        float elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            text.fontSize = baseFontSize * Mathf.Lerp(1f, _bumpScale, t);
            await UniTask.Yield();
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / half);
            text.fontSize = baseFontSize * Mathf.Lerp(_bumpScale, 1f, t);
            await UniTask.Yield();
        }

        text.fontSize = baseFontSize;
    }

    private async UniTask FadeInButton()
    {
        if (_continueButtonText != null)
            await CanvasAnimator.FadeGraphic(_continueButtonText, 0f, 1f, _buttonFadeDuration, CanvasAnimator.Easing.EaseInOut);
    }

    private float GetBaseFontSize(TextMeshProUGUI text)
    {
        if (!_baseFontSizes.TryGetValue(text, out float baseSize))
        {
            baseSize = text.fontSize;
            _baseFontSizes[text] = baseSize;
        }

        return baseSize;
    }

    private void ResetTextVisual(TextMeshProUGUI text)
    {
        if (text == null) return;

        text.rectTransform.localScale = Vector3.one;
        if (_baseFontSizes.TryGetValue(text, out float baseSize))
            text.fontSize = baseSize;
    }

    private void ClearTexts()
    {
        if (_peopleGainedText != null) { ResetTextVisual(_peopleGainedText); _peopleGainedText.text = ""; _peopleGainedText.gameObject.SetActive(false); }
        if (_peopleLostText != null) { ResetTextVisual(_peopleLostText); _peopleLostText.text = ""; _peopleLostText.gameObject.SetActive(false); }
        if (_satisfactionText != null) { ResetTextVisual(_satisfactionText); _satisfactionText.text = ""; _satisfactionText.gameObject.SetActive(false); }
        if (_finalVoteText != null) { ResetTextVisual(_finalVoteText); _finalVoteText.text = ""; _finalVoteText.gameObject.SetActive(false); }
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        if (_currentStats == null) return;
        RefreshAllWordsAsync().Forget();
    }

    private async UniTaskVoid RefreshAllWordsAsync()
    {
        var (gained, lost, satisfaction) = await UniTask.WhenAll(
            LocalizationService.GetLocalizedStringAsync(LocalizationService.MainTable, _currentStats.GainedWordKey),
            LocalizationService.GetLocalizedStringAsync(LocalizationService.MainTable, _currentStats.LostWordKey),
            LocalizationService.GetLocalizedStringAsync(LocalizationService.MainTable, _currentStats.GeneralSatisfactionWordKey)
        );

        if (_peopleGainedText != null) _peopleGainedText.text = gained;
        if (_peopleLostText != null) _peopleLostText.text = lost;
        if (_satisfactionText != null) _satisfactionText.text = satisfaction;

        AlignStatValues();
    }

    private void PrepareStatTexts()
    {
        foreach (var text in new[]
        {
            _peopleGainedLabel, _peopleLostLabel, _satisfactionLabel,
            _peopleGainedText, _peopleLostText, _satisfactionText
        })
        {
            if (text == null) continue;
            text.textWrappingMode = TextWrappingModes.NoWrap;
        }
    }

    private void AlignStatValues()
    {
        RectTransform parent = GetStatRowParent();
        if (parent == null) return;

        float parentWidth = parent.rect.width;

        AlignStatRow(_peopleGainedLabel, _peopleGainedText, -25f, parentWidth);
        AlignStatRow(_peopleLostLabel, _peopleLostText, -75f, parentWidth);
        AlignStatRow(_satisfactionLabel, _satisfactionText, -125f, parentWidth);
    }

    private void AlignStatRow(TextMeshProUGUI label, TextMeshProUGUI value, float anchoredY, float parentWidth)
    {
        float labelWidth = MeasureLabelWidth(label);
        float valueColumnX = labelWidth + StatValueColumnGap;
        float valueColumnWidth = Mathf.Max(parentWidth - valueColumnX, 100f);

        ConfigureLabelColumn(label, labelWidth, anchoredY);
        ConfigureValueColumn(value, valueColumnX, valueColumnWidth, anchoredY);
    }

    private static float MeasureLabelWidth(TextMeshProUGUI label)
    {
        if (label == null) return FallbackLabelColumnWidth;

        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.ForceMeshUpdate();
        float width = label.GetPreferredValues(label.text, 10000f, 10000f).x;
        return width > 0f ? width : FallbackLabelColumnWidth;
    }

    private RectTransform GetStatRowParent()
    {
        if (_peopleGainedLabel != null)
            return _peopleGainedLabel.rectTransform.parent as RectTransform;

        if (_peopleGainedText != null)
            return _peopleGainedText.rectTransform.parent as RectTransform;

        return null;
    }

    private static void ConfigureLabelColumn(TextMeshProUGUI label, float width, float anchoredY)
    {
        if (label == null) return;

        RectTransform rect = label.rectTransform;
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(0f, anchoredY);
        rect.sizeDelta = new Vector2(width, rect.sizeDelta.y);

        Vector4 margin = label.margin;
        margin.x = 0f;
        label.margin = margin;
    }

    private static void ConfigureValueColumn(TextMeshProUGUI value, float columnX, float width, float anchoredY)
    {
        if (value == null) return;

        RectTransform rect = value.rectTransform;
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(columnX, anchoredY);
        rect.sizeDelta = new Vector2(width, rect.sizeDelta.y);

        Vector4 margin = value.margin;
        margin.x = 0f;
        value.margin = margin;
    }

    public void Btn_ContinueToMenu()
    {
        TransitionToMenuAsync().Forget();
    }

    private async UniTaskVoid TransitionToMenuAsync()
    {
        if (_continueButton != null)
            _continueButton.interactable = false;

        if (_screenFade != null)
            await _screenFade.FadeInAsync();

        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Scene_Menu");
    }
}
