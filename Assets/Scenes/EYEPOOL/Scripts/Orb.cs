using UnityEngine;

public class Orb : MonoBehaviour
{
    // Read-only to every other script
    public int orbID { get; private set; }
    public int targetSinkID { get; private set; }
    public Color orbColor {get; private set; }

    public bool IsAttached { get; private set; }

    private OrbSpawner spawner;

    private bool _initialised = false;

    /// <summary>Call this right after AddComponent. Subsequent calls are ignored.</summary>
    public void Initialise(int _orbID, int _sinkID, Color _orbColor, OrbSpawner owner)
    {
        if (_initialised)
        {
            Debug.LogWarning($"{name} is already initialised – ignoring.");
            return;
        }

        orbID = _orbID;
        targetSinkID = _sinkID;
        orbColor = _orbColor;
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
            transform.SetParent(null, true); // detach movement of orb from parent
            spawner.ReplaceOrb(this);   // spawns a fresh one & removes me
        }
    }

    // ─────────────────────────────────────────────── Only orbs lying free
    private void Update() // TODO: this Update() method and AugmentaPickup's Update() method do the same thing; prune logic here
    {
        if (IsAttached) return;

        int sinkHere = SinkHelpers.GetSinkID(transform.position);
        Debug.Log($"currently at sink: {sinkHere}");
        if (sinkHere == targetSinkID)            // correct corner reached by itself
        {
            Detach(true);                        // deletes + respawns
        }
    }

    void OnCollisionEnter(Collision collision) 
    {
        GameObject other = collision.gameObject;

        // Check if it hit another Orb
        if (other.TryGetComponent<Orb>(out var otherOrb))
        {
            HandleOrbCollision(otherOrb);
        }

        // Check if it hit a Sink
        else if (other.TryGetComponent<Sink>(out var sink))
        {
            HandleSinkCollision(sink);
        }
        // Likely a Player collision
        else
        {
            Debug.Log($"Player collision detected with orb {orbID} and target {targetSinkID}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("I'm an orb and I have been triggered");
    }

    void HandleOrbCollision(Orb otherOrb)
    {
        Debug.Log($"{name} collided with another orb: {otherOrb.name}");
        // Trigger your specific orb-orb interaction here
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
