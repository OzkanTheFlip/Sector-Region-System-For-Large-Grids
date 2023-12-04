using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data structure containing an (x,y) coordinate and a traversability boolean
/// </summary>
public class Tile
{
    //Is this traversable?
    public bool traversable;

    //Coordinates of this tile
    public readonly int xCoordinate;
    public readonly int yCoordinate;

    /// <summary>
    /// Constructor
    /// Sets the initial traversability and the (x,y) coordinates
    /// </summary>
    public Tile(bool traversable, int xCoordinate, int yCoordinate)
    {
        this.traversable = traversable;
        this.xCoordinate = xCoordinate;
        this.yCoordinate = yCoordinate;
    }
}

/// <summary>
/// A Tile plus some data used for pathfinding
/// </summary>
struct PathfindingTile
{
    public Tile tile;

    //Distance from start tile
    public int gCost;
    //Distance from end tile
    public int hCost;
    //gCost + hCost
    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int parentTileIndex;

    public PathfindingTile(Tile tile, int gCost, int hCost, int parentTileIndex)
    {
        this.tile = tile;
        this.gCost = gCost;
        this.hCost = hCost;
        this.parentTileIndex = parentTileIndex;
    }
}
