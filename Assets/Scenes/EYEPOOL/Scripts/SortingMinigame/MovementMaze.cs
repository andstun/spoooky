using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Creates a sparse graph of points with nodes guaranteed to be a minimum distance 
/// </summary>
public class MovementMaze : MonoBehaviour
{
    public float separationRadius = 2.4f; // minimum distance in Unity scale units (1.2 units approx. equal to 1 ft)
    public Vector2 regionSize = new Vector2(27.8f, 27.8f); 
    public int rejectionSamples = 30;
    private int nodeIDCounter = 0;

    private List<Vector2> points; // points in the xz-plane
    private List<MovementMazeNode> nodes;
    private Queue<MovementMazeNode> randomAvailableNodes;

    public void Initialise(Vector2? _regionSize=null) // should be initializable only once; 
    {
        regionSize = _regionSize ?? regionSize;
        Vector2 centre = Util.XYZ_to_XZ(transform.position);
        Vector2 half = regionSize * 0.5f;

        // 1) generate list of well-spaced coordinates (poisson disk)
        points = PoissonDiscSampling.GeneratePoints(separationRadius, regionSize, rejectionSamples);

        foreach (Vector2 p in points)
        {
            Vector3 worldPos = centre - half + p; // bottom-left corner of the region in world space
        }

        int N = points.Count;

        nodes = new List<MovementMazeNode>(N);
        foreach (Vector2 p in points)
        {
            Vector2 worldPos = centre - half + new Vector2(p.x, p.y);
            nodes.Add(new MovementMazeNode(nodeIDCounter++, worldPos));
        }

        // 2) run KNN
        var graph = KNN.Build(nodes);   // parallel to points list
        for (int i = 0; i < N; i++)
        {
            foreach (int j in graph[i])
            {
                nodes[i].Neighbours.Add(nodes[j]);   // TODO: the graph is directed, add the reverse direction if an undirected edge is desired. No need for HashSet; List has size at most 6, which means .Contains runs O(K)
            }
        }

        int[] randomNodeOrder = Util.GetShuffledArray(N); // setup quick random sampling of nodes (for Ghost selection)

        randomAvailableNodes = new Queue<MovementMazeNode>();
        for (int i=0; i < N; i++)
        {
            int randomIndex = randomNodeOrder[i];
            randomAvailableNodes.Enqueue(nodes[randomIndex]);
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

    public void makeMazeNodeAvailable(MovementMazeNode node)
    {
        randomAvailableNodes.Enqueue(node);
    }

    public MovementMazeNode getAvailableMazeNode()
    {
        if (randomAvailableNodes.Count == 0)
        {
            return null;
        }
        return randomAvailableNodes.Dequeue();
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

    /// <summary> This is old code, when movement was centralized at the spawner-level. 
    /// Keeping it around for now as ghost movement mechanics haven't been fully finalized. </summary>
    // public List<(Ghost, MovementMazeNode, MovementMazeNode)> getNextMovesUnbounded(List<Ghost> ghosts)
    // {
    //     var rng = new System.Random(); // TODO: not sure whether to use System or UnityEngine
    //     List<(Ghost, MovementMazeNode, MovementMazeNode)> moves = new List<(Ghost, MovementMazeNode, MovementMazeNode)>();
    //     HashSet<MovementMazeNode> unavailable = new HashSet<MovementMazeNode>();

    //     foreach (Ghost g in ghosts)
    //     {
    //         if (g == null || g.state == Ghost.GhostState.Attached) continue;

    //         MovementMazeNode from = g.node;
    //         MovementMazeNode to = sampleWithoutReplacement(unavailable, from);
    //         from.setOccupancy(false);
    //         to.setOccupancy(true);
    //         moves.Add((g, from, to));
    //     }
    //     return moves;
    // }

    // public List<(Ghost, MovementMazeNode, MovementMazeNode)> getNextMovesBounded(List<Ghost> ghosts)
    // {
    //     var rng = new System.Random();
    //     List<(Ghost, MovementMazeNode, MovementMazeNode)> moves = new List<(Ghost, MovementMazeNode, MovementMazeNode)>();
    //     foreach (Ghost g in ghosts)
    //     {
    //         if (g == null || g.state == Ghost.GhostState.Attached) continue;


    //         List<MovementMazeNode> availableNeighbours = new List<MovementMazeNode>();
    //         foreach (MovementMazeNode neighbour in g.node.Neighbours)
    //         {
    //             if (!neighbour.isOccupied()) availableNeighbours.Add(neighbour);
    //         }
    //         MovementMazeNode from = g.node;
    //         if (availableNeighbours.Count == 0)
    //         {
    //             moves.Add((g, from, from));
    //             continue;
    //         }
    //         int randInd = rng.Next(0, availableNeighbours.Count);
    //         MovementMazeNode to = availableNeighbours[randInd];
    //         from.setOccupancy(false);
    //         to.setOccupancy(true);
    //         moves.Add((g, from, to));
    //     }
    //     return moves;
    // }
}
