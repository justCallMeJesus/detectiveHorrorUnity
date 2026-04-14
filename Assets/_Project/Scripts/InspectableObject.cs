using UnityEngine;

public class InspectableObject : MonoBehaviour, IInspectable
{
    [SerializeField] private string inspectMessage = "You inspect the object.";
    [SerializeField]
    private Transform inspectPoint;

    public Transform InspectPoint => inspectPoint != null ? inspectPoint : transform;


    public void Inspect()
    {
        Debug.Log(inspectMessage);
    }
}