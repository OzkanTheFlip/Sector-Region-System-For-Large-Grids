using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayManager : MonoBehaviour
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

    private bool showRegions = false;

    private Vector3 mousePosition;

    private List<Color> colors = new List<Color>();

    private Grid grid;

    private List<List<DisplayTile>> displayTiles = new List<List<DisplayTile>>();

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
        colors.Add(new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, .4f));
    }

    private void Update()
    {
        if (showRegions)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    displayTiles[x][y].ResetDisplay();
                }
            }

            mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int x1 = Mathf.FloorToInt(mousePosition.x);
            int y1 = Mathf.FloorToInt(mousePosition.y);
            if (x1 >= 0 && x1 < gridWidth && y1 >= 0 && y1 < gridHeight)
            {
                Debug.Log(x1 + "," + y1);
                DisplayRegion(grid.GetTile(x1, y1));
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
        foreach(Tile regionTile in grid.GetTileRegion(tile).GetTiles())
        {
            displayTiles[regionTile.xCoordinate][regionTile.yCoordinate].overlapSpriteRenderer.color = new Color(255, 155, 0, .4f);
        }
    }

    public void DisplaySectors()
    {
        int index;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                index = grid.GetTileSector(grid.GetTile(x, y)).id / 6;
                displayTiles[x][y].overlapSpriteRenderer.color = colors[grid.GetTileSector(grid.GetTile(x, y)).id - 6 * index];
            }
        }
    }

    public void ShowRegions()
    {
        showRegions = !showRegions;
    }
}
