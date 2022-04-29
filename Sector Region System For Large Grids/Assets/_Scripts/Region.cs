using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Region
{
    private List<Tile> tiles;

    public Region(List<Tile> tiles)
    {
        this.tiles = tiles;
    }

    public bool Contains(Tile tile)
    {
        return tiles.Contains(tile);
    }

    public List<Tile> GetTiles()
    {
        return tiles;
    }
}
