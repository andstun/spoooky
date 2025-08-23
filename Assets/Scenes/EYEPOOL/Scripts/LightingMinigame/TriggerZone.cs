using UnityEngine;

[ExecuteAlways] // works in edit mode
public class TriggerZone : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool debugPlanes = false;

    [Header("Entrance Settings")]
    [Min(0f)]
    public float entranceTolerance = 0.5f;

    private void OnValidate()
    {
        // Adjust entrance colliders (only TunnelEntranceB/D)
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("TunnelEntrance"))
            {
                BoxCollider bc = child.GetComponent<BoxCollider>();
                if (bc != null)
                {
                    Vector3 size = bc.size;
                    size.y = entranceTolerance;
                    bc.size = size;
                }
            }
        }
    }

    void Update()
    {
        // Toggle all child MeshRenderers
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);
        foreach (var r in renderers)
        {
            r.enabled = debugPlanes;
        }
    }
}
