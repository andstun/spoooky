using UnityEngine;

[ExecuteAlways]
public class WallsViewSwitcher : MonoBehaviour
{
    [Header("Toggle Debug View (shows Debug Walls instead of Display Walls)")]
    public bool debugView = false;
    [SerializeField] private KeypressManager keypressManager;
    private Transform displayWalls;
    private Transform debugWalls;

    private bool lastDebugView;

    void Awake()
    {
        keypressManager.OnWPressed.AddListener(ApplyView);
    }

    void OnEnable()
    {
        FindChildren();
        ApplyView();
    }

    void Update()
    {
        if (debugView != lastDebugView)
        {
            ApplyView();
            lastDebugView = debugView;
        }
    }

    private void FindChildren()
    {
        displayWalls = transform.Find("Display Walls");
        debugWalls = transform.Find("Debug Walls");

        if (!displayWalls || !debugWalls)
        {
            // Debug.LogWarning("[WallsViewSwitcher] 'Display Walls' or 'Debug Walls' child is missing.");
        }
    }

    private void ApplyView()
    {
        if (displayWalls) displayWalls.gameObject.SetActive(!debugView);
        if (debugWalls) debugWalls.gameObject.SetActive(debugView);
    }
}
