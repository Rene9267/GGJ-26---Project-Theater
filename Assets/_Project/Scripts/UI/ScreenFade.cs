using Cysharp.Threading.Tasks;
using UnityEngine;

public class ScreenFade : MonoBehaviour
{
    [Header("Canvas Groups")]
    [SerializeField] private CanvasGroup _backgroundCanvasGroup;
    [SerializeField] private CanvasGroup _textCanvasGroup;

    [Header("Fade In - Black then Text")]
    [SerializeField] private float _backgroundFadeInDuration = 0.5f;
    [SerializeField] private float _textFadeInDelay = 0.3f;
    [SerializeField] private float _textFadeInDuration = 0.5f;

    [Header("Fade Out - Black then Text")]
    [SerializeField] private float _backgroundFadeOutDuration = 0.5f;
    [SerializeField] private float _textFadeOutDelay = 0.3f;
    [SerializeField] private float _textFadeOutDuration = 0.5f;

    [Header("Block Raycasts")]
    [SerializeField] private bool _blockRaycastsWhenVisible = true;

    private void Awake()
    {
        if (_backgroundCanvasGroup != null)
        {
            _backgroundCanvasGroup.alpha = 0f;
            _backgroundCanvasGroup.blocksRaycasts = false;
        }
        if (_textCanvasGroup != null)
        {
            _textCanvasGroup.alpha = 0f;
            _textCanvasGroup.blocksRaycasts = false;
        }
    }

    public async UniTask FadeInAsync()
    {
        if (_backgroundCanvasGroup != null)
        {
            _backgroundCanvasGroup.blocksRaycasts = _blockRaycastsWhenVisible;
            await CanvasAnimator.FadeCanvasGroup(_backgroundCanvasGroup, 0f, 1f, _backgroundFadeInDuration);
        }

        if (_textFadeInDelay > 0f)
            await UniTask.Delay(Mathf.RoundToInt(_textFadeInDelay * 1000), DelayType.UnscaledDeltaTime);

        if (_textCanvasGroup != null)
            await CanvasAnimator.FadeCanvasGroup(_textCanvasGroup, 0f, 1f, _textFadeInDuration);
    }

    public void Hide()
    {
        if (_backgroundCanvasGroup != null)
        {
            _backgroundCanvasGroup.alpha = 0f;
            _backgroundCanvasGroup.blocksRaycasts = false;
        }
        if (_textCanvasGroup != null)
        {
            _textCanvasGroup.alpha = 0f;
            _textCanvasGroup.blocksRaycasts = false;
        }
    }

    public async UniTask FadeOutAsync()
    {
        if (_backgroundCanvasGroup != null)
        {
            _backgroundCanvasGroup.blocksRaycasts = false;
            await CanvasAnimator.FadeCanvasGroup(_backgroundCanvasGroup, 1f, 0f, _backgroundFadeOutDuration);
        }

        if (_textFadeOutDelay > 0f)
            await UniTask.Delay(Mathf.RoundToInt(_textFadeOutDelay * 1000), DelayType.UnscaledDeltaTime);

        if (_textCanvasGroup != null)
            await CanvasAnimator.FadeCanvasGroup(_textCanvasGroup, 1f, 0f, _textFadeOutDuration);
    }
}
