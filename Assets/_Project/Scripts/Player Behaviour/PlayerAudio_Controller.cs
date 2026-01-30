using UnityEngine;

public class PlayerAudio_Controller : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    public void PlayAudio(AudioClip newAudio)
    {
        _audioSource.clip = newAudio;
        _audioSource.Play();
    }

    public void StopAudio()
    {
        _audioSource.Stop();
        _audioSource.clip = null;
    }
}
