using UnityEngine;
using Augmenta;

/// Trigger “hand” for an Augmenta person -- adds a colour-changing,
/// pulsing influence ring and orbits ONE ghost around it.
[RequireComponent(typeof(Collider))]
public class AugmentaPickup : MonoBehaviour
{
    private AugmentaObject augmentaObject;

    public float speedToRingRadiusFactor = 0.5f; // TODO: this needs to be set in the client
    public float speedDifferenceThreshold = 0.1f;
    /* ───────── Inspector / tuning ───────── */
    [Header("Orbit")]
    [SerializeField] float ringRadius   = 1.0f;
    [SerializeField] float velocity     = 1.0f;     // radians per second

    [Header("Ring Look")]
    [SerializeField] float ringStroke   = 0.20f;
    [SerializeField] int   ringSegments = 64;
    [SerializeField] float pulseAmplitude = 0.25f;  // +/-25 % width
    [SerializeField] float pulseSpeed     = 2.0f;   // Hz

    [Header("Delays")]
    [SerializeField] float pickupDelay = 2.0f;

    /* ───────── Private state ───────── */
    Ghost        carriedOrb;
    float        angle;
    LineRenderer ring;
    Material     ringMat;
    Color        currentClr  = Color.white;
    Color        targetClr   = Color.white;
    float        baseWidth;

    private Ghost overlappingGhost;
    private float pickupTimer;
    private bool isOverlapping = false;

    private float lastSpeed = -1f;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = false;
        augmentaObject = GetComponent<AugmentaObject>();
        var augmentaObjects = Object.FindObjectsOfType<AugmentaObject>();
        BuildRing();
    }

    void Start()
    {
        // Debug.Log($"[Start] Augmenta object id: {augmentaObject.id}, oid: {augmentaObject.oid}");
    }

    // Create aura ring
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

    // Called in Update() for orbit logic (radius change)
    void UpdateRingRadius(float newRadius)
    {
        ringRadius = newRadius;

        Vector3[] pts = new Vector3[ringSegments];
        float step = 2 * Mathf.PI / ringSegments;
        for (int i = 0; i < ringSegments; i++)
        {
            float a = i * step;
            pts[i] = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * ringRadius;
        }
        ring.SetPositions(pts);
    }

    // Entered ghost collider
    void OnTriggerEnter(Collider other)
    {
        if (carriedOrb != null) return;

        if (other.TryGetComponent(out Ghost ghost) && ghost.state != Ghost.GhostState.Attached)
        {
            overlappingGhost = ghost;
            pickupTimer = 0f;
            isOverlapping = true;
        }
    }

    // Exited ghost collider
    void OnTriggerExit(Collider other)
    {
        if (overlappingGhost != null && other.gameObject == overlappingGhost.gameObject)
        {
            overlappingGhost = null;
            isOverlapping = false;
        }
    }

    void Update()
    {
        // Debug.Log($"[{augmentaObject.id}] WorldPos: {augmentaObject.worldPosition2D}, UnityPos: {transform.position}, WorldVelocity3D: {augmentaObject.worldVelocity3D}");

        // 1) Orbit motion
        if (carriedOrb != null) // "I am already holding a ghost"
        {
            float speed = augmentaObject.worldVelocity3D.magnitude;
            // Update only if speed changed significantly
            if (Mathf.Abs(speed - lastSpeed) > speedDifferenceThreshold)
            {
                UpdateRingRadius(1f + speed * speedToRingRadiusFactor);
                lastSpeed = speed;
            }

            // orb spinning logic
            angle += (velocity + (speed * speedToRingRadiusFactor)) * Time.deltaTime;
            Vector3 offs = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * ringRadius; 
            carriedOrb.transform.localPosition = offs;
        }
        else if (isOverlapping && overlappingGhost != null) // "I am colliding with a ghost"
        {
            pickupTimer += Time.deltaTime;
            if (pickupTimer >= pickupDelay)
            {
                // Debug.Log("Ghost state should be attached");
                carriedOrb = overlappingGhost;
                angle = Random.value * 2 * Mathf.PI;
                targetClr = carriedOrb.ghostColor;
                carriedOrb.AttachTo(transform);

                overlappingGhost = null;
                isOverlapping = false;
            }
        }
        else
        {
            targetClr = Color.white;
        }

        // Pulsing ring width
        float pulse = 1 + Mathf.Sin(Time.time * Mathf.PI * pulseSpeed) * pulseAmplitude;
        ring.startWidth = ring.endWidth = baseWidth * pulse;

        // Smooth colour fade
        currentClr = Color.Lerp(currentClr, targetClr, Time.deltaTime * 5f);
        currentClr.a = 1f; // force opaque
        ringMat.color = currentClr;
    }
    
    // called externally by another class to help drop the ghost possession
    public void DropGhost() 
    {
        UpdateRingRadius(1.0f); // return ring to original size
        if (carriedOrb == null) return;

        carriedOrb = null;           // Update() will fade back to white
    }
}
