using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Draws viewport-to-floor rays and lets you force the camera’s
/// orthographic footprint to an exact width × height (in metres).
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraFootprintDebugger : MonoBehaviour
{
    // ───── Desired footprint ───── 
    [Header("Footprint (metres)")]
    [Min(0.001f)] public float footprintWidth = 1.92f;
    [Min(0.001f)] public float footprintHeight = 1.08f;

    // ───── Floor plane ───── 
    [Header("Floor plane")]
    [Tooltip("World-space Y coordinate of the floor the projector hits.")]
    public float floorY = 0f;

    // ───── Gizmo look ───── 
    [Header("Gizmo appearance")]
    public Color gizmoColour = Color.cyan;

    public bool drawDebug = false;
    public bool drawCentre = true;

    private Camera cam;

    // initialisation

    private void OnEnable() => cam = GetComponent<Camera>();

    private void OnValidate() // runs when you edit values in inspector
    {
        cam = GetComponent<Camera>();
        ApplyFootprint(footprintWidth, footprintHeight);
    }

    /// <summary>
    /// Forces camera to render exactly a specific width/height
    /// Works by setting aspect + orthographicSize.
    /// </summary>
    public void ApplyFootprint(float width, float height, Camera targetCam = null)
    {
        if (!cam) cam = GetComponent<Camera>();
        var c = targetCam ? targetCam : cam;
        if (!c) return;

        c.orthographic = true;               // make sure we’re ortho
        c.aspect = width / height;    // width : height

        // Cache the latest values so the inspector stays in sync
        footprintWidth = width;
        footprintHeight = height;
    }

    // draws debug lines to outline the camera frustum
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (drawDebug == false) return;

        if (!cam) cam = GetComponent<Camera>();
        if (!cam) return;

        Gizmos.color = gizmoColour;

        // corners in viewport space (BL, BR, TR, TL)
        Vector2[] vps =
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f)
        };

        var worldCorners = new Vector3[4];

        for (int i = 0; i < vps.Length; i++)
        {
            Ray   r = cam.ViewportPointToRay(new Vector3(vps[i].x, vps[i].y, 0f));
            float t = (floorY - r.origin.y) / r.direction.y;   // solve for Y = floorY
            Vector3 hit = r.origin + r.direction * t;
            worldCorners[i] = hit;

            Debug.DrawRay(r.origin, r.direction * t, gizmoColour); // down-ray
        }

        // outline rectangle on the floor
        for (int i = 0; i < 4; i++)
        {
            Debug.DrawLine(worldCorners[i],
                           worldCorners[(i + 1) % 4],
                           gizmoColour);
        }

        // dot the centre point of the viewport rect
        if (drawCentre)
        {
            Ray centreRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            float tC      = (floorY - centreRay.origin.y) / centreRay.direction.y;
            Vector3 centre= centreRay.origin + centreRay.direction * tC;
            Gizmos.DrawSphere(centre, 0.05f);
            Debug.DrawRay(centreRay.origin, centreRay.direction * tC, gizmoColour);
        }
    }
#endif
}
