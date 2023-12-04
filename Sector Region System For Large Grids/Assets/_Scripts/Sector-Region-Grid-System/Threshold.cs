using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Threshold
{
    public readonly Tile tile;
    public readonly List<NeighborThreshold> intraNeighbors = new List<NeighborThreshold>();
    public readonly List<NeighborThreshold> interNeighbors = new List<NeighborThreshold>();
    public readonly int room;

    public Threshold(Tile tile, int room)
    {
        this.tile = tile;
        this.room = room;
    }

    public void AddIntraNeighbor(Threshold neighbor, float distance)
    {
        intraNeighbors.Add(new NeighborThreshold(neighbor, distance));
    }

    public void AddInterNeighbor(Threshold neighbor)
    {
        interNeighbors.Add(new NeighborThreshold(neighbor, 1));
    }
    public List<NeighborThreshold> GetIntraNeighbors()
    {
        return intraNeighbors;
    }

    public List<NeighborThreshold> GetInterNeighbors()
    {
        return interNeighbors;
    }

    public List<NeighborThreshold> GetNeighbors()
    {
        List<NeighborThreshold> neighbors = intraNeighbors;
        neighbors.AddRange(interNeighbors);
        return neighbors;
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