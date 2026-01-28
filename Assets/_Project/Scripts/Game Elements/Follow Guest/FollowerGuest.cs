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

    private FollowerGuestMovement _movement;
    private readonly string _stunGuestTag = "StunGuest";
    private PlayerController playerController;
    private CancellationTokenSource _cts;
    private int _runAwayTimer;
    public Color MyColor;

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
        _hurryUpIcon.SetActive(false);
    }

    void Awake()
    {
        TryGetComponent(out _movement);
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

    private void HurryUp()
    {
        _hurryUpIcon.SetActive(true);
    }

    void TaskFailed()
    {
        if (playerController == null) return;

        if (MyColor != null && MyColor != Color.clear)
        {
            OnRunAway?.Invoke(MyColor);
            playerController.GuestFamilyColor = Color.clear;
        }
    }

    private void CompleteMessageTask()
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
