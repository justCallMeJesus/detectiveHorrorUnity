using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Listens to notebook open/close events and reacts to the current inspection state.
/// When the notebook is opened while the player is inspecting something:
///   - saves the inspected object's data and RoomId (once only), and
///   - immediately navigates to the room page the object belongs to.
/// </summary>
public class NotebookManager : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Reference to the NotebookUI that drives open/close events.")]
    [SerializeField] private NotebookUI notebookUI;

    [Header("Room Data")]
    [Tooltip("All RoomUIDataSOs in the game. Order does not matter ? looked up by RoomID enum.")]
    [SerializeField] private RoomUIDataSO[] roomDataEntries;

    private readonly List<NotebookEntry> _savedEntries = new();

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

        RoomId roomId = inspectable.RoomId;
        int evidenceId = inspectable.evidenceId;

        // --- Save (once only) ------------------------------------------------
        if (!IsAlreadySaved(evidenceId))
        {
            _savedEntries.Add(new NotebookEntry(data, roomId, evidenceId));
            Debug.Log($"[NotebookManager] Saved '{data.EvidenceName}' (evidenceId {evidenceId}) from {roomId}.");
        }
        else
        {
            Debug.Log($"[NotebookManager] '{data.EvidenceName}' (evidenceId {evidenceId}, {roomId}) is already saved ? skipping save.");
        }

        // --- Navigate to the object's room page ------------------------------
        RoomUIDataSO roomData = GetRoomData(roomId);
        if (roomData != null)
        {
            notebookUI.OpenRoomPage(roomData);
            notebookUI.roomPageController.OpenEvidence(evidenceId);
            Debug.Log($"[NotebookManager] Opened room page for {roomId} and selected evidenceId {evidenceId}.");
        }
        else
        {
            Debug.LogWarning($"[NotebookManager] No RoomUIDataSO found for {roomId} ? falling back to map page.");
        }
    }

    // -------------------------------------------------------------------------
    // Entry mutation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Updates the player-authored fields of an existing entry.
    /// Pass null for any parameter you do not want to change.
    /// Returns true if an entry with the given evidenceId was found.
    /// </summary>
    public bool TryUpdateEntry(int evidenceId, string notes = null, UnityEngine.Sprite photo = null)
    {
        NotebookEntry entry = GetEntry(evidenceId);
        if (entry == null) return false;

        if (notes != null) entry.playerNotes = notes;
        if (photo != null) entry.playerPhoto = photo;

        return true;
    }

    /// <summary>
    /// Appends a stroke (list of points) to the drawing of an existing entry.
    /// Returns true if an entry with the given evidenceId was found.
    /// </summary>
    public bool TryAddDrawingStroke(int evidenceId, List<UnityEngine.Vector2> stroke)
    {
        NotebookEntry entry = GetEntry(evidenceId);
        if (entry == null) return false;

        entry.playerDrawingStrokes.Add(stroke);
        return true;
    }

    /// <summary>
    /// Clears all drawing strokes from an existing entry.
    /// Returns true if an entry with the given evidenceId was found.
    /// </summary>
    public bool TryClearDrawing(int evidenceId)
    {
        NotebookEntry entry = GetEntry(evidenceId);
        if (entry == null) return false;

        entry.playerDrawingStrokes.Clear();
        return true;
    }

    // -------------------------------------------------------------------------
    // Queries
    // -------------------------------------------------------------------------

    /// <summary>Read-only access to all saved entries (e.g. for UI display).</summary>
    public IReadOnlyList<NotebookEntry> SavedEntries => _savedEntries;

    /// <summary>Returns the entry with the given evidenceId, or null if not found.</summary>
    public NotebookEntry GetEntry(int evidenceId)
        => _savedEntries.Find(e => e.evidenceId == evidenceId);

    private bool IsAlreadySaved(int evidenceId)
        => _savedEntries.Exists(e => e.evidenceId == evidenceId);

    /// <summary>
    /// Finds the <see cref="RoomUIDataSO"/> whose RoomID matches the given enum value.
    /// Returns null if none is found.
    /// </summary>
    private RoomUIDataSO GetRoomData(RoomId roomId)
    {
        if (roomDataEntries == null) return null;

        foreach (RoomUIDataSO entry in roomDataEntries)
        {
            if (entry != null && entry.RoomID == roomId)
                return entry;
        }

        Debug.LogWarning($"[NotebookManager] No RoomUIDataSO entry found for {roomId}.");
        return null;
    }
}