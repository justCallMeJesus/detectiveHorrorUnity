using UnityEngine;

public interface IInspectable
{
    Transform InspectPoint { get; }
    void Inspect();
}