using Cysharp.Threading.Tasks;
using UnityEngine;

public static class AudioMiscellaneous
{
    public static async UniTask FadeAudio(AudioSource source, bool isFadeIn, float duration, float volume = 0)
    {
        if (source == null) return;

        float startVolume = isFadeIn ? 0f : source.volume;
        float endVolume = isFadeIn ? volume : 0f;
        float elapsed = 0f;

        if (isFadeIn && !source.isPlaying) source.Play();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            source.volume = Mathf.Lerp(startVolume, endVolume, progress);

            await UniTask.Yield();
        }

        source.volume = endVolume;

        if (!isFadeIn) source.Stop();
    }

}
