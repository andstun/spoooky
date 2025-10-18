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
    [SerializeField] private int maxGhostsInRoom = 20; // CAP
    public int ghostCount;
    private float minimumPresence = 1.25f;
    private Dictionary<int, Coroutine> presenceTimers = new Dictionary<int, Coroutine>();
    [SerializeField] bool zeroFlag = false;

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

    [Header("Ghost Spawn Settings")]

    [Header("Portal Colour Palette")]
    [SerializeField] private GhostPalette ghostPaletteAsset;

    /* ───────── Private state ───────── */
    private static GhostPalette.Entry[] ghostPalette;
    // private static GameObject[] prefabPalette;

    private int nextGhostID = 0;

    private MovementMaze maze;

    private AudioManager audioManager;

    [SerializeField] private Transform spawnParent;     // optional parent for hierarchy
    [SerializeField] private GameObject hardFallback;

    void Awake()
    {
        ghostPalette = ghostPaletteAsset.GetEntries();
        // prefabPalette = ghostPaletteAsset.GetPrefabs();
        sinkBoundary = Mathf.Abs(xRange.x) - 3.5f; // add buffer zone between ghost area and sink area
        maze = this.GetComponent<MovementMaze>();
        maze.Initialise(Util.GetExtents(xRange, zRange)); // TODO: might need onValidate()
        audioManager = FindAnyObjectByType<AudioManager>();
    }

    void Start()
    {
        if (augmentaManager != null)
        {
            augmentaManager.augmentaObjectEnter += OnAugmentaObjectEnter;
            augmentaManager.augmentaObjectLeave += OnAugmentaObjectLeave;
        }

    }

    void Update()
    {
        // Keeps ghost count above 0
        if (ghostCount <= 0 && !zeroFlag)
        {
            zeroFlag = true;
            StartCoroutine((NewPlayerGhostSpawn()));
        }
        if(ghostCount > 0)
        {
            zeroFlag = false;
        }
    }

    private Ghost SpawnGhost()
    {
        // GameObject sprite = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        // Position: random on X-Z plane, y = radius (≈0.5) so it rests on the floor
        MovementMazeNode availNode = maze.getAvailableMazeNode();
        if (availNode == null)
        {
            // Debug.Log("No maze nodes available");
            ghostCount--;
            return null;
        }

        // if (prefab == null) prefab = hardFallback;
        Vector3 pos = Util.XZ_to_XYZ(availNode.getPos());
        Quaternion rot = Quaternion.Euler(90, -90, 0); // no rotation to start

        // Colour
        int portalID = UnityEngine.Random.Range(0, ghostPalette.Length);
        GameObject sprite = Instantiate(ghostPalette[portalID].prefab, pos, rot, spawnParent);
        // sprite.transform.localScale *= 5f;
        // Renderer rend = sprite.GetComponent<Renderer>();
        // rend.material = ghostPalette[portalID];

        // Physics & behaviour
        float colliderRadius = 0.10f;
        Ghost ghost = sprite.AddComponent<Ghost>();
        SphereCollider triggerCol = sprite.AddComponent<SphereCollider>();
        triggerCol.isTrigger = true;
        triggerCol.radius = colliderRadius;

        // 2) Add a SECOND collider that is NON-TRIGGER for particle collisions
        SphereCollider solidCol = sprite.AddComponent<SphereCollider>();
        solidCol.isTrigger = false;
        solidCol.radius = colliderRadius;

        // 3) Add a kinematic Rigidbody so the moving solid collider is “dynamic”
        //    (avoids “moving static collider” cost/warnings and plays nice with PS)
        var rb = sprite.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        float ghostMovementSpeed;
        do
        {
            ghostMovementSpeed = Util.RandomExtensions.Gaussian(2f, 0.5f);
        } while (ghostMovementSpeed < 1.0f || ghostMovementSpeed > 3.0f);

        ghost.state = Ghost.GhostState.Hovering;
        float hoverCountdown = UnityEngine.Random.Range(2f, 5f); // or constant
        ghost.hopsUntilHover = UnityEngine.Random.Range(2, 5);   // e.g. 2–4 hops before resting
        ghost.maze = this.maze;

        ghost.Initialise(nextGhostID++,
                        sprite,
                        ghostPalette[portalID].captureSprite,
                        portalID,
                        ghostPalette[portalID].material.color,
                        this,
                        availNode,
                        ghostMovementSpeed,
                        hoverCountdown,
                        ghostPalette[portalID].splat
                        );
        ghost.gameObject.layer = LayerMask.NameToLayer("GameLogicLayer");
        Instantiate(ghostPalette[portalID].splat, pos + new Vector3(-0.025f, 0f, 0f), Quaternion.Euler(90, -90, 0)); // run the splat with offset
        audioManager.Play("Ghost Appears");

        return ghost;
    }

    // Called by Ghost when it scores
    public void DestroyGhost(Ghost oldGhost)
    {
        maze.makeMazeNodeAvailable(oldGhost.node);
        ghostCount--;
        Destroy(oldGhost.gameObject);
    }

    // public IEnumerator DelayedReplaceGhost(Ghost oldGhost, float delay)
    // {
    //     oldGhost.gameObject.SetActive(false); 
    //     yield return new WaitForSeconds(delay);
    //     ReplaceGhost(oldGhost);
    // }

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
        if (obj.GetComponentInChildren<Ghost>() != null)
        {
            DestroyGhost(obj.GetComponentInChildren<Ghost>());
        }
        // can put else statement here if we want ghosts to despawn when player leaves
        // Cancel ghost spawn if they left early
        // if (presenceTimers.TryGetValue(id, out Coroutine c))
        // {
        //     StopCoroutine(c);
        //     presenceTimers.Remove(id);
        //     // Debug.Log($"Cancelled spawn for object {id} due to early exit");
        // }

    }

    private IEnumerator ConfirmPresenceAfterDelay(AugmentaObject obj, int id)
    {
        yield return new WaitForSeconds(2f);

        // If we're still tracking the object after 2.5 seconds, they didn't leave
        if (presenceTimers.ContainsKey(id))
        {
            // Debug.Log($"Object {id} confirmed present after {minimumPresence} seconds");
            presenceTimers.Remove(id);
            StartCoroutine(NewPlayerGhostSpawn());
        }
    }

    public IEnumerator DelayedGhostSpawn(float manDelay)
    {
        // Debug.Log("Spawning ghost after delay");
        yield return new WaitForSeconds(minimumPresence);
        if (!(ghostCount >= maxGhostsInRoom))
        {
            ghostCount++;
            // Debug.Log("Reached max count early exit");
            yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 5f)); // time between ghost spawns
            // isSpawningWithDelay = false;
            SpawnGhost();
        }
    }

    public IEnumerator NewPlayerGhostSpawn()
    {
        // Debug.Log("Spawning new ghost set");
        for(int i = 0; i < ghostsPerPerson; i++)
        {
            if (ghostCount < maxGhostsInRoom)
            {
                StartCoroutine(DelayedGhostSpawn(0f));
            }
            else
            {
                yield return null;
            }
        }
    }
    private IEnumerator ConsumeGhostsUntilAvailable()
    {

        yield return null;
        // updateNumGhostsToSpawn();
        // int ghostsNeeded = ghostsPerPerson;

        // while (ghostsNeeded > 0)
        // {
        //     if (ghostsQueue.Count == 0) // consume until ghosts is empty
        //     {
        //         yield return null;
        //         continue;
        //     }
        //     // ghost.deleteInsteadOfReplace = true;
        //     ghostsNeeded--;
        //     // RemoveGhostFromGhostList(ghost); // remove ghost from ghostlist
        // }
    }


    public float GetSinkBoundary()
    {
        return sinkBoundary;
    }
    
    public int GetGhosts()
    {
        return ghostCount;
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
