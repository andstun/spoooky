using UnityEngine;
using Augmenta;

/// Adds interactivity to an Augmenta object:
/// Pulsing influence ring and orbits ONE ghost around it.
public class AugmentaPickup : MonoBehaviour
{
    /* ───────── Inspector Params ───────── */
    
    [Header("Orbit")]
    [SerializeField] float ringRadius   = 1.0f;
    [SerializeField] float velocity     = 1.0f;     // radians per second
    public float speedToRingRadiusFactor = 0.5f;  // Degree to which orbit gets bigger upon speed change.
    public float speedDifferenceThreshold = 0.1f; // Saves computation

    [Header("Ring Look")]
    [SerializeField] float ringStroke   = 0.20f;
    [SerializeField] int   ringSegments = 64;
    [SerializeField] float pulseAmplitude = 0.25f;  // +/-25 % width
    [SerializeField] float pulseSpeed     = 2.0f;   // Hz

    [Header("Delays")]
    [SerializeField] float pickupDelay = 0.5f;

    /* ───────── Private state ───────── */
    private AugmentaObject myAugmentaObject;
    private CapsuleCollider myCollider;

    private Ghost        carriedGhost;
    private float        angle;
    private LineRenderer ring;
    private Material     ringMat;
    private Color        currentClr  = Color.white;
    private Color        targetClr   = Color.white;
    private float        baseWidth;

    private Ghost overlappingGhost;
    private float pickupTimer;
    private bool isOverlapping = false;

    private float lastSpeed = -1f;

    void Awake()
    {
        myAugmentaObject = GetComponent<AugmentaObject>();
    }

    public void Initialise(float _ringRadius) 
    {
        ringRadius = _ringRadius;
        myCollider = gameObject.AddComponent<CapsuleCollider>();
        myCollider.radius = _ringRadius;
		myCollider.direction = 1; // 0 = X, 1 = Y, 2 = Z 
		myCollider.height = 3.0f;
		myCollider.isTrigger = false;

		Rigidbody rb = gameObject.AddComponent<Rigidbody>();
		rb.isKinematic = true;                  // no forces, just follows pivot
		rb.useGravity = false;

        BuildRing();
    }

    void OnValidate()
    {
        if (myCollider == null)
        {
            return;
        }
        if (ringRadius != myCollider.radius) // only change if there's been a radius change
        {
            myCollider.radius = ringRadius; 
            UpdateRingRadius(ringRadius);
        }
    }

    // void Start()
    // {
    //     Debug.Log($"[Start] Augmenta object id: {myAugmentaObject.id}, oid: {myAugmentaObject.oid}");
    // }

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
        if (carriedGhost != null) return;

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
        // Debug.Log($"[{myAugmentaObject.id}] WorldPos: {myAugmentaObject.worldPosition2D}, UnityPos: {transform.position}, WorldVelocity3D: {myAugmentaObject.worldVelocity3D}");

        // Orbit motion
        if (carriedGhost != null) // "I am already holding a ghost"
        {
            float speed = myAugmentaObject.worldVelocity3D.magnitude;
            // Update only if speed changed significantly
            if (Mathf.Abs(speed - lastSpeed) > speedDifferenceThreshold)
            {
                UpdateRingRadius(1f + speed * speedToRingRadiusFactor);
                lastSpeed = speed;
            }

            // orb spinning logic
            angle += (velocity + (speed * speedToRingRadiusFactor)) * Time.deltaTime;
            Vector3 offs = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * ringRadius; 
            carriedGhost.transform.localPosition = offs;
        }
        else if (isOverlapping && overlappingGhost != null && !overlappingGhost.isCoveredByCobwebs) // "I am colliding with a ghost and it isn't covered in cobwebs"
        {
            pickupTimer += Time.deltaTime;
            if (pickupTimer >= pickupDelay)
            {
                // Debug.Log("Ghost state should be attached");
                carriedGhost = overlappingGhost;
                angle = Random.value * 2 * Mathf.PI;
                targetClr = carriedGhost.ghostColor;
                carriedGhost.AttachTo(transform);

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
        if (carriedGhost == null) return;

        carriedGhost = null;           // Update() will fade back to white
    }
}
