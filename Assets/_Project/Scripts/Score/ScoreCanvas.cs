using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCanvas : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _rootPanel;
    [SerializeField] private Graphic _background;
    [SerializeField] private RectTransform _statPanel;

    [Header("Stat Texts")]
    [SerializeField] private TextMeshProUGUI _peopleGainedText;
    [SerializeField] private TextMeshProUGUI _peopleLostText;
    [SerializeField] private TextMeshProUGUI _satisfactionText;

    [Header("Continue Button")]
    [SerializeField] private Button _continueButton;

    [Header("Animation Settings")]
    [SerializeField] private float _backgroundFadeDuration = 0.3f;
    [SerializeField] private float _panelSlideDuration = 0.6f;
    [SerializeField] private float _counterDuration = 0.8f;
    [SerializeField] private float _delayBetweenStats = 0.3f;
    [SerializeField] private float _panelHiddenY = 1040f;

    private string _suffix = "/10";

    public void Show(SessionStats stats)
    {
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
        _statPanel.localScale = Vector3.one;

        if (_continueButton != null)
            _continueButton.interactable = false;

        ClearTexts();

        if (_background != null)
            await CanvasAnimator.FadeGraphic(_background, 0f, 0.5f, _backgroundFadeDuration, CanvasAnimator.Easing.SmoothStep);
        await CanvasAnimator.MoveY(_statPanel, _panelHiddenY, 0f, _panelSlideDuration, CanvasAnimator.Easing.BounceOut);

        await AnimateStatCounter(_peopleGainedText, stats.GainedScore);
        await UniTask.Delay(Mathf.RoundToInt(_delayBetweenStats * 1000), DelayType.UnscaledDeltaTime);

        await AnimateStatCounter(_peopleLostText, stats.LostScore);
        await UniTask.Delay(Mathf.RoundToInt(_delayBetweenStats * 1000), DelayType.UnscaledDeltaTime);

        await AnimateSatisfaction(_satisfactionText, stats.SatisfactionLabel);
        await UniTask.Delay(Mathf.RoundToInt(_delayBetweenStats * 1000), DelayType.UnscaledDeltaTime);

        if (_continueButton != null)
            _continueButton.interactable = true;
    }

    private async UniTask AnimateStatCounter(TextMeshProUGUI text, int value)
    {
        if (text == null) return;

        text.text = "0";
        text.gameObject.SetActive(true);

        await CanvasAnimator.Counter(value, _counterDuration, s => text.text = s);

        if (!string.IsNullOrEmpty(_suffix))
            text.text = $"{value}{_suffix}";
    }

    private async UniTask AnimateSatisfaction(TextMeshProUGUI text, string label)
    {
        if (text == null) return;

        text.text = "";
        text.gameObject.SetActive(true);

        float elapsed = 0f;
        float duration = _counterDuration * 0.5f;
        Color textColor = text.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            textColor.a = t;
            text.color = textColor;
            await UniTask.Yield();
        }
        textColor.a = 1f;
        text.color = textColor;
        text.text = label;
    }

    private void ClearTexts()
    {
        if (_peopleGainedText != null) { _peopleGainedText.text = ""; _peopleGainedText.gameObject.SetActive(false); }
        if (_peopleLostText != null) { _peopleLostText.text = ""; _peopleLostText.gameObject.SetActive(false); }
        if (_satisfactionText != null) { _satisfactionText.text = ""; _satisfactionText.gameObject.SetActive(false); }
    }

    public void Btn_ContinueToMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Scene_Menu");
    }
}
