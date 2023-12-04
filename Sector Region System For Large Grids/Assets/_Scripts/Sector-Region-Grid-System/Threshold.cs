using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Threshold
{
    public readonly Tile tile;
    public readonly List<NeighborThreshold> neighbors = new List<NeighborThreshold>();

    public Threshold(Tile tile)
    {
        this.tile = tile;
    }

    public void AddNeighbor(Threshold neighbor, float distance)
    {
        neighbors.Add(new NeighborThreshold(neighbor, distance));
    }
}

public struct NeighborThreshold
{
    public readonly Threshold threshold;
    public readonly float distance;

    public NeighborThreshold(Threshold threshold, float distance)
    {
        this.threshold = threshold;
        this.distance = distance;
    }
}