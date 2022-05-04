using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayTile : MonoBehaviour
{
    private Tile tile;
    public Tile Tile
    {
        get { return tile; }
        set
        {
            tile = value;
            spriteRenderer.color = tile.traversable ? Color.white : Color.grey;
        }
    }

    public SpriteRenderer spriteRenderer;
    public SpriteRenderer overlapSpriteRenderer;

    public TextMesh textMesh;

    public void ResetDisplay()
    {
        spriteRenderer.color = tile.traversable ? Color.white : Color.grey;
        overlapSpriteRenderer.color = Color.clear;
    }
}
