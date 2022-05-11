# Sector-Region-System-For-Large-Grids

This is a grid system I developed for one of my Unity game projects that uses A* Pathfinding on a very large grid.

A* is very innefficient when trying to path to an unreachable point as it will check the entire map before concluding it can't get there. 

A standard solution to this is floodfilling the map after generation, giving each tile a room number. Before you calculate A*, you simply check to see if the tile you're at has the same room number as your target, if not don't bother with A* because it's unreachable.

This works pretty good unless you plan on tiles changing their traversability mid-game. 

A tile becoming traversable is easy, just look at the room numbers of its neighbors, calculate the biggest room out of those, and change all other neighboring rooms and the newly traversable tile to the biggest room's number.

The problem is when a tile becomes intraversable, the only thing you can really do is flood fill the entire map again to regenerate the rooms. This is honestly good enough most of the time, but gets really slow for very large grids.

Another downside of A* on a grid of tiles that can change their traversability is that you need to recalculate an entire path over and over. A unit calculates a path to a spot on the map using A*, takes one step on that path, and then recalculates the entire thing because the map may have changed.

This grid system aims to alleviate these inefficiency problems that come from using a normal grid. This system was inspired by the Rimworld developers who use similar data structures to break down their grid for Rimworld.

The Grid
Same as a normal grid system this class is comprised of an NxM grid of Tiles

The grid is also broken down into Sectors

Each Sector is broken down further into Regions
