using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

	public int xNumGraves = 20;
	public int yNumGraves = 20;
	public float xSpacing = 20.0f;
	public float ySpacing = 20.0f;

	[Range(0.0f, 1.0f)]
	public float leeway = 0.4f;

	public GameObject grave1;
	public GameObject grave2;
	public GameObject grave3;
	public GameObject tree;
	public GameObject lamp;
	public GameObject fence;

	[Range(0.0f, 1.0f)]
	public float fillPercent;

	public int pocketMinSize = 20;
	public int wallMinSize = 10;

	private int[,] grid;

	// Use this for initialization
	void Start () {
		if (grave1 == null || grave2 == null || grave3 == null ||
		    tree == null || lamp == null) {
			return; // nothing to do here
		}
		grid = new int[xNumGraves, yNumGraves];

		GenerateBorders ();
		GeneratePaths ();
		GenerateTombstones ();
	}

	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			GenerateBorders();
		}
	}

	// Border generation (cellular automata)
	void GenerateBorders () {
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

	// Path generation (recursively generate inside borders)
	void GeneratePaths () {

	}

	// Tombstone generation	(on remaining empty cells)
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
