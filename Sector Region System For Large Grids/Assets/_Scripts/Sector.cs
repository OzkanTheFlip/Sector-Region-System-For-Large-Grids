using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sector
{
    private List<Tile> tiles;

    public readonly int id;

    public Sector(List<Tile> tiles, int id)
    {
        this.tiles = tiles;
        this.id = id;
    }

    public Tile GetBottomLeftTile()
    {
        return tiles[0];
    }

    public Tile GetTopRightTile()
    {
        return tiles[tiles.Count - 1];
    }

    public bool Contains(Tile tile)
    {
        return tiles.Contains(tile);
    }
}
