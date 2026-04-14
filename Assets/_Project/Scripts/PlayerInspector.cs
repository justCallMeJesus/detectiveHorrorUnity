using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInspector : MonoBehaviour
{
    [SerializeField] private InputActionReference inspectAction;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float inspectRange = 3f;
    [SerializeField] private float inspectViewDistance = 1.5f;  // how far in front of the object the camera parks
    [SerializeField] private float transitionDuration = 0.35f;

    private IInspectable _currentTarget;
    private Transform _currentTargetTransform;   // stored from raycast, avoids needing Transform on IInspectable

    private bool _isInspecting;
    private Coroutine _transitionCoroutine;

    public bool IsInspecting => _isInspecting;
    public event System.Action<bool> OnInspectStateChanged;

    // saved so we can restore the camera exactly
    private Transform _camOriginalParent;
    private Vector3 _camOriginalLocalPos;
    private Quaternion _camOriginalLocalRot;

    private void OnEnable()
    {
        inspectAction.action.Enable();
        inspectAction.action.performed += OnInspectPerformed;
    }

    private void OnDisable()
    {
        inspectAction.action.performed -= OnInspectPerformed;
        inspectAction.action.Disable();
    }

    private void Update()
    {
        if (_isInspecting) return;  // lock targeting while focused

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * inspectRange, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, inspectRange))
        {
            _currentTarget = hit.collider.GetComponent<IInspectable>();
            _currentTargetTransform = _currentTarget != null ? hit.collider.transform : null;
        }
        else
        {
            _currentTarget = null;
            _currentTargetTransform = null;
        }
    }

    private void OnInspectPerformed(InputAction.CallbackContext ctx)
    {
        if (_isInspecting)
            ExitInspect();
        else if (_currentTarget != null)
            EnterInspect();
    }

    // ─────────────────────────────────────────────

    private void EnterInspect()
    {
        _isInspecting = true;
        OnInspectStateChanged?.Invoke(true);
        _currentTarget.Inspect();

        _camOriginalParent = playerCamera.transform.parent;
        _camOriginalLocalPos = playerCamera.transform.localPosition;
        _camOriginalLocalRot = playerCamera.transform.localRotation;

        playerCamera.transform.SetParent(null, worldPositionStays: true);

        Transform ip = _currentTarget.InspectPoint;
        // look FROM the inspect point TOWARD the object — rotation in the editor no longer matters
        Quaternion lookRot = Quaternion.LookRotation(_currentTargetTransform.position - ip.position);
        RestartTransition(ip.position, lookRot);
    }

    private void ExitInspect()
    {
        _isInspecting = false;
        OnInspectStateChanged?.Invoke(false);

        // convert saved local pose back to world space (parent may have moved)
        Vector3 worldPos = _camOriginalParent != null
            ? _camOriginalParent.TransformPoint(_camOriginalLocalPos)
            : _camOriginalLocalPos;
        Quaternion worldRot = _camOriginalParent != null
            ? _camOriginalParent.rotation * _camOriginalLocalRot
            : _camOriginalLocalRot;

        RestartTransition(worldPos, worldRot, onComplete: () =>
        {
            // snap & re-parent to eliminate any float drift
            playerCamera.transform.SetParent(_camOriginalParent, worldPositionStays: true);
            playerCamera.transform.localPosition = _camOriginalLocalPos;
            playerCamera.transform.localRotation = _camOriginalLocalRot;
        });
    }

    private void RestartTransition(Vector3 toPos, Quaternion toRot, System.Action onComplete = null)
    {
        if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
        _transitionCoroutine = StartCoroutine(
            MoveCamera(playerCamera.transform.position, playerCamera.transform.rotation,
                       toPos, toRot, onComplete));
    }

    private IEnumerator MoveCamera(Vector3 fromPos, Quaternion fromRot,
                                   Vector3 toPos, Quaternion toRot,
                                   System.Action onComplete = null)
    {
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);
            playerCamera.transform.SetPositionAndRotation(
                Vector3.Lerp(fromPos, toPos, t),
                Quaternion.Slerp(fromRot, toRot, t)
            );
            elapsed += Time.deltaTime;
            yield return null;
        }
        // guarantee exact end state
        playerCamera.transform.SetPositionAndRotation(toPos, toRot);
        onComplete?.Invoke();
    }
}