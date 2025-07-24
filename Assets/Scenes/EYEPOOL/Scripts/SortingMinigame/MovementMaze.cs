using UnityEngine;
using System;
using System.Collections.Generic;

public class MovementMaze : MonoBehaviour
{

    public float radius = 2.4f; // not applicable to this case (3D case only)
    public Vector2 regionSize = new Vector2(27.8f, 27.8f);
    public int rejectionSamples = 30;
    public float displayRadius = 0.5f;

    private int nodeIDCounter = 0;

    private List<Vector2> points; // points in the xz-plane
    private List<MovementMazeNode> nodes;

    public void Initialise() // should be initializable only once; 
    {
        Vector2 centre = Util.XYZ_to_XZ(transform.position);
        Vector2 half = regionSize * 0.5f;

        // generate list of well-spaced coordinates (poisson disk)
        points = PoissonDiscSampling.GeneratePoints(radius, regionSize, rejectionSamples);

        foreach (Vector2 p in points)
        {
            Vector3 worldPos = centre - half + p; // bottom-left corner of the region in world space
        }

        nodes = new List<MovementMazeNode>(points.Count);
        foreach (Vector2 p in points)
        {
            Vector2 worldPos = centre - half + new Vector2(p.x, p.y);
            nodes.Add(new MovementMazeNode(nodeIDCounter++, worldPos));
        }

        // 2) run KNN
        var graph = KNN.Build(nodes);   // parallel to points list
        for (int i = 0; i < nodes.Count; i++)
        {
            foreach (int j in graph[i])
            {
                nodes[i].Neighbours.Add(nodes[j]);   // TODO: undirected? add the reverse if desired
            }
        }
    }

    public List<MovementMazeNode> Nodes()
    {
        return nodes;
    }

    public int NumNodes()
    {
        return nodes.Count;
    }

    public MovementMazeNode getAvailableMazeNode()
    {
        MovementMazeNode availNode;
        var rng = new System.Random();

        do
        {
            availNode = nodes[rng.Next(0, nodes.Count)]; // generate 
        } while (availNode.isOccupied());
        availNode.setOccupancy(true);
        return availNode;
    }

    public List<(Ghost, MovementMazeNode, MovementMazeNode)> getNextMovesUnbounded(List<Ghost> ghosts)
    {
        var rng = new System.Random(); // TODO: not sure whether to use System or UnityEngine
        List<(Ghost, MovementMazeNode, MovementMazeNode)> moves = new List<(Ghost, MovementMazeNode, MovementMazeNode)>();
        HashSet<MovementMazeNode> unavailable = new HashSet<MovementMazeNode>();
        
        foreach (Ghost g in ghosts)
        {
            if (g.state == Ghost.GhostState.Attached) continue;

            MovementMazeNode from = g.node;
            MovementMazeNode to = sampleWithoutReplacement(unavailable, from);
            from.setOccupancy(false);
            to.setOccupancy(true);
            moves.Add((g, from, to));
        }
        return moves;
    }

    public List<(Ghost, MovementMazeNode, MovementMazeNode)> getNextMovesBounded(List<Ghost> ghosts)
    {
        var rng = new System.Random();
        List<(Ghost, MovementMazeNode, MovementMazeNode)> moves = new List<(Ghost, MovementMazeNode, MovementMazeNode)>();
        foreach (Ghost g in ghosts)
        {
            if (g.state == Ghost.GhostState.Attached) continue;


            List<MovementMazeNode> availableNeighbours = new List<MovementMazeNode>();
            foreach (MovementMazeNode neighbour in g.node.Neighbours)
            {
                if (!neighbour.isOccupied()) availableNeighbours.Add(neighbour);
            }
            MovementMazeNode from = g.node;
            if (availableNeighbours.Count == 0)
            {
                moves.Add((g, from, from));
                continue;
            }
            int randInd = rng.Next(0, availableNeighbours.Count);
            MovementMazeNode to = availableNeighbours[randInd];
            from.setOccupancy(false);
            to.setOccupancy(true);
            moves.Add((g, from, to));
        }
        return moves;
    }

    public MovementMazeNode sampleWithoutReplacement(HashSet<MovementMazeNode> unavailable, MovementMazeNode from)
    {
        MovementMazeNode to;
        do
        {
            to = nodes[UnityEngine.Random.Range(0, nodes.Count)];
        } while (unavailable.Contains(to) && from != to);
        unavailable.Add(to);
        unavailable.Remove(from);
        return to;
    }

    // create an algorithm that randomly assigns ghosts a path from one node to the next
}
