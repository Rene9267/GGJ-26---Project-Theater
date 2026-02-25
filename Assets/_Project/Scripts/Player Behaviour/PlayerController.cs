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
    private void OnValidate()
    {
        if (_playerSettings == null)
        {
            DevLog.LogWarning("[Player]: PlayerSettings ScriptableObject non assegnato.");
        }
        if (_animator == null)
        {
            DevLog.LogWarning("[Player]: animator non assegnato.");
        }
    }

    private void Awake()
    {
        if (!TryGetComponent(out _controller))
        {
            DevLog.LogError("[Player]: CharacterController mancante.");
        }

        if (GetComponent<PlayerInput>() == null)
        {
            DevLog.LogError("[Player]: Manca il componente PlayerInput!");
        }

        ActualMessage = new Message();
        ActualMessage.MessageColor = Color.clear;

        _animIDWalking = Animator.StringToHash("IsWalking");
        _animIDRunning = Animator.StringToHash("IsRunning");
        _animIDInteract = Animator.StringToHash("IsInteracting");
        _animIDSorry = Animator.StringToHash("IsStun");

        _sprintAction = _playerInput.actions["Sprint"];
    }

    public void OnMove(InputValue value)
    {
        _inputVector = value.Get<Vector2>();
    }


    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            TryInteract();
        }
    }
    // -------------------------------------------------------------------------

    private void UpdateAnimator()
    {
        if (_animator == null) return;
        bool walking;
        if (_inputVector.magnitude > 0.01f && _isSprinting == false)
        {
            walking = true;
        }
        else
            walking = false;
        _animator.SetBool(_animIDWalking, walking);


        bool sprinting;
        if (_inputVector.magnitude > 0.01f && _isSprinting == true)
        {
            sprinting = true;
        }
        else sprinting = false;
        _animator.SetBool(_animIDRunning, sprinting);


    }

    private void Update()
    {
        UpdateAnimator();

        if (_isStunned || _isInteracting) return;

        if (_sprintAction != null)
        {
            _isSprinting = _sprintAction.IsPressed();
        }
        HandleMovement();
    }

    private void HandleMovement()
    {
        // if(CanMove == false) return;

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
            DevLog.Log($"[{this.gameObject}]: Sto interagendo con {_currentInteractable}");
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
            _audioController.PlayOneShot(_audioController.MmhmmhSound, 1.2f,1);

            DevLog.Log($"[{this.gameObject}]: Ho interagito con {_currentInteractable}");
            _currentInteractable.Interact();
            StartCoroutine(Interaction(interactionDelay));
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag(_playerSettings.StunGuestTag))
        {
            SetStunState();
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

    public void SetStunState()
    {
        if (!_isStunned && !_isInvulnerable)
        {
            StartCoroutine(StunRoutine());
        }
    }

    public void ResetMessageColor()
    {
        ActualMessage.MessageColor = Color.clear;
    }

    private IEnumerator StunRoutine()
    {
        _isStunned = true;
        _animator.SetBool(_animIDSorry, _isStunned);
        _inputVector = Vector2.zero;
        _isSprinting = false;

        DevLog.Log("[Player]: Sbattuto contro uno spettatore");

        yield return new WaitForSeconds(1.2f);

        StartCoroutine(Invulnerableroutine());
        _isStunned = false;
        //_audioController.StopAudio();
        _animator.SetBool(_animIDSorry, _isStunned);
    }

    private IEnumerator Interaction(float interactionDelay)
    {
        _isInteracting = true;
        _inputVector = Vector2.zero;

        yield return new WaitForSeconds(interactionDelay);
        _animator.SetBool(_animIDInteract, false);
        _currentInteractable.CompleteInteraction();
        _isInteracting = false;
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


    public void SetHeadIcon(bool isHeart)
    {
        _isHeart = isHeart;
       
    }

    public void HandleIconHEadInteraction(Color color)
    {
        if (_iconCts != null) { _iconCts.Cancel(); _iconCts.Dispose(); _iconCts = null; }

        if (_isHeart)
        {
            _heart.gameObject.SetActive(true);
            _heart.GetComponent<Renderer>().material.color = color;
            _letter.gameObject.SetActive(false);
        }
        else
        {
            _letter.gameObject.SetActive(true);
            _letter.GetComponent<Renderer>().material.color = color;
            _heart.gameObject.SetActive(false);
        }
        _animation.Play(_messageSpawn);
    }

    public async void DisableHeadIcon()
    {
        _iconCts = new CancellationTokenSource();
        var token = _iconCts.Token;

        _animation.Play(_getMessage);

        try
        {
            await UniTask.Delay(1000, cancellationToken: token);

            _letter.gameObject.SetActive(false);
            _heart.gameObject.SetActive(false);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (_iconCts != null) { _iconCts.Dispose(); _iconCts = null; }
        }
    }
}