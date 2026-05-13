using TMPro;
using UnityEngine;

public class RoomPageController : MonoBehaviour
{
    [SerializeField]
    private GameObject roomInfo;
    [SerializeField]
    private TextMeshProUGUI roomName;

    [Header("Evidence List")]
    [Tooltip("Prefab containing an EvidenceUI component - reused for every evidence entry.")]
    [SerializeField] private EvidenceUI evidencePrefab;
    [Tooltip("Parent transform that holds the instantiated evidence rows. Cleared on each Populate call.")]
    [SerializeField] private Transform evidenceContainer;

    [SerializeField] private TMP_InputField evidenceNotes;

    [Header("Debug")]
    [Tooltip("RoomId whose saved entries will be printed when P is pressed.")]
    [SerializeField] private RoomId testRoomId;

    // -------------------------------------------------------------------------

    private NotebookManager _notebookManager;

    /// <summary>Tracks which evidence entry is currently open. -1 means none.</summary>
    private int _currentEvidenceId = -1;

    private void Awake()
    {
        _notebookManager = FindFirstObjectByType<NotebookManager>();
        if (_notebookManager == null)
            Debug.LogWarning("[RoomPageController] NotebookManager not found in scene.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            TestPrintRoomEntries();
    }

    // -------------------------------------------------------------------------

    public void Populate(RoomUIDataSO data)
    {
        roomName.text = data.RoomName;
        PopulateEvidence(data.RoomID);
    }

    private void PopulateEvidence(RoomId roomId)
    {
        // Clear existing children.
        foreach (Transform child in evidenceContainer)
            Destroy(child.gameObject);

        if (evidencePrefab == null)
        {
            Debug.LogError("[RoomPageController] evidencePrefab is not assigned!", this);
            return;
        }

        if (_notebookManager == null) return;

        int spawned = 0;
        foreach (var entry in _notebookManager.SavedEntries)
        {
            if (entry.roomId != roomId) continue;

            EvidenceUI instance = Instantiate(evidencePrefab, evidenceContainer);
            instance.evidenceName.text = entry.data.EvidenceName;

            // Capture evidenceId for the lambda closure.
            int capturedId = entry.evidenceId;
            instance.button.onClick.AddListener(() => OpenEvidence(capturedId));

            spawned++;
        }

        Debug.Log($"[RoomPageController] Populated {spawned} evidence entries for {roomId}.");
    }

    /// <summary>
    /// Prints all saved notebook entries that belong to the given roomId.
    /// </summary>
    public void PrintEntriesForRoom(RoomId roomId)
    {
        if (_notebookManager == null)
        {
            Debug.Log("[RoomPageController] Could not reach NotebookManager.");
            return;
        }

        bool found = false;
        foreach (var entry in _notebookManager.SavedEntries)
        {
            if (entry.roomId != roomId) continue;
            Debug.Log($"[RoomPageController] {roomId} | evidenceId {entry.evidenceId} | '{entry.data.EvidenceName}'");
            found = true;
        }

        if (!found)
            Debug.Log($"[RoomPageController] No saved entries for {roomId}.");
    }

    /// <summary>
    /// Test hook - press P in Play mode to print entries for testRoomId.
    /// </summary>
    private void TestPrintRoomEntries()
    {
        Debug.Log($"[RoomPageController] (Test) Printing saved entries for {testRoomId}...");
        PrintEntriesForRoom(testRoomId);
    }

    public void OpenEvidence(int evidenceId)
    {
        NotebookEntry entry = _notebookManager.GetEntry(evidenceId);
        if (entry == null)
        {
            Debug.LogWarning($"[RoomPageController] No entry found for evidenceId {evidenceId}.");
            return;
        }

        _currentEvidenceId = evidenceId;

        Debug.Log($"[RoomPageController] Opening evidence | " +
                  $"evidenceId: {entry.evidenceId} | " +
                  $"name: '{entry.data.EvidenceName}' | " +
                  $"room: {entry.roomId} | " +
                  $"hasNotes: {entry.HasNotes} | " +
                  $"hasDrawing: {entry.HasDrawing} | " +
                  $"hasPhoto: {entry.HasPhoto} | " +
                  $"notes: '{entry.playerNotes}' | " +
                  $"strokeCount: {entry.playerDrawingStrokes?.Count ?? 0}");

        LoadNotes();
    }

    // -------------------------------------------------------------------------
    // Notes
    // -------------------------------------------------------------------------

    /// <summary>
    /// Saves the current text in <see cref="evidenceNotes"/> to the open entry's
    /// playerNotes field via NotebookManager. Does nothing if no evidence is open.
    /// </summary>
    public void SaveNotes()
    {
        if (_currentEvidenceId == -1)
        {
            Debug.LogWarning("[RoomPageController] SaveNotes called but no evidence is currently open.");
            return;
        }

        if (evidenceNotes == null)
        {
            Debug.LogError("[RoomPageController] evidenceNotes TMP_InputField reference is missing!", this);
            return;
        }

        bool updated = _notebookManager.TryUpdateEntry(_currentEvidenceId, notes: evidenceNotes.text);
        if (updated)
            Debug.Log($"[RoomPageController] Saved notes for evidenceId {_currentEvidenceId}.");
        else
            Debug.LogWarning($"[RoomPageController] SaveNotes: no entry found for evidenceId {_currentEvidenceId}.");
    }

    /// <summary>
    /// Loads the saved playerNotes of the open entry into <see cref="evidenceNotes"/>.
    /// Does nothing if no evidence is open.
    /// </summary>
    public void LoadNotes()
    {
        if (_currentEvidenceId == -1)
        {
            Debug.LogWarning("[RoomPageController] LoadNotes called but no evidence is currently open.");
            return;
        }

        if (evidenceNotes == null)
        {
            Debug.LogError("[RoomPageController] evidenceNotes TMP_InputField reference is missing!", this);
            return;
        }

        NotebookEntry entry = _notebookManager.GetEntry(_currentEvidenceId);
        if (entry == null)
        {
            Debug.LogWarning($"[RoomPageController] LoadNotes: no entry found for evidenceId {_currentEvidenceId}.");
            return;
        }

        evidenceNotes.text = entry.playerNotes;
        Debug.Log($"[RoomPageController] Loaded notes for evidenceId {_currentEvidenceId}.");
    }
}