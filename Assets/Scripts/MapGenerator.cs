﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

	// basic properties
	public int xNumGraves = 20;
	public int yNumGraves = 20;
	public float xSpacing = 20.0f;
	public float ySpacing = 20.0f;

	// range of randomized placement
	[Range(0.0f, 0.5f)]
	public float leeway = 0.4f;

	public GameObject player;
	// GameObjects to be instantiated
	public GameObject grave1;
	public GameObject grave2;
	public GameObject grave3;
	public GameObject tree;
	public GameObject lamp;
	public GameObject fence;
	public GameObject shortFence;
	public GameObject gate;
	public GameObject CursedGrave;
	public GameObject path;

	// grid random fill percent
	[Range(0.0f, 1.0f)]
	public float fillPercent;

	// EliminatePockets information
	public int pocketMinSize = 20;
	public int wallMinSize = 10;

	// grids and vars for generation
	private int[,] grid;
	public SquareGrid squareGrid;


	/// <summary>
	/// Called at beginning of program, creates map.
	/// </summary>
	void Start () {
		if (grave1 == null || grave2 == null || grave3 == null || tree == null || CursedGrave == null ||
		    lamp == null || fence == null || gate == null || path == null) {
			return; // nothing to do here
		}
		if (shortFence == null) { // default shortFence to fence
			shortFence = fence;
		}
		grid = new int[xNumGraves, yNumGraves];

		bool gatePlaced = false;
		while (!gatePlaced) {
			GenerateBitmap ();
			GenerateMesh ();
			gatePlaced = PlaceGate ();
		}
		CreateWalls ();
		PlacePlayer ();
		PlaceCursedGrave ();
//		GeneratePaths ();
		GenerateTombstones ();

	}

	/// <summary>
	/// Generates the bitmap and cleans up pockets/empty parts.
	/// </summary>

	// Border generation (cellular automata)
	void GenerateBitmap () {
		// randomly fill map and initialize mapFlags
		for (int x = 0; x < xNumGraves; x++) {
			for (int y = 0; y < yNumGraves; y++) {
				if (x == 0 || x == xNumGraves - 1 ||
				    y == 0 || y == yNumGraves - 1) {
					grid[x,y] = 1;
				} else {
					if (Random.value < fillPercent) {
						grid[x,y] = 1;
					} else {
						grid[x,y] = 0;
					}
				}
			}
		}

		for (int i = 0; i < 7; i++) {
			SmoothMap ();
		}

		EliminatePockets ();
	}

	// coord struct used in EliminatePockets
	struct Coord {
		public int x;
		public int y;

		public Coord(int xin, int yin) {
			x = xin;
			y = yin;
		}
	}

	/// <summary>
	/// Eliminates pockets smaller than minPocketSize and
	/// out-of-bounds areas less than minWallSize.
	/// </summary>
	void EliminatePockets() {
		List<List<Coord>> wallRegions = GetRegions (1);
		
		foreach (List<Coord> wallRegion in wallRegions) {
			if (wallRegion.Count < wallMinSize) {
				foreach (Coord tile in wallRegion) {
					grid[tile.x,tile.y] = 0;
				}
			}
		}
		
		List<List<Coord>> roomRegions = GetRegions (0);
		
		foreach (List<Coord> roomRegion in roomRegions) {
			if (roomRegion.Count < pocketMinSize) {
				foreach (Coord tile in roomRegion) {
					grid[tile.x,tile.y] = 1;
				}
			}
		}
	}

	/// <summary>
	/// Gets a list of all regions of a given
	/// tile type in the bitmap.
	/// </summary>
	List<List<Coord>> GetRegions(int tileType) {
		List<List<Coord>> regions = new List<List<Coord>> ();
		int[,] mapFlags = new int[xNumGraves,yNumGraves];
		
		for (int x = 0; x < xNumGraves; x ++) {
			for (int y = 0; y < yNumGraves; y ++) {
				if (mapFlags[x,y] == 0 && grid[x,y] == tileType) {
					List<Coord> newRegion = GetRegionTiles(x,y);
					regions.Add(newRegion);
					
					foreach (Coord tile in newRegion) {
						mapFlags[tile.x, tile.y] = 1;
					}
				}
			}
		}
		
		return regions;
	}

	/// <summary>
	/// Gets all region tiles of a region
	/// containing the given location.
	/// The list is ordered from closest to furthest
	/// coord.
	/// </summary>
	List<Coord> GetRegionTiles(int startX, int startY) {
		List<Coord> tiles = new List<Coord> ();
		int[,] mapFlags = new int[xNumGraves,yNumGraves];
		int tileType = grid [startX, startY];
		
		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (new Coord (startX, startY));
		mapFlags [startX, startY] = 1;
		
		while (queue.Count > 0) {
			Coord tile = queue.Dequeue();
			tiles.Add(tile);
			
			for (int x = tile.x - 1; x <= tile.x + 1; x++) {
				for (int y = tile.y - 1; y <= tile.y + 1; y++) {
					if (IsInMapRange(x,y) && (y == tile.y || x == tile.x)) {
						if (mapFlags[x,y] == 0 && grid[x,y] == tileType) {
							mapFlags[x,y] = 1;
							queue.Enqueue(new Coord(x,y));
						}
					}
				}
			}
		}
		
		return tiles;
	}
	
	bool IsInMapRange(int x, int y) {
		return x >= 0 && x < xNumGraves && y >= 0 && y < yNumGraves;
	}

	/// <summary>
	/// Smooths the bitmap through cellular
	/// automaton-like behavior.
	/// </summary>
	void SmoothMap () {
		for (int x = 0; x < xNumGraves; x++) {
			for (int y = 0; y < yNumGraves; y++) {
				int neighbors = CountNeighbors(x,y);

				if (neighbors > 5) {
					grid[x,y] = 1;
				} else if (neighbors < 4) {
					grid[x,y] = 0;
				}
			}
		}
	}

	/// <summary>
	/// Counts number of bitmap 1s next
	/// to a given neighbor.
	/// </summary>
	int CountNeighbors (int xin, int yin) {
		int wallCount = 0;
		for (int neighbourX = xin - 1; neighbourX <= xin + 1; neighbourX ++) {
			for (int neighbourY = yin - 1; neighbourY <= yin + 1; neighbourY ++) {
				if (neighbourX >= 0 && neighbourX < xNumGraves && neighbourY >= 0 && neighbourY < yNumGraves) {
					if (neighbourX != xin || neighbourY != yin) {
						wallCount += grid[neighbourX,neighbourY];
					}
				}
				else {
					wallCount ++;
				}
			}
		}
		
		return wallCount;
	}	

	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////

	
	public void GenerateMesh() {
		squareGrid = new SquareGrid(grid, xSpacing, ySpacing);
	}

	public class SquareGrid {
		public Square[,] squares;
		public Square gateSquare;
		
		public SquareGrid(int[,] map, float xspace, float yspace) {
			int nodeCountX = map.GetLength(0);
			int nodeCountY = map.GetLength (1);
			float mapWidth = nodeCountX * xspace;
			float mapHeight = nodeCountY * yspace;
			
			ControlNode[,] controlNodes = new ControlNode[nodeCountX,nodeCountY];
			
			for (int x = 0; x < nodeCountX; x ++) {
				for (int y = 0; y < nodeCountY; y ++) {
					Vector3 pos = new Vector3(-mapWidth/2.0f + x * xspace + xspace/2.0f,
					                          0.0f,
					                          -mapHeight/2.0f + y * yspace + yspace/2.0f);
					controlNodes[x,y] = new ControlNode(pos, map[x,y] == 1, yspace, xspace);
				}
			}
			
			squares = new Square[nodeCountX -1,nodeCountY -1];
			for (int x = 0; x < nodeCountX-1; x ++) {
				for (int y = 0; y < nodeCountY-1; y ++) {
					squares[x,y] = new Square(controlNodes[x,y+1],
					                          controlNodes[x+1,y+1],
					                          controlNodes[x+1,y],
					                          controlNodes[x,y]);
				}
			}
			gateSquare = null;
		}
	}
	
	public class Square {
		
		public ControlNode topLeft, topRight, bottomRight, bottomLeft;
		public Node centreTop, centreRight, centreBottom, centreLeft;
		public int configuration;
		
		public Square (ControlNode _topLeft,
		               ControlNode _topRight,
		               ControlNode _bottomRight,
		               ControlNode _bottomLeft) {
			topLeft = _topLeft;
			topRight = _topRight;
			bottomRight = _bottomRight;
			bottomLeft = _bottomLeft;
			
			centreTop = topLeft.right;
			centreRight = bottomRight.above;
			centreBottom = bottomLeft.right;
			centreLeft = bottomLeft.above;
			
			if (topLeft.active)
				configuration += 8;
			if (topRight.active)
				configuration += 4;
			if (bottomRight.active)
				configuration += 2;
			if (bottomLeft.active)
				configuration += 1;
		}
		
	}
	
	public class Node {
		public Vector3 position;
		public int vertexIndex = -1;
		
		public Node(Vector3 _pos) {
			position = _pos;
		}
	}
	
	public class ControlNode : Node {
		
		public bool active;
		public Node above, right;
		
		public ControlNode(Vector3 _pos, bool _active, float yspace, float xspace) : base(_pos) {
			active = _active;
			above = new Node(position + Vector3.forward * yspace / 2.0f);
			right = new Node(position + Vector3.right * xspace / 2.0f);
		}
		
	}
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	


	/// <summary>
	/// Places the cursed grave.
	/// </summary>
	void PlaceCursedGrave () {
		int x = 0, y = 0; // find gate square because I'm too lazy to actually put that data somewhere in here
		bool placed = false;

		for (x = 0; x < xNumGraves - 2; x++) {
			for (y = 0; y < yNumGraves - 2; y++) {
				if (squareGrid.gateSquare == squareGrid.squares[x, y]) {
					break;
				}
			}
			if (squareGrid.gateSquare == squareGrid.squares[x, y]) {
				break;
			}
		}

		List<Coord> region = GetRegionTiles (x, y + 1);
		float placementProbability = 0.2f;
		float increment = (1.0f - placementProbability) / (0.4f * region.Count);
		// placed in the furthest 40% somewhere
		while (!placed) {
			if (Random.value < placementProbability) {
				Coord loc = region[region.Count - 1];
				grid[loc.x, loc.y] = -1; // mark as cursed tomb
				placed = true;
			} else {
				region.RemoveAt(region.Count - 1);
				placementProbability += increment;
			}
		}
	}

	// used when generating paths
	struct Path {
		public int x, y;
		public int maxLength;
		public int distSoFar;
		public int direction;
		public Path(int xin, int yin, int maxLen, int dir) {
			x = xin;
			y = yin;
			maxLength = maxLen;
			distSoFar = 0;
			direction = dir;
		}
	}

	/// <summary>
	/// Generates the paths.
	/// </summary>
	void GeneratePaths () {
		Queue<Path> pathQueue = new Queue<Path>();
		int x = 0, y = 0; // find gate square because I'm too lazy to actually put that data somewhere in here
		int avglen = 4;

		for (x = 0; x < xNumGraves - 2; x++) {
			for (y = 0; y < yNumGraves - 2; y++) {
				if (squareGrid.gateSquare == squareGrid.squares[x, y]) {
					break;
				}
			}
			if (squareGrid.gateSquare == squareGrid.squares[x, y]) {
				break;
			}
		}

		pathQueue.Enqueue (new Path (x, y + 1, Random.Range(avglen - 1, avglen + 2), 0));
		while (pathQueue.Count > 0) {
			Path popped = pathQueue.Dequeue ();
			while (CheckNextPathSpace(popped)) {
				popped.distSoFar++;
			}
			ReservePathSquares(popped);
			MakeKidPaths(popped, pathQueue);
		}
	}

	void MakeKidPaths(Path path, Queue<Path> pathQueue) {

	}

	void ReservePathSquares (Path path) {
		int i;
		for (i = 0; i < path.distSoFar; i++) {
			switch (path.direction) {
			case 0: { // up
				squareGrid.squares[path.x, path.y + i].configuration = -16; // flag as path
				break;
			}
			case 1: { // up right
				squareGrid.squares[path.x + i, path.y + i].configuration = -16;
				break;
			}
			case 2: { // right
				squareGrid.squares[path.x + i, path.y].configuration = -16;
				break;
			}
			case 3: { // down right
				squareGrid.squares[path.x + i, path.y - i].configuration = -16;
				break;
			}
			case 4: { // down
				squareGrid.squares[path.x, path.y - i].configuration = -16;
				break;
			}
			case 5: { // down left
				squareGrid.squares[path.x - i, path.y - i].configuration = -16;
				break;
			}
			case 6: { // left
				squareGrid.squares[path.x - i, path.y].configuration = -16;
				break;
			}
			case 7: { // up left
				squareGrid.squares[path.x - i, path.y + i].configuration = -16;
				break;
			}
			}
		}
	}

	bool CheckNextPathSpace (Path path) {
		switch (path.direction) {
		case 0: { // up
			if (path.distSoFar < path.maxLength &&
			    squareGrid.squares[path.x, path.y + path.distSoFar].configuration == 0) {
				return true;
			} else {
				return false;
			}
		};
		case 1: { // up right
			if (path.distSoFar < path.maxLength &&
			    squareGrid.squares[path.x + path.distSoFar, path.y + path.distSoFar].configuration == 0) {
				return true;
			} else {
				return false;
			}
		}
		case 2: { // right
			if (path.distSoFar < path.maxLength &&
			    squareGrid.squares[path.x + path.distSoFar, path.y].configuration == 0) {
				return true;
			} else {
				return false;
			}
		}
		case 3: { // down right
			if (path.distSoFar < path.maxLength &&
			    squareGrid.squares[path.x + path.distSoFar, path.y - path.distSoFar].configuration == 0) {
				return true;
			} else {
				return false;
			}
		}
		case 4: { // down
			if (path.distSoFar < path.maxLength &&
			    squareGrid.squares[path.x, path.y - path.distSoFar].configuration == 0) {
				return true;
			} else {
				return false;
			}
		}
		case 5: { // down left
			if (path.distSoFar < path.maxLength &&
			    squareGrid.squares[path.x - path.distSoFar, path.y - path.distSoFar].configuration == 0) {
				return true;
			} else {
				return false;
			}
		}
		case 6: { // left
			if (path.distSoFar < path.maxLength &&
			    squareGrid.squares[path.x - path.distSoFar, path.y].configuration == 0) {
				return true;
			} else {
				return false;
			}
		}
		case 7: { // up left
			if (path.distSoFar < path.maxLength &&
			    squareGrid.squares[path.x - path.distSoFar, path.y + path.distSoFar].configuration == 0) {
				return true;
			} else {
				return false;
			}
		}
		}
		return false;
	}

	
	/// <summary>
	/// Generates tombstones in any open bitmap spot.
	/// </summary>
	void GenerateTombstones() {
		Vector3 graveOrigin = new Vector3 (-xSpacing * xNumGraves / 2.0f + xSpacing / 2.0f,
		                                   0.0f,
		                                   -ySpacing * yNumGraves / 2.0f + 2.0f * ySpacing / 2.0f);
		for (int x = 0; x < xNumGraves; x++) {
			for (int y = 0; y < yNumGraves; y++) {
				// only place graves in places not out of bounds or used by paths
				if (grid[x,y] == 0) {
					Vector3 loc = new Vector3(xSpacing * x + graveOrigin.x - leeway/2.0f +
					                          	Random.value * leeway * xSpacing,
					                          0.0f,
					                          ySpacing * y + graveOrigin.z - leeway/2.0f +
					                          	Random.value * leeway * ySpacing);
					GameObject grave = null;
					switch (Random.Range (0, 11)) {
					case 0:
					case 1:
					case 2: {
						grave = (GameObject)(Instantiate (grave1, transform.position + loc, Quaternion.identity));
						RotateTombstone (grave, Random.Range (0,12));
						break;
					}
					case 3:
					case 4:
					case 5: {
						grave = (GameObject)(Instantiate (grave2, transform.position + loc, Quaternion.identity));
						RotateTombstone (grave, Random.Range (0,12));
						break;
					}
					case 6:
					case 7:
					case 8: {
						grave = (GameObject)(Instantiate (grave3, transform.position + loc, Quaternion.identity));
						RotateTombstone (grave, Random.Range (0,12));
						break;
					}
					case 9: {
						grave = (GameObject)(Instantiate (tree,
						                                  transform.position + loc + new Vector3(0.0f, 3.0f, 0.0f),
						                                  Quaternion.identity));
						RotateTree (grave, Random.Range (0, 12));
						break;
					}
					case 10: {
						grave = (GameObject)(Instantiate (lamp,
						                                  transform.position + loc + new Vector3(0.0f, 2.7f, 0.0f),
						                                  Quaternion.identity));
						grave.transform.eulerAngles = new Vector3(270.0f, 0.0f, 0.0f);
						break;
					}
					}
				} else if (grid[x, y] == -1) {
					Vector3 loc = new Vector3(xSpacing * x + graveOrigin.x - leeway/2.0f +
					                          Random.value * leeway * xSpacing,
					                          0.0f,
					                          ySpacing * y + graveOrigin.z - leeway/2.0f +
					                          Random.value * leeway * ySpacing);
					GameObject grave = (GameObject)(Instantiate (CursedGrave,
					             								 transform.position + loc + new Vector3(0.0f, 2.7f, 0.0f),
					             								 Quaternion.identity));
					RotateTombstone (grave, Random.Range (0, 12));
				}
			}
		}
	}

	/// <summary>
	/// Rotates a tombstone/normal object.
	/// </summary>	
	void RotateTombstone(GameObject tombstone, int direction) {
		switch (direction) {
		case 0: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
			break;
		}
		case 1: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 30.0f, 0.0f);
			break;
		}
		case 2: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 60.0f, 0.0f);
			break;
		}
		case 3: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
			break;
		}
		case 4: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 120.0f, 0.0f);
			break;
		}
		case 5: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 150.0f, 0.0f);
			break;
		}
		case 6: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
			break;
		}
		case 7: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 210.0f, 0.0f);
			break;
		}
		case 8: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 240.0f, 0.0f);
			break;
		}
		case 9: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 270.0f, 0.0f);
			break;
		}
		case 10: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 300.0f, 0.0f);
			break;
		}
		case 11: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 330.0f, 0.0f);
			break;
		}
		}
	}

	/// <summary>
	/// Rotates a tree object.
	/// </summary>
	void RotateTree(GameObject tombstone, int direction) {
		switch (direction) {
		case 0: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 0.0f, 0.0f);
			break;
		}
		case 1: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 30.0f, 0.0f);
			break;
		}
		case 2: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 60.0f, 0.0f);
			break;
		}
		case 3: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 90.0f, 0.0f);
			break;
		}
		case 4: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 120.0f, 0.0f);
			break;
		}
		case 5: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 150.0f, 0.0f);
			break;
		}
		case 6: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 180.0f, 0.0f);
			break;
		}
		case 7: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 210.0f, 0.0f);
			break;
		}
		case 8: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 240.0f, 0.0f);
			break;
		}
		case 9: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 270.0f, 0.0f);
			break;
		}
		case 10: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 300.0f, 0.0f);
			break;
		}
		case 11: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 330.0f, 0.0f);
			break;
		}
		}
	}

	bool PlaceGate () {
		Queue<Square> gateSpots = new Queue<Square> ();
		// first, enqueue possible spots
		for (int x = xNumGraves - 3; x > 0; x--) {
			for (int y = yNumGraves - 3; y > 0; y--) {
				if (squareGrid.squares[x, y].configuration == 3 &&
				    squareGrid.squares[x - 1, y].configuration == 3 &&
				    squareGrid.squares[x + 1, y].configuration == 3) {
					gateSpots.Enqueue (squareGrid.squares[x, y]);
				}
			}
		}
		float chance = 1.0f / gateSpots.Count;
		float step = chance;

		// now place it
		while (gateSpots.Count > 0) {
			Square popped = gateSpots.Dequeue();
			if (Random.value < chance) {
				squareGrid.gateSquare = popped;
				return true;
			} else {
				chance += step;
			}
		}
		return false;
	}

	/// <summary>
	/// Places fences based on configuration of squaregrid.
	/// </summary>
	void CreateWalls () {
		Vector3 fenceOrigin = new Vector3 (-xSpacing * xNumGraves / 2.0f + 1.0f * xSpacing,
		                                   0.0f,
		                                   -ySpacing * yNumGraves / 2.0f + 1.5f * ySpacing);
		if (squareGrid != null) {
			for (int x = 0; x < xNumGraves - 1; x++) {
				for (int y = 0; y < yNumGraves - 1; y++) {
					Vector3 pos = fenceOrigin + new Vector3 (x *xSpacing, 0.0f, y * ySpacing);
					GameObject newfence;
					switch (squareGrid.squares[x,y].configuration) {
					case 1: { // bot left diag, lr
						pos += new Vector3(-0.25f * xSpacing, 0.0f, -0.25f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
						break;
					}
					case 2: { // bot right diag, lr
						pos += new Vector3(0.25f * xSpacing, 0.0f, -0.25f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 00.0f, 0.0f);
						break;
					}
					case 3: { // horizontal, lr
						if (squareGrid.squares[x,y] == squareGrid.gateSquare) {
							newfence = (GameObject)(Instantiate (gate, pos, Quaternion.identity));
							newfence.transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
						} else if (squareGrid.squares[x - 1, y] != squareGrid.gateSquare && // not adjacent
						           squareGrid.squares[x + 1, y] != squareGrid.gateSquare) {
							newfence = (GameObject)(Instantiate (fence, pos, Quaternion.identity));
							newfence.transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
						}
						break;
					}
					case 4: { // top right diag, rl
						pos += new Vector3(0.25f * xSpacing, 0.0f, 0.25f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 270.0f, 0.0f);
						break;
					}
					case 5: { // 2 diag, cases 1 & 4
						pos += new Vector3(-0.25f * xSpacing, 0.0f, -0.25f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
						pos -= new Vector3(-0.25f * xSpacing, 0.0f, -0.25f * ySpacing);

						pos += new Vector3(0.25f * xSpacing, 0.0f, 0.25f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 270.0f, 0.0f);
						break;
					}
					case 6: { // vertical, bt
						newfence = (GameObject)(Instantiate (fence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
						break;
					}
					case 7: { // top left diag, lr
						pos += new Vector3(-0.25f * xSpacing, 0.0f, 0.25f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 00.0f, 0.0f);
						break;
					}
					case 8: { // top left diag, rl
						pos += new Vector3(-0.25f * xSpacing, 0.0f, 0.25f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
						break;
					}
					case 9: { // vertical, tb
						newfence = (GameObject)(Instantiate (fence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 00.0f, 0.0f);
						break;
					}
					case 10: { // 2 diag, cases 8 & 2
						pos += new Vector3(0.25f * xSpacing, 0.0f, -0.25f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 00.0f, 0.0f);
						pos -= new Vector3(0.25f * xSpacing, 0.0f, -0.25f * ySpacing);

						pos += new Vector3(-0.25f * xSpacing, 0.0f, 0.25f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
						break;
					}
					case 11: { // top right diag, lr
						pos += new Vector3(0.25f * xSpacing, 0.0f, 0.25f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
						break;
					}
					case 12: { // horizontal, rl
						newfence = (GameObject)(Instantiate (fence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
						break;
					}
					case 13: { // bot right diag, rl
						pos += new Vector3(0.25f * xSpacing, 0.0f, -0.25f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
						break;
					}
					case 14: { // bot left diag, rl
						pos += new Vector3(-0.25f * xSpacing, 0.0f, -0.25f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 270.0f, 0.0f);
						break;
					}
					case -16: { // path
						Instantiate (path, pos + new Vector3 (0.0f, 0.05f, 0.0f), Quaternion.identity);
						break;
					}
					}
				}
			}
		}
	}

	void PlacePlayer () {
		Vector3 pos = new Vector3 (-xSpacing * xNumGraves / 2.0f + 1.0f * xSpacing,
		                           0.0f,
		                           -ySpacing * yNumGraves / 2.0f + 1.75f * ySpacing);
		for (int x = 1; x < xNumGraves - 3; x++) {
			for (int y = 1; y < yNumGraves - 3; y++) {
				if (squareGrid.squares[x,y] == squareGrid.gateSquare) {
					pos += new Vector3(x * xSpacing, 0.0f, y * ySpacing);
					player.transform.position = pos;
				}
			}
		}
	}

	/// <summary>
	/// To make sure stuff works.
	/// </summary>
	void OnDrawGizmos() {
		if (grid != null) {
			for (int x = 0; x < xNumGraves; x ++) {
				for (int y = 0; y < yNumGraves; y ++) {
					Gizmos.color = (grid[x,y] == 1)?Color.black:Color.white;
					Vector3 pos = new Vector3((-xNumGraves/2 + x + .5f)*xSpacing,0, (-yNumGraves/2 + y+.5f)*ySpacing);
					Gizmos.DrawCube(pos,Vector3.one * xSpacing);
				}
			}
		}
	}
}
