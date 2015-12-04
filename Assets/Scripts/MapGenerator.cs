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
	public GameObject CursedGrave;

	// grid random fill percent
	[Range(0.0f, 1.0f)]
	public float fillPercent;

	// EliminatePockets information
	public int pocketMinSize = 20;
	public int wallMinSize = 10;

	// grids and vars for generation
	private int[,] grid;
	private Square[,] SquareGrid;
	private Square GateSquare;

	// used for marching squares
	struct Square {
		public Coord topLeft;
		public Coord topRight;
		public Coord botLeft;
		public Coord botRight;
		public int configuration;
		public bool processed;
		// initializer
		public Square(Coord tLin, Coord tRin,
		              Coord bLin, Coord bRin) {
			topLeft = tLin; topRight = tRin;
			botLeft = bLin; botRight = bRin;
			configuration = 0;
			processed = false;
		}
	}

	/// <summary>
	/// Called at beginning of program, creates map.
	/// </summary>
	void Start () {
		if (grave1 == null || grave2 == null || grave3 == null ||
		    tree == null || lamp == null) {
			return; // nothing to do here
		}
		grid = new int[xNumGraves, yNumGraves];
		SquareGrid = new Square[xNumGraves - 1,yNumGraves - 1];

		bool gatePlaced = false;
		//while (!gatePlaced) {
			// step 1: cellular automata and cleaning
			GenerateBitmap ();
			// step 2: calculate marching squares
			CalculateMarchingSquares ();
			// step 2.5: place gate
			//gatePlaced = PlaceGate ();
		//}
		// step 3: create paths and lamps
		//GeneratePaths ();
		// step 4: place cursed grave
		//PlaceCursedGrave ();
		// step 5: place graves
		GenerateTombstones ();
		// final: create walls, place player
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

	/// <summary>
	/// Calculates marching squares
	/// based on bitmap.
	/// </summary>
	void CalculateMarchingSquares () {
		for (int x = 0; x < xNumGraves - 1; x++) {
			for (int y = 0; y < yNumGraves - 1; y++) {
				SquareGrid[x,y] = new Square(new Coord(x, y),
				                             new Coord(x, y + 1),
				                             new Coord(x + 1, y),
				                             new Coord(x + 1, y + 1));
				CalculateConfiguration(SquareGrid[x,y]);
			}
		}
	}

	/// <summary>
	/// Calculates the configuration of
	/// a square based on state of grid.
	/// </summary>
	void CalculateConfiguration (Square square) {
		square.configuration += 8 * grid [square.topLeft.x, square.topLeft.y];

		if (IsInMapRange (square.topRight.x, square.topRight.y)) {
			square.configuration += 4 * grid [square.topRight.x, square.topRight.y];
		} else {
			square.configuration += 4;
		}
		if (IsInMapRange (square.botRight.x, square.botRight.y)) {
			square.configuration += 2 * grid[square.botRight.x, square.botRight.y];
		} else {
			square.configuration += 2;
		}
		if (IsInMapRange (square.botLeft.x, square.botLeft.y)) {
			square.configuration += grid[square.botLeft.x, square.botLeft.y];
		} else {
			square.configuration += 1;
		}
	}

	/// <summary>
	/// Places the gate in a marching square of type 3.
	/// If none exists, restarts the generation process.
	/// </summary>
	bool PlaceGate() {
		List<Square> gatePossibilities = GetGatePlaces ();
		if (gatePossibilities.Count > 0) {
			int choice = Random.Range (0, gatePossibilities.Count);
			GateSquare = gatePossibilities[choice];
			GateSquare.processed = true;
			return true;
		} else {
			return false;
		}
	}

	List<Square> GetGatePlaces () {
		List<Square> gatePlaces = new List<Square> ();
		for (int x = 0; x < xNumGraves - 1; x++) {
			for (int y = 0; y < yNumGraves - 1; y++) {
				if (SquareGrid[x, y].configuration == 3 && x > 0 && // is flat lower bound
				    SquareGrid[x - 1, y].configuration == 0) { // one above is empty
					gatePlaces.Add (SquareGrid[x,y]);
				}
			}
		}
		return gatePlaces;
	}



	/// <summary>
	/// Generates the paths.
	/// </summary>
	void GeneratePaths () {

	}

	/// <summary>
	/// Places the cursed grave
	/// in an available place.
	/// </summary>
	void PlaceCursedGrave () {
		List<Coord> possiblePlaces = GetRegionTiles (GateSquare.topLeft.x, GateSquare.topLeft.y);
		int regionSize = possiblePlaces.Count;
		float chance = 0.1f;
		float increment = (1.0f - chance) / (regionSize * 0.4f);
		int idx = regionSize - 1;

		while (chance <= 1.0f) {
			if (Random.value <= chance) {
				Coord place = possiblePlaces[idx];
				Vector3 pos = new Vector3(place.x * xSpacing, 0, place.y * ySpacing);
				Instantiate(CursedGrave, pos, Quaternion.identity);
				break;
			}
			idx--;
			chance += increment;
		}
	}

	/// <summary>
	/// Generates tombstones in any open bitmap spot.
	/// </summary>
	void GenerateTombstones() {
		for (int x = 0; x < xNumGraves; x++) {
			for (int y = 0; y < yNumGraves; y++) {
				// only place graves in places not out of bounds or used by paths
				if (grid[x,y] == 0) {
					Vector3 loc = new Vector3(xSpacing * x - xSpacing * xNumGraves/2 - leeway/2 + Random.value * leeway * xSpacing,
					                          0,
					                          ySpacing * y - ySpacing * yNumGraves/2 - leeway/2 + Random.value * leeway * ySpacing);
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
