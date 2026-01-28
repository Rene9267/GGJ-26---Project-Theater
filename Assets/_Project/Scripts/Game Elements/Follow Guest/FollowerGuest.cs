using UnityEngine;

[RequireComponent(typeof(FollowerGuestMovement))]
public class FollowerGuest : MonoBehaviour
{
    public FollowerGuestMovement Movement;
    private readonly string StunGuestTag = "StunGuest";
    private PlayerController playerController;

    public void SetPlayer(PlayerController player) { playerController = player; }

    private void OnDisable()
    {
        playerController = null;
    }

    void Awake()
    {
        TryGetComponent(out Movement);
    }

    public void SetUpTarget(Transform target)
    {
        Movement.SetUpTarget(target);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag(StunGuestTag))
        {
            playerController.SetStunState();
        }
    }
}
