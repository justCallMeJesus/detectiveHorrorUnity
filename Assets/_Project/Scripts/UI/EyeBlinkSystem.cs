using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the player "closing their eyes" when a button is held.
/// Uses the new Unity Input System via an InputActionReference,
/// assignable directly in the Inspector from any Input Action Asset.
///
/// Setup:
///   1. Create a Canvas (Screen Space - Overlay, Sort Order e.g. 100).
///   2. Add a full-screen Image child (stretch anchors, black color, Raycast Target off).
///   3. Assign that Image to the `overlayImage` field.
///   4. Assign an InputActionReference (from your .inputactions asset) to `closeEyesAction`.
///      The action should be a Button type.
/// </summary>
public class EyeBlinkSystem : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("Input")]
    [Tooltip("Assign a Button-type action from your Input Action Asset.")]
    [SerializeField] private InputActionReference closeEyesAction;

    [Header("Overlay")]
    [Tooltip("Full-screen black UI Image used to darken the screen.")]
    [SerializeField] private Image overlayImage;

    [Header("Fade Settings")]
    [Tooltip("Seconds to fully close / open eyes.")]
    [SerializeField] private float fadeSpeed = 3f;

    // -------------------------------------------------------------------------
    // Animation hook (future use)
    // -------------------------------------------------------------------------

    [Header("Animation (optional – wire up later)")]
    [Tooltip("Animator that contains the eye-closing animation. Leave empty until ready.")]
    [SerializeField] private Animator eyeAnimator;

    private const string AnimatorClosedParam = "EyesClosed";

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private float _targetAlpha = 0f;
    private float _currentAlpha = 0f;
    private bool _isHolding = false;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void OnEnable()
    {
        if (closeEyesAction == null)
        {
            Debug.LogWarning("[EyeBlinkSystem] No InputActionReference assigned.");
            return;
        }

        closeEyesAction.action.started += OnEyeActionStarted;
        closeEyesAction.action.canceled += OnEyeActionCanceled;
        closeEyesAction.action.Enable();
    }

    private void OnDisable()
    {
        if (closeEyesAction == null) return;

        closeEyesAction.action.started -= OnEyeActionStarted;
        closeEyesAction.action.canceled -= OnEyeActionCanceled;
        closeEyesAction.action.Disable();
    }

    private void Start()
    {
        if (overlayImage == null)
        {
            Debug.LogWarning("[EyeBlinkSystem] No overlay Image assigned.");
            return;
        }

        overlayImage.gameObject.SetActive(true);

        SetOverlayAlpha(0f);
    }

    private void Update()
    {
        if (overlayImage == null) return;

        _targetAlpha = _isHolding ? 1f : 0f;

        if (eyeAnimator != null)
            eyeAnimator.SetBool(AnimatorClosedParam, _isHolding);

        _currentAlpha = Mathf.MoveTowards(
            _currentAlpha,
            _targetAlpha,
            fadeSpeed * Time.deltaTime
        );

        SetOverlayAlpha(_currentAlpha);
    }

    // -------------------------------------------------------------------------
    // Input callbacks
    // -------------------------------------------------------------------------

    private void OnEyeActionStarted(InputAction.CallbackContext ctx) => _isHolding = true;
    private void OnEyeActionCanceled(InputAction.CallbackContext ctx) => _isHolding = false;

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void SetOverlayAlpha(float alpha)
    {
        Color c = overlayImage.color;
        c.a = alpha;
        overlayImage.color = c;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Returns true while the overlay is fully (or mostly) closed.</summary>
    public bool EyesAreClosed => _currentAlpha >= 0.99f;

    /// <summary>Programmatically force the eyes closed/open (e.g. cutscenes).</summary>
    public void ForceEyeState(bool closed) => _targetAlpha = closed ? 1f : 0f;
}