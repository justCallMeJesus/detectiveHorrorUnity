using UnityEngine;

[CreateAssetMenu(fileName = "RoomUIDataSO", menuName = "Scriptable Objects/RoomUIDataSO")]
public class RoomUIDataSO : ScriptableObject
{
    public string RoomName;

    public RoomId RoomID;
}