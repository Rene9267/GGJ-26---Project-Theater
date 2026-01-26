using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerSettings _playerSettings;
    
    private PlayerInput _playerInput;
    private Rigidbody _rb;
    private Vector2 moveInput;
    private bool isSprinting => _playerInput.actions["Run"].IsPressed();

    private void OnValidate()
    {
        if (_playerSettings == null)
        {
            Debug.LogWarning("PlayerSettings ScriptableObject is not assigned in PlayerMovement.");
        }
    }

    private void Awake()
    {
        if (TryGetComponent<Rigidbody>(out _rb) == false)
        {
            Debug.LogError("Rigidbody component missing from the player object.");
        }
        if(TryGetComponent<PlayerInput>(out var _playerInput) == false)
        {
            Debug.LogError("PlayerInput component missing from the player object.");
        }
    }

    private void Update() 
    {
        moveInput = _playerInput.actions["Move"].ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // Movimento basato sulla fisica
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);
        _rb.MovePosition(_rb.position + movement * _playerSettings.MoveSpeed * Time.fixedDeltaTime * (isSprinting ? _playerSettings.SprintMultiplier : 1f));

        if (movement != Vector3.zero)
        {
            transform.forward = movement;
        }
    }

}
