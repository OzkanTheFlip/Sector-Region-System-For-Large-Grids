using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data structure containing a list of tiles and the mins and maxes of those tiles' coordinates.
/// Also contains a room number generated by the Grid.
/// </summary>
public class Region
{
    //The Tiles
    private List<Tile> tiles;

    //The smallest xCoordinate found in the tiles
    public readonly int minX;
    //The largest xCoordinate found in the tiles
    public readonly int maxX;
    //The smallest yCoordinate found in the tiles
    public readonly int minY;
    //The largest yCoordinate found in the tiles
    public readonly int maxY;

    //What enclosed space is this tile a part of
    //Before you pathfind from one tile to another, just check if they're in the same room, if they're not then don't bother
    public int room = -1;

    //List of this region's thresholds
    //A Threshold is an (x,y) coordinate that is marked as an entrance for this region
    private List<Vector2> thresholds = new List<Vector2>();

    /// <summary>
    /// Constructor
    /// Sets the tiles List and the mins and maxes
    /// </summary>
    public Region(List<Tile> tiles)
    {
        this.tiles = tiles;
        minX = tiles[0].xCoordinate;
        maxX = tiles[0].xCoordinate;
        minY = tiles[0].yCoordinate;
        maxY = tiles[0].yCoordinate;
        foreach (Tile tile in tiles)
        {
            if (tile.xCoordinate < minX)
                minX = tile.xCoordinate;
            if (tile.xCoordinate > maxX)
                maxX = tile.xCoordinate;
            if (tile.yCoordinate < minY)
                minY = tile.yCoordinate;
            if (tile.yCoordinate > maxY)
                maxY = tile.yCoordinate;
        }
    }

    /// <summary>
    /// Checks to see if this Region contains a certain Tile
    /// </summary>
    public bool Contains(Tile tile)
    {
        return tiles.Contains(tile);
    }

    /// <summary>
    /// Grabs a list of the Tiles in this Region
    /// </summary>
    public List<Tile> GetTiles()
    {
        return tiles;
    }

    /// <summary>
    /// Grabs a list of this Region's thresholds
    /// </summary>
    public List<Vector2> GetThresholds()
    {
        return thresholds;
    }

    /// <summary>
    /// Adds a threshold to this Region
    /// </summary>
    public void AddThreshold(Vector2 threshold)
    {
        if (!thresholds.Contains(threshold))
            thresholds.Add(threshold);
    }

    /// <summary>
    /// Clears out all thresholds in this Region
    /// </summary>
    public void ClearThresholds()
    {
        thresholds.Clear();
    }
}
