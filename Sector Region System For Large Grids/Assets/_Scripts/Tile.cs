using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public bool traversable;

    public readonly int xCoordinate;
    public readonly int yCoordinate;

    public Tile(bool traversable, int xCoordinate, int yCoordinate)
    {
        this.traversable = traversable;
        this.xCoordinate = xCoordinate;
        this.yCoordinate = yCoordinate;
    }
}
