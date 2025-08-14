using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ghost : MonoBehaviour
{
    // Read-only to every other script
    public int ghostID { get; private set; }
    public GameObject sprite { get; private set; }
    public int targetSinkID { get; private set; }
    public Color ghostColor { get; private set; }
    public MovementMaze maze; // cached ref to allow 
    public MovementMazeNode node; // public, but should only be set by GhostSpawner. // TODO: maybe GhostSpawner needs to be a friend class of Ghost.
    public float movementSpeed { get; private set; }

    public enum GhostState
    {
        Hovering,
        Planning,
        Moving,
        Attached
    };

    public GhostState state = GhostState.Hovering; // maintain ghost state for movement
    public Queue<MovementMazeNode> path = new Queue<MovementMazeNode>(); // movement path inside the maze
    public int hopsUntilHover = 3; // path length 
    public float hoverTime = 2f; // how long before ghost calculate next path
    private float hoverCountdown = 0f; // works with hoverTime
    public float ghostMovementCurveIntensity = 2f;

    public float duration = 1.0f; // how long the total 
    public float dropoffDelay { get; private set; } = 1.0f;
    public bool deleteInsteadOfReplace = false; // public, but should only be set by GhostSpawner.

    /* ───────── Private state ───────── */
    private bool _initialised = false;
    private AugmentaPickup personAttached;
    [SerializeField] private GhostSpawner spawner;
    private float dropoffTimer = 0f;
    private Coroutine moveRoutine;   // handle to FollowPath()

    public bool isCoveredByCobwebs { get; private set; }
    private float cobwebCoveredUntil = -1f;

    /// <summary> Call this right after AddComponent. Subsequent calls are ignored. </summary>
    public void Initialise(int _ghostID, GameObject _sprite, int _targetSinkID, Color _ghostColor, GhostSpawner owner, MovementMazeNode _node, float _movementSpeed, float _hoverCountdown)
    {
        if (_initialised)
        {
            Debug.LogWarning($"{name} is already initialised – ignoring.");
            return;
        }
        ghostID        = _ghostID;
        sprite         = _sprite;
        targetSinkID   = _targetSinkID;
        ghostColor     = _ghostColor; 
        node           = _node;
        movementSpeed  = _movementSpeed;
        hoverCountdown = _hoverCountdown;
        hoverTime      = _hoverCountdown;
        spawner        = owner;
        dropoffTimer   = 0f;

        _initialised   = true;
    }

    private void Update() 
    {
        isCoveredByCobwebs = Time.time < cobwebCoveredUntil;
        if (isCoveredByCobwebs)
        {
            Debug.Log($"Ghost covered with cobweb: {isCoveredByCobwebs}");
        }
        switch (state)
        {
            case GhostState.Hovering:
                if (spawner.toggleGhostMovement)
                {
                    hoverCountdown -= Time.deltaTime;
                    if (hoverCountdown <= 0f)
                    {
                        state = GhostState.Planning;
                    }
                }
                break;

            case GhostState.Planning:
                if (spawner.toggleGhostMovement)
                {
                    PlanPath();
                }
                break;

            case GhostState.Moving:
                // noop — handled in coroutine
                break;
            case GhostState.Attached:
                int sinkHere = Util.GetSinkID(transform.position, spawner.GetSinkBoundary());
                if (sinkHere != targetSinkID) dropoffTimer = 0f;

                dropoffTimer += Time.deltaTime;
                if (dropoffTimer >= dropoffDelay)
                {
                    Detach(true);
                }
                break;
        }
    }

    // private void LateUpdate()
    // {
    //     isCoveredByCobwebs = false;
    // }

    // Called by AugmentaPickup to attach this Ghost to a person.
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
        maze.makeMazeNodeAvailable(node);
        if (personAttached == null)
        {
            Debug.Log("unable to pick up person properly");
        }
    }

    // Release this Ghost from AugmentaPickup object (person)
    public void Detach(bool reachedCorrectSink)
    {
        if (state != GhostState.Attached) return;

        if (reachedCorrectSink)
        {
            state = GhostState.Hovering; // TODO: maybe create a new dead state? 
            transform.SetParent(null, true); // detach movement of ghost from parent
            personAttached.DropGhost();

            if (deleteInsteadOfReplace)
            {
                spawner.RemoveGhostFromGhostList(this);
                return;
            }

            float delay = Random.Range(1f, 8f);
            spawner.StartCoroutine(spawner.DelayedReplaceGhost(this, delay));
        }
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
        if (!spawner.toggleGhostMovement) yield break;

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

    /// <summary> Smooth Ghost speed movement between two vectors, "from" and "to." </summary>
    private IEnumerator LerpGhost(Vector2 from, Vector2 to)
    {
        Vector3 fromPos = new Vector3(from.x, transform.position.y, from.y);
        Vector3 toPos = new Vector3(to.x, transform.position.y, to.y);

        // --- Add wobble offset to midpoint ---
        // Option 1: Simple upward arc (hover effect)
        // Vector3 offset = Vector3.up * 0.5f;

        // Option 2: Randomized wobble (can tweak scale)
        // TODO: ghostMovementCurveIntensity should be a function of distance between the nodes since movement between nodes that are further apart look straighter
        Vector3 randomXZ = UnityEngine.Random.insideUnitCircle.normalized * ghostMovementCurveIntensity;
        Vector3 offset = new Vector3(randomXZ.x, 0.25f, randomXZ.y);  // small wobble on XZ + slight lift

        Vector3 mid = (fromPos + toPos) / 2f + offset;

        float elapsed = 0f;
        float ghostDuration = duration / movementSpeed; // actual time it takes for ghost to lerp

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

    public void NotifyCobwebHit()
    {
        cobwebCoveredUntil = Time.time + 0.15f; // short grace window
    }
}
