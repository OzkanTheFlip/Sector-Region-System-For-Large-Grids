using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    private List<List<Tile>> tiles = new List<List<Tile>>();
    private List<Sector> sectors = new List<Sector>();
    List<Region> regions = new List<Region>();
    Dictionary<Vector2, List<Region>> thresholdRegionDictionary = new Dictionary<Vector2, List<Region>>();

    public readonly int gridWidth;
    public readonly int gridHeight;

    public Grid(int gridWidth, int gridHeight, int sectorWidth, int sectorHeight)
    {
        this.gridWidth = gridWidth;
        this.gridHeight = gridHeight;
        for(int x = 0; x < gridWidth; x++)
        {
            tiles.Add(new List<Tile>());
            for(int y = 0; y < gridHeight; y++)
            {
                tiles[x].Add(new Tile(true, x, y));
                thresholdRegionDictionary.Add(new Vector2(x, y), new List<Region>());
            }
        }

        int num = 0;
        int yIndex = 0;
        int xIndex = 0;
        for (int i = 0; i < Mathf.CeilToInt((float)gridWidth / sectorWidth) * Mathf.CeilToInt((float)gridHeight / sectorHeight); i++)
        {
            int xSubtrahend = 1;
            int ySubtrahend = 1;
            while(xIndex + sectorWidth - xSubtrahend >= gridWidth)
            {
                xSubtrahend++;
            }
            while (yIndex + sectorHeight - ySubtrahend >= gridHeight)
            {
                ySubtrahend++;
            }
            sectors.Add(new Sector(new Vector2(xIndex, yIndex), new Vector2(xIndex+sectorWidth-xSubtrahend, yIndex+sectorHeight-ySubtrahend)));
            xIndex += sectorWidth;
            num++;
            if (num % Mathf.CeilToInt((float)gridWidth / sectorWidth) == 0)
            {
                xIndex = 0;
                yIndex += sectorHeight;
            }
        }

        foreach (Sector sector in sectors)
        {
            GenerateRegions(sector);
        }
    }

    private void GenerateRegions(Sector sector)
    {
        //Initialize tiles for flood fill
        List<List<int>> regionNums = new List<List<int>>();
        int xLowerBound = (int)sector.lowerBounds.x;
        int yLowerBound = (int)sector.lowerBounds.y;
        int xUpperBound = (int)sector.upperBounds.x;
        int yUpperBound = (int)sector.upperBounds.y;
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
                        x = xUpperBound;
                        y = yUpperBound;
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

        List<Region> removeRegions = new List<Region>();
        foreach (Region region in regions)
        {
            if (GetRegionSector(region) == sector)
                removeRegions.Add(region);
        }
        foreach(Region removeRegion in removeRegions)
        {
            regions.Remove(removeRegion);
            foreach(KeyValuePair<Vector2, List<Region>> regionList in thresholdRegionDictionary)
            {
                regionList.Value.Remove(removeRegion);
            }
        }

        foreach (Region newRegion in newRegions)
        {
            GenerateThresholds(newRegion);
            regions.Add(newRegion);
        }
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

    private void GenerateThresholds(Region region)
    {
        region.ClearThresholds();
        List<Tile> regionTiles = region.GetTiles();
        foreach (Tile regionTile in regionTiles)
        {
            List<Tile> neighbors = GetNeighbors(regionTile);
            foreach (Tile neighbor in neighbors)
            {
                if (region.Contains(neighbor) || !neighbor.traversable)
                    continue;
                if ((neighbor.xCoordinate < regionTile.xCoordinate && neighbor.yCoordinate == regionTile.yCoordinate)
                    || (neighbor.yCoordinate < regionTile.yCoordinate && neighbor.xCoordinate == regionTile.xCoordinate))
                {
                    region.AddThreshold(new Vector2(regionTile.xCoordinate, regionTile.yCoordinate));
                }
                else if ((neighbor.xCoordinate > regionTile.xCoordinate && neighbor.yCoordinate == regionTile.yCoordinate)
                    || (neighbor.xCoordinate == regionTile.xCoordinate && neighbor.yCoordinate > regionTile.yCoordinate)
                    || (neighbor.xCoordinate > regionTile.xCoordinate && neighbor.yCoordinate > regionTile.yCoordinate)
                    || (neighbor.xCoordinate < region.minX && neighbor.yCoordinate > region.maxY))
                {
                    region.AddThreshold(new Vector2(neighbor.xCoordinate, neighbor.yCoordinate));
                }
            }
        }

        foreach(Vector2 threshold in region.GetThresholds())
        {
            if(!thresholdRegionDictionary[threshold].Contains(region))
                thresholdRegionDictionary[threshold].Add(region);
        }
    }

    public List<Tile> GetNeighbors(Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                //This is you not your neighbors
                if (x == 0 && y == 0)
                    continue;
                //Check if inside grid
                int checkX = tile.xCoordinate + x;
                int checkY = tile.yCoordinate + y;
                if (checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight)
                {
                    neighbors.Add(tiles[checkX][checkY]);
                }
            }
        }
        return neighbors;
    }

    public void SetTileTraversable(Tile tile, bool traversable)
    {
        tile.traversable = traversable;
        Sector sector = GetTileSector(tile);
        GenerateRegions(sector);
        if (!traversable)
        {
            foreach (KeyValuePair<Vector2, List<Region>> thresholdRegion in thresholdRegionDictionary)
            {
                if (thresholdRegion.Key == new Vector2(tile.xCoordinate, tile.yCoordinate))
                {
                    foreach (Region region in thresholdRegion.Value)
                    {
                        GenerateThresholds(region);
                    }
                }
            }
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

    public Sector GetRegionSector(Region region)
    {
        int checkX = region.GetTiles()[0].xCoordinate;
        int checkY = region.GetTiles()[0].yCoordinate;
        foreach(Sector sector in sectors)
        {
            if (checkX >= sector.lowerBounds.x && checkY >= sector.lowerBounds.y
                && checkX <= sector.upperBounds.x && checkY <= sector.upperBounds.y)
                return sector;
        }

        Debug.LogError("Region not in any sector, doesn't make sense");
        return null;
    }

    public List<Region> GetRegionNeighbors(Region region)
    {
        List<Region> regionNeighbors = new List<Region>();
        foreach(Vector2 threshold in region.GetThresholds())
        {
            foreach(Region regionToAdd in thresholdRegionDictionary[threshold])
            {
                if (!regionNeighbors.Contains(regionToAdd))
                    regionNeighbors.Add(regionToAdd);
            }
        }
        return regionNeighbors;
    }

    public Region GetTileRegion(Tile tile)
    {
        foreach(Region region in regions)
        {
            if (region.Contains(tile))
                return region;
        }

        return null;
    }

    public List<Sector> GetSectors()
    {
        return sectors;
    }
}
