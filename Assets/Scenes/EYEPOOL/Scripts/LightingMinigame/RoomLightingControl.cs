using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class RoomLightingMasterControl : MonoBehaviour
{
    [Header("Master Intensity (Lumens)")]
    [Range(0, 10)]
    public float masterIntensity = 6f;

    [Header("Area Lights (Assign all 5)")]
    public Light[] areaLights;

    void Update()
    {
        foreach (var light in areaLights)
        {
            if (light == null) continue;

            // Force unit to Lumen (used for area lights)
            // light.intensityUnit = LightIntensityUnit.Lumen; // TODO: my unity might be a beta version, doesn't have this API

            // Apply master intensity
            light.intensity = masterIntensity;
        }
    }
}
