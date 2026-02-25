using UnityEngine;

public class TutorialUIController : MonoBehaviour
{
  [SerializeField] private AudioSource _audioSource;

    public void PlayMessageAudio(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
    }
    
    public void PlayAudio()
    {
        _audioSource.Play();
    }
}
