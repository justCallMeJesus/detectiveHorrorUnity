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

    // ── Private state ──────────────────────────────────────────────────────────
    private CharacterController _characterController;
    private Vector3 _verticalVelocity;
    private float _cameraPitch = 0f;

    // ── Unity lifecycle ────────────────────────────────────────────────────────

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        // Movement and look are blocked by anything — they hold no lock themselves,
        // so any active lock (Inspect, Notebook, Dialogue...) will block them.
        if (!PlayerManager.Instance.IsBlockedBy(0))
        {
            HandleMovement();
            HandleLook();
        }
    }

    // ── Movement ───────────────────────────────────────────────────────────────

    private void HandleMovement()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 move = transform.right * input.x
                     + transform.forward * input.y;

        _characterController.Move(move * (moveSpeed * Time.deltaTime));
    }

    // ── Look ───────────────────────────────────────────────────────────────────

    private void HandleLook()
    {
        Vector2 lookDelta = lookAction.action.ReadValue<Vector2>();

        bool isMouse = lookAction.action.activeControl?.device is Mouse;

        float sensitivity = isMouse ? mouseSensitivity
                                    : joystickSensitivity * Time.deltaTime;

        float yaw = lookDelta.x * sensitivity;
        float pitch = -lookDelta.y * sensitivity;

        transform.Rotate(Vector3.up * yaw);

        _cameraPitch = Mathf.Clamp(_cameraPitch + pitch, -verticalClampAngle, verticalClampAngle);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
    }
}