using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Light))]
public class FireplacePulse : MonoBehaviour
{
    Light hd;
    void Awake() => hd = GetComponent<Light>();

    void Update()
    {
        float pulse = 1.0f + Mathf.Sin(Time.time * 1.2f + Mathf.Sin(Time.time * 0.42f)) * 0.2f;
        hd.intensity = 2000f * pulse;
    }
}
