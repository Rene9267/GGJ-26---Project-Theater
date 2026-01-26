using UnityEngine;

public class GameController : MonoBehaviour
{
  [SerializeField] private GuestController _guestController;
  [SerializeField] private LightController _lightController;
  [SerializeField] private MessageController _messageController;

    private void OnValidate()
    {
        if (_guestController == null)
        {
            Debug.LogWarning("GuestController is not assigned in GameController.");
        }
        if (_lightController == null)
        {
            Debug.LogWarning("LightController is not assigned in GameController.");
        }
        if (_messageController == null)
        {
            Debug.LogWarning("MessageController is not assigned in GameController.");
        }
    }
}
