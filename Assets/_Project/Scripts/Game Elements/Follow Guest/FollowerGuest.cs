using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(FollowerGuestMovement))]
public class FollowerGuest : MonoBehaviour
{
    public event Action<Color> OnRunAway;

    [Header("UI Icon")]
    [SerializeField] private GameObject _hurryUpIcon;
    [SerializeField] private Animation _animation;

    [SerializeField] private StaticGuest_Controller _meshController;
    [SerializeField] private Animator _animator;

    private FollowerGuestMovement _movement;
    private readonly string _stunGuestTag = "StunGuest";
    private PlayerController playerController;
    private CancellationTokenSource _cts;
    private int _runAwayTimer;

    private Color MyColor;

    private readonly string _hurryUp = "AC_HurryUP";
    private int _animIDWalking;

    void OnValidate()
    {
        if(_hurryUpIcon == null)
        {
            DevLog.LogWarning($"[{this.gameObject}]: Hurry up icon mancante");
        }
    }

    private void OnDisable()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
        HurryUpEnd();
    }

    void Awake()
    {
        _meshController.SetRandomMaterialOnAwake = false;
        TryGetComponent(out _movement);
        _animIDWalking = Animator.StringToHash("IsWalking");
    }

    public void SetMyColor(Color chosenColor)
    {
        MyColor = chosenColor;
        _meshController.SetMaterialColor(MyColor);
    }
    void Update()
    {
        HandleAnimations();
    }

    private void HandleAnimations()
    {
        if (_animator == null || _movement == null) return;

        bool isWalking = _movement.CurrentSpeed > 1f;
        _animator.SetBool(_animIDWalking, isWalking);
    }

    public void SetUpTarget(Transform target, int runAwayTimer = 0)
    {
        _movement.SetUpTarget(target);

        if (runAwayTimer != 0)
        {
            _runAwayTimer = runAwayTimer;
            _cts = new();
            GuestDropTimerStart(_runAwayTimer, _cts.Token);
        }
    }

    private async void GuestDropTimerStart(int time, CancellationToken cts)
    {
        int halfTime = (int)(time * 0.5);
        try
        {
            await UniTask.Delay(halfTime, cancellationToken: cts);
            HurryUp();

            await UniTask.Delay(halfTime, cancellationToken: cts);
            TaskFailed();
        }
        catch (OperationCanceledException)
        {

        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(_stunGuestTag))
        {
            playerController.SetStunState();
        }
    }

    public void HurryUp()
    {
        _animation.Play(_hurryUp);
    }

    public void HurryUpEnd()
    {
        _hurryUpIcon.SetActive(false);
        _animation.Stop();
    }


    void TaskFailed()
    {
        if (playerController == null) return;

        if (MyColor != Color.clear)
        {
            playerController.GuestFamilyColor = Color.clear;
            OnRunAway?.Invoke(MyColor);
        }
    }

    public void CompleteMessageTask()
    {
        if (_cts == null) return;

        try
        {
            _cts.Cancel();
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
        }
    }

    public void SetPlayer(PlayerController player)
    { playerController = player; }
}
