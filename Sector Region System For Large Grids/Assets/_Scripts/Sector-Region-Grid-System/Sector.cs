using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data structure containing an Upper Bounds (x,y) coordinate and a Lower Bounds (x,y) coordinate.
/// </summary>
public class Sector
{
    public readonly Vector2Int lowerBounds;
    public readonly Vector2Int upperBounds;

    /// <summary>
    /// Constructor
    /// Sets the upper and lower bounds
    /// </summary>
    public Sector(Vector2Int lowerBounds, Vector2Int upperBounds)
    {
        this.lowerBounds = lowerBounds;
        this.upperBounds = upperBounds;
    }

    /// <summary>
    /// Checks to see if a tile is in this Sector
    /// </summary>
    public bool Contains(Tile tile)
    {
        return tile.xCoordinate >= lowerBounds.x && tile.xCoordinate <= upperBounds.x
            && tile.yCoordinate >= lowerBounds.y && tile.yCoordinate <= upperBounds.y;
    }
}
