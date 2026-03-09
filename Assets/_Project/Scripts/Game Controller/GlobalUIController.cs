using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalUIController : MonoBehaviour
{
    public GameObject GamePlayUI;
    public CanvasGroup EndUI;

    [Header("Pause UI")]
    public GameObject PauseMenuUI; // Riferimento al pannello del menu di pausa

    [SerializeField] private TextMeshProUGUI _peopleNumber;
    [SerializeField] private CanvasGroup _fadeScreen;
    [SerializeField] private Animation _anim;
    [SerializeField] private Image _timerBarImage;

    private readonly string FadeIn = "AC_FadeInCanvas_Start";
    private readonly string EndGameFade = "AC_FadeOutCanvas_EndGame";
    private Coroutine _currentFadeRoutine;
    private CancellationTokenSource fadeCts;

    private void Start()
    {
        // Iscrizione all'evento della pausa
        if (PauseController.Instance != null)
        {
            PauseController.Instance.OnPauseToggled += HandlePauseState;
        }
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

    public async UniTask FadeCanvas(bool isFadeIn, float duration, CanvasGroup _fadeScreen)
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

    public void UpdateTimerBar(float progress)
    {
        if (_timerBarImage != null)
        {
            _timerBarImage.fillAmount = progress;
        }
    }
}