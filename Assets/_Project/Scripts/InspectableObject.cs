using UnityEngine;

public class InspectableObject : MonoBehaviour, IInspectable
{
    [SerializeField] private string inspectMessage = "You inspect the object.";
    [SerializeField]
    private Transform inspectPoint;

    [SerializeField]
    private InspectableDataSO data;

    public Transform InspectPoint => inspectPoint != null ? inspectPoint : transform;

    public InspectableDataSO InspectableDataSO => data;

    public void Inspect()
    {
        Debug.Log(inspectMessage);
    }
}