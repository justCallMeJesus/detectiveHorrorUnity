using UnityEngine;
using UnityEngine.UI;

public class InspectUI : MonoBehaviour
{
    [SerializeField] private PlayerInspector playerInspector;
    [SerializeField] private GameObject inspectPanel; // your Image's parent GO

    private void OnEnable()
    {
        playerInspector.OnInspectStateChanged += SetVisible;
    }

    private void OnDisable()
    {
        playerInspector.OnInspectStateChanged -= SetVisible;
    }

    private void SetVisible(bool visible)
    {
        inspectPanel.SetActive(visible);

        if(visible)
        {
            Debug.Log(playerInspector.TryGetCurrentlyInspectingSO().EvidenceName);
        }
    }
}