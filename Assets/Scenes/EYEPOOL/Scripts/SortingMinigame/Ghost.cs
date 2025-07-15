using UnityEngine;

public class Ghost : MonoBehaviour
{
    // Read-only to every other script
    public int ghostID { get; private set; }
    public int targetSinkID { get; private set; }
    public Color ghostColor {get; private set; }

    public bool IsAttached { get; private set; }

    private GhostSpawner spawner;

    private bool _initialised = false;

    /// <summary>Call this right after AddComponent. Subsequent calls are ignored.</summary>
    public void Initialise(int _ghostID, int _sinkID, Color _ghostColor, GhostSpawner owner)
    {
        if (_initialised)
        {
            // Debug.LogWarning($"{name} is already initialised – ignoring.");
            return;
        }

        ghostID = _ghostID;
        targetSinkID = _sinkID;
        ghostColor = _ghostColor;
        _initialised = true;

        spawner = owner;

        // rb = GetComponent<Rigidbody>();
    }

    // ─────────────────────────────────────────────── Called by AugmentaPickup
    public void AttachTo(Transform parent)
    {
        if (IsAttached) return;

        IsAttached = true;
        // rb.isKinematic = true;     // follow parent perfectly
        transform.SetParent(parent, true);
    }

    public void Detach(bool reachedCorrectSink)
    {
        if (!IsAttached) return;

        if (reachedCorrectSink)
        {
            IsAttached = false;
            transform.SetParent(null, true); // detach movement of ghost from parent
            spawner.ReplaceGhost(this);   // spawns a fresh one & removes me
        }
    }

    // ─────────────────────────────────────────────── Only orbs lying free
    private void Update() // TODO: this Update() method and AugmentaPickup's Update() method do the same thing; prune logic here
    {
        // if (IsAttached) return; // TODO: what was the purpose of this line? was causing bug earlier

        int sinkHere = Util.GetSinkID(transform.position, spawner.getLimit());
        if (sinkHere == targetSinkID)            // correct corner reached by itself
        {
            Detach(true);                        // deletes + respawns
        }
    }

    void OnCollisionEnter(Collision collision) 
    {
        GameObject other = collision.gameObject;

        // Check if it hit another Ghost
        if (other.TryGetComponent<Ghost>(out var otherGhost))
        {
            HandleGhostCollision(otherGhost);
        }

        // Check if it hit a Sink
        else if (other.TryGetComponent<Sink>(out var sink))
        {
            HandleSinkCollision(sink);
        }
        // Likely a Player collision
        else
        {
            // Debug.Log($"Player collision detected with ghost {ghostID} and target {targetSinkID}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("I'm a ghost and I have been triggered");
    }

    void HandleGhostCollision(Ghost otherGhost)
    {
        // Debug.Log($"{name} collided with another ghost: {otherGhost.name}");
    }

    void HandleSinkCollision(Sink sink)
    {
        if (sink.sinkID == targetSinkID)
        {
            Debug.Log($"{name} entered the correct sink (ID {sink.sinkID})");
            // Correct sink logic
        }
        else
        {
            Debug.Log($"{name} collided with wrong sink (ID {sink.sinkID})");
            // Ignore or bounce off
        }
    }
}
