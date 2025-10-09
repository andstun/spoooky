using UnityEngine;
using UnityEngine.Rendering;
using Augmenta;  // Required for AugmentaObject

[RequireComponent(typeof(Light))]
public class TorchFlicker : MonoBehaviour
{
    Light hd;
    float baseIntensity;
    bool triggerBlackFlicker = false;

    void Awake()
    {
        hd = GetComponent<Light>();
        baseIntensity = hd.intensity;
    }

    void Update()
    {
        if (!triggerBlackFlicker)
        {
            // 3  <-> speed, 2 <-> roughness of flicker
            float n = Mathf.PerlinNoise(Time.time * 3f, 0);
            hd.intensity = baseIntensity * Mathf.Lerp(0.7f, 1.3f, n);

            // subtle colour shift
            hd.color = Color.Lerp(
                new Color(1f, 0.77f, 0.52f),   // deep orange
                new Color(1f, 0.88f, 0.6f),    // lighter
                n);
        }
        else
        {
            // 3  <-> speed, 2 <-> roughness of flicker
            float n = Mathf.PerlinNoise(Time.time * 8f, 0);
            hd.intensity = baseIntensity * Mathf.Lerp(0.8f, 0.01f, n);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        AugmentaObject augmenta = other.GetComponent<AugmentaObject>();
        if (augmenta == null)
        {
            return;
        }

        triggerBlackFlicker = true;
    }

    void OnTriggerExit(Collider other)
    {
        AugmentaObject augmenta = other.GetComponent<AugmentaObject>();
        if (augmenta == null)
        {
            return;
        }

        triggerBlackFlicker = false;
    }

    
}
