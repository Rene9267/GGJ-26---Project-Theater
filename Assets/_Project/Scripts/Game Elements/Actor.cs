using UnityEngine;

public class Actor : MonoBehaviour
{
    #region Parameters
    [SerializeField] private AudioSource _speaker;
    [SerializeField] private Animator _animator;

    private AudioClip myAudio;
    private int _animIDAct;

    private static readonly int _bending = Animator.StringToHash("IsBending");

    #endregion  


    #region Unity Methods

    private void Awake()
    {
        _animIDAct = Animator.StringToHash("IsActing");
    }
    #endregion

    #region Class Methods
    public void PrepareMySpeach(AudioClip randomAudio)
    {
        myAudio = randomAudio;
        _animator.SetTrigger(_animIDAct);
    }

    public void PlayAudio()
    {
        _speaker.PlayOneShot(myAudio);
    }

    public void Bend()
    {
        _animator.SetBool(_bending, true);
    }
    #endregion
}
