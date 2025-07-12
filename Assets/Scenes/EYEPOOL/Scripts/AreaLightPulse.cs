using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(HDAdditionalLightData))]
public class FireplacePulse : MonoBehaviour
{
    HDAdditionalLightData hd;
    void Awake() => hd = GetComponent<HDAdditionalLightData>();

    void Update()
    {
        float pulse = 1.0f + Mathf.Sin(Time.time * 1.2f + Mathf.Sin(Time.time * 0.42f)) * 0.08f;
        hd.intensity = 6000f * pulse;
    }
}
