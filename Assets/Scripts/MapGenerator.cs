using UnityEngine;
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

	// GameObjects to be instantiated
	public GameObject grave1;
	public GameObject grave2;
	public GameObject grave3;
	public GameObject tree;
	public GameObject lamp;
	public GameObject fence;
	public GameObject shortFence;
	public GameObject CursedGrave;

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
		if (grave1 == null || grave2 == null || grave3 == null ||
		    tree == null || lamp == null || fence == null) {
			return; // nothing to do here
		}
		if (shortFence == null) { // default shortFence to fence
			shortFence = fence;
		}
		grid = new int[xNumGraves, yNumGraves];

		GenerateBitmap ();
		GenerateMesh ();
		GenerateTombstones ();
		CreateWalls ();
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
	/// Generates the paths.
	/// </summary>
	void GeneratePaths () {

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
					Vector3 loc = new Vector3(xSpacing * x + graveOrigin.x - leeway/2.0f + Random.value * leeway * xSpacing,
					                          0.0f,
					                          ySpacing * y + graveOrigin.z - leeway/2.0f + Random.value * leeway * ySpacing);
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
						grave = (GameObject)(Instantiate (tree, transform.position + loc + new Vector3(0.0f, 3.0f, 0.0f), Quaternion.identity));
						RotateTree (grave, Random.Range (0, 12));
						break;
					}
					case 10: {
						grave = (GameObject)(Instantiate (lamp, transform.position + loc + new Vector3(0.0f, 2.7f, 0.0f), Quaternion.identity));
						grave.transform.eulerAngles = new Vector3(270.0f, 0.0f, 0.0f);
						break;
					}
					}
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

	/// <summary>
	/// Places fences based on configuration of squaregrid.
	/// </summary>
	void CreateWalls () {
		Vector3 fenceOrigin = new Vector3 (-xSpacing * xNumGraves / 2.0f + 1.0f * xSpacing,
		                                   0.0f,
		                                   -ySpacing * yNumGraves / 2.0f + 1.0f * ySpacing);
		if (squareGrid != null) {
			for (int x = 0; x < xNumGraves - 1; x++) {
				for (int y = 0; y < yNumGraves - 1; y++) {
					Vector3 pos = fenceOrigin + new Vector3 (x *xSpacing, 0.0f, y * ySpacing);
					GameObject newfence;
					switch (squareGrid.squares[x,y].configuration) {
					case 1: {
						goto case 14;
					}
					case 2: {
						goto case 13;
					}
					case 3: {
						goto case 12;
					}
					case 4: {
						goto case 11;
					}
					case 5: { // 2 diag topright & botleft
						pos += new Vector3 (0.40f * xSpacing, 0.0f, 0.65f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, -45.0f, 0.0f);
						newfence.transform.localScale = new Vector3(1.0f, 1.0f, 0.707f);
						pos -= new Vector3 (0.40f * xSpacing, 0.0f, 0.65f * ySpacing);

						pos += new Vector3 (-0.1f * xSpacing, 0.0f, 0.15f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, -45.0f, 0.0f);
						newfence.transform.localScale = new Vector3(1.0f, 1.0f, 0.707f);
						break;
					}
					case 6: {
						goto case 9;
					}
					case 7: {
						goto case 8;
					}
					case 8: { // top left diag
						pos += new Vector3 (-0.40f * xSpacing, 0.0f, 0.65f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 45.0f, 0.0f);
						newfence.transform.localScale = new Vector3(1.0f, 1.0f, 0.707f);
						break;
					}
					case 9: { // vertical
						pos += new Vector3 (0.0f, 0.0f, +0.25f * ySpacing);
						newfence = (GameObject)(Instantiate (fence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 00.0f, 0.0f);
						break;
					}
					case 10: { // 2 diag topleft & botright
						pos += new Vector3 (-0.40f * xSpacing, 0.0f, 0.65f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 45.0f, 0.0f);
						newfence.transform.localScale = new Vector3(1.0f, 1.0f, 0.707f);
						pos -= new Vector3 (-0.40f * xSpacing, 0.0f, 0.65f * ySpacing);

						pos += new Vector3 (0.1f * xSpacing, 0.0f, 0.15f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 45.0f, 0.0f);
						newfence.transform.localScale = new Vector3(1.0f, 1.0f, 0.707f);
						break;
					}
					case 11: { // top right diag
						pos += new Vector3 (0.40f * xSpacing, 0.0f, 0.65f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, -45.0f, 0.0f);
						newfence.transform.localScale = new Vector3(1.0f, 1.0f, 0.707f);
						break;
					}
					case 12: { // horizontal
						pos += new Vector3 (-0.3f * xSpacing, 0.0f, 0.55f * ySpacing);
						newfence = (GameObject)(Instantiate (fence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
						break;
					}
					case 13: { // bot right diag
						pos += new Vector3 (0.1f * xSpacing, 0.0f, 0.15f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, 45.0f, 0.0f);
						newfence.transform.localScale = new Vector3(1.0f, 1.0f, 0.707f);
						break;
					}
					case 14: { // bot left diag
						pos += new Vector3 (-0.1f * xSpacing, 0.0f, 0.15f * ySpacing);
						newfence = (GameObject)(Instantiate (shortFence, pos, Quaternion.identity));
						newfence.transform.eulerAngles = new Vector3(0.0f, -45.0f, 0.0f);
						newfence.transform.localScale = new Vector3(1.0f, 1.0f, 0.707f);
						break;
					}
					}
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
