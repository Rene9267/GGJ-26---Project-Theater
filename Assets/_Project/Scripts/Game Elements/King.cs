using UnityEngine;

public enum KingState
{
    Happy,
    Sad,
    Ok,
}

public class King : MonoBehaviour
{
    private Animator _animator;

    private int _animKingHappy;
    private int _animKingSad;
    private int _animKingOk;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animKingHappy = Animator.StringToHash("isHappy");
        _animKingSad = Animator.StringToHash("isSad");
        _animKingOk = Animator.StringToHash("isOk");
    }

    public void EndRate(KingState state)
    {
        switch (state)
        {
            case KingState.Happy:
                _animator.SetTrigger(_animKingHappy);
                break;
            case KingState.Sad:
                _animator.SetTrigger(_animKingSad);
                break;
            case KingState.Ok:
                _animator.SetTrigger(_animKingOk);
                break;
            default:
                break;
        }
    }

}
