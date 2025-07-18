using UnityEngine;               // Physics engine & core API
using System.Collections;
using System.Collections.Generic; // (replaces raw array with List)

// Attach this to an empty GameObject in the scene
public class GhostSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int ghostsToSpawn = 10;
    public bool toggleMovement = true;
    [SerializeField] private float ghostHoverTime = 5f; // hover time in seconds
    private float countdown = 5f; // used in tandem with ghostHoverTime
    public float duration = 1.0f;         // Time to move from `from` to `to`

    [Header("Spawn Area (XZ)")]
    [SerializeField] private Vector2 xRange = new Vector2(-13.9f, 13.9f);
    [SerializeField] private Vector2 zRange = new Vector2(-13.9f, 13.9f);

    private float Limit;

    [SerializeField] private MaterialColorPalette colorPaletteAsset;

    private static Color[] colourPalette;

    private List<Ghost> ghosts = new List<Ghost>(); // TODO: a collection of ghosts may not be necessary

    private int nextGhostID = 0;

    private MovementMaze maze;

    void Awake() 
    {
        colourPalette = colorPaletteAsset.GetColors();
        Limit = Mathf.Abs(xRange.x) + 0.15f; // add buffer zone between ghost area and sink area
        maze = this.GetComponent<MovementMaze>();
        maze.Initialise();
    }

    // ────────────────────────────────────────────────────────────────
    void Start()
    {
        for (int i = 0; i < ghostsToSpawn; i++)
        {
            ghosts.Add(SpawnGhost());
        }
    }

    void Update()
    {
        if (toggleMovement)
        {
            // repeating timer to handle movement. ghosts hover in-between.
            countdown -= Time.deltaTime;

            if (countdown <= 0f)
            {
                List<(Ghost, MovementMazeNode, MovementMazeNode)> nextMoves = maze.getNextMoves(ghosts); // get a changeset of next moves
                StartLerping(nextMoves); // lerp over the changeset
                countdown = ghostHoverTime;
            }
        }
    }

    // ────────────────────────────────────────────────────────────────
    private Ghost SpawnGhost()  
    {
        GameObject sprite = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        // Position: random on X-Z plane, y = radius (≈0.5) so it rests on the floor
        MovementMazeNode availNode = maze.getAvailableMazeNode();
        availNode.setOccupancy(true);
        sprite.transform.position = Util.XZ_to_XYZ(availNode.getPos());

        // Colour
        Renderer rend = sprite.GetComponent<Renderer>();
        int colorID = Random.Range(0, colourPalette.Length);
        rend.material.color = colourPalette[colorID];

        // Physics & behaviour
        Ghost ghost = sprite.AddComponent<Ghost>();        
        SphereCollider sc = sprite.GetComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = 0.5f;

        ghost.Initialise(nextGhostID++, sprite, colorID, colourPalette[colorID], this, availNode);
        return ghost;
    }

    // Called by Ghost when it scores
    public void ReplaceGhost(Ghost oldGhost) // TODO: this logic may have to be slightly replaced if ghosts move
    {
        int i = ghosts.IndexOf(oldGhost);
        if (i >= 0)
        {
            ghosts[i] = SpawnGhost();     // keep list length & order intact
        }
        Destroy(oldGhost.gameObject);
    }

    private Color RandomColour()
    {
        int idx = Random.Range(0, colourPalette.Length);
        return colourPalette[idx];
    }

    public float GetLimit() 
    {
        return Limit;
    }

    public void StartLerping(List<(Ghost ghost, MovementMazeNode from, MovementMazeNode to)> path)
    {
        for (int i = 0; i < path.Count; i++)
        {
            var (ghost, from, to) = path[i];
            StartCoroutine(LerpGhost(ghost, from.getPos(), to.getPos()));
            ghost.node = to;
        }
    }

    private IEnumerator LerpGhost(Ghost ghost, Vector2 from, Vector2 to)
    {
        Vector3 fromPos = new Vector3(from.x, ghost.transform.position.y, from.y);
        Vector3 toPos   = new Vector3(to.x,   ghost.transform.position.y, to.y);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            ghost.transform.position = Vector3.Lerp(fromPos, toPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        ghost.transform.position = toPos; // snap to final position
    }
}
