// TODO: could replace this with a faster example that uses kD trees.
// naive implementation of KNN that runs in O(n^2logn) time 

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds a neighbour list for each point.  
/// For every point it:
///   - randomly picks k in [3,6]  
///   - runs a KNN implementation (standard or variation)
/// </summary>
public static class KNN
{
    public static List<int>[] Build(List<MovementMazeNode> nodes, int minK = 3, int maxK = 6)
    {
        int n = nodes.Count;
        var neighbours = new List<int>[n];
        var rng = new System.Random();

        for (int i = 0; i < n; i++)
        {
            int k = rng.Next(minK, maxK + 1);     // inclusive upper-bound
            neighbours[i] = GetSetForPoint(i, k, nodes);
        }

        return neighbours;
    }

    // Standard KNN implementation
    private static List<int> GetSetForPoint(int idx, int k, List<MovementMazeNode> nodes)
    {
        var dists = new List<(int j, float sqr)>();

        Vector2 p = nodes[idx].getPos();
        for (int j = 0; j < nodes.Count; j++)
        {
            if (j == idx) continue;
            float sqr = (nodes[j].getPos() - p).sqrMagnitude;
            dists.Add((j, sqr));
        }

        dists.Sort((a, b) => a.sqr.CompareTo(b.sqr));      // ascending

        var result = new List<int>(k);
        int neededNearest = Mathf.Min(k, dists.Count);

        // k-1 closest
        for (int n = 0; n < neededNearest; n++)
            result.Add(dists[n].j);

        return result;
    }

    // Variation of KNN where the kth slot gets mapped to the FURTHEST neighbour instead of the next-closest
    private static List<int> QuirkyGetSetForPoint(int idx, int k, List<MovementMazeNode> nodes)
    {
        var dists = new List<(int j, float sqr)>();

        Vector2 p = nodes[idx].getPos();
        for (int j = 0; j < nodes.Count; j++)
        {
            if (j == idx) continue;
            float sqr = (nodes[j].getPos() - p).sqrMagnitude;
            dists.Add((j, sqr));
        }

        dists.Sort((a, b) => a.sqr.CompareTo(b.sqr));      // ascending

        var result = new List<int>(k);
        int neededNearest = Mathf.Min(k - 1, dists.Count);

        // k-1 closest
        for (int n = 0; n < neededNearest; n++)
            result.Add(dists[n].j);

        // *furthest* neighbour
        result.Add(dists[^1].j);

        return result;
    }
}
