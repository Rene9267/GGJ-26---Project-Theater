using UnityEngine;

public class Actor : MonoBehaviour
{
    [SerializeField] private AudioSource _speaker;
    [SerializeField] private Animation _myAnim;
    private AudioClip myAudio;
    private readonly string _animAct = "AC_Act";

    public void PrepareMySpeach(AudioClip randomAudio)
    {
        myAudio = randomAudio;
        _myAnim.Play(_animAct);
    }

    public void PlayAudio()
    {
        _speaker.PlayOneShot(myAudio);
    }
}
