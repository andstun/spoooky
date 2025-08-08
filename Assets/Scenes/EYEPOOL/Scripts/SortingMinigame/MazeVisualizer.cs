using UnityEngine;

[ExecuteAlways]                 // so it runs in Edit mode too
public class MazeGraphGizmos : MonoBehaviour
{
    [Header("Maze Reference")]
    [SerializeField] MovementMaze maze;        // your data
    [Header("Visualization Options")]
    [SerializeField] bool showMaze = false; // the toggle
    public float nodeDisplayRadius = 0.5f;

    void OnDrawGizmos()
    {
        if (!showMaze) return;

        // Make sure we have a reference
        if (maze == null)
            maze = GetComponent<MovementMaze>();   // same GameObject

        // If the maze hasn’t been built yet, bail out
        var nodes = maze != null ? maze.Nodes() : null;
        if (nodes == null || nodes.Count == 0) return;

        Gizmos.color = Color.white;
        foreach (var n in nodes)
        {
            Gizmos.DrawSphere(Util.XZ_to_XYZ(n.getPos()), nodeDisplayRadius);

            if (n.getNeighbours() == null) continue;   // extra safety
            foreach (var neighbour in n.getNeighbours())
                Gizmos.DrawLine(Util.XZ_to_XYZ(n.getPos()), Util.XZ_to_XYZ(neighbour.getPos()));
        }
    }
}
