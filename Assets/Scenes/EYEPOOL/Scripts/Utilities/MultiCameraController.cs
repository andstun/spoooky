using UnityEngine;
using UnityEditor;  // Required for buttons in the Inspector
using System.Collections.Generic;

// 

[ExecuteAlways]
public class MultiCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float fieldOfView = 60f;
    public float nearClipPlane = 0.1f;
    public float farClipPlane = 1000f;
    public CameraClearFlags clearFlags = CameraClearFlags.SolidColor;
    public Color backgroundColor = Color.black;
    public LayerMask cullingMask = ~0; // everything

    [ContextMenu("Apply Settings To All Child Cameras")]
    public void ApplyToAllCameras()
    {
        var cameras = GetComponentsInChildren<Camera>(includeInactive: true);

        foreach (Camera cam in cameras)
        {
            cam.fieldOfView = fieldOfView;
            cam.nearClipPlane = nearClipPlane;
            cam.farClipPlane = farClipPlane;
            cam.clearFlags = clearFlags;
            cam.backgroundColor = backgroundColor;
            cam.cullingMask = cullingMask;
        }

        // Debug.Log($"Applied settings to {cameras.Length} cameras under {name}.");
    }
}
