using UnityEngine;               // Physics engine & core API
using System;
using System.Collections;
using System.Collections.Generic; // (replaces raw array with List)
using Augmenta;

// Attach this to an empty GameObject in the scene
public class GhostSpawner : MonoBehaviour
{
    /* ───────── Inspector Params ───────── */
    [Header("Augmenta Manager Reference")]
    [SerializeField] private AugmentaManager augmentaManager;

    [Header("Spawn Settings")]
    public int ghostsToSpawn { get; private set; } = 10; 
    public int ghostsPerPerson = 4;
    [SerializeField] private int maxGhostsInRoom = 60;
    private float minimumPresence = 10f;
    private Dictionary<int, Coroutine> presenceTimers = new Dictionary<int, Coroutine>();

    [Header("Movement Parameters")]
    public bool toggleGhostMovement = true;

    // former movement parameters
    /* public float ghostMovementCurveIntensity = 2f; 
    // private float countdown = 5f; 
    // public float duration = 1.0f;         // Time to move from `from` to `to` */

    [Header("Spawn Area (XZ)")]
    [SerializeField] private Vector2 xRange = new Vector2(-13.9f, 13.9f);
    [SerializeField] private Vector2 zRange = new Vector2(-13.9f, 13.9f);

    private float sinkBoundary;

    [Header("Portal Colour Palette")]
    [SerializeField] private MaterialColorPalette sinkPaletteAsset;

    /* ───────── Private state ───────── */
    private static Material[] materialPalette;

    private List<Ghost> ghosts = new List<Ghost>();
    private Queue<Ghost> ghostsQueue = new Queue<Ghost>(); // for creation / deletion cycles
    private int nextGhostID = 0;

    private MovementMaze maze;

    void Awake()
    {
        materialPalette = sinkPaletteAsset.GetMaterials();
        sinkBoundary = Mathf.Abs(xRange.x) + 0.15f; // add buffer zone between ghost area and sink area
        maze = this.GetComponent<MovementMaze>();
        maze.Initialise(Util.GetExtents(xRange, zRange)); // TODO: might need onValidate()
        maxGhostsInRoom = maze.NumNodes();
    }

    void Start()
    {
        if (augmentaManager != null)
        {
            augmentaManager.augmentaObjectEnter += OnAugmentaObjectEnter;
            augmentaManager.augmentaObjectLeave += OnAugmentaObjectLeave;
        }

        int peopleInRoom = ghostsToSpawn / 4;  // augmentaManager.augmentaScene.augmentaObjectCount;
        ghostsToSpawn = peopleInRoom * ghostsPerPerson;
        for (int i = 0; i < peopleInRoom; i++)
        {
            StartCoroutine(DelayedSpawnGhostsPerPerson());
        }
    }

    private Ghost SpawnGhost()
    {
        GameObject sprite = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        // Position: random on X-Z plane, y = radius (≈0.5) so it rests on the floor
        MovementMazeNode availNode = maze.getAvailableMazeNode();
        if (availNode == null)
        {
            // Debug.Log("No maze nodes available");
            return null;
        }

        sprite.transform.position = Util.XZ_to_XYZ(availNode.getPos());

        // Colour
        Renderer rend = sprite.GetComponent<Renderer>();
        int materialID = UnityEngine.Random.Range(0, materialPalette.Length);
        rend.material = materialPalette[materialID];

        // Physics & behaviour
        Ghost ghost = sprite.AddComponent<Ghost>();
        SphereCollider triggerCol = sprite.GetComponent<SphereCollider>();
        triggerCol.isTrigger = true;
        triggerCol.radius = 0.5f;

        // 2) Add a SECOND collider that is NON-TRIGGER for particle collisions
        SphereCollider solidCol = sprite.AddComponent<SphereCollider>();
        solidCol.isTrigger = false;
        solidCol.radius = 0.5f;

        // 3) Add a kinematic Rigidbody so the moving solid collider is “dynamic”
        //    (avoids “moving static collider” cost/warnings and plays nice with PS)
        var rb = sprite.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Register ghost collider with cobweb particle system’s trigger controller
        CobwebTriggerController triggerCtrl = FindObjectOfType<CobwebTriggerController>();
        if (triggerCtrl != null)
        {
            int slot = triggerCtrl.RegisterGhostCollider(triggerCol, ghost);
            ghost.gameObject.AddComponent<GhostSlotTracker>().Init(triggerCtrl, slot);
        }

        float ghostMovementSpeed;
        do
        {
            ghostMovementSpeed = Util.RandomExtensions.Gaussian(2f, 0.5f);
        } while (ghostMovementSpeed < 1.0f || ghostMovementSpeed > 3.0f);

        ghost.state = Ghost.GhostState.Hovering;
        float hoverCountdown = UnityEngine.Random.Range(2f, 5f); // or constant
        ghost.hopsUntilHover = UnityEngine.Random.Range(2, 5);   // e.g. 2–4 hops before resting
        ghost.maze = this.maze;

        ghost.Initialise(nextGhostID++, sprite, materialID, materialPalette[materialID].color, this, availNode, ghostMovementSpeed, hoverCountdown);
        ghost.gameObject.layer = LayerMask.NameToLayer("GameLogicLayer");
        return ghost;
    }

    // Called by Ghost when it scores
    public void ReplaceGhost(Ghost oldGhost)
    {
        int i = ghosts.IndexOf(oldGhost);
        if (i < 0) return;
        maze.makeMazeNodeAvailable(oldGhost.node);
        Ghost newGhost = SpawnGhost();
        if (newGhost != null)
        {
            ghosts[i] = newGhost;     // keep list length & order intact
        } else 
        {
            ghosts.RemoveAt(i); // ghost is totally removed from list
        }
        Destroy(oldGhost.gameObject);
    }

    public IEnumerator DelayedReplaceGhost(Ghost oldGhost, float delay)
    {
        oldGhost.gameObject.SetActive(false); 
        yield return new WaitForSeconds(delay);
        ReplaceGhost(oldGhost);
    }

    public void RemoveGhostFromGhostList(Ghost ghostToRemove)
    {
        ghosts.Remove(ghostToRemove); // TODO: can optimize this, probably
        Destroy(ghostToRemove.gameObject);
    }

    public void OnAugmentaObjectEnter(AugmentaObject obj, Augmenta.AugmentaDataType dataType)
    {
        int id = obj.id; // Assume unique per person
        // Debug.Log($"Object {id} is entering");
        
        if (!presenceTimers.ContainsKey(id))
        {
            Coroutine c = StartCoroutine(ConfirmPresenceAfterDelay(obj, id));
            presenceTimers[id] = c;
        }
    }

    public void OnAugmentaObjectLeave(AugmentaObject obj, Augmenta.AugmentaDataType dataType)
    {
        int id = obj.id;
        // Debug.Log($"Object {id} is leaving");

        // Cancel ghost spawn if they left early
        if (presenceTimers.TryGetValue(id, out Coroutine c))
        {
            StopCoroutine(c);
            presenceTimers.Remove(id);
            // Debug.Log($"Cancelled spawn for object {id} due to early exit");
        }

        StartCoroutine(ConsumeGhostsUntilAvailable());
    }

    private IEnumerator ConfirmPresenceAfterDelay(AugmentaObject obj, int id)
    {
        yield return new WaitForSeconds(minimumPresence);

        // If we're still tracking the object after 5 seconds, they didn't leave
        if (presenceTimers.ContainsKey(id))
        {
            // Debug.Log($"Object {id} confirmed present after {minimumPresence} seconds");
            presenceTimers.Remove(id);
            StartCoroutine(DelayedSpawnGhostsPerPerson());
        }
    }

    public IEnumerator DelayedSpawnGhostsPerPerson()
    {
        yield return new WaitForSeconds(minimumPresence);
        updateNumGhostsToSpawn();
        for (int i = 0; i < ghostsPerPerson; i++)
        {
            float delay = UnityEngine.Random.Range(1f, 8f); // time between ghost spawns
            yield return new WaitForSeconds(delay);
            if (ghosts.Count >= maxGhostsInRoom)
            {
                // Debug.Log("Reached max count early exit");
                break;
            }
            Ghost ghost = SpawnGhost();
            if (ghost == null) continue;
            ghosts.Add(ghost);
            ghostsQueue.Enqueue(ghost);
        }
    }

    private IEnumerator ConsumeGhostsUntilAvailable()
    {
        updateNumGhostsToSpawn();
        int ghostsNeeded = ghostsPerPerson;

        while (ghostsNeeded > 0)
        {
            if (ghostsQueue.Count == 0) // consume until ghosts is empty
            {
                yield return null;
                continue;
            }
            Ghost ghost = ghostsQueue.Dequeue();
            ghost.deleteInsteadOfReplace = true;
            ghostsNeeded--;
            // RemoveGhostFromGhostList(ghost); // remove ghost from ghostlist
        }
    }

    public void updateNumGhostsToSpawn()
    {
        int newNumGhosts = augmentaManager.augmentaObjects.Count * ghostsPerPerson;
        ghostsToSpawn = Mathf.Clamp(newNumGhosts, 0, maxGhostsInRoom);
    }

    public float GetSinkBoundary()
    {
        return sinkBoundary;
    }

    public float getAvgGhostMovementSpeed()
    {
        float totalSpeed = 0f;
        foreach (Ghost g in ghosts)
        {
            totalSpeed += g.movementSpeed;
        }
        return totalSpeed / ghosts.Count;
    }

    // Destructor
    void OnDestroy()
    {
        if (augmentaManager != null)
        {
            augmentaManager.augmentaObjectEnter -= OnAugmentaObjectEnter;
            augmentaManager.augmentaObjectLeave -= OnAugmentaObjectLeave;
        }
    }

    /// <summary> This is old code, when movement was centralized at the spawner-level. 
    /// Keeping it around for now as ghost movement mechanics haven't been fully finalized. </summary>
    // void Update()
    // {
    //     if (toggleGhostMovement)
    //     {
    //         // repeating timer to handle movement. ghosts hover in-between.
    //         countdown -= Time.deltaTime;
    //         if (countdown <= 0f)
    //         {
    //             List<(Ghost, MovementMazeNode, MovementMazeNode)> nextMoves = maze.getNextMovesBounded(ghosts); // get a changeset of next moves
    //             StartLerping(nextMoves); // lerp over the changeset
    //             countdown = ghostMovemementStepWindow;
    //         }
    //     }
    // }

    // public void StartLerping(List<(Ghost ghost, MovementMazeNode from, MovementMazeNode to)> path)
    // {
    //     for (int i = 0; i < path.Count; i++)
    //     {
    //         var (ghost, from, to) = path[i];
    //         StartCoroutine(LerpGhost(ghost, from.getPos(), to.getPos()));
    //         ghost.node = to;
    //     }
    // }

    // private Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
    // {
    //     Vector3 ab = Vector3.Lerp(a, b, t);
    //     Vector3 bc = Vector3.Lerp(b, c, t);
    //     return Vector3.Lerp(ab, bc, t);
    // }


    // // add a parameter here: call it speed, and make it per-ghost. 
    // private IEnumerator LerpGhost(Ghost ghost, Vector2 from, Vector2 to)
    // {
    //     Vector3 fromPos = new Vector3(from.x, ghost.transform.position.y, from.y);
    //     Vector3 toPos = new Vector3(to.x, ghost.transform.position.y, to.y);

    //     // --- Add wobble offset to midpoint ---
    //     // Option 1: Simple upward arc (hover effect)
    //     // Vector3 offset = Vector3.up * 0.5f;

    //     // Option 2: Randomized wobble (feel free to tweak scale)
    //     // TODO: ghostMovementCurveIntensity should be a function of distance between the nodes
    //     Vector3 randomXZ = UnityEngine.Random.insideUnitCircle.normalized * ghostMovementCurveIntensity;
    //     Vector3 offset = new Vector3(randomXZ.x, 0.25f, randomXZ.y);  // small wobble on XZ + slight lift

    //     Vector3 mid = (fromPos + toPos) / 2f + offset;

    //     float elapsed = 0f;
    //     float ghostDuration = duration / ghost.movementSpeed;

    //     while (elapsed < ghostDuration)
    //     {
    //         float t = elapsed / ghostDuration;
    //         ghost.transform.position = QuadraticBezier(fromPos, mid, toPos, t);
    //         elapsed += Time.deltaTime;
    //         yield return null;
    //     }

    //     ghost.transform.position = toPos; // snap to final position
    // }
}
