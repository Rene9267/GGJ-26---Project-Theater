using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalUIController : MonoBehaviour
{
    public GameObject GamePlayUI;
    public CanvasGroup EndUI;

    [Header("Pause UI")]
    public GameObject PauseMenuUI;

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI _peopleNumber;
    [SerializeField] private CanvasGroup _fadeScreen;
    [SerializeField] private Animation _anim;
    [SerializeField] private Image _timerBarImage;

    [Header("End Screen")]
    [SerializeField] private CanvasGroup _endScreenGroup;
    [SerializeField] private TextMeshProUGUI _endTitleText;
    [SerializeField] private TextMeshProUGUI _endGuestsSeatedText;
    [SerializeField] private TextMeshProUGUI _endGuestsLeftText;
    [SerializeField] private TextMeshProUGUI _endSatisfactionText;
    [SerializeField] private TextMeshProUGUI _endScoreText;
    [SerializeField] private string _menuScene = "MainMenu";

    private readonly string FadeIn = "AC_FadeInCanvas_Start";
    private readonly string EndGameFade = "AC_FadeOutCanvas_EndGame";
    private Coroutine _currentFadeRoutine;
    private CancellationTokenSource fadeCts;

    private void Start()
    {
        if (PauseController.Instance != null)
        {
            PauseController.Instance.OnPauseToggled += HandlePauseState;
        }

        TrySetupEndScreen();
    }

    private void TrySetupEndScreen()
    {
        if (_endScreenGroup != null) return;

        _endScreenGroup = new GameObject("EndScreenGroup", typeof(CanvasGroup)).GetComponent<CanvasGroup>();
        _endScreenGroup.gameObject.SetActive(false);
        _endScreenGroup.transform.SetParent(transform, false);

        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        if (canvas != null) _endScreenGroup.transform.SetParent(canvas.transform, false);

        var rect = _endScreenGroup.gameObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        CreateEndText(ref _endTitleText, "EndTitle", new Vector2(0, 200), 48, FontStyles.Bold);
        CreateEndText(ref _endGuestsSeatedText, "GuestsSeated", new Vector2(0, 100), 36);
        CreateEndText(ref _endGuestsLeftText, "GuestsLeft", new Vector2(0, 40), 36);
        CreateEndText(ref _endSatisfactionText, "Satisfaction", new Vector2(0, -20), 36);
        CreateEndText(ref _endScoreText, "Score", new Vector2(0, -100), 72, FontStyles.Bold);

        CreateEndButton("ContinueButton", new Vector2(0, -200), "CONTINUA");
    }

    private void CreateEndButton(string name, Vector2 anchoredPos, string label)
    {
        var go = new GameObject(name, typeof(CanvasRenderer));
        go.transform.SetParent(_endScreenGroup.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(250, 60);

        var img = go.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        var btn = go.AddComponent<UnityEngine.UI.Button>();
        btn.onClick.AddListener(HandleContinueFromEnd);

        var textGo = new GameObject("Text", typeof(TextMeshProUGUI));
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        var tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 32;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
    }

    private void HandleContinueFromEnd()
    {
        HideEndScreen();
        SceneManager.LoadScene(_menuScene);
    }

    private void CreateEndText(ref TextMeshProUGUI field, string name, Vector2 anchoredPos, int fontSize, FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject(name, typeof(TextMeshProUGUI));
        go.transform.SetParent(_endScreenGroup.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(400, 50);
        field = go.GetComponent<TextMeshProUGUI>();
        field.alignment = TextAlignmentOptions.Center;
        field.fontSize = fontSize;
        field.fontStyle = style;
        field.color = Color.white;
    }

    private void OnDestroy()
    {
        // Disiscrizione per evitare memory leaks
        if (PauseController.Instance != null)
        {
            PauseController.Instance.OnPauseToggled -= HandlePauseState;
        }
    }

    private void HandlePauseState(bool isPaused)
    {
        if (PauseMenuUI != null)
        {
            PauseMenuUI.SetActive(isPaused);
        }
    }

    public void SetPeopleNumber(int newCount)
    {
        _peopleNumber.text = newCount.ToString();
    }

    public void StartUp()
    {
        // _anim.Play(FadeIn);
    }

    public void EndGame()
    {
        _anim.Play(EndGameFade);
    }

    public async UniTask FadeCanvas(bool isFadeIn, float duration)
    {
        if (_fadeScreen == null) return;

        var cts = this.GetCancellationTokenOnDestroy();

        float startAlpha = _fadeScreen.alpha;
        float endAlpha = isFadeIn ? 1f : 0f;
        float elapsed = 0f;

        if (isFadeIn) _fadeScreen.blocksRaycasts = true;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            _fadeScreen.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);

            await UniTask.Yield(PlayerLoopTiming.Update, cts);
        }

        _fadeScreen.alpha = endAlpha;

        _fadeScreen.interactable = isFadeIn;
        _fadeScreen.blocksRaycasts = isFadeIn;
    }

    public async UniTask FadeCanvas(bool isFadeIn, float duration, CanvasGroup targetCanvas)
    {
        if (targetCanvas == null) return;

        var cts = this.GetCancellationTokenOnDestroy();

        float startAlpha = targetCanvas.alpha;
        float endAlpha = isFadeIn ? 1f : 0f;
        float elapsed = 0f;

        if (isFadeIn) targetCanvas.blocksRaycasts = true;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            targetCanvas.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);

            await UniTask.Yield(PlayerLoopTiming.Update, cts);
        }

        targetCanvas.alpha = endAlpha;

        targetCanvas.interactable = isFadeIn;
        targetCanvas.blocksRaycasts = isFadeIn;
    }

    public void UpdateTimerBar(float progress)
    {
        if (_timerBarImage != null)
        {
            _timerBarImage.fillAmount = progress;
        }
    }

    public void ShowEndScreen(bool isVictory, int remainingGuests, GameStatsTracker stats, float satisfaction, int score)
    {
        if (_endScreenGroup == null) return;

        if (_endTitleText != null)
            _endTitleText.text = isVictory ? "VITTORIA!" : "SCONFITTA";

        if (_endGuestsSeatedText != null && stats != null)
            _endGuestsSeatedText.text = stats.GuestsSeated.ToString();

        if (_endGuestsLeftText != null && stats != null)
            _endGuestsLeftText.text = stats.GuestsLeft.ToString();

        if (_endSatisfactionText != null)
            _endSatisfactionText.text = $"{Mathf.RoundToInt(satisfaction * 100)}%";

        if (_endScoreText != null)
            _endScoreText.text = isVictory ? $"{score}/10" : "-";

        _endScreenGroup.gameObject.SetActive(true);
        _endScreenGroup.alpha = 1f;
        _endScreenGroup.interactable = true;
        _endScreenGroup.blocksRaycasts = true;
    }

    public void HideEndScreen()
    {
        if (_endScreenGroup != null)
        {
            _endScreenGroup.gameObject.SetActive(false);
            _endScreenGroup.alpha = 0f;
            _endScreenGroup.interactable = false;
            _endScreenGroup.blocksRaycasts = false;
        }
    }
}