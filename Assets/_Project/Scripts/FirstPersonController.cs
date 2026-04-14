using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float joystickSensitivity = 150f;
    [SerializeField] private float verticalClampAngle = 80f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    private PlayerInspector _inspector;

    private bool canMove = true;

    // ── Private state ──────────────────────────────────────────────────────────
    private CharacterController _characterController;
    private Vector3 _verticalVelocity;
    private float _cameraPitch = 0f;   // up / down rotation

    // ── Unity lifecycle ────────────────────────────────────────────────────────

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        // Fall back to main camera if nothing is assigned
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _inspector = GetComponentInParent<PlayerInspector>(); // or however you reference it
        _inspector.OnInspectStateChanged += OnInspectStateChanged;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();
    }

    private void Update()
    {
        if(!canMove) { return; }

        HandleMovement();
        HandleLook();

    }

    // ── Movement ───────────────────────────────────────────────────────────────

    private void HandleMovement()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        // Build a move direction relative to where the player is facing (Y-axis only)
        Vector3 move = transform.right * input.x
                     + transform.forward * input.y;

        _characterController.Move(move * (moveSpeed * Time.deltaTime));

        // Simple gravity
        //if (_characterController.isGrounded && _verticalVelocity.y < 0f)
        //    _verticalVelocity.y = -2f;   // small negative keeps isGrounded reliable

        //_verticalVelocity.y += gravity * Time.deltaTime;
        //_characterController.Move(_verticalVelocity * Time.deltaTime);
    }

    // ── Look ───────────────────────────────────────────────────────────────────

    private void HandleLook()
    {
        Vector2 lookDelta = lookAction.action.ReadValue<Vector2>();

        // Detect whether input comes from a mouse (deltaControl) or a gamepad stick.
        // Mouse deltas are already frame-relative; stick values need Time.deltaTime.
        bool isMouse = lookAction.action.activeControl?.device is Mouse;

        float sensitivity = isMouse ? mouseSensitivity
                                    : joystickSensitivity * Time.deltaTime;

        float yaw = lookDelta.x * sensitivity;   // left / right  → rotate body
        float pitch = -lookDelta.y * sensitivity;   // up   / down   → rotate camera

        // Rotate the player body horizontally
        transform.Rotate(Vector3.up * yaw);

        // Rotate the camera vertically (clamped)
        _cameraPitch = Mathf.Clamp(_cameraPitch + pitch, -verticalClampAngle, verticalClampAngle);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
    }

    private void OnDestroy()
    {
        _inspector.OnInspectStateChanged -= OnInspectStateChanged;
    }

    private void OnInspectStateChanged(bool inspecting)
    {
        canMove = !inspecting; // disables the whole look script while inspecting
    }
}