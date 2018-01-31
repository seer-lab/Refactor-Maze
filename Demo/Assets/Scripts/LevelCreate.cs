using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
	public bool foundNeighbours = false;
	public Vector2 position;
	public List<Cell> neighbours = new List<Cell>();
}

public class LevelCreate : MonoBehaviour {


	const int PASSABLE = 0;
	const int OCCUPIED = 1;
	const int INVALID = 2;
	const int IMPASSABLE = 3;

	const int NOT_VISITED = 0;
	const int VISISTED = 1;

	public int columns = 8;
	public int rows = 8;

	public int mazesX = 2;
	public int mazesY = 2;

	public GameObject[] floorTiles;
	public GameObject wallTiles;
	public GameObject key;
	public GameObject door; 

	public GameObject player;
	public GameObject mainCamera;

	public GameObject tester;
	public GameObject code;


	public GameObject outerWallTile; 

	//	private Transform boardHolder;  
	//int value is the type of tile
	public Dictionary<Vector2, int> theTiles = new Dictionary<Vector2, int>();

	private Dictionary<Vector2, GameObject> outerWalls = new Dictionary<Vector2, GameObject> ();

	private List <Vector2> validPositions = new List<Vector2>();

	private List <List<Vector2>> vPositions = new List<List<Vector2>>();


	//private List <Vector2> paths = new List<Vector2>();
	private List <Vector2> thePath = new List<Vector2>();

	private Stack<Cell> cells = new Stack<Cell>();

	private Dictionary<Vector2, int> visitedCells = new Dictionary<Vector2, int>();

	private Transform level;  
	private Transform codeLevel;

	private int width;
	private int height;

	//Might scrap this method entirely, if not rework it drastically.
	void InitialiseList ()
	{
		 List <List<Vector2>> test = new List<List<Vector2>>();

		for (int i = 0; i < mazesX; i++) 
		{
			test.Add (new List<Vector2> ());
		}

		//This loop is currently going over points that should be walls and skipped
		//Needs to be reworked
		//Loop through x axis (columns).
		for(int x = 0; x < columns + width; x++)
		{
			//Within each column, loop through y axis (rows).
			for(int y = 0; y < rows + height; y++)
			{
				//At each index add a new Vector2 to our list with the x and y coordinates of that position.
				//theTiles.Add(new Vector2(x, y), PASSABLE);
				theTiles.Add(new Vector2(x, y), IMPASSABLE);

				validPositions.Add (new Vector2 (x, y));
			}
		}
	}

	void BoardSetup ()
	{
		level = new GameObject ("Level").transform;
		codeLevel = new GameObject ("CodeLevel").transform;
		GameObject whatever = Instantiate (code, Vector2.zero, Quaternion.identity);
		whatever.transform.SetParent (codeLevel);

		Transform outer = new GameObject ("OuterWalls").transform;
		outer.SetParent (level);

		for(int x = -1; x <= columns + width; x++)
		{
			for(int y = -1; y <= rows + height; y++)
			{
				GameObject toInstantiate;

				//Could probably rework this loop, doing this check every time through isn't great.
				//Off the top of my head would require at least 3 loops, we'll see
				if (((y + 1) % (rows + 1) == 0) ||((x + 1) % (columns + 1) == 0) )
				{
					toInstantiate = outerWallTile;
					GameObject instance = Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity);
					instance.transform.SetParent (level);

					outerWalls.Add (new Vector2 (x, y), instance);
				}
			}
		}
	}

	//RandomPosition returns a random position from our list gridPositions.
	//This function is modified http://unity3d.com/learn/tutorials/projects/2d-roguelike
/*	Vector3 RandomPosition ()
	{
		int randomIndex = Random.Range (0, validPositions.Count);
		Vector3 randomPosition = validPositions[randomIndex];
		//Remove the entry at randomIndex from the list so that it can't be re-used.
		validPositions.RemoveAt (randomIndex);

		theTiles[new Vector2(randomPosition.x, randomPosition.y)] = OCCUPIED;

		//Return the randomly selected Vector3 position.
		return randomPosition;
	}*/

	//RandomPosition returns a random position from our list gridPositions.
	//This function is modified http://unity3d.com/learn/tutorials/projects/2d-roguelike
	Vector3 RandomPosition (int currentLevel)
	{
		int randomIndex = Random.Range (0, vPositions[currentLevel].Count);
		Vector3 randomPosition = vPositions[currentLevel][randomIndex];
		//Remove the entry at randomIndex from the list so that it can't be re-used.
		vPositions[currentLevel].RemoveAt (randomIndex);

		theTiles[new Vector2(randomPosition.x, randomPosition.y)] = OCCUPIED;

		//Return the randomly selected Vector3 position.
		return randomPosition;
	}

	 //still need to remove wall tiles from list of valid positions
	void BackTracker(Vector2 minBounds, Vector2 maxBounds, int currentLevel)
	{
		int unvisitedCells = 0;

		for(int x = (int)minBounds.x; x < maxBounds.x; x+=2)
		{
			//Within each column, loop through y axis (rows).
			for(int y = (int)minBounds.y; y < maxBounds.y; y+=2)
			{
				visitedCells.Add(new Vector2(x, y), NOT_VISITED);
				unvisitedCells++;
			}
		}
	
		List <Vector2> wallIndexes = new List<Vector2>();

		vPositions.Add (new List<Vector2> ());

		for (int x = (int)minBounds.x; x < maxBounds.x; x++) 
		{
			for (int y = (int)minBounds.y; y < maxBounds.y; y++) 
			{
				wallIndexes.Add (new Vector2 (x, y));
				vPositions [currentLevel].Add (new Vector2 (x, y));
			}
		}
			

		Cell currentCell = new Cell();
		//currentCell.position = new Vector2(maxBounds.x / 2 , maxBounds.y / 2 );
		currentCell.position = new Vector2(minBounds.x , minBounds.y );
		visitedCells[currentCell.position] = VISISTED;
		unvisitedCells--;
		thePath.Add (currentCell.position);

		while (unvisitedCells > 0) 
		{
			if (cellHasNeighbours (currentCell, minBounds, maxBounds)) 
			{
				int neighbour = Random.Range(0,currentCell.neighbours.Count);
				cells.Push (currentCell);
				//add to path

				Vector2 something = currentCell.position + (currentCell.neighbours [neighbour].position - currentCell.position) / 2;
				//thePath.Add (currentCell.position);
				theTiles [currentCell.position] = PASSABLE;

				wallIndexes.Remove (currentCell.position);
				currentCell = currentCell.neighbours[neighbour];
				//thePath.Add (something);
				//thePath.Add (currentCell.position);

				theTiles [something] = PASSABLE;
				wallIndexes.Remove (something);
				theTiles [currentCell.position] = PASSABLE;
				wallIndexes.Remove (currentCell.position);

				visitedCells [currentCell.position] = VISISTED;

				unvisitedCells--;
			} 
			else
			{
				currentCell = cells.Pop ();
			}
		}

		RemoveRandomWalls(wallIndexes, minBounds, maxBounds);

		SetWalls (wallIndexes, currentLevel);

		visitedCells.Clear ();
	}

	bool cellHasNeighbours(Cell cell, Vector2 minBounds, Vector2 maxBounds)
	{
		if (!cell.foundNeighbours) 
		{
			if (cell.position.x - 2 >= minBounds.x) 
			{
				Cell newCell = new Cell ();
				newCell.position = new Vector2 (cell.position.x - 2, cell.position.y);
				cell.neighbours.Add (newCell);
			}
			if (cell.position.x + 2 < maxBounds.x) 
			{
				Cell newCell = new Cell ();
				newCell.position = new Vector2 (cell.position.x + 2, cell.position.y);
				cell.neighbours.Add (newCell);
			}
			if (cell.position.y - 2 >= minBounds.y ) 
			{
				Cell newCell = new Cell ();
				newCell.position = new Vector2 (cell.position.x, cell.position.y - 2);
				cell.neighbours.Add (newCell);
			}
			if (cell.position.y + 2 < maxBounds.y) 
			{
				Cell newCell = new Cell ();
				newCell.position = new Vector2 (cell.position.x, cell.position.y + 2);
				cell.neighbours.Add (newCell);
			}
			cell.foundNeighbours = true;
		}

		for (int i = cell.neighbours.Count - 1; i >= 0; i --) 
		{
			if (visitedCells [cell.neighbours [i].position] == VISISTED)
			{
				cell.neighbours.RemoveAt (i);
			}
		}

		return cell.neighbours.Count > 0;
	}

	public bool InBounds(Vector2 position)
	{
		return position.x > -1 && position.x < columns && position.y > -1 && position.y < rows;
	}
	public bool IsPassable(Vector2 position)
	{
		return InBounds(position) && theTiles[position] != IMPASSABLE;
	}

	public bool InBounds(Vector2 position, Vector2 minBound, Vector2 maxBound)
	{
		return position.x >= minBound.x && position.x < maxBound.x && position.y >= minBound.y && position.y < maxBound.y;
	}
	public bool IsPassable(Vector2 position, Vector2 minBound, Vector2 maxBound)
	{
		return InBounds(position, minBound, maxBound) && theTiles[position] != IMPASSABLE;
	}

	//This function turns the maze into an imperfect maze, meaning that there are loops
	void RemoveRandomWalls(List<Vector2> wallIndexes, Vector2 minBounds, Vector2 maxBounds)
	{
		for (int i = 0; i < columns/2; i++)
		{
			int index = Random.Range (0, wallIndexes.Count);

			Vector2 right = new Vector2 (wallIndexes [index].x + 1, wallIndexes [index].y);
			Vector2 left = new Vector2 (wallIndexes [index].x - 1, wallIndexes [index].y);
			Vector2 up = new Vector2 (wallIndexes [index].x, wallIndexes [index].y + 1);
			Vector2 down = new Vector2 (wallIndexes [index].x, wallIndexes [index].y - 1);

			if (!IsPassable (right, minBounds, maxBounds) &&
				!IsPassable (left, minBounds, maxBounds) &&
				(IsPassable (up, minBounds, maxBounds) &&
					IsPassable (down, minBounds, maxBounds))) 
			{
				wallIndexes.RemoveAt (index);
			} 
			else if (!IsPassable (up, minBounds, maxBounds) &&
				!IsPassable (down, minBounds, maxBounds) &&
				(IsPassable (left, minBounds, maxBounds) &&
					IsPassable (right, minBounds, maxBounds))) 
			{
				wallIndexes.RemoveAt (index);

			}
			else 
			{
				i--;
			}
		}
	}

	//Set walls for all the remaining wall indexes
	void SetWalls(List<Vector2> wallIndexes, int currentLevel)
	{
		for (int i = 0; i < wallIndexes.Count; i++)
		{
			GameObject instance = Instantiate(wallTiles, wallIndexes [i], Quaternion.identity);
			instance.transform.SetParent (level);
			validPositions.Remove (wallIndexes [i]);
			vPositions [currentLevel].Remove (wallIndexes [i]);
		} 
	}

	void CreateDoor(Vector2 position)
	{
		GameObject instance = Instantiate (door, position, Quaternion.identity);
		instance.transform.SetParent (level);
		Destroy (outerWalls [instance.transform.position]);
		outerWalls.Remove (instance.transform.position);

		GameObject exitObject = new GameObject("Exit");
		exitObject.tag = "Exit";
		BoxCollider2D bc = exitObject.AddComponent (typeof(BoxCollider2D)) as BoxCollider2D;
		bc.isTrigger = true;
		bc.size = new Vector2 (0.9f, 0.9f);

		exitObject.transform.position = instance.transform.position;
	}

	void SetDoors()
	{
		int x = 1;
		int y = 0;
		bool right = true;
		for (int i = 0; i < mazesX * mazesY; i++) 
		{
			if (right) 
			{
				if (x == mazesX) 
				{
					y++;
					CreateDoor (new Vector2 (((columns - 1) + ((columns + 1) * (x - 1))), ((rows + 1) * y) - 1));
					x--;
					right = false;
				}
				else
				{
					//CreateDoor (new Vector2 (((columns + 1) * x) - 1, 0 + ((y - 1) * rows) + 1));
					CreateDoor (new Vector2 (((columns + 1) * x) - 1, (rows - 1) + (rows + 1) * y)); //starts at the top

					x++;
				}

			}
			else
			{
				if (x == 0) 
				{
					y++;

					CreateDoor (new Vector2 (((columns) * x), ((rows + 1) * y) - 1));
					x++;
					right = true;
				}
				else
				{
					CreateDoor (new Vector2 (((columns + 1) * x) - 1, 0 + (rows + 1) * y));
					x--;
				}
			}
		}
	}

	// Use this for initialization
	void Start () 
	{

		if (mazesX <= 1) 
		{
			width = 0;
		}
		else 
		{
			width = (mazesX - 1) * (columns + 1);
		}

		if (mazesY <= 1) 
		{
			height = 0;
		}
		else 
		{
			height = (mazesY - 1) * (rows + 1);
		}

		InitialiseList ();
		BoardSetup ();

		GameObject instance;
		//need to rework this
		int currentLevel = 0;
		for (int i = 0; i < mazesX; i++) 
		{
			for (int c = 0; c < mazesY; c++) 
			{
				BackTracker(new Vector2(i * (columns + 1), c * (rows + 1)), 
					new Vector2(columns + (columns + 1) * i, rows + (rows + 1) * c), currentLevel);
				instance = Instantiate (key, RandomPosition (currentLevel), Quaternion.identity);
				currentLevel++;
				instance.transform.SetParent (level);

			}
		}
		//Instantiate (key, RandomPosition (), Quaternion.identity);
		//Instantiate (key, RandomPosition (), Quaternion.identity);

		//instance = Instantiate (key, new Vector2(0, 20), Quaternion.identity);

		SetDoors ();

		instance = Instantiate (player, new Vector2 (columns / 2, rows / 2), Quaternion.identity);
		instance.transform.SetParent (level);
		Instantiate (tester, Vector2.zero, Quaternion.identity);


		mainCamera.transform.position = new Vector3 (columns / 2, rows / 2, -10.0f);
		mainCamera.GetComponent<Camera> ().orthographicSize = (columns/2) + 1.5f;
	}

	// Update is called once per frame
	void Update () 
	{

	}
}
