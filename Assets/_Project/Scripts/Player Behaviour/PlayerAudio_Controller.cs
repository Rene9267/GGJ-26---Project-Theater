using UnityEngine;

public class PlayerAudio_Controller : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    public AudioClip MmhmmhSound;

    public void PlayAudio(AudioClip newAudio)
    {
        _audioSource.pitch = 0.8f;
        _audioSource.volume = 0.8f;

        _audioSource.clip = newAudio;
        _audioSource.Play();
    }

    public void PlayOneShot(AudioClip newAudio, float volumeScale, float pitch)
    {
        _audioSource.pitch = pitch;

        _audioSource.PlayOneShot(newAudio, volumeScale);
    }

    public void PlayStep(AudioClip newAudio)
    {
        _audioSource.volume = 0.4f;
        _audioSource.pitch = Random.Range(0.75f, 1.15f);
        _audioSource.clip = newAudio;
        _audioSource.Play();
    }

    public void StopAudio()
    {
        _audioSource.Stop();
        _audioSource.clip = null;
    }
}
