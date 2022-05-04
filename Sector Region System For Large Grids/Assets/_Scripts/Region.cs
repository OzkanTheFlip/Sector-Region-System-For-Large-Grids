using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Region
{
    private List<Tile> tiles;

    public readonly int minX;
    public readonly int maxX; 
    public readonly int minY;
    public readonly int maxY;

    private List<Vector2> thresholds = new List<Vector2>();

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

    public void AddThreshold(Vector2 threshold)
    {
        if(!thresholds.Contains(threshold))
            thresholds.Add(threshold);
    }

    public bool Contains(Tile tile)
    {
        return tiles.Contains(tile);
    }

    public List<Tile> GetTiles()
    {
        return tiles;
    }

    public List<Vector2> GetThresholds()
    {
        return thresholds;
    }

    internal void ClearThresholds()
    {
        thresholds.Clear();
    }
}
