using UnityEngine;

/// Trigger “hand” for an Augmenta person -- adds a colour-changing,
/// pulsing influence ring and orbits ONE ghost around it.
[RequireComponent(typeof(Collider))]
public class AugmentaPickup : MonoBehaviour
{
    /* ───────── Inspector / tuning ───────── */
    [Header("Orbit")]
    [SerializeField] float ringRadius   = 1.0f;
    [SerializeField] float velocity     = 1.0f;     // radians per second

    [Header("Ring Look")]
    [SerializeField] float ringStroke   = 0.05f;
    [SerializeField] int   ringSegments = 64;
    [SerializeField] float pulseAmplitude = 0.25f;  // +/-25 % width
    [SerializeField] float pulseSpeed     = 2.0f;   // Hz

    /* ───────── Private state ───────── */
    Ghost          carriedOrb;
    float        angle;
    LineRenderer ring;
    Material     ringMat;
    Color        currentClr  = Color.white;
    Color        targetClr   = Color.white;
    float        baseWidth;

    /* ───────── Setup ───────── */
    void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        BuildRing();
    }

    void BuildRing()
    {
        var go = new GameObject("InfluenceRing");
        go.transform.SetParent(transform, false);

        ring = go.AddComponent<LineRenderer>();
        ring.useWorldSpace = false;
        ring.loop          = true;
        ring.positionCount = ringSegments;
        baseWidth          = ringStroke;
        ring.startWidth = ring.endWidth = baseWidth;

        ringMat         = new Material(Shader.Find("Sprites/Default"));
        ringMat.color   = currentClr;
        ring.material   = ringMat;
        ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ring.receiveShadows    = false;

        Vector3[] pts = new Vector3[ringSegments];
        float step = 2 * Mathf.PI / ringSegments;
        for (int i = 0; i < ringSegments; i++)
        {
            float a = i * step;
            pts[i] = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * ringRadius;
        }
        ring.SetPositions(pts);
    }

    /* ───────── Picking up ───────── */
    void OnTriggerEnter(Collider other)
    {
        if (carriedOrb != null) return;              // already holding one

        if (other.TryGetComponent(out Ghost ghost) && !ghost.IsAttached)
        {
            carriedOrb = ghost;
            angle      = Random.value * 2 * Mathf.PI;
            targetClr  = ghost.ghostColor;               // ring fades to ghost colour

            ghost.AttachTo(transform);
        }
    }

    /* ───────── Per-frame ───────── */
    void Update()
    {
        /* 1) Orbit motion */
        if (carriedOrb != null)
        {
            angle += velocity * Time.deltaTime;
            Vector3 offs = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * ringRadius;
            carriedOrb.transform.localPosition = offs;
        }
        else
        {
            targetClr = Color.white;                 // fade back when empty
        }

        /* 2) Pulsing ring width */
        float pulse = 1 + Mathf.Sin(Time.time * Mathf.PI * pulseSpeed) * pulseAmplitude;
        ring.startWidth = ring.endWidth = baseWidth * pulse;

        /* 3) Smooth colour fade */
        currentClr = Color.Lerp(currentClr, targetClr, Time.deltaTime * 5f);
        ringMat.color = currentClr;
    }

    /* ───────── External helper for dropping ───────── */
    public void DropOrb(bool reachedCorrectSink)
    {
        if (carriedOrb == null) return;

        carriedOrb.Detach(reachedCorrectSink);
        carriedOrb = null;           // Update() will fade back to white
    }
}
