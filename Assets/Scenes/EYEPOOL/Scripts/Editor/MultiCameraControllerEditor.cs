using UnityEditor;
using UnityEngine;

/// <summary>
/// Allows for master-slave relationship between cameras
/// </summary>
[CustomEditor(typeof(MultiCameraController))]
public class MultiCameraControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MultiCameraController script = (MultiCameraController)target;

        if (GUILayout.Button("Apply Settings To All Child Cameras"))
        {
            script.ApplyToAllCameras();
        }
    }
}
