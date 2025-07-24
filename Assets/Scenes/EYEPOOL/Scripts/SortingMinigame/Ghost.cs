using UnityEngine;
using System.Collections.Generic;

public class Ghost : MonoBehaviour
{
    // Read-only to every other script
    public int ghostID { get; private set; } // TODO: research difference between this and [Serialize] private
    public GameObject sprite;
    public int targetSinkID { get; private set; }
    public Color ghostColor { get; private set; }
    private bool _initialised = false;

    // public bool IsAttached { get; private set; } // TODO: make this private?
    private AugmentaPickup personAttached;

    [SerializeField] float dropoffDelay = 1.0f;

    public bool deleteInsteadOfReplace = false;

    private GhostSpawner spawner;

    private float dropoffTimer = 0f;

    public MovementMazeNode node; // TODO: make this private

    public float movementSpeed; // TODO: make this private

    public enum GhostState
    {
        Hovering,
        Planning,
        Moving,
        Attached
    }

    public GhostState state = GhostState.Hovering;
    public Queue<MovementMazeNode> path = new Queue<MovementMazeNode>();
    public int hopsUntilHover = 3;
    public float hoverTime = 2f;
    private float hoverCountdown = 0f;
    private MovementMaze maze; // cached ref set during Initialize()


    /// <summary>Call this right after AddComponent. Subsequent calls are ignored.</summary>
    public void Initialise(int _ghostID, GameObject _sprite, int _sinkID, Color _ghostColor, GhostSpawner owner, MovementMazeNode _node, float _movementSpeed)
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
        movementSpeed = _movementSpeed;
        _initialised = true;

        spawner = owner;
        dropoffTimer = 0f;
    }

    // ─────────────────────────────────────────────── Called by AugmentaPickup
    public void AttachTo(Transform parent)
    {
        if (state == GhostState.Attached) return;

        state = GhostState.Attached;
        transform.SetParent(parent, true);
        personAttached = parent.GetComponent<AugmentaPickup>();
        if (personAttached == null)
        {
            Debug.Log("unable to pick up person properly");
        }
    }

    public void Detach(bool reachedCorrectSink)
    {
        if (state != GhostState.Attached) return;

        if (reachedCorrectSink)
        {
            state = GhostState.Hovering; // TODO: really, this should be dead but idt the state matters
            transform.SetParent(null, true); // detach movement of ghost from parent
            personAttached.DropOrb();

            if (deleteInsteadOfReplace)
            {
                Debug.Log("Delete instead of replace");
                spawner.RemoveGhostFromGhostList(this);
                return;
            }

            float delay = Random.Range(1f, 8f); // TODO: modularize into range, probs
            spawner.StartCoroutine(spawner.DelayedReplaceGhost(this, delay));
        }
    }

    // ─────────────────────────────────────────────── Only orbs lying free
    private void Update() // TODO: this Update() method and AugmentaPickup's Update() method do the same thing; prune logic here
    {
        switch (state)
        {
            case GhostState.Hovering:
                hoverCountdown -= Time.deltaTime;
                if (hoverCountdown <= 0f)
                {
                    state = GhostState.Planning;
                }
                break;

            case GhostState.Planning:
                PlanPath();
                break;

            case GhostState.Moving:
                // noop — handled in coroutine
                break;
            case GhostState.Attached:
                int sinkHere = Util.GetSinkID(transform.position, spawner.GetLimit());
                if (sinkHere != targetSinkID) dropoffTimer = 0f;

                dropoffTimer += Time.deltaTime;
                if (dropoffTimer >= dropoffDelay)
                {
                    Detach(true);
                }
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("triggered from ghost");
    }

    private void PlanPath()
    {
        
    }
}
