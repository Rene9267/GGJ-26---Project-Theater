using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public static class CanvasAnimator
{
    public enum Easing { Linear, SmoothStep, BounceOut }

    private static float ApplyEase(float t, Easing ease)
    {
        return ease switch
        {
            Easing.SmoothStep => t * t * (3f - 2f * t),
            Easing.BounceOut => BounceOut(t),
            _ => t
        };
    }

    private static float BounceOut(float t)
    {
        if (t >= 1f) return 1f;
        return t + Mathf.Sin(t * Mathf.PI * 4.5f) * Mathf.Exp(-t * 4f) * 0.12f;
    }

    private static float Delta => Time.unscaledDeltaTime;

    public static async UniTask FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration, Easing ease = Easing.Linear)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Delta;
            float t = ApplyEase(Mathf.Clamp01(elapsed / duration), ease);
            cg.alpha = Mathf.Lerp(from, to, t);
            await UniTask.Yield();
        }
        cg.alpha = to;
    }

    public static async UniTask FadeGraphic(Graphic graphic, float from, float to, float duration, Easing ease = Easing.Linear)
    {
        float elapsed = 0f;
        Color c = graphic.color;
        while (elapsed < duration)
        {
            elapsed += Delta;
            float t = ApplyEase(Mathf.Clamp01(elapsed / duration), ease);
            c.a = Mathf.Lerp(from, to, t);
            graphic.color = c;
            await UniTask.Yield();
        }
        c.a = to;
        graphic.color = c;
    }

    public static async UniTask MoveY(RectTransform rt, float from, float to, float duration, Easing ease = Easing.Linear)
    {
        float elapsed = 0f;
        Vector2 pos = rt.anchoredPosition;
        while (elapsed < duration)
        {
            elapsed += Delta;
            float t = ApplyEase(Mathf.Clamp01(elapsed / duration), ease);
            pos.y = Mathf.Lerp(from, to, t);
            rt.anchoredPosition = pos;
            await UniTask.Yield();
        }
        pos.y = to;
        rt.anchoredPosition = pos;
    }

    public static async UniTask Scale(RectTransform rt, float from, float to, float duration, Easing ease = Easing.Linear)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Delta;
            float t = ApplyEase(Mathf.Clamp01(elapsed / duration), ease);
            float s = Mathf.Lerp(from, to, t);
            rt.localScale = Vector3.one * s;
            await UniTask.Yield();
        }
        rt.localScale = Vector3.one * to;
    }

    public static async UniTask Counter(int targetValue, float duration, System.Action<string> onUpdate)
    {
        float elapsed = 0f;
        int lastDisplayed = -1;
        while (elapsed < duration)
        {
            elapsed += Delta;
            float t = Mathf.Clamp01(elapsed / duration);
            int current = Mathf.RoundToInt(Mathf.Lerp(0, targetValue, t));
            if (current != lastDisplayed)
            {
                lastDisplayed = current;
                onUpdate?.Invoke(current.ToString());
            }
            await UniTask.Yield();
        }
        onUpdate?.Invoke(targetValue.ToString());
    }
}
