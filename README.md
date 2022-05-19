# Sector-Region-System-For-Large-Grids

This is a grid system I developed for one of my Unity game projects that uses A* Pathfinding on a very large grid.

## What's it for?

A* is very innefficient when trying to path to an unreachable point as it will check the entire map before concluding it can't get there. 

A standard solution to this is flood filling the map after generation, giving each tile a room number. Before you calculate A*, you simply check to see if the tile you're at has the same room number as your target. If not, don't bother with A* because it's unreachable.

This works pretty well unless you plan on tiles changing their traversability mid-game. 

A tile becoming traversable is easy, just look at the room numbers of its neighbors, calculate the biggest room out of those, and change all other neighboring rooms and the newly traversable tile to the biggest room's number.

The problem is when a tile becomes intraversable, the only thing you can really do is flood fill the entire map again to regenerate the rooms. This is honestly good enough most of the time, but gets really slow for very large grids.

Another downside of A* on a grid of tiles that can change their traversability is that you need to recalculate an entire path over and over. A unit calculates a path to a spot on the map using A*, takes one step on that path, and then recalculates the entire thing because the map may have changed.

There's also the matter of finding the closest tile with a certain attribute. Let's say a unit wants to find the closest item. It's easy for us to see that the item 2 tiles away from the unit is actually very far because of the giant wall seperating the 2, but it's harder for your search algorithm to know this.

This grid system aims to alleviate these inefficiency problems that come from using a normal grid. This system was inspired by the Rimworld developers who use similar data structures to break down their grid.

## The Grid

Same as a normal grid system this class is comprised of an NxM grid of a mix of traversable and intraversable Tiles

<img src="https://user-images.githubusercontent.com/104275328/167961822-8d78aa3b-432a-4a6a-ae13-ec52340ab3df.png" width="702" height="639" />

The grid is also broken down into Sectors

<img src="https://user-images.githubusercontent.com/104275328/167961833-8c5ebdc2-c504-442f-9cc0-9793b56abced.png" width="702" height="639" />

Each Sector is broken down further into Regions

<img src="https://user-images.githubusercontent.com/104275328/167961842-fdadc1fc-ba14-41ed-9675-88e609e848d6.png" width="702" height="639" />

Regions also keep track of their "threshold" tiles which is how we look up each region's neighbors. To set a region's thresholds, we loop through the region's tiles and each of those tile's neighbors. Any neighbor tile that is in a different region, is traversable, and has a higher x coordinate of our farthest-right tile or a higher y coordinate of our highest tile, gets set as a threshold. Every far left and bottom tile of our region is also set as a threshold. The idea is if 2 regions share a threshold tile, they are neighbors.

<img src="https://user-images.githubusercontent.com/104275328/167975404-7dd42dc9-1f20-4bde-81ec-7b00424c6c1a.png" />

## How it helps

Works with the flood fill the map to make rooms solution, but you flood fill by region instead of by tile which cuts the time it takes significantly. Regenerating regions also goes quick because when you change a tile in a sector, you only need to regenerate the regions of that sector, not the whole map.

Recalculating your A* path over and over also goes faster if you do you pathfinding by region first and then A* only to the next region you need to get to.

Finally if we need to find the closest tile of a specific attribute, we can search our neighbors first, then their neighbors and so on until we find it. Going back to our example of an item being only 2 tiles away but is seperated by a wall, the item's region won't be a neighbor so we won't look there first.

## What you'll find in this Unity project

- Classes for Grid, Sector, Region, and Tile
- A Demo Scene that lets you generate and display a Grid with the options to see the Sectors, see the Regions with or without its neighbors and thresholds, and see the rooms. You can also Right Click to make a tile intraversable or Left Click to make a tile traversable
