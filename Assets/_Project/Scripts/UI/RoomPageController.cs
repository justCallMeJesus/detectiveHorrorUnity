using TMPro;
using UnityEngine;

public class RoomPageController : MonoBehaviour
{
    [SerializeField]
    private GameObject roomInfo;
    [SerializeField]
    private TextMeshProUGUI roomName;

    [Header("Evidence List")]
    [Tooltip("Prefab containing an EvidenceUI component — reused for every evidence entry.")]
    [SerializeField] private EvidenceUI evidencePrefab;
    [Tooltip("Parent transform that holds the instantiated evidence rows. Cleared on each Populate call.")]
    [SerializeField] private Transform evidenceContainer;

    [Header("Debug")]
    [Tooltip("RoomId whose saved entries will be printed when P is pressed.")]
    [SerializeField] private int testRoomId;

    // -------------------------------------------------------------------------

    private NotebookManager _notebookManager;

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

    private void PopulateEvidence(int roomId)
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
            spawned++;
        }

        Debug.Log($"[RoomPageController] Populated {spawned} evidence entries for Room {roomId}.");
    }

    /// <summary>
    /// Prints all saved notebook entries that belong to the given roomId.
    /// </summary>
    public void PrintEntriesForRoom(int roomId)
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
            Debug.Log($"[RoomPageController] Room {roomId} | evidenceId {entry.evidenceId} | '{entry.data.EvidenceName}'");
            found = true;
        }

        if (!found)
            Debug.Log($"[RoomPageController] No saved entries for Room {roomId}.");
    }

    /// <summary>
    /// Test hook — press P in Play mode to print entries for testRoomId.
    /// </summary>
    private void TestPrintRoomEntries()
    {
        Debug.Log($"[RoomPageController] (Test) Printing saved entries for Room {testRoomId}...");
        PrintEntriesForRoom(testRoomId);
    }
}