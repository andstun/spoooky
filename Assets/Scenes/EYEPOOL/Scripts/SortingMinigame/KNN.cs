// TODO: replace this with a faster example that uses kD trees.
// naive implementation of KNN that runs in O(n^2logn) time 

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds a neighbour list for each point.  
/// For every point it:
///   • randomly picks k in [3,6]  
///   • takes the k-1 closest neighbours  
///   • and forces the k-th slot to be *the* furthest neighbour
/// Returned array is parallel to <paramref name="points"/>:
///   neighbours[i]  → indices of points that are neighbours of point i.
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

    /* ---------- helpers ---------- */

    private static List<int> GetSetForPoint(int idx, int k, List<MovementMazeNode> nds)
    {
        var dists = new List<(int j, float sqr)>();

        Vector2 p = nds[idx].getPos();
        for (int j = 0; j < nds.Count; j++)
        {
            if (j == idx) continue;
            float sqr = (nds[j].getPos() - p).sqrMagnitude;
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

    private static List<int> QuirkyGetSetForPoint(int idx, int k, List<MovementMazeNode> nds)
    {
        var dists = new List<(int j, float sqr)>();

        Vector2 p = nds[idx].getPos();
        for (int j = 0; j < nds.Count; j++)
        {
            if (j == idx) continue;
            float sqr = (nds[j].getPos() - p).sqrMagnitude;
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
