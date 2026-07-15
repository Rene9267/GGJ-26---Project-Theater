using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using Color = UnityEngine.Color;

public struct Message
{
    public Color MessageColor;
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerSettings _playerSettings;
    [SerializeField] private Animator _animator;
    [SerializeField] private Animation _animation;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private GameObject _letter;
    [SerializeField] private GameObject _heart;
    [SerializeField] private PlayerAudio_Controller _audioController;

    public bool CanMove = true;

    private CharacterController _controller;
    private Vector3 _moveDirection;

    private bool _isStunned;
    private bool _isSprinting;
    private bool _isInvulnerable;
    private bool _isInteracting;
    private bool _canInteract;

    private Vector2 _inputVector;

    private int _animIDWalking;
    private int _animIDRunning;
    private int _animIDInteract;
    private int _animIDSorry;

    public Message ActualMessage;
    public Color GuestFamilyColor = Color.clear;
    private IInteractable _currentInteractable;
    private InputAction _sprintAction;

    private readonly string _messageSpawn = "AC_MessageSpawn";
    private readonly string _getMessage = "AC_Player_GetMessage";

    private bool _isHeart;
    private CancellationTokenSource _iconCts;
    private int _collisionCount;
    public int CollisionCount => _collisionCount;

    private void OnValidate()
    {
        if (_playerSettings == null) DevLog.LogWarning("[Player]: PlayerSettings non assegnato.");
        if (_animator == null) DevLog.LogWarning("[Player]: animator non assegnato.");
    }

    private void Awake()
    {
        if (!TryGetComponent(out _controller)) DevLog.LogError("[Player]: CharacterController mancante.");

        ActualMessage = new Message
        {
            MessageColor = Color.clear
        };

        _animIDWalking = Animator.StringToHash("IsWalking");
        _animIDRunning = Animator.StringToHash("IsRunning");
        _animIDInteract = Animator.StringToHash("IsInteracting");
        _animIDSorry = Animator.StringToHash("IsStun");

        if (_playerInput != null)
            _sprintAction = _playerInput.actions["Sprint"];
    }

    public void OnMove(InputValue value)
    {
        // Se siamo in pausa, ignoriamo l'input
        if (PauseController.Instance != null && PauseController.Instance.IsPaused) return;

        _inputVector = value.Get<Vector2>();

        // LOG DI TEST: Rimuovilo una volta verificato che funziona
        if (_inputVector.magnitude > 0) DevLog.Log($"[INPUT] Movimento rilevato: {_inputVector}");
    }

    public void OnInteract(InputValue value)
    {
        if (PauseController.Instance != null && PauseController.Instance.IsPaused) return;

        if (value.isPressed)
        {
            TryInteract();
        }
    }

    public void OnPause(InputValue value)
    {
        if (value.isPressed)
        {
            // DevLog.Log("[INPUT] Tasto Pausa premuto!");
            if (PauseController.Instance != null)
                PauseController.Instance.TogglePause();
        }
    }

    private void Update()
    {
        if (PauseController.Instance != null && PauseController.Instance.IsPaused) return;

        UpdateAnimator();

        if (_isStunned || _isInteracting) return;

        if (_sprintAction != null)
            _isSprinting = _sprintAction.IsPressed();

        HandleMovement();
    }

    private void HandleMovement()
    {
        if (!CanMove) return;

        Vector3 input = new Vector3(-_inputVector.x, 0, -_inputVector.y).normalized;

        if (input.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(input);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _playerSettings.RotationSpeed * Time.deltaTime);

            float currentSpeed = _playerSettings.MoveSpeed * (_isSprinting ? _playerSettings.SprintMultiplier : 1f);
            _moveDirection = input * currentSpeed;
        }
        else
        {
            _moveDirection = Vector3.zero;
        }

        Vector3 finalVelocity = _moveDirection + (Physics.gravity * 0.5f);
        _controller.Move(finalVelocity * Time.deltaTime);
    }

    private void TryInteract()
    {
        if (_canInteract && _currentInteractable != null)
        {
            float interactionDelay = 0;
            switch (_currentInteractable.InteactableType)
            {
                case InteractType.MessageReciver:
                    _animator.SetBool(_animIDInteract, true);
                    interactionDelay = _playerSettings.Interaction_ReleaseMessageDelat;
                    ActualMessage.MessageColor = Color.clear;
                    DisableHeadIcon();
                    break;
                case InteractType.MessageSender:
                    _animator.SetBool(_animIDInteract, true);
                    interactionDelay = _playerSettings.Interaction_GetMessageDelay;
                    ActualMessage.MessageColor = _currentInteractable.MyInteractionColor;
                    HandleIconHEadInteraction(ActualMessage.MessageColor);
                    break;
                case InteractType.TakeGuest:
                    interactionDelay = _playerSettings.Interaction_GetGuest;
                    GuestFamilyColor = _currentInteractable.MyInteractionColor;
                    break;
                case InteractType.DrobGuest:
                    interactionDelay = _playerSettings.Interaction_Dropguest;
                    GuestFamilyColor = Color.clear;
                    break;
                case InteractType.Candle:
                    _animator.SetBool(_animIDInteract, true);
                    interactionDelay = _playerSettings.Interaction_TurnOnCandle;
                    break;
            }
            _audioController.PlayOneShot(_audioController.MmhmmhSound, 1.2f, 1);
            _currentInteractable.Interact();
            StartCoroutine(Interaction(interactionDelay, _currentInteractable));
        }
    }

    private void UpdateAnimator()
    {
        if (_animator == null) return;
        bool isMoving = _inputVector.magnitude > 0.01f;
        _animator.SetBool(_animIDWalking, isMoving && !_isSprinting);
        _animator.SetBool(_animIDRunning, isMoving && _isSprinting);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        _collisionCount++;
        if (hit.gameObject.CompareTag(_playerSettings.StunGuestTag)) SetStunState();
    }

    public void OnInteractionAreaEnter(IInteractable area) { _canInteract = true; _currentInteractable = area; }
    public void OnInteractionAreaExit() { _canInteract = false; _currentInteractable = null; }

    public void SetStunState() { if (!_isStunned && !_isInvulnerable) StartCoroutine(StunRoutine()); }
    public void ResetMessageColor() => ActualMessage.MessageColor = Color.clear;

    private IEnumerator StunRoutine()
    {
        _isStunned = true;
        _animator.SetBool(_animIDSorry, true);
        _inputVector = Vector2.zero;
        yield return new WaitForSeconds(1.2f);
        StartCoroutine(Invulnerableroutine());
        _isStunned = false;
        _animator.SetBool(_animIDSorry, false);
    }

    private IEnumerator Interaction(float delay, IInteractable target)
    {
        _isInteracting = true;
        _inputVector = Vector2.zero;
        yield return new WaitForSeconds(delay);
        _animator.SetBool(_animIDInteract, false);
        target?.CompleteInteraction();
        _isInteracting = false;
    }

    private IEnumerator Invulnerableroutine()
    {
        _isInvulnerable = true;
        yield return new WaitForSeconds(2f);
        _isInvulnerable = false;
    }

    public void SetHeadIcon(bool isHeart) => _isHeart = isHeart;

    public void HandleIconHEadInteraction(Color color)
    {
        if (_iconCts != null) { _iconCts.Cancel(); _iconCts.Dispose(); }
        if (_isHeart) { _heart.SetActive(true); _heart.GetComponent<Renderer>().material.color = color; _letter.SetActive(false); }
        else { _letter.SetActive(true); _letter.GetComponent<Renderer>().material.color = color; _heart.SetActive(false); }
        _animation.Play(_messageSpawn);
    }

    public async void DisableHeadIcon()
    {
        _iconCts = new CancellationTokenSource();
        _animation.Play(_getMessage);
        try { await UniTask.Delay(1000, cancellationToken: _iconCts.Token); _letter.SetActive(false); _heart.SetActive(false); }
        catch (OperationCanceledException) { }
    }
}