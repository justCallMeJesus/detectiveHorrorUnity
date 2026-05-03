using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Opens and closes the player's notebook panel.
/// Registers its own lock with PlayerManager — no direct knowledge of other systems needed.
/// </summary>
public class NotebookUI : MonoBehaviour
{
    private const string LockId = "Notebook";

    [Header("Input")]
    [Tooltip("Input Action that toggles the notebook (e.g. bound to N).")]
    public InputActionReference toggleNotebookAction;

    [Header("UI")]
    [Tooltip("The root Panel / GameObject of the notebook UI.")]
    public GameObject notebookPanel;

    public event System.Action<bool> OnNotebookStateChanged;

    private bool _isOpen = false;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (notebookPanel == null)
        {
            Debug.LogError("[NotebookUI] No notebook panel assigned!", this);
            return;
        }

        SetPanelVisible(false);
    }

    private void OnEnable()
    {
        if (toggleNotebookAction == null) return;

        toggleNotebookAction.action.Enable();
        toggleNotebookAction.action.performed += OnToggleNotebook;
    }

    private void OnDisable()
    {
        if (toggleNotebookAction == null) return;

        toggleNotebookAction.action.performed -= OnToggleNotebook;
        toggleNotebookAction.action.Disable();
    }

    // -------------------------------------------------------------------------

    private void OnToggleNotebook(InputAction.CallbackContext ctx)
    {
        // Notebook can open on top of Inspect, but nothing should block it at its priority.
        // If you ever want something to block the notebook, raise its priority in LockPriority.
        SetPanelVisible(!_isOpen);
    }

    /// <summary>Call this from other scripts (e.g. a close button) to force open/close.</summary>
    public void SetPanelVisible(bool visible)
    {
        if (notebookPanel == null) return;

        _isOpen = visible;
        notebookPanel.SetActive(visible);

        if (visible)
            PlayerManager.Instance.AddLock(LockId, LockPriority.Notebook);
        else
            PlayerManager.Instance.RemoveLock(LockId);

        OnNotebookStateChanged?.Invoke(visible);

        Debug.Log($"[NotebookUI] Notebook {(visible ? "opened" : "closed")}.");
    }

    public bool IsOpen => _isOpen;
}