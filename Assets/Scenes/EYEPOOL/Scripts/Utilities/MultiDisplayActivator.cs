using UnityEngine;

/// Activates all connected displays and logs their resolutions.
public class MultiDisplayActivator : MonoBehaviour
{
    void Awake()
    {
        int displayCount = Display.displays.Length;
        // Debug.Log($"[MultiDisplayActivator] Detected {displayCount} display(s).");

        for (int i = 1; i < displayCount; i++)
        {
            Display.displays[i].Activate();
            // Debug.Log($"[MultiDisplayActivator] Activated Display {i}: {Display.displays[i].renderingWidth}x{Display.displays[i].renderingHeight}");
        }
    }
}
