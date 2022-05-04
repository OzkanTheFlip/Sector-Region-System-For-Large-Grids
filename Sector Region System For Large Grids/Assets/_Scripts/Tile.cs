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
