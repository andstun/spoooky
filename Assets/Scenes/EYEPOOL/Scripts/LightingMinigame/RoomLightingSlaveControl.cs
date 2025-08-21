using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class RoomLightingSlaveControl : RoomLightingMasterControl
{
    [Header("Master Control Object Reference")]
    public RoomLightingMasterControl master;

    void Awake()
    {
        foreach (var light in areaLights)
        {
            if (light == null) continue;

            // Starting luminosity of 2
            light.intensity = 10f;
        }
    }
    void Update()
    {
        foreach (var light in areaLights)
        {
            if (light == null) continue;

            // Apply master object's master intensity
            light.intensity = master.masterIntensity + 2;
        }
    }
}
