using UnityEngine;
using System.Collections;
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

    // TODO: change this later
    public GhostState state = GhostState.Hovering;
    public Queue<MovementMazeNode> path = new Queue<MovementMazeNode>();
    public int hopsUntilHover = 3;
    public float hoverTime = 2f;
    private float hoverCountdown = 0f;
    public MovementMaze maze; // cached ref set during Initialize() // TODO: change

    public float ghostMovementCurveIntensity = 2f;

    public float duration = 1.0f; // TODO: this reuses a lot of pointless logic, becomes redundant

    private Coroutine moveRoutine;   // handle to FollowPath()

    /// <summary>Call this right after AddComponent. Subsequent calls are ignored.</summary>
    public void Initialise(int _ghostID, GameObject _sprite, int _sinkID, Color _ghostColor, GhostSpawner owner, MovementMazeNode _node, float _movementSpeed, float _hoverCountdown)
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
        hoverCountdown = _hoverCountdown;
        hoverTime = _hoverCountdown;
        _initialised = true;

        spawner = owner;
        dropoffTimer = 0f;
    }

    // ─────────────────────────────────────────────── Called by AugmentaPickup
    public void AttachTo(Transform parent)
    {
        if (state == GhostState.Attached) return;

        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
            path.Clear();                    
        }

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
        // Debug.Log($"Ghost state: {state}");
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
        path.Clear();
        MovementMazeNode current = this.node;

        for (int i = 0; i < hopsUntilHover; i++)
        {
            var candidates = current.Neighbours.FindAll(n => !n.isOccupied());
            if (candidates.Count == 0) break;

            MovementMazeNode next = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            next.setOccupancy(true);
            current.setOccupancy(false);
            path.Enqueue(next);
            current = next;
        }

        if (path.Count > 0)
        {
            state = GhostState.Moving;
            moveRoutine = StartCoroutine(FollowPath());
        }
        else
        {
            // No valid path; hover again
            hoverCountdown = hoverTime;
            state = GhostState.Hovering;
        }
    }

    private IEnumerator FollowPath()
    {
        while (path.Count > 0)
        {
            if (state == GhostState.Attached) yield break; // early exit in case state changes

            MovementMazeNode next = path.Dequeue();
            Vector2 from = Util.XYZ_to_XZ(transform.position);
            Vector2 to = next.getPos();

            yield return StartCoroutine(LerpGhost(from, to));

            node = next; // update current node
        }

        // After completing path
        hopsUntilHover = UnityEngine.Random.Range(2, 5); // or whatever range you want
        hoverCountdown = hoverTime;
        state = GhostState.Hovering;
    }

    private Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        Vector3 ab = Vector3.Lerp(a, b, t);
        Vector3 bc = Vector3.Lerp(b, c, t);
        return Vector3.Lerp(ab, bc, t);
    }

    // add a parameter here: call it speed, and make it per-ghost. 
    private IEnumerator LerpGhost(Vector2 from, Vector2 to)
    {
        Vector3 fromPos = new Vector3(from.x, transform.position.y, from.y);
        Vector3 toPos = new Vector3(to.x, transform.position.y, to.y);

        // --- Add wobble offset to midpoint ---
        // Option 1: Simple upward arc (hover effect)
        // Vector3 offset = Vector3.up * 0.5f;

        // Option 2: Randomized wobble (feel free to tweak scale)
        // TODO: ghostMovementCurveIntensity should be a function of distance between the nodes
        Vector3 randomXZ = UnityEngine.Random.insideUnitCircle.normalized * ghostMovementCurveIntensity;
        Vector3 offset = new Vector3(randomXZ.x, 0.25f, randomXZ.y);  // small wobble on XZ + slight lift

        Vector3 mid = (fromPos + toPos) / 2f + offset;

        float elapsed = 0f;
        float ghostDuration = duration / movementSpeed;

        while (elapsed < ghostDuration)
        {
            if (state == GhostState.Attached) yield break;   // bail mid-lerp
            float t = elapsed / ghostDuration;
            transform.position = QuadraticBezier(fromPos, mid, toPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = toPos; // snap to final position
    }
}
