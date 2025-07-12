using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(HDAdditionalLightData))]
public class TorchFlicker : MonoBehaviour
{
    HDAdditionalLightData hd;
    float baseIntensity;

    void Awake()
    {
        hd = GetComponent<HDAdditionalLightData>();
        baseIntensity = hd.intensity;
    }

    void Update()
    {
        // 3  <-> speed, 2 <-> roughness of flicker
        float n = Mathf.PerlinNoise(Time.time * 3f, 0);
        hd.intensity = baseIntensity * Mathf.Lerp(0.7f, 1.3f, n);

        // subtle colour shift
        hd.color = Color.Lerp(
            new Color(1f, 0.44f, 0.22f),   // deep orange
            new Color(1f, 0.55f, 0.3f),    // lighter
            n);
    }
}
