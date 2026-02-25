using UnityEngine;

public class FollowerGuestTutorial : MonoBehaviour
{
    private FollowerGuestMovement _movement;
    private PlayerController _playerController;
    public Color MyColor { get; private set; }

    private void Awake()
    {
        TryGetComponent(out _movement);
    }

    public void SetUpTarget(Transform target)
    {
        _movement.SetUpTarget(target);
    }

    public void SetPlayer(PlayerController player)
    {
        _playerController = player;
    }
}
