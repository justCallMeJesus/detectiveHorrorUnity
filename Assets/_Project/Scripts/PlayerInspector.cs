using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles object inspection. Registers its own lock with PlayerManager.
/// Blocked by anything with higher priority (e.g. Notebook open on top of it is fine,
/// but Inspect cannot START if something higher-priority is already active).
/// </summary>
public class PlayerInspector : MonoBehaviour
{
    private const string LockId = "Inspect";

    [SerializeField] private InputActionReference inspectAction;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float inspectRange = 3f;
    [SerializeField] private float transitionDuration = 0.35f;

    private IInspectable _currentTarget;
    private Transform _currentTargetTransform;

    private bool _isInspecting;
    private Coroutine _transitionCoroutine;

    public bool IsInspecting => _isInspecting;
    public IInspectable currentlyInspecting {  get; private set; }
    public event System.Action<bool> OnInspectStateChanged;

    private Transform _camOriginalParent;
    private Vector3 _camOriginalLocalPos;
    private Quaternion _camOriginalLocalRot;

    private InspectableDataSO currentlyInspectingData;

    // -------------------------------------------------------------------------

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
        if (_isInspecting) return;

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

    // -------------------------------------------------------------------------

    private void OnInspectPerformed(InputAction.CallbackContext ctx)
    {
        if (_isInspecting)
        {
            ExitInspect();
        }
        else if (_currentTarget != null)
        {
            // Don't start inspecting if something higher-priority is active (e.g. dialogue).
            // Notebook is HIGHER priority so it also blocks starting a new inspect,
            // which is the behaviour you had before with the NotebookOpen check.
            if (PlayerManager.Instance.IsBlockedBy(LockPriority.Inspect)) return;

            EnterInspect(_currentTarget.InspectableDataSO);
        }
    }

    // -------------------------------------------------------------------------

    private void EnterInspect(InspectableDataSO data)
    {
        _isInspecting = true;
        PlayerManager.Instance.AddLock(LockId, LockPriority.Inspect);
        currentlyInspectingData = data;
        OnInspectStateChanged?.Invoke(true);
        _currentTarget.Inspect();
        currentlyInspecting = _currentTarget;

        _camOriginalParent = playerCamera.transform.parent;
        _camOriginalLocalPos = playerCamera.transform.localPosition;
        _camOriginalLocalRot = playerCamera.transform.localRotation;

        playerCamera.transform.SetParent(null, worldPositionStays: true);

        Transform ip = _currentTarget.InspectPoint;
        Quaternion lookRot = Quaternion.LookRotation(_currentTargetTransform.position - ip.position);
        RestartTransition(ip.position, lookRot);
    }

    private void ExitInspect()
    {
        _isInspecting = false;
        PlayerManager.Instance.RemoveLock(LockId);
        currentlyInspectingData = null;
        OnInspectStateChanged?.Invoke(false);
        currentlyInspecting = null;

        Vector3 worldPos = _camOriginalParent != null
            ? _camOriginalParent.TransformPoint(_camOriginalLocalPos)
            : _camOriginalLocalPos;
        Quaternion worldRot = _camOriginalParent != null
            ? _camOriginalParent.rotation * _camOriginalLocalRot
            : _camOriginalLocalRot;

        RestartTransition(worldPos, worldRot, onComplete: () =>
        {
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
        playerCamera.transform.SetPositionAndRotation(toPos, toRot);
        onComplete?.Invoke();
    }

    public InspectableDataSO TryGetCurrentlyInspectingSO()
    {
        if(currentlyInspectingData != null)
        {
            return currentlyInspectingData;
        }
        // else
        return null;
    }
}