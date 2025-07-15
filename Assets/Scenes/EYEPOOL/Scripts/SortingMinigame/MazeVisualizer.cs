using UnityEngine;

[ExecuteAlways]                 // so it runs in Edit mode too
public class MazeGraphGizmos : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] MovementMaze maze;        // your data
    [Header("Designer options")]
    [SerializeField] bool showGizmos = true; // the toggle

    void OnDrawGizmos()
    {
        if (!showGizmos || maze == null) return;

        Gizmos.color = Color.white;
        foreach (var n in maze.Nodes())
        {
            Gizmos.DrawSphere(Util.XZ_to_XYZ(n.getPos()), .1f);
            foreach (var neighbour in n.getNeighbours())
                Gizmos.DrawLine(Util.XZ_to_XYZ(n.getPos()), Util.XZ_to_XYZ(neighbour.getPos()));
        }
    }
}
