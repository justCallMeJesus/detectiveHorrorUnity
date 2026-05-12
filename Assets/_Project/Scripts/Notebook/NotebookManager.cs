using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Listens to notebook open/close events and reacts to the current inspection state.
/// When the notebook is opened while the player is inspecting something,
/// it saves the inspected object's data and RoomId (once only).
/// </summary>
public class NotebookManager : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Reference to the NotebookUI that drives open/close events.")]
    [SerializeField] private NotebookUI notebookUI;

    /// <summary>Stores each saved entry as (data SO, roomId, evidenceId).</summary>
    private readonly List<(InspectableDataSO data, int roomId, int evidenceId)> _savedEntries = new();

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (notebookUI == null)
            Debug.LogError("[NotebookManager] NotebookUI reference is missing!", this);
    }

    private void OnEnable()
    {
        if (notebookUI != null)
            notebookUI.OnNotebookStateChanged += OnNotebookStateChanged;
    }

    private void OnDisable()
    {
        if (notebookUI != null)
            notebookUI.OnNotebookStateChanged -= OnNotebookStateChanged;
    }

    // -------------------------------------------------------------------------

    private void OnNotebookStateChanged(bool isOpen)
    {
        if (!isOpen) return;

        PlayerInspector inspector = PlayerManager.Instance?.playerInspector;
        if (inspector == null) return;
        if (!inspector.IsInspecting) return;

        // We need the full InspectableObject to read RoomId.
        InspectableObject inspectable = (inspector.currentlyInspecting as Component)?.GetComponent<InspectableObject>();
        if (inspectable == null)
        {
            Debug.Log("[NotebookManager] Notebook opened while inspecting, but target is not an InspectableObject.");
            return;
        }

        InspectableDataSO data = inspectable.InspectableDataSO;
        if (data == null)
        {
            Debug.Log("[NotebookManager] Notebook opened while inspecting, but no InspectableDataSO found on target.");
            return;
        }

        int roomId = inspectable.RoomId;
        int evidenceId = inspectable.evidenceId;

        if (IsAlreadySaved(evidenceId))
        {
            Debug.Log($"[NotebookManager] '{data.EvidenceName}' (evidenceId {evidenceId}, Room {roomId}) is already saved — skipping.");
            return;
        }

        _savedEntries.Add((data, roomId, evidenceId));
        Debug.Log($"[NotebookManager] Saved '{data.EvidenceName}' (evidenceId {evidenceId}) from Room {roomId}.");
    }

    private bool IsAlreadySaved(int evidenceId)
    {
        return _savedEntries.Exists(e => e.evidenceId == evidenceId);
    }

    /// <summary>Read-only access to all saved entries for other systems (e.g. UI).</summary>
    public IReadOnlyList<(InspectableDataSO data, int roomId, int evidenceId)> SavedEntries => _savedEntries;
}