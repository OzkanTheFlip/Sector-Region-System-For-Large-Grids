using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

    private HashSet<Threshold> thresholdMap = new HashSet<Threshold>();

    //Room id number, a room is a section of map floodfilled by region
    private int roomIndex = 0;

    //The width and height of the grid
    public readonly int gridWidth;
    public readonly int gridHeight;

    /// <summary>
    /// Constructor
    /// Generates the grid with all traversable tiles, the Sectors, and the initial Regions
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
            }
        }

        //Generate Sectors of grid
        GenerateSectors(sectorWidth, sectorHeight);

        //Generate initial Regions of grid
        foreach (Sector sector in sectors)
        {
            GenerateRegions(sector);
        }

        //Set the rooms
        SetRooms();


    }

    /// <summary>
    /// Constructor
    /// Generates the grid, the Sectors, and the initial Regions based off of an input sprite
    /// </summary>
    public Grid(Sprite gridSprite, int sectorWidth, int sectorHeight)
    {
        Texture2D texture = gridSprite.texture;

        gridWidth = texture.width;
        gridHeight = texture.height;

        //Generate grid of Tiles
        for (int x = 0; x < gridWidth; x++)
        {
            tiles.Add(new List<Tile>());
            for (int y = 0; y < gridHeight; y++)
            {
                //If the pixel is white it's traversable, otherwise it's intraversable
                Color pixel = texture.GetPixel(x, y);
                tiles[x].Add(new Tile(pixel == Color.white, x, y));
            }
        }

        //Generate Sectors of grid
        GenerateSectors(sectorWidth, sectorHeight);

        //Generate initial Regions of grid
        foreach (Sector sector in sectors)
        {
            GenerateRegions(sector);
        }

        //Connect the thresholds of the region
        ConnectThresholds();

        //Set the rooms
        SetRooms();
    }


    /// <summary>
    /// Generates the Sectors for grid with a base width and height
    /// </summary>
    private void GenerateSectors(int sectorWidth, int sectorHeight)
    {
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
            while (xIndex + sectorWidth - xSubtrahend >= gridWidth)
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
            sectors.Add(new Sector(new Vector2Int(xIndex, yIndex), new Vector2Int(xIndex + sectorWidth - xSubtrahend, yIndex + sectorHeight - ySubtrahend)));
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
        List<Threshold> regionThresholds = new List<Threshold>();

        //Loop through the region's tiles
        List<Tile> regionTiles = region.GetTiles();
        foreach (Tile regionTile in regionTiles)
        {
            bool isThreshold = false;
            //Loop through each of the tile's neighbors
            List<Tile> regionTileNeighbors = GetNeighbors(regionTile);
            foreach(Tile regionTileNeighbor in regionTileNeighbors)
            {
                //If the neighbor isn't in this region then this tile is a threshold
                if (!region.Contains(regionTileNeighbor))
                    isThreshold = true;
            }

            //If there were any neighbors in a different region, create a threshold for the region
            if(isThreshold)
                regionThresholds.Add(new Threshold(regionTile, region.room));
        }

        //Loop through all the thresholds you made
        foreach(Threshold regionThreshold in regionThresholds)
        {
            //Set their neighbors to eachother
            foreach (Threshold neighbor in regionThresholds)
            {
                //If it's not itself
                if(neighbor != regionThreshold)
                {
                    //Add it as a neighbor and grab distance to cache
                    regionThreshold.AddIntraNeighbor(neighbor, GetDistance(regionThreshold.tile, neighbor.tile));
                }
            }

            //Add it to the region
            region.AddThreshold(regionThreshold);
        }
    }

    private void ConnectThresholds()
    {
        //For each threshold
        foreach (Region region in regions)
        {
            foreach (Threshold threshold in region.GetThresholds())
            {
                //For each of its neigbors
                foreach (Tile neighbor in GetNeighbors(threshold.tile))
                {
                    //Get the threshold that has this neighbor
                    Threshold neighborThreshold = GetThreshold(neighbor);

                    //If we found a threshold with the neighbor and it's in a different region
                    if (neighborThreshold != null && !region.ContainsThreshold(neighborThreshold))
                    {
                        //Add them as neighbors with a distance of 1
                        threshold.AddInterNeighbor(neighborThreshold);
                    }
                }
            }
        }
    }

    private Threshold GetThreshold(Tile tile)
    {
        foreach(Threshold threshold in thresholdMap)
        {
            if (threshold.tile == tile)
                return threshold;
        }

        return null;
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
        roomIndex = 0;
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
                    FloodFillRegions(region, roomIndex);
                    roomIndex++;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Flood Fill Algorithm
    /// </summary>
    private void FloodFillRegions(Region region, int id, bool reflood = false)
    {
        if ((reflood && region.room != id) || region.room == -1)
        {
            region.room = id;
            foreach (Region neighbor in GetRegionNeighbors(region))
            {
                FloodFillRegions(neighbor, id, reflood);
            }
        }
    }

    //A* Pathfinding
    public List<Tile> FindPath(Tile startTile, Tile endTile)
    {
        //Tiles to evaluate
        List<PathfindingTile> openSet = new List<PathfindingTile>();
        //Tiles already evaluated
        List<PathfindingTile> closedSet = new List<PathfindingTile>();
        //Start with startTile
        openSet.Add(new PathfindingTile(startTile, 0, 0, -1));

        //Go while there are tiles to evaluate
        while(openSet.Count > 0)
        {
            //Tile we're evaluating
            PathfindingTile currentTile = openSet[0];

            //Loop through the tiles in the open set
            for (int i = 1; i < openSet.Count; i++)
            {
                //Get the tile with the lowest fCost in currentTile. If there's a tie, take the tile with the lower hCost
                if (openSet[i].fCost <= currentTile.fCost)
                {
                    if(openSet[i].hCost < currentTile.hCost)
                        currentTile = openSet[i];
                }
            }

            //Move the current tile with the lowest fCost from the open set to the closed set
            openSet.Remove(currentTile);
            closedSet.Add(currentTile);

            //If the current tile is the end tile, we're done pathfinding
            if(currentTile.tile == endTile)
            {
                return RetracePath(startTile, currentTile, closedSet);
            }

            //Loop through each neighbor and add it to the open set if it's valid for evaluation
            foreach(Tile neighbor in GetNeighbors(currentTile.tile))
            {
                //Find out if the neighbor is valid for evaluation
                bool validForEvaluation = true;

                //If it's not traversable
                if (!neighbor.traversable)
                    validForEvaluation = false;

                //If it's in the closed set
                foreach(PathfindingTile closedSetTile in closedSet)
                {
                    if (closedSetTile.tile == neighbor)
                        validForEvaluation = false;
                }

                if (!validForEvaluation)
                    continue;

                //Find out if the neighbor is already in the open set
                int inOpenSet = -1;

                //If it's in the open set
                foreach (PathfindingTile openSetTile in openSet)
                {
                    if (openSetTile.tile == neighbor)
                        inOpenSet = openSet.IndexOf(openSetTile);
                }

                int newMovementCost = currentTile.gCost + GetDistance(currentTile.tile, neighbor);
                if(inOpenSet > -1)
                {
                    if(newMovementCost < openSet[inOpenSet].gCost)
                        openSet[inOpenSet] = new PathfindingTile(openSet[inOpenSet].tile, newMovementCost, GetDistance(neighbor, currentTile.tile), closedSet.IndexOf(currentTile));
                }
                else
                {
                    openSet.Add(new PathfindingTile(neighbor, newMovementCost, GetDistance(neighbor, currentTile.tile), closedSet.IndexOf(currentTile)));
                }    
            }

        }    

        return null;
    }

    private int GetDistance(Tile tileA, Tile tileB)
    {
        int distanceX = (int)Mathf.Abs(tileA.xCoordinate - tileB.xCoordinate);
        int distanceY = (int)Mathf.Abs(tileA.yCoordinate - tileB.yCoordinate);

        return distanceX > distanceY ? distanceX : distanceY;
    }

    private List<Tile> RetracePath(Tile startTile, PathfindingTile endTile, List<PathfindingTile> closedSet)
    {
        List<Tile> path = new List<Tile>();
        PathfindingTile currentTile = endTile;
        while (currentTile.tile != startTile)
        {
            path.Add(currentTile.tile);
            currentTile = closedSet[currentTile.parentTileIndex];
        }
        path.Reverse();
        return path;
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
        if (!tile.traversable) return null;

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
        foreach(Threshold threshold in region.GetThresholds())
        {
            foreach(NeighborThreshold neighborThreshold in threshold.interNeighbors)
            {
                if(!regionNeighbors.Contains(GetTileRegion(neighborThreshold.threshold.tile)))
                    regionNeighbors.Add(GetTileRegion(neighborThreshold.threshold.tile));
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
            //foreach (Region region in thresholdRegionDictionary[new Vector2Int(tile.xCoordinate, tile.yCoordinate)])
            //{
            //    GenerateThresholds(region);
            //}

            //Flood fill a new room for each region without a room number
            foreach (Region region in regions)
            {
                if (region.room == -1)
                {
                    roomIndex++;
                    FloodFillRegions(region, roomIndex, true);
                }
            }
        }
        //If we made the tile newly traversable
        else
        {
            //We need to regenerate the thresholds of any region that contains a traversable neighbor to the tile
            List<Region> regionsToGenerateThresholds = new List<Region>();
            foreach (Tile neighbor in GetNeighbors(tile))
            {
                //If the neighbor has a region and it's not already in the list
                Region neighborRegion = GetTileRegion(neighbor);
                if (neighborRegion != null && !regionsToGenerateThresholds.Contains(neighborRegion))
                    regionsToGenerateThresholds.Add(neighborRegion);
            }
            foreach (Region region in regionsToGenerateThresholds)
            {
                GenerateThresholds(region);
            }

            //Generate new rooms
            foreach (Region region in regions)
            {
                if (region.room == -1)
                {
                    Dictionary<int, int> roomCounts = new Dictionary<int, int>();
                    //Find each neighbor with a valid room number
                    foreach (Region neighbor in GetRegionNeighbors(region))
                    {
                        if(neighbor.room != -1 && !roomCounts.ContainsKey(neighbor.room))
                            roomCounts.Add(neighbor.room, 0);
                    }

                    //If there are no neighbors with a valid room number, flood fill a new room
                    if (roomCounts.Count == 0)
                    {
                        roomIndex++;
                        FloodFillRegions(region, roomIndex, true);
                        continue;
                    }

                    //If there's more than one room number, pick the one with the most rooms
                    foreach (Region region1 in regions)
                    {
                        if (region1.room != -1 && roomCounts.ContainsKey(region1.room))
                            roomCounts[region1.room]++;
                    }
                    int largestRoom = -1;
                    foreach(KeyValuePair<int,int> roomCount in roomCounts)
                    {
                        if (largestRoom == -1 || roomCounts[largestRoom] < roomCount.Value)
                            largestRoom = roomCount.Key;
                    }
                    //Flood fill using the neighbor's room number
                    FloodFillRegions(region, largestRoom, true);
                }
            }
        }
    }
}
