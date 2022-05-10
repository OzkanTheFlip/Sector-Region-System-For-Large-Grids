using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoManager : MonoBehaviour
{
    [SerializeField]
    private int gridWidth;
    [SerializeField]
    private int gridHeight;
    [SerializeField]
    private int sectorWidth;
    [SerializeField]
    private int sectorHeight;
    [SerializeField]
    private GameObject displayTile;


    private bool blinkRegionThresholds = false;
    private bool showRegionNeigbors = false;

    private bool showSectors = false;

    private bool showRegions = false;

    private bool showRooms = false;

    private Vector3 mousePosition;

    private List<Color> colors = new List<Color>();

    private Grid grid;

    private List<List<SpriteRenderer>> displayTileSpriteRenderers = new List<List<SpriteRenderer>>();
    private List<List<SpriteRenderer>> displayTileOverlapSpriteRenderers = new List<List<SpriteRenderer>>();

    private float timer = 0f;

    private Region mouseRegion;

    private void Start()
    {
        grid = new Grid(gridWidth, gridHeight, sectorWidth, sectorHeight);

        GenerateWalls();

        for (int x = 0; x < gridWidth; x++)
        {
            displayTileSpriteRenderers.Add(new List<SpriteRenderer>());
            displayTileOverlapSpriteRenderers.Add(new List<SpriteRenderer>());
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject newDisplayTile = Instantiate(displayTile);
                newDisplayTile.transform.position = new Vector3(x, y, 0);
                displayTileSpriteRenderers[x].Add(newDisplayTile.GetComponent<SpriteRenderer>());
                displayTileOverlapSpriteRenderers[x].Add(newDisplayTile.GetComponentsInChildren<SpriteRenderer>()[1]);
            }
        }

        mouseRegion = grid.GetTileRegion(grid.GetTile(0, 0));

        DisplayGrid();

        colors.Add(new Color(Color.blue.r, Color.blue.g, Color.blue.b, .4f));
        colors.Add(new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, .4f));
        colors.Add(new Color(Color.green.r, Color.green.g, Color.green.b, .4f));
        colors.Add(new Color(Color.magenta.r, Color.magenta.g, Color.magenta.b, .4f));
        colors.Add(new Color(Color.red.r, Color.red.g, Color.red.b, .4f));
        colors.Add(new Color(100f / 255f, 0f, 100f / 255f, .4f));
        colors.Add(new Color(255f / 255f, 100f / 255f, 0f, .4f));
        colors.Add(new Color(100f / 255f, 50f / 255f, 0f, .4f));
        colors.Add(new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, .4f));
    }

    private void Update()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int x1 = Mathf.RoundToInt(mousePosition.x);
        int y1 = Mathf.RoundToInt(mousePosition.y);

        if (showRegions)
        {
            if ((grid.GetTileRegion(grid.GetTile(x1, y1)) != null && mouseRegion != grid.GetTileRegion(grid.GetTile(x1, y1)))
                || (timer >= 2f && blinkRegionThresholds))
            {
                foreach (Tile tile in mouseRegion.GetTiles())
                {
                    displayTileOverlapSpriteRenderers[tile.xCoordinate][tile.yCoordinate].color = Color.clear;
                }
                foreach (Region neighborRegion in grid.GetRegionNeighbors(mouseRegion))
                {
                    foreach (Tile regionTile in neighborRegion.GetTiles())
                    {
                        displayTileOverlapSpriteRenderers[regionTile.xCoordinate][regionTile.yCoordinate].color = Color.clear;
                    }
                }

                mouseRegion = grid.GetTileRegion(grid.GetTile(x1, y1));

                if (x1 >= 0 && x1 < gridWidth && y1 >= 0 && y1 < gridHeight)
                {
                    DisplayRegion(mouseRegion);
                }
            }
            else if (timer < 2f && blinkRegionThresholds)
            {
                foreach (Vector2Int threshold in mouseRegion.GetThresholds())
                {
                    displayTileOverlapSpriteRenderers[threshold.x][threshold.y].color = new Color(1f, 0, 1f, .4f);
                }
            }
            timer += Time.deltaTime;
            if (timer >= 4f)
                timer = 0;
        }

        if (x1 >= 0 && x1 < gridWidth && y1 >= 0 && y1 < gridHeight)
        {
            if (Input.GetMouseButton(0) && !grid.GetTile(x1,y1).traversable)
            {
                grid.SetTileTraversable(grid.GetTile(x1, y1), true);
                displayTileSpriteRenderers[x1][y1].color = grid.GetTile(x1, y1).traversable ? Color.white : Color.grey;
                displayTileOverlapSpriteRenderers[x1][y1].color = Color.clear;
            }
            else if (Input.GetMouseButton(1) && grid.GetTile(x1, y1).traversable)
            {
                grid.SetTileTraversable(grid.GetTile(x1, y1), false);
                displayTileSpriteRenderers[x1][y1].color = grid.GetTile(x1, y1).traversable ? Color.white : Color.grey;
                displayTileOverlapSpriteRenderers[x1][y1].color = Color.clear;
            }
        }
    }

    private void GenerateWalls()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = gridHeight/2; y < gridHeight/2+1; y++)
            {
                grid.SetTileTraversable(grid.GetTile(x, y), false);
            }
        }

        for (int x = gridWidth / 2; x < gridWidth / 2 + 1; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid.SetTileTraversable(grid.GetTile(x, y), false);
            }
        }
    }

    private void DisplayGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                displayTileSpriteRenderers[x][y].color = grid.GetTile(x, y).traversable ? Color.white : Color.grey;
                displayTileOverlapSpriteRenderers[x][y].color = Color.clear;
            }
        }
    }

    private void DisplayRegion(Region region)
    {
        foreach(Tile regionTile in region.GetTiles())
        {
            displayTileOverlapSpriteRenderers[regionTile.xCoordinate][regionTile.yCoordinate].color = new Color(1f, 1f, 0, .4f);
        }

        if (showRegionNeigbors)
        {
            int num = 0;
            foreach (Region neighborRegion in grid.GetRegionNeighbors(region))
            {
                foreach (Tile regionTile in neighborRegion.GetTiles())
                {
                    displayTileOverlapSpriteRenderers[regionTile.xCoordinate][regionTile.yCoordinate].color = colors[num];
                }
                num++;
                if (num >= colors.Count - 1)
                    num = 0;
            }
        }
    }

    public void DisplaySectors()
    {
        showSectors = !showSectors;
        if (showSectors)
        {
            HashSet<Sector> sectors = grid.GetSectors();
            int num = 0;
            foreach (Sector sector in sectors)
            {
                int index = num / colors.Count;
                for (int x = sector.lowerBounds.x; x <= sector.upperBounds.x; x++)
                {
                    for (int y = sector.lowerBounds.y; y <= sector.upperBounds.y; y++)
                    {
                        SpriteRenderer[] spriteRenderers = displayTileSpriteRenderers[x][y].GetComponentsInChildren<SpriteRenderer>();
                        displayTileOverlapSpriteRenderers[x][y].color = colors[num - colors.Count * index];
                    }
                }
                num++;
            }
        }
        else
        {
            DisplayGrid();
        }
    }

    public void ShowRegions()
    {
        showRegions = !showRegions;
        if(!showRegions)
        {
            DisplayGrid();
        }
    }

    public void ToggleShowNeighbors()
    {
        showRegionNeigbors = !showRegionNeigbors;
    }

    public void ToggleShowThresholds()
    {
        blinkRegionThresholds = !blinkRegionThresholds;
    }

    public void ToggleShowRooms()
    {
        showRooms = !showRooms;
        if(showRooms)
        {
            foreach (Region region in grid.GetRegions())
            {
                foreach (Tile tile in region.GetTiles())
                {
                    displayTileOverlapSpriteRenderers[tile.xCoordinate][tile.yCoordinate].color = colors[region.room - (int)(colors.Count * Mathf.Floor(region.room / colors.Count))];
                }
            }
        }
        else
        {
            DisplayGrid();
        }
    }
}
