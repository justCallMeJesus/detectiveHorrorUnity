using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a crosshair image perfectly centred on screen.
/// Swap the sprite at any time — in the Inspector or via code — and it updates instantly.
/// 
/// Setup:
///   1. Create a Canvas (Screen Space – Overlay, pixel-perfect on).
///   2. Add an Image child GameObject to the Canvas.
///   3. Attach this script to that Image GameObject.
///   4. Assign your crosshair sprite to the "Crosshair Sprite" field.
/// </summary>
[RequireComponent(typeof(Image))]
[ExecuteAlways]   // previews changes in Edit mode too
public class Crosshair : MonoBehaviour
{
    [Header("Crosshair Sprite")]
    [Tooltip("Swap this at any time — including during Play mode — to change the crosshair.")]
    [SerializeField] private Sprite crosshairSprite;

    [Header("Appearance")]
    [SerializeField] private Vector2 size = new Vector2(32f, 32f);
    [SerializeField] private Color tint = Color.white;

    // ── Private refs ───────────────────────────────────────────────────────────
    private Image _image;
    private RectTransform _rect;

    // Track previous values so we only redraw when something actually changed
    private Sprite _lastSprite;
    private Vector2 _lastSize;
    private Color _lastTint;

    // ── Unity lifecycle ────────────────────────────────────────────────────────

    private void Awake() => GrabComponents();
    private void OnEnable() => Apply();

    private void Update()
    {
        // Lightweight dirty-check — works in both Edit and Play mode
        if (crosshairSprite != _lastSprite || size != _lastSize || tint != _lastTint)
            Apply();
    }

    // ── Internal ───────────────────────────────────────────────────────────────

    private void GrabComponents()
    {
        _image = GetComponent<Image>();
        _rect = GetComponent<RectTransform>();

        // Centre on the Canvas
        _rect.anchorMin = new Vector2(0.5f, 0.5f);
        _rect.anchorMax = new Vector2(0.5f, 0.5f);
        _rect.pivot = new Vector2(0.5f, 0.5f);
        _rect.anchoredPosition = Vector2.zero;

        _image.raycastTarget = false;   // crosshair should never block UI clicks
    }

    private void Apply()
    {
        if (_image == null) GrabComponents();

        _image.sprite = crosshairSprite;
        _image.color = tint;

        // Use a plain white box when no sprite is assigned so it's still visible
        _image.type = Image.Type.Simple;
        _image.preserveAspect = false;

        _rect.sizeDelta = size;
        _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

        // Cache to avoid redundant updates
        _lastSprite = crosshairSprite;
        _lastSize = size;
        _lastTint = tint;
    }

    // ── Public API — call these from any other script ──────────────────────────

    /// <summary>Swap the crosshair sprite at runtime.</summary>
    public void SetSprite(Sprite newSprite)
    {
        crosshairSprite = newSprite;
        Apply();
    }

    /// <summary>Change crosshair tint / opacity at runtime.</summary>
    public void SetTint(Color newTint)
    {
        tint = newTint;
        Apply();
    }

    /// <summary>Change crosshair size at runtime.</summary>
    public void SetSize(Vector2 newSize)
    {
        size = newSize;
        Apply();
    }

    /// <summary>Show or hide the crosshair.</summary>
    public void SetVisible(bool visible) => _image.enabled = visible;

}