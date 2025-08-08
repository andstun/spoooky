using UnityEngine;
using System.Collections.Generic;

public class MovementMazeNode
{
    public bool Debug = true;

    private int NodeID;
    private bool Occupied; // check if null, otherwise contains occupant ghostID
    private Vector2 Pos;
    public List<MovementMazeNode> Neighbours;

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
}