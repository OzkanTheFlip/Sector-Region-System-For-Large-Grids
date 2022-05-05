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

    private List<List<DisplayTile>> displayTiles = new List<List<DisplayTile>>();

    private float timer = 0f;

    private void Start()
    {
        grid = new Grid(gridWidth, gridHeight, sectorWidth, sectorHeight);

        GenerateWalls();

        for (int x = 0; x < gridWidth; x++)
        {
            displayTiles.Add(new List<DisplayTile>());
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject newDisplayTile = Instantiate(displayTile);
                newDisplayTile.transform.position = new Vector3(x, y, 0);
                newDisplayTile.GetComponent<DisplayTile>().Tile = grid.GetTile(x, y);
                newDisplayTile.GetComponent<DisplayTile>().textMesh.text = "(" + x + "," + y + ")";
                displayTiles[x].Add(newDisplayTile.GetComponent<DisplayTile>());
            }
        }

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

        if (!showSectors)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    displayTiles[x][y].ResetDisplay();
                }
            }
        }

        if (showRegions)
        {
            if (x1 >= 0 && x1 < gridWidth && y1 >= 0 && y1 < gridHeight)
            {
                DisplayRegion(grid.GetTile(x1, y1));
            }
        }

        if(showRooms)
        {
            foreach(Region region in grid.GetRegions())
            {
                foreach(Tile tile in region.GetTiles())
                {
                    displayTiles[tile.xCoordinate][tile.yCoordinate].overlapSpriteRenderer.color = colors[region.room - (int)(colors.Count * Mathf.Floor(region.room / colors.Count))];
                }
            }
        }

        if (x1 >= 0 && x1 < gridWidth && y1 >= 0 && y1 < gridHeight)
        {
            if(Input.GetMouseButton(0))
            {
                grid.SetTileTraversable(grid.GetTile(x1, y1), true);
                displayTiles[x1][y1].ResetDisplay();
            }
            else if (Input.GetMouseButton(1))
            {
                grid.SetTileTraversable(grid.GetTile(x1, y1), false);
                displayTiles[x1][y1].ResetDisplay();
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

    private void DisplayRegion(Tile tile)
    {
        if (grid.GetTileRegion(tile) == null)
            return;
        timer += Time.deltaTime;

        foreach(Tile regionTile in grid.GetTileRegion(tile).GetTiles())
        {
            displayTiles[regionTile.xCoordinate][regionTile.yCoordinate].overlapSpriteRenderer.color = new Color(1f, 1f, 0, .4f);
        }

        if (timer >= 2f && blinkRegionThresholds)
        {
            foreach (Vector2 threshold in grid.GetTileRegion(tile).GetThresholds())
            {
                displayTiles[(int)threshold.x][(int)threshold.y].overlapSpriteRenderer.color = new Color(1f, 0, 1f, .4f);
            }
            if (timer >= 4f)
                timer = 0;
        }

        if (showRegionNeigbors)
        {
            int num = 0;
            foreach (Region region in grid.GetRegionNeighbors(grid.GetTileRegion(tile)))
            {
                foreach (Tile regionTile in region.GetTiles())
                {
                    displayTiles[regionTile.xCoordinate][regionTile.yCoordinate].overlapSpriteRenderer.color = colors[num];
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
        List<Sector> sectors = grid.GetSectors();
        int num = 0;
        foreach(Sector sector in sectors)
        {
            int index = num / colors.Count;
            for (int x = (int)sector.lowerBounds.x; x <= sector.upperBounds.x; x++)
            {
                for (int y = (int)sector.lowerBounds.y; y <= sector.upperBounds.y; y++)
                {
                    displayTiles[x][y].overlapSpriteRenderer.color = colors[num - colors.Count * index];
                }
            }
            num++;
        }
    }

    public void ShowRegions()
    {
        showRegions = !showRegions;
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
    }
}