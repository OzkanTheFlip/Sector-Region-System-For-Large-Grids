using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    private List<List<Tile>> tiles = new List<List<Tile>>();
    private List<Sector> sectors = new List<Sector>();
    private Dictionary<Sector, List<Region>> regions = new Dictionary<Sector, List<Region>>();

    public Grid(int gridWidth, int gridHeight, int sectorWidth, int sectorHeight)
    {
        for(int x = 0; x < gridWidth; x++)
        {
            tiles.Add(new List<Tile>());
            for(int y = 0; y < gridHeight; y++)
            {
                tiles[x].Add(new Tile(true, x, y));
            }
        }

        int id = 0;
        List<Tile> sectorTiles = new List<Tile>();
        int yIndex = 0;
        int xIndex = 0;
        for (int i = 0; i < Mathf.CeilToInt((float)gridWidth / sectorWidth) * Mathf.CeilToInt((float)gridHeight / sectorHeight); i++)
        {
            for (int x = xIndex; x < xIndex + sectorWidth; x++)
            {
                for (int y = yIndex; y < yIndex + sectorHeight; y++)
                {
                    if (x < gridWidth && y < gridHeight)
                        sectorTiles.Add(tiles[x][y]);
                }
            }
            xIndex += sectorWidth;
            sectors.Add(new Sector(sectorTiles, id));
            sectorTiles = new List<Tile>();
            id++;
            if (id % Mathf.CeilToInt((float)gridWidth / sectorWidth) == 0)
            {
                xIndex = 0;
                yIndex += sectorHeight;
            }
        }

        foreach(Sector sector in sectors)
        {
            List<Region> newRegions = GenerateRegions(sector);
            foreach(Region newRegion in newRegions)
            {
                if (!regions.ContainsKey(sector))
                {
                    regions.Add(sector, new List<Region>());
                }
                regions[sector].Add(newRegion);
            }
        }
    }

    private List<Region> GenerateRegions(Sector sector)
    {
        regions.Remove(sector);
        //Initialize tiles for flood fill
        List<List<int>> regionNums = new List<List<int>>();
        int xLowerBound = sector.GetBottomLeftTile().xCoordinate;
        int yLowerBound = sector.GetBottomLeftTile().yCoordinate;
        int xUpperBound = sector.GetTopRightTile().xCoordinate;
        int yUpperBound = sector.GetTopRightTile().yCoordinate;
        for (int x = xLowerBound; x < xUpperBound + 1; x++)
        {
            regionNums.Add(new List<int>());
            for (int y = yLowerBound; y < yUpperBound + 1; y++)
            {
                //-1 for unzoned, -2 for intraversable
                regionNums[x-xLowerBound].Add(tiles[x][y].traversable ? -1 : -2);
            }
        }

        bool notFullyFlooded = true;
        int id = 0;
        while (notFullyFlooded)
        {
            notFullyFlooded = false;
            for (int x = 0; x < regionNums.Count; x++)
            {
                for (int y = 0; y < regionNums[0].Count; y++)
                {
                    if(regionNums[x][y] == -1)
                    {
                        notFullyFlooded = true;
                        FloodFill(x, y, id, ref regionNums);
                        id++;
                        x = sector.GetTopRightTile().xCoordinate;
                        y = sector.GetTopRightTile().yCoordinate;
                    }
                }
            }
        }

        List<List<Tile>> regionTiles = new List<List<Tile>>();
        for(int i = 0; i < id; i++)
        {
            regionTiles.Add(new List<Tile>());
        }
        for (int x = xLowerBound; x < xUpperBound + 1; x++)
        {
            for (int y = yLowerBound; y < yUpperBound + 1; y++)
            {
                if (regionNums[x - xLowerBound][y - yLowerBound] == -2)
                    continue;
                regionTiles[regionNums[x-xLowerBound][y-yLowerBound]].Add(tiles[x][y]);
            }
        }

        List<Region> newRegions = new List<Region>();
        foreach(List<Tile> tileList in regionTiles)
        {
            newRegions.Add(new Region(tileList));
        }
        return newRegions;
    }

    private void FloodFill(int x, int y, int id, ref List<List<int>> regionNums)
    {
        //Make sure coordinates are within the board
        if (x >= 0 && x < regionNums.Count && y >= 0 && y < regionNums[0].Count)
        {
            if (regionNums[x][y] == -1)
            {
                regionNums[x][y] = id;
                FloodFill(x + 1, y, id, ref regionNums);
                FloodFill(x - 1, y, id, ref regionNums);
                FloodFill(x, y + 1, id, ref regionNums);
                FloodFill(x, y - 1, id, ref regionNums);
            }
        }
    }

    public void SetTileTraversable(Tile tile, bool traversable)
    {
        tile.traversable = traversable;
        Sector sector = GetTileSector(tile);
        List<Region> newRegions = GenerateRegions(sector);
        foreach (Region newRegion in newRegions)
        {
            if (!regions.ContainsKey(sector))
            {
                regions.Add(sector, new List<Region>());
            }
            regions[sector].Add(newRegion);
        }
    }

    public Tile GetTile(int x, int y)
    {
        return tiles[x][y];
    }

    public Sector GetTileSector(Tile tile)
    {
        foreach(Sector sector in sectors)
        {
            if (sector.Contains(tile))
                return sector;
        }

        Debug.LogError("Tile not in any sector, Tile shouldn't exist");
        return null;
    }

    public Region GetTileRegion(Tile tile)
    {
        foreach (KeyValuePair<Sector, List<Region>> regionList in regions)
        {
            foreach(Region region in regionList.Value)
            {
                if (region.Contains(tile))
                    return region;
            }
        }

        return null;
    }
}
