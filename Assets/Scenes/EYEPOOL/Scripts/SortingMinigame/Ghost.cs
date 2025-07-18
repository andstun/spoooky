using UnityEngine;

public class Ghost : MonoBehaviour
{
    // Read-only to every other script
    public int ghostID { get; private set; }
    public GameObject sprite;
    public int targetSinkID { get; private set; }
    public Color ghostColor { get; private set; }

    public bool IsAttached { get; private set; } // TODO: make this private?
    private AugmentaPickup personAttached;

    [SerializeField] float dropoffDelay = 1.0f;

    private GhostSpawner spawner;

    private bool _initialised = false;

    private float dropoffTimer = 0f;

    public MovementMazeNode node; // TODO: make this private

    /// <summary>Call this right after AddComponent. Subsequent calls are ignored.</summary>
    public void Initialise(int _ghostID, GameObject _sprite, int _sinkID, Color _ghostColor, GhostSpawner owner, MovementMazeNode _node)
    {
        if (_initialised)
        {
            Debug.LogWarning($"{name} is already initialised – ignoring.");
            return;
        }
        ghostID = _ghostID;
        sprite = _sprite;
        targetSinkID = _sinkID;
        ghostColor = _ghostColor;
        node = _node;
        _initialised = true;

        spawner = owner;
        dropoffTimer = 0f;
    }

    // ─────────────────────────────────────────────── Called by AugmentaPickup
    public void AttachTo(Transform parent)
    {
        if (IsAttached) return;

        IsAttached = true;
        transform.SetParent(parent, true);
        personAttached = parent.GetComponent<AugmentaPickup>();
        if (personAttached == null)
        {
            Debug.Log("unable to pick up person properly");
        }
    }

    public void Detach(bool reachedCorrectSink)
    {
        Debug.Log("Detach called");
        if (!IsAttached) return;

        if (reachedCorrectSink)
        {
            IsAttached = false;
            transform.SetParent(null, true); // detach movement of ghost from parent
            spawner.ReplaceGhost(this);   // spawns a fresh one & removes me
            personAttached.DropOrb();
        }
    }

    // ─────────────────────────────────────────────── Only orbs lying free
    private void Update() // TODO: this Update() method and AugmentaPickup's Update() method do the same thing; prune logic here
    {
        int sinkHere = Util.GetSinkID(transform.position, spawner.GetLimit());
        if (sinkHere != targetSinkID) dropoffTimer = 0f;

        dropoffTimer += Time.deltaTime;
        Debug.Log($"dropoffTimer: {dropoffTimer}");
        if (dropoffTimer >= dropoffDelay)
        {
            Detach(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("triggered from ghost");
    }
}
