using UnityEngine;

public class Ghost : MonoBehaviour
{
    // Read-only to every other script
    public int ghostID { get; private set; }
    public GameObject sprite;
    public int targetSinkID { get; private set; }
    public Color ghostColor {get; private set; }

    public bool IsAttached { get; private set; } // TODO: make this private?

    private GhostSpawner spawner;

    private bool _initialised = false;

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
    }

    // ─────────────────────────────────────────────── Called by AugmentaPickup
    public void AttachTo(Transform parent)
    {
        if (IsAttached) return;

        IsAttached = true;
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
        int sinkHere = Util.GetSinkID(transform.position, spawner.GetLimit());
        if (sinkHere == targetSinkID)            // correct corner reached by itself
        {
            Detach(true);                        // deletes + respawns
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("triggered from ghost");
    }

    void OnCollisionStay(Collision collisionInfo)
    {
        Debug.Log("On collision stay triggered from ghost");
    }
}
