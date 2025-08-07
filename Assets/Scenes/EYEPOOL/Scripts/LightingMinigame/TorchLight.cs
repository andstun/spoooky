using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Light))]
public class TorchFlicker : MonoBehaviour
{
    Light hd;
    float baseIntensity;

    void Awake()
    {
        hd = GetComponent<Light>();
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
