using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Opens and closes the player's notebook panel using Unity's uGUI Canvas system.
/// Attach to any GameObject in your scene (e.g. "NotebookManager").
///
/// Setup:
///   1. Create a Canvas > Panel GameObject for the notebook and assign it to notebookPanel.
///   2. Create an Input Action (e.g. "ToggleNotebook") and assign the reference below.
/// </summary>
public class NotebookUI : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Input Action that toggles the notebook (e.g. bound to N).")]
    public InputActionReference toggleNotebookAction;

    [Header("UI")]
    [Tooltip("The root Panel / GameObject of the notebook UI.")]
    public GameObject notebookPanel;

    // -------------------------------------------------------------------------

    private bool _isOpen = false;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (notebookPanel == null)
        {
            Debug.LogError("[NotebookUI] No notebook panel assigned!", this);
            return;
        }

        // Start hidden.
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
        SetPanelVisible(!_isOpen);
    }

    /// <summary>Call this from other scripts (e.g. a close button) to force open/close.</summary>
    public void SetPanelVisible(bool visible)
    {
        if (notebookPanel == null) return;

        _isOpen = visible;
        notebookPanel.SetActive(visible);

        Debug.Log($"[NotebookUI] Notebook {(visible ? "opened" : "closed")}.");
    }

    public bool IsOpen => _isOpen;
}