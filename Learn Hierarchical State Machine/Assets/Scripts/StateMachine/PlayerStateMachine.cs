using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    PlayerInput _playerInput;
    CharacterController _characterController;
    Animator _animator;

    int _isWalkingHash;
    int _isRunningHash;
    int _isJumpingHash;


    Vector2 _currentMovementInput;
    Vector3 _currentMovement;
    Vector3 _currentRunMovement;
    Vector3 _appliedMovement;
    bool _isMovementPressed;
    bool _isRunPressed;


    float _rotationFactorPerFrame = 15.0f;
    float _runMultiplier = 3.0f;
    int _zero = 0;

    float _gravity = -9.8f;

    bool _isJumpPressed = false;
    float _initialJumpVelocity;
    float _maxJumpHeight = 1f;
    float _maxJumpTime = 0.75f;
    bool _isJumping = false;
    bool _requireNewJumpPress = false;

    PlayerBaseState _currentState;
    PlayerStateFactory _states;

    public PlayerInput PlayerInput { get => _playerInput; set => _playerInput = value; }
    public CharacterController CharacterController { get => _characterController; set => _characterController = value; }
    public Animator Animator { get => _animator; set => _animator = value; }
    public int IsWalkingHash { get => _isWalkingHash; set => _isWalkingHash = value; }
    public int IsRunningHash { get => _isRunningHash; set => _isRunningHash = value; }
    public int IsJumpingHash { get => _isJumpingHash; set => _isJumpingHash = value; }
    public Vector2 CurrentMovementInput { get => _currentMovementInput; set => _currentMovementInput = value; }
    public Vector3 CurrentMovement { get => _currentMovement; set => _currentMovement = value; }
    public float CurrentMovementY { get { return _currentMovement.y; } set { _currentMovement.y = value; } }
    public Vector3 CurrentRunMovement { get => _currentRunMovement; set => _currentRunMovement = value; }
    public Vector3 AppliedMovement { get => _appliedMovement; set => _appliedMovement = value; }
    public float AppliedMovementY { get { return _appliedMovement.y; } set { _appliedMovement.y = value; } }
    public float AppliedMovementX { get { return _appliedMovement.x; } set { _appliedMovement.x = value; } }
    public float AppliedMovementZ { get { return _appliedMovement.z; } set { _appliedMovement.z = value; } }
    public bool IsMovementPressed { get => _isMovementPressed; set => _isMovementPressed = value; }
    public bool IsRunPressed { get => _isRunPressed; set => _isRunPressed = value; }
    public float RotationFactorPerFrame { get => _rotationFactorPerFrame; set => _rotationFactorPerFrame = value; }
    public float RunMultiplier { get => _runMultiplier; set => _runMultiplier = value; }
    public int Zero { get => _zero; set => _zero = value; }
    public float Gravity { get => _gravity; }
    public bool IsJumpPressed { get => _isJumpPressed; set => _isJumpPressed = value; }
    public float InitialJumpVelocity { get => _initialJumpVelocity; set => _initialJumpVelocity = value; }
    public float MaxJumpHeight { get => _maxJumpHeight; set => _maxJumpHeight = value; }
    public float MaxJumpTime { get => _maxJumpTime; set => _maxJumpTime = value; }
    public bool IsJumping { get => _isJumping; set => _isJumping = value; }
    public bool RequireNewJumpPress { get => _requireNewJumpPress; set => _requireNewJumpPress = value; }
    public PlayerBaseState CurrentState { get => _currentState; set => _currentState = value; }
    public PlayerStateFactory States { get => _states; set => _states = value; }

    //public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
    //public Animator Animator { get { return _animator; } }
    //public int IsJumpingHash { get { return _isJumpingHash; } }
    //public bool IsJumpAnimating { set { _isJumpAnimating = value; } }
    //public bool IsJumping { set { _isJumping = value; } }
    //public bool IsJumpPressed { get { return _isJumpPressed; } }
    //public float CurrentMovementY { get { return _currentMovement.y; } set { _currentMovement.y = value; } }
    //public float AppliedMovementY { get { return _appliedMovement.y; } set { _appliedMovement.y = value; } }
    //public float InitialJumpVelocities { get { return _initialJumpVelocity; } }
    //public CharacterController CharacterController { get { return _characterController; } set { _characterController = value; } }
    //public float Gravity { get { return _gravity; } }

    private void Awake()
    {
        _playerInput = new PlayerInput();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        _states = new PlayerStateFactory(this);
        _currentState = _states.Grounded();
        _currentState.EnterState();

        _isWalkingHash = Animator.StringToHash("isWalking");
        _isRunningHash = Animator.StringToHash("isRunning");
        _isJumpingHash = Animator.StringToHash("isJumping");

        _playerInput.CharacterControls.Move.started += OnMovementInput;
        _playerInput.CharacterControls.Move.canceled += OnMovementInput;
        _playerInput.CharacterControls.Move.performed += OnMovementInput;
        _playerInput.CharacterControls.Run.started += OnRun;
        _playerInput.CharacterControls.Run.canceled += OnRun;
        _playerInput.CharacterControls.Jump.started += OnJump;
        _playerInput.CharacterControls.Jump.canceled += OnJump;

        SetupJumpVariables();
    }

    void SetupJumpVariables()
    {
        float timeToApex = _maxJumpTime / 2;
        float initialGravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        _initialJumpVelocity = (2 * _maxJumpHeight) / timeToApex;
    }
    void Start()
    {
        _characterController.Move(_appliedMovement * Time.deltaTime);
    }
    void Update()
    {
        HandleRotation();
        _currentState.UpdateStates();
        _characterController.Move(_appliedMovement * Time.deltaTime);
    }
    void HandleRotation()
    {
        Vector3 positionToLookAt;

        positionToLookAt.x = _currentMovementInput.x;
        positionToLookAt.y = _zero;
        positionToLookAt.z = _currentMovementInput.y;

        Quaternion currentRotation = transform.rotation;

        if (_isMovementPressed)
        {

            Quaternion _targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, _targetRotation, _rotationFactorPerFrame * Time.deltaTime);

        }
    }
    void OnMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();
        _isMovementPressed = _currentMovementInput.x != _zero || _currentMovementInput.y != _zero;
    }
    void OnJump(InputAction.CallbackContext context)
    {
        _isJumpPressed = context.ReadValueAsButton();
        _requireNewJumpPress = false;
    }

    void OnRun(InputAction.CallbackContext context)
    {
        _isRunPressed = context.ReadValueAsButton();
    }
    private void OnEnable()
    {
        _playerInput.CharacterControls.Enable();
    }
    private void OnDisable()
    {
        _playerInput.CharacterControls.Disable();
    }
}
