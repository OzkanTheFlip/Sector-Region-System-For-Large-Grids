using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An NxM grid of Tiles.
/// Grid is broken down into Sectors.
/// Sectors are boken down into Regions.
/// 
/// For large grids where standard A* pathfinding and other search algorithm speeds become excessively slow. 
/// </summary>
public class Grid
{
    //The grid of Tiles
    private List<List<Tile>> tiles = new List<List<Tile>>();
    //The grid is broken down into Sectors
    private HashSet<Sector> sectors = new HashSet<Sector>();
    //Each Sector is broken down into Regions
    private HashSet<Region> regions = new HashSet<Region>();

    //Each (x,y) coordinate is a Threshold for a particular number of Regions
    //If 2 Regions share any 1 Threshold, they are Neighbors
    Dictionary<Vector2Int, List<Region>> thresholdRegionDictionary = new Dictionary<Vector2Int, List<Region>>();

    //The width and height of the grid
    public readonly int gridWidth;
    public readonly int gridHeight;

    /// <summary>
    /// Constructor
    /// Generates the grid, the Sectors, and the initial Regions
    /// </summary>
    public Grid(int gridWidth, int gridHeight, int sectorWidth, int sectorHeight)
    {
        this.gridWidth = gridWidth;
        this.gridHeight = gridHeight;

        //Generate grid of Tiles
        for(int x = 0; x < gridWidth; x++)
        {
            tiles.Add(new List<Tile>());
            for(int y = 0; y < gridHeight; y++)
            {
                tiles[x].Add(new Tile(true, x, y));
                thresholdRegionDictionary.Add(new Vector2Int(x, y), new List<Region>());
            }
        }

        //Generate Sectors of grid

        //Keep track of the number of sectors you've made
        int num = 0;
        //Start at (0,0) as the first Sector's lower bound
        int yIndex = 0;
        int xIndex = 0;
        //Create N Sectors where I is the grid width divided by the sector width rounded up, J is the grid height divided by the sector height rounded up
        //And N = I*J
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
            //Create a new Sector with the lower bound of (x,y) and an upper bound of (x+sectorWidth-1, y+sectorWidth-1)
            //Note: If you can't fit a full SECTOR_WIDTHxSECTOR_HEIGHT sector, that sector will be smaller
            //And the upperbound needs to subtract more than 1 from the x and/or y coordinate, hence the subtrahends
            sectors.Add(new Sector(new Vector2Int(xIndex, yIndex), new Vector2Int(xIndex+sectorWidth-xSubtrahend, yIndex+sectorHeight-ySubtrahend)));
            //Shift over the xIndex for the next Sector
            xIndex += sectorWidth;
            //If you've filled up this row of sectors, reset the xIndex and shift up the yIndex
            num++;
            if (num % Mathf.CeilToInt((float)gridWidth / sectorWidth) == 0)
            {
                xIndex = 0;
                yIndex += sectorHeight;
            }
        }

        //Generate initial Regions of grid
        foreach (Sector sector in sectors)
        {
            GenerateRegions(sector);
        }

        //Set the rooms
        SetRooms();
    }

    /// <summary>
    /// Generates the Regions of a certain Sector
    /// </summary>
    private void GenerateRegions(Sector sector)
    {
        //Create a 2D List of ints to flood fill
        List<List<int>> regionNums = new List<List<int>>();
        //Width and Height of the 2D List determined by the Sector's upper/lower bounds
        int xLowerBound = sector.lowerBounds.x;
        int yLowerBound = sector.lowerBounds.y;
        int xUpperBound = sector.upperBounds.x;
        int yUpperBound = sector.upperBounds.y;
        for (int x = xLowerBound; x < xUpperBound + 1; x++)
        {
            regionNums.Add(new List<int>());
            for (int y = yLowerBound; y < yUpperBound + 1; y++)
            {
                //-1 for unzoned, -2 for intraversable
                regionNums[x-xLowerBound].Add(tiles[x][y].traversable ? -1 : -2);
            }
        }

        //Keep track of the number of regions flood filled
        int num = 0;
        bool notFullyFlooded = true;
        while (notFullyFlooded)
        {
            notFullyFlooded = false;
            //Loop through every spot
            for (int x = 0; x < regionNums.Count; x++)
            {
                for (int y = 0; y < regionNums[0].Count; y++)
                {
                    //If you find a -1, flood fill from there, then keep going
                    if(regionNums[x][y] == -1)
                    {
                        notFullyFlooded = true;
                        FloodFill(x, y, num, ref regionNums);
                        num++;
                    }
                }
            }
        }

        //For each region flood filled, create a List of Tiles
        List<List<Tile>> regionTiles = new List<List<Tile>>();
        for(int i = 0; i < num; i++)
        {
            regionTiles.Add(new List<Tile>());
        }
        //Loop through the Sector and divvy the tiles up into their respective region
        for (int x = xLowerBound; x < xUpperBound + 1; x++)
        {
            for (int y = yLowerBound; y < yUpperBound + 1; y++)
            {
                //If a tile isn't traversable, it doesn't get a region
                if (regionNums[x - xLowerBound][y - yLowerBound] == -2)
                    continue;
                regionTiles[regionNums[x-xLowerBound][y-yLowerBound]].Add(tiles[x][y]);
            }
        }

        //Create a list to actually create the new Regions
        List<Region> newRegions = new List<Region>();
        //Create the new regions by giving them the tiles you divvied up
        foreach(List<Tile> tileList in regionTiles)
        {
            newRegions.Add(new Region(tileList));
        }

        //Remove any regions that were already in this Sector
        List<Region> removeRegions = new List<Region>();
        foreach (Region region in regions)
        {
            if (GetRegionSector(region) == sector)
                removeRegions.Add(region);
        }
        foreach(Region removeRegion in removeRegions)
        {
            regions.Remove(removeRegion);
            foreach(KeyValuePair<Vector2Int, List<Region>> regionList in thresholdRegionDictionary)
            {
                regionList.Value.Remove(removeRegion);
            }
        }

        //Generate thresholds for each new region and add them to the Regions list!
        foreach (Region newRegion in newRegions)
        {
            GenerateThresholds(newRegion);
            regions.Add(newRegion);
        }
    }

    /// <summary>
    /// Flood Fill Algorithm
    /// </summary>
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

                //Remove these if you don't support diagnol crossings
                FloodFill(x + 1, y + 1, id, ref regionNums);
                FloodFill(x + 1, y - 1, id, ref regionNums);
                FloodFill(x - 1, y + 1, id, ref regionNums);
                FloodFill(x - 1, y - 1, id, ref regionNums);
            }
        }
    }

    /// <summary>
    /// Generates the Thresholds contained in a specific Region
    /// </summary>
    private void GenerateThresholds(Region region)
    {
        //Clear the current thresholds
        region.ClearThresholds();

        //Loop through the region's tiles
        List<Tile> regionTiles = region.GetTiles();
        foreach (Tile regionTile in regionTiles)
        {
            //If the tile lines the bottom or left of the region, it's a threshold coordinate
            if (regionTile.xCoordinate == region.minX
                || regionTile.yCoordinate == region.minY)
            {
                region.AddThreshold(new Vector2Int(regionTile.xCoordinate, regionTile.yCoordinate));
            }

            //Loop through each region's neighbors
            List<Tile> neighbors = GetNeighbors(regionTile);
            foreach (Tile neighbor in neighbors)
            {
                //If the neighbor is also in this region or it's not traversable, skip it
                if (region.Contains(neighbor) || !neighbor.traversable)
                    continue;

                //If the neighbor is in a different region, is traversable,
                //And has a coordinate greater than the region's maxes, it's a threshold coordinate
                //Note: If you don't support diagonal crossings, add the other coordinate being the same as regionTile's on each line
                if (neighbor.xCoordinate > region.maxX
                    || neighbor.yCoordinate > region.maxY)
                {
                    region.AddThreshold(new Vector2Int(neighbor.xCoordinate, neighbor.yCoordinate));
                }
            }
        }

        //Add the region to the thresholdRegionDictionary under each of its thresholds
        foreach(Vector2Int threshold in region.GetThresholds())
        {
            if(!thresholdRegionDictionary[threshold].Contains(region))
                thresholdRegionDictionary[threshold].Add(region);
        }
    }

    /// <summary>
    /// Floodfills the map by Regions. Each enclosed space has its own room number assigned to each region
    /// </summary>
    private void SetRooms()
    {
        //Reset all the regions to -1, indicating they're not assigned to rooms
        foreach (Region region in regions)
        {
            region.room = -1;
        }
        //Keep track of the number of rooms flood filled
        int roomNum = 0;
        bool notFullyFlooded = true;
        while (notFullyFlooded)
        {
            notFullyFlooded = false;
            foreach (Region region in regions)
            {
                //If any region doesn't have a room, flood fill from there
                if (region.room == -1)
                {
                    notFullyFlooded = true;
                    FloodFillRegions(region, roomNum);
                    roomNum++;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Flood Fill Algorithm
    /// </summary>
    private void FloodFillRegions(Region region, int id)
    {
        if (region.room == -1)
        {
            region.room = id;
            foreach (Region neighbor in GetRegionNeighbors(region))
            {
                FloodFillRegions(neighbor, id);
            }
        }
    }

    /// <summary>
    /// Grabs the Sector that contains a certain Tile
    /// </summary>
    private Sector GetTileSector(Tile tile)
    {
        foreach (Sector sector in sectors)
        {
            if (sector.Contains(tile))
                return sector;
        }

        Debug.LogError("Tile not in any sector, Tile shouldn't exist");
        return null;
    }

    /// <summary>
    /// Grabs the Sector that contains a certain Region
    /// </summary>
    private Sector GetRegionSector(Region region)
    {
        foreach (Sector sector in sectors)
        {
            if (sector.Contains(region.GetTiles()[0]))
                return sector;
        }

        Debug.LogError("Region not in any sector, doesn't make sense");
        return null;
    }

    /// <summary>
    /// Grabs the Tile at the coordinates (x,y)
    /// </summary>
    public Tile GetTile(int x, int y)
    {
        if(x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            return tiles[x][y];
        return null;
    }

    /// <summary>
    /// Grabs a List of the neighbor Tiles of a certain Tile
    /// </summary>
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

    /// <summary>
    /// Grabs the Region that contains a certain Tile
    /// </summary>
    public Region GetTileRegion(Tile tile)
    {
        foreach (Region region in regions)
        {
            if (region.Contains(tile))
                return region;
        }

        return null;
    }

    /// <summary>
    /// Grabs a List of the neighbor Regions of a certain Region
    /// </summary>
    public List<Region> GetRegionNeighbors(Region region)
    {
        List<Region> regionNeighbors = new List<Region>();
        foreach (Vector2Int threshold in region.GetThresholds())
        {
            foreach (Region regionToAdd in thresholdRegionDictionary[threshold])
            {
                if (!regionNeighbors.Contains(regionToAdd) && regionToAdd != region)
                    regionNeighbors.Add(regionToAdd);
            }
        }
        return regionNeighbors;
    }

    /// <summary>
    /// Grabs the list of Sectors
    /// Note: Only reason I have this function currently is to provide a visual aid of the sectors in the demo scene
    /// </summary>
    public HashSet<Sector> GetSectors()
    {
        return sectors;
    }

    /// <summary>
    /// Grabs the list of Regions
    /// Note: Only reason I have this function currently is to provide a visual aid of the regions in the demo scene
    /// </summary>
    public HashSet<Region> GetRegions()
    {
        return regions;
    }

    /// <summary>
    /// Sets a Tile's traversability (build or remove a wall)
    /// </summary>
    public void SetTileTraversable(Tile tile, bool traversable)
    {
        //If we're setting it to what it already is, don't do anything
        if (tile.traversable == traversable) return;

        //Set the boolean
        tile.traversable = traversable;

        //Find the Sector that contains the tile and regenerate its Regions
        Sector sector = GetTileSector(tile);
        GenerateRegions(sector);

        //If we made the tile newly intraversable
        if (!traversable)
        {
            //We need to regenerate the thresholds of any regions that had it as a threshold
            foreach (KeyValuePair<Vector2Int, List<Region>> thresholdRegion in thresholdRegionDictionary)
            {
                if (thresholdRegion.Key == new Vector2Int(tile.xCoordinate, tile.yCoordinate))
                {
                    foreach (Region region in thresholdRegion.Value)
                    {
                        GenerateThresholds(region);
                    }
                }
            }
        }
        //If we made the tile newly traversable
        else
        {
            //We need to regenerate the thresholds of any region that contains a traversable neighbor to the tile
            List<Region> regionsToGenerateThresholds = new List<Region>();
            foreach(Tile neighbor in GetNeighbors(tile))
            {
                //If the neighbor has a region and it's not already in the list
                if (GetTileRegion(neighbor) != null && !regionsToGenerateThresholds.Contains(GetTileRegion(neighbor)))
                    regionsToGenerateThresholds.Add(GetTileRegion(neighbor));
            }
            foreach(Region region in regionsToGenerateThresholds)
            {
                GenerateThresholds(region);
            }
        }

        SetRooms();
    }
}
