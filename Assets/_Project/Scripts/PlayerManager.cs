using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks which systems are currently active via a priority-sorted lock list.
/// Systems add/remove their own locks — this class knows nothing about them directly.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    private readonly List<(string id, int priority)> _activeLocks = new();

    public PlayerInspector playerInspector {  get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerInspector = GetComponent<PlayerInspector>();
    }

    /// <summary>
    /// Register that a system is now active.
    /// </summary>
    public void AddLock(string id, int priority)
    {
        // Avoid duplicate entries (e.g. if a system adds twice without removing)
        RemoveLock(id);

        _activeLocks.Add((id, priority));
        _activeLocks.Sort((a, b) => b.priority.CompareTo(a.priority)); // highest priority first
    }

    /// <summary>
    /// Unregister a system — it is no longer active.
    /// </summary>
    public void RemoveLock(string id)
    {
        _activeLocks.RemoveAll(l => l.id == id);
    }

    /// <summary>
    /// Returns true if something with HIGHER priority than the caller is active.
    /// Use this to decide whether your system should be blocked.
    /// </summary>
    public bool IsBlockedBy(int myPriority)
    {
        foreach (var l in _activeLocks)
        {
            if (l.priority > myPriority) return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if this specific system currently holds a lock.
    /// </summary>
    public bool HasLock(string id)
    {
        return _activeLocks.Exists(l => l.id == id);
    }

    /// <summary>
    /// Returns the id of the highest-priority active lock, or null if none.
    /// Useful for debugging or UI display.
    /// </summary>
    public string GetTopLock()
    {
        return _activeLocks.Count > 0 ? _activeLocks[0].id : null;
    }
}