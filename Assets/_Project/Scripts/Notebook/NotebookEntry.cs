using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a single saved entry in the player's notebook.
/// Holds the core evidence data alongside any player-authored content
/// (notes, a drawing made of strokes, and an optional photo).
/// </summary>
[System.Serializable]
public class NotebookEntry
{
    // -------------------------------------------------------------------------
    // Core evidence data
    // -------------------------------------------------------------------------

    public InspectableDataSO data;
    public RoomId roomId;
    public int evidenceId;

    // -------------------------------------------------------------------------
    // Player-authored content
    // -------------------------------------------------------------------------

    /// <summary>Free-form text notes written by the player.</summary>
    public string playerNotes = "";

    /// <summary>
    /// Drawing stored as a list of strokes; each stroke is a list of
    /// screen-space points. Use a canvas component to read/write this.
    /// </summary>
    public List<List<Vector2>> playerDrawingStrokes = new();

    /// <summary>Optional photo/polaroid the player captured in-game.</summary>
    public Sprite playerPhoto;

    // -------------------------------------------------------------------------

    public NotebookEntry(InspectableDataSO data, RoomId roomId, int evidenceId)
    {
        this.data = data;
        this.roomId = roomId;
        this.evidenceId = evidenceId;
    }

    // -------------------------------------------------------------------------
    // Convenience helpers
    // -------------------------------------------------------------------------

    /// <summary>Returns true when the player has written at least one character of notes.</summary>
    public bool HasNotes => !string.IsNullOrWhiteSpace(playerNotes);

    /// <summary>Returns true when the player has drawn at least one stroke.</summary>
    public bool HasDrawing => playerDrawingStrokes != null && playerDrawingStrokes.Count > 0;

    /// <summary>Returns true when a photo has been assigned to this entry.</summary>
    public bool HasPhoto => playerPhoto != null;
}