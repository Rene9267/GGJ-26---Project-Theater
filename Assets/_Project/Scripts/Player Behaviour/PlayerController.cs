using System.Collections;
using UnityEngine;

public struct Message
{
    public Color MessageColor;
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerSettings _playerSettings;

    private CharacterController _controller;
    private Vector3 _moveDirection;
    private bool _isStunned;
    private bool isSprinting => Input.GetKey(KeyCode.LeftShift);
    private bool _isInvulnerable;
    private bool _canInteract;

    public Message ActualMessage;

    public Color GuestFamilyColor = Color.clear;
    private IInteractable _currentInteractable;

    private void OnValidate()
    {
        if (_playerSettings == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("[Player]: PlayerSettings ScriptableObject is not assigned in PlayerMovement.");
#endif
        }
    }

    private void Awake()
    {
        if (TryGetComponent<CharacterController>(out _controller) == false)
        {
#if UNITY_EDITOR
            Debug.LogError("[Player]: PlayerInput component missing from the player object.");
#endif
        }

        ActualMessage = new Message();
        ActualMessage.MessageColor = Color.clear;
    }

    private void Update()
    {
        if (_isStunned) return;

        if (_canInteract && _currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("[Player]: Voglio Interagire");

            switch (_currentInteractable.InteactableType)
            {
                case InteractType.MessageReciver:
                    ActualMessage.MessageColor = Color.clear;
                    break;
                case InteractType.MessageSender:
                    ActualMessage.MessageColor = _currentInteractable.MyInteractionColor;
                    break;
                case InteractType.TakeGuest:
                    GuestFamilyColor = _currentInteractable.MyInteractionColor;
                    break;
                case InteractType.DrobGuest:
                    GuestFamilyColor = Color.clear;
                    break;

            }

            Debug.Log("[Player]: Ho Interatto");
            _currentInteractable.Interact();
        }

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(-x, 0, -z).normalized;

        if (input.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(input);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _playerSettings.RotationSpeed * Time.deltaTime);

            float currentSpeed = _playerSettings.MoveSpeed * (isSprinting ? _playerSettings.SprintMultiplier : 1f);
            _moveDirection = input * currentSpeed;
        }
        else
        {
            _moveDirection = Vector3.zero;
        }

        Vector3 finalVelocity = _moveDirection + (Physics.gravity * 0.5f);
        _controller.Move(finalVelocity * Time.deltaTime);
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag(_playerSettings.StunGuestTag) && !_isStunned && !_isInvulnerable)
        {
            StartCoroutine(StunRoutine());
        }
    }

    public void OnInteractionAreaEnter(IInteractable area)
    {
        _canInteract = true;
        _currentInteractable = area;
    }

    public void OnInteractionAreaExit()
    {
        _canInteract = false;
        _currentInteractable = null;
    }


    private IEnumerator StunRoutine()
    {
        _isStunned = true;

#if UNITY_EDITOR
        Debug.Log("[Player]: Sbattuto contro uno spettatore");
#endif
        yield return new WaitForSeconds(2f);

        StartCoroutine(Invulnerableroutine());
        _isStunned = false;
    }

    private IEnumerator Invulnerableroutine()
    {
        _isInvulnerable = true;

#if UNITY_EDITOR
        Debug.Log("[Player]: Sono Invulnerabile");
#endif
        yield return new WaitForSeconds(2f);

        _isInvulnerable = false;
    }
}
