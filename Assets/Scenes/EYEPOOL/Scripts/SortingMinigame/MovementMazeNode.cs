using UnityEngine;
using System.Collections.Generic;

public class MovementMazeNode
{
    public bool Debug = true;

    private int NodeID;
    private bool Occupied; // check if null, otherwise contains occupant ghostID
    private Vector2 Pos;
    public List<MovementMazeNode> Neighbours; // TODO: find a better way to set this later

    public MovementMazeNode(int nodeID, Vector2 position)
    {
        NodeID = nodeID;
        Pos = position;
        Neighbours = new List<MovementMazeNode>();
    }

    public MovementMazeNode(int nodeID, Vector2 position, List<MovementMazeNode> neighbours)
    {
        NodeID = nodeID;
        Pos = position;
        Neighbours = neighbours;
    }

    public bool isOccupied() 
    {
        return Occupied;
    }

    public void setOccupancy(bool occupied)
    {
        Occupied = occupied;
    }

    public Vector2 getPos() 
    {
        return Pos;
    }

    public List<MovementMazeNode> getNeighbours() 
    {
        return Neighbours;
    }

    public void ReserveNode() // or call this Swap()
    {
        
    }

    // list emptyNodes; 
    // list fullNodes;

    // graph cases: 
    // ghosts can all move to a new area. 
    // a solver: empty nodes can randomly sample from neighbour nodes to become occupied. 
    // subsequently, the following nodes can sample from new neighbour nodes (except previously visited ones)
    // this reduces to a BFS on the node graphs. 
    // the solver spits out the new frame transition for movement (for now, assume all ghosts move)

    // then, all that's left is to animate movement. 
    // edges don't necessarily have to be animated. movement between two nodes can be a randomized edge function. 

    // orbs can be added to nodes. initially, the graph is built for N+2 nodes. when a new ghost is added, 

    // the maze shouldn't be rendered, but there should be a debug flag that allows a wireframe to be rendered. 

    // ghosts should be able to swap (advanced)
}