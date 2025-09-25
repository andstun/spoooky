using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Augmenta;  // Required for AugmentaObject

[RequireComponent(typeof(Collider))]
public class TunnelTrigger : MonoBehaviour
{
    [Header("Lighting Control")]
    public RoomLightingMasterControl lightingControl;
    [Range(0, 10)]
    public float maxLightValue = 10f;
    public float rampDuration = 10.0f;
    public float fadeDuration = 10.0f;
    public GameObject campFireLight;
    [SerializeField] private Vector3 initCampFireLightSize;
    [SerializeField] private Vector3 maxCampFireLightSize = new Vector3(1.5f, 1.5f, 1.5f);
    private Collider triggerZone;
    private readonly HashSet<AugmentaObject> usersInZone = new();
    private float lastActivityTime = Mathf.NegativeInfinity;
    private Coroutine fadeCoroutine;
    private Coroutine rampCoroutine;
    private bool lightHasTriggered = false;

    void Awake()
    {
        triggerZone = GetComponent<Collider>();
        if (!triggerZone.isTrigger)
            triggerZone.isTrigger = true;

        initCampFireLightSize = campFireLight.transform.localScale;
    }

    void Update()
    {
        Debug.Log($"Users in zone: {usersInZone.Count}");
        Debug.Log("lighting scale: " + campFireLight.transform.localScale);
        // Fade out if empty and inactive long enough
        if (usersInZone.Count == 0 && lightHasTriggered)
        {
            lightHasTriggered = false;
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(RampLight(lightingControl.masterIntensity, 0f, fadeDuration));
        }
        else if (usersInZone.Count > 0 && lightHasTriggered)
        {
            // Increase campfire light size up to max
            if (campFireLight.transform.localScale.x < maxCampFireLightSize.x && campFireLight.transform.localScale.x <= maxCampFireLightSize.x)
            {
                campFireLight.transform.localScale += new Vector3(0.0025f, 0.0025f, 0.0025f);
            }
        }
        else
        {
            // Reset campfire light size
            if (campFireLight.transform.localScale.x > initCampFireLightSize.x)
            {
                campFireLight.transform.localScale -= new Vector3(0.0025f, 0.0025f, 0.0025f);
            }
        }
    }

    public void TriggerEnter(Collider other)
    {
        AugmentaObject augmenta = other.GetComponent<AugmentaObject>();
        if (augmenta == null) return;

        bool wasEmpty = usersInZone.Count == 0;
        usersInZone.Add(augmenta);

        if (wasEmpty && !lightHasTriggered)
        {
            
            lightHasTriggered = true;
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            rampCoroutine = StartCoroutine(RampLight(lightingControl.masterIntensity, maxLightValue, rampDuration));
        }
    }

    void OnTriggerExit(Collider other)
    {
        AugmentaObject augmenta = other.GetComponent<AugmentaObject>();
        if (augmenta == null) return;

        usersInZone.Remove(augmenta);


        if (rampCoroutine != null && usersInZone.Count == 0)
        {
            StopCoroutine(rampCoroutine);
            rampCoroutine = null;
        }

    }

    IEnumerator RampLight(float start, float end, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            lightingControl.masterIntensity = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }
        lightingControl.masterIntensity = end;
    }
}
