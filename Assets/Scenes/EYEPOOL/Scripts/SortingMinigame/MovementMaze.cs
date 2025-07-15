using System.Collections.Generic;

public class MovementMaze
{
    private int numNodes = 60;
    private int nodeIDCounter = 0;

    private List<MovementMazeNode> nodes;
    MovementMaze(int _numNodes)
    {
        numNodes = _numNodes;
        // spawn maze with numNodes, randomly spawn each 

        // generate list of well-spaced coordinates (poisson disk)

        for (int i=0; i < numNodes; i++) {

        }

        // randomly decide edge relationships, or can use k-nn
    }

    public List<MovementMazeNode> Nodes() {
        return nodes;
    }
}
