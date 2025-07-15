using UnityEngine;               // Physics engine & core API
using System.Collections.Generic; // (replaces raw array with List)

// Attach this to an empty GameObject in the scene
public class GhostSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int ghostsToSpawn = 10;

    [Header("Spawn Area (XZ)")]
    [SerializeField] private Vector2 xRange = new Vector2(-13.9f, 13.9f);
    [SerializeField] private Vector2 zRange = new Vector2(-13.9f, 13.9f);

    private float Limit;

    private static readonly Color[] colourPalette =
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow
    };

    private List<GameObject> ghosts = new List<GameObject>();

    private int nextGhostID = 0;

    void Awake() 
    {
        Limit = Mathf.Abs(xRange.x) + 0.15f;
    }

    // ────────────────────────────────────────────────────────────────
    void Start()
    {
        Random.InitState(12345);
        for (int i = 0; i < ghostsToSpawn; i++)
        {
            ghosts.Add(SpawnGhost());
        }
    }

    // ────────────────────────────────────────────────────────────────
    private GameObject SpawnGhost() // TODO: this call will need to be repurposed to create a maze, since for now ghost spawning is stackable. in the future, the maze will have to be set for people.. or, the maze will automtiaclly be set for 25 people (100 orbs). 
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        // Position: random on X-Z plane, y = radius (≈0.5) so it rests on the floor
        sphere.transform.position = GenerateOrbPosition();

        // Colour
        Renderer rend = sphere.GetComponent<Renderer>();
        int colorID = Random.Range(0, colourPalette.Length);
        rend.material.color = colourPalette[colorID];

        // Physics & behaviour
        Ghost ghost = sphere.AddComponent<Ghost>();           // your custom script
        SphereCollider sc = sphere.GetComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = 0.5f;

        ghost.Initialise(nextGhostID++, colorID, colourPalette[colorID], this);
        return sphere;
    }

    // Called by Ghost when it scores
    public void ReplaceGhost(Ghost oldGhost)
    {
        int i = ghosts.IndexOf(oldGhost.gameObject);
        if (i >= 0)
        {
            ghosts[i] = SpawnGhost();     // keep list length & order intact
        }
    Destroy(oldGhost.gameObject);
    }

    private Vector3 GenerateOrbPosition()
    {
        float x = Random.Range(xRange.x, xRange.y);
        float z = Random.Range(zRange.x, zRange.y);
        return new Vector3(x, 0.5f, z);       // 0.5f so it’s not half-sunk
    }

    private Color RandomColour()
    {
        int idx = Random.Range(0, colourPalette.Length);
        return colourPalette[idx];
    }

    public float getLimit() 
    {
        return Limit;
    }
}
