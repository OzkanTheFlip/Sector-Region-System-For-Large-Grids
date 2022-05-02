using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sector
{
    public readonly Vector2 lowerBounds;
    public readonly Vector2 upperBounds;


    public Sector(Vector2 lowerBounds, Vector2 upperBounds)
    {
        this.lowerBounds = lowerBounds;
        this.upperBounds = upperBounds;
    }

    public bool Contains(Tile tile)
    {
        return tile.xCoordinate >= lowerBounds.x && tile.xCoordinate <= upperBounds.x
            && tile.yCoordinate >= lowerBounds.y && tile.yCoordinate <= upperBounds.y;
    }
}
