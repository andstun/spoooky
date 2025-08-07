using UnityEditor;
using UnityEngine;

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
