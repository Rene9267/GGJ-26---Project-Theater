using UnityEngine;

public class Actor : MonoBehaviour
{
    [SerializeField] private AudioSource _speaker;
    [SerializeField] private Animator _animator;

    private AudioClip myAudio;
    private int _animIDAct;

    private void Awake()
    {
        _animIDAct = Animator.StringToHash("IsActing");
    }

    public void PrepareMySpeach(AudioClip randomAudio)
    {
        myAudio = randomAudio;
        _animator.SetTrigger(_animIDAct);
    }

    public void PlayAudio()
    {
        _speaker.PlayOneShot(myAudio);
    }
}
