using UnityEngine;

[RequireComponent(typeof(FollowerGuestMovement))]
public class FollowerGuest : MonoBehaviour
{
    public FollowerGuestMovement Movement;

    void Awake()
    {
        TryGetComponent(out Movement);
    }

    public void SetUpTarget(Transform target)
    {
        Movement.SetUpTarget(target);
    }

}
