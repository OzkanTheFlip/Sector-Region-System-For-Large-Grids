using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Region
{
    private List<Tile> tiles;

    private List<Vector2> thresholds = new List<Vector2>();

    public Region(List<Tile> tiles)
    {
        this.tiles = tiles;
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
}
