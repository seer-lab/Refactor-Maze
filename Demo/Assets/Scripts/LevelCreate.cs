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

	int unvisitedCells;

	public int columns = 8;
	public int rows = 8;
	public GameObject[] floorTiles;
	public GameObject wallTiles;
	public GameObject key;
	public GameObject door; 

	public GameObject player;
	public GameObject camera;

	public GameObject tester;
	public GameObject code;


	public GameObject outerWallTile; 

	//	private Transform boardHolder;  
	//int value is the type of tile
	public Dictionary<Vector2, int> theTiles = new Dictionary<Vector2, int>();
	private List <Vector2> validPositions = new List<Vector2>();

	private List <Vector2> paths = new List<Vector2>();
	private List <Vector2> thePath = new List<Vector2>();

	private Stack<Cell> cells = new Stack<Cell>();

	private Dictionary<Vector2, int> visitedCells = new Dictionary<Vector2, int>();

	private Transform level;  
	private Transform codeLevel;

	void InitialiseList ()
	{
		//Loop through x axis (columns).
		for(int x = 0; x < columns; x++)
		{
			//Within each column, loop through y axis (rows).
			for(int y = 0; y < rows; y++)
			{
				//At each index add a new Vector2 to our list with the x and y coordinates of that position.
				//theTiles.Add(new Vector2(x, y), PASSABLE);
				theTiles.Add(new Vector2(x, y), IMPASSABLE);

				validPositions.Add(new Vector2(x, y));
			}
		}

		for(int x = 0; x < columns; x+=2)
		{
			//Within each column, loop through y axis (rows).
			for(int y = 0; y < rows; y+=2)
			{
				paths.Add(new Vector2(x, y));
				visitedCells.Add(new Vector2(x, y), NOT_VISITED);
				unvisitedCells++;
			}
		}

	}

	void BoardSetup ()
	{
		level = new GameObject ("Level").transform;
		//codeLevel = new GameObject ("CodeLevel").transform;
		//GameObject whatever = Instantiate (code, Vector2.zero, Quaternion.identity);
		//whatever.transform.SetParent (codeLevel);
		//codeLevel.gameObject.SetActive (false);


		for(int x = -1; x <= columns; x++)
		{
			for(int y = -1; y <= rows; y++)
			{
				GameObject toInstantiate;

				//Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
				if(x == -1 || x == columns || y == -1 || y == rows)
				{
					if (x == 20 && y == -1)
					{
						//eww
					} 
					else 
					{
						toInstantiate = outerWallTile;
						GameObject instance = Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity);
						instance.transform.SetParent (level);
					}
				}

			}
		}
	}

	//RandomPosition returns a random position from our list gridPositions.
	//This function is modified http://unity3d.com/learn/tutorials/projects/2d-roguelike
	Vector3 RandomPosition ()
	{
		int randomIndex = Random.Range (0, validPositions.Count);
		Vector3 randomPosition = validPositions[randomIndex];
		//Remove the entry at randomIndex from the list so that it can't be re-used.
		validPositions.RemoveAt (randomIndex);

		theTiles[new Vector2(randomPosition.x, randomPosition.y)] = OCCUPIED;

		//Return the randomly selected Vector3 position.
		return randomPosition;
	}
	 //still need to remove wall tiles from list of valid positions
	void BackTracker()
	{
		Cell currentCell = new Cell();
		currentCell.position = new Vector2(columns / 2, rows / 2 );
		visitedCells[currentCell.position] = VISISTED;
		unvisitedCells--;
		thePath.Add (currentCell.position);

		List <Vector2> wallIndexes = new List<Vector2>(validPositions);

		while (unvisitedCells > 0) 
		{
			if (cellHasNeighbours (currentCell)) 
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

		//Remove random walls
		for (int i = 0; i < columns/2; i++)
		{
			int index = Random.Range (0, wallIndexes.Count);

			Vector2 right = new Vector2 (wallIndexes [index].x + 1, wallIndexes [index].y);
			Vector2 left = new Vector2 (wallIndexes [index].x - 1, wallIndexes [index].y);
			Vector2 up = new Vector2 (wallIndexes [index].x, wallIndexes [index].y + 1);
			Vector2 down = new Vector2 (wallIndexes [index].x, wallIndexes [index].y - 1);

			if (!IsPassable (right) &&
			    !IsPassable (left) &&
			    (IsPassable (up) &&
			    IsPassable (down))) 
			{
				wallIndexes.RemoveAt (index);
			} 
			else if (!IsPassable (up) &&
			        !IsPassable (down) &&
			        (IsPassable (left) &&
					IsPassable (right))) 
			{
				wallIndexes.RemoveAt (index);

			}
			else 
			{
				i--;
			}
		//	wallIndexes.RemoveAt (Random.Range (0, wallIndexes.Count));

		}

		for (int i = 0; i < wallIndexes.Count; i++)
		{
			//if (theTiles [wallIndexes [i]] == IMPASSABLE)

			GameObject instance = Instantiate(wallTiles, wallIndexes [i], Quaternion.identity);
			instance.transform.SetParent (level);
			validPositions.Remove (wallIndexes [i]);

		}
	}

	bool cellHasNeighbours(Cell cell)
	{
		if (!cell.foundNeighbours) 
		{
			if (cell.position.x - 2 >= 0) 
			{
				Cell newCell = new Cell ();
				newCell.position = new Vector2 (cell.position.x - 2, cell.position.y);
				cell.neighbours.Add (newCell);
			}
			if (cell.position.x + 2 < columns) 
			{
				Cell newCell = new Cell ();
				newCell.position = new Vector2 (cell.position.x + 2, cell.position.y);
				cell.neighbours.Add (newCell);
			}
			if (cell.position.y - 2 >= 0) 
			{
				Cell newCell = new Cell ();
				newCell.position = new Vector2 (cell.position.x, cell.position.y - 2);
				cell.neighbours.Add (newCell);
			}
			if (cell.position.y + 2 < rows) 
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

	void SetWalls(int walls)
	{
		for (int currentWall = 0; currentWall < walls;)
		{
			//lookingForPosition will be set to false once a valid start position has been found
			bool lookingForPosition = true;
			while(lookingForPosition)
			{
				//From where a wall starts being built. If this is equal to 0 the wall builds along the X axis, else it builds along the Y
				int startAtX =  Random.Range(0,2);
				//Determines the direction the wall builds in. 
				int direction = Random.Range(0,2);

				//The direction the wall builds in
				int movingX = 0;
				int movingY = 0;

				//The starting position of the wall being built and the position of the first tile of the wall
				int startingX = 0; 
				int startingY =  0;

				if(startAtX == 0)
				{

					startingX = Random.Range(2, columns - 2);
					if(direction == 0)
					{
						startingY = 0;
						movingY = 1;

					}
					else
					{
						startingY = rows - 1;
						movingY = -1;
					}
				}
				else
				{
					startingY = Random.Range(2, rows - 2);
					if(direction == 0)
					{
						startingX = 0;
						movingX = 1;
					}
					else
					{
						startingX = rows - 1;
						movingX = -1;
					}
				}
				Vector2 currentPosition = new Vector2(startingX, startingY);


				if(theTiles[currentPosition] == PASSABLE)
				{
					//The positions adjacent to the current(starting) position
					//These are the positions above and below the the current position when the wall is building horizontally and the
					//positions to the left and right if building vertically
					Vector2 leftAbovePosition;
					Vector2 rightBelowPosition;
					if(movingX != 0)
					{
						leftAbovePosition = new Vector2
							(currentPosition.x, currentPosition.y + 1);
						rightBelowPosition = new Vector2
							(currentPosition.x, currentPosition.y - 1);
					}
					else
					{
						leftAbovePosition = new Vector2
							(currentPosition.x - 1, currentPosition.y);
						rightBelowPosition = new Vector2
							(currentPosition.x + 1, currentPosition.y);
					}
					//Make sure adjacent positions are in the list of positions
					if(theTiles[leftAbovePosition] != IMPASSABLE && theTiles[rightBelowPosition] != IMPASSABLE)
					{
						List <Vector2> wallIndexes = new List<Vector2>();
						lookingForPosition = false;	
						//This will be set to true when a position the invalidPositions list is hit
						bool hitInvalid = false;
						//lookingForEnd will be set to false when an invalid position or a position not in the list of positions is found
						bool lookingForEnd = true;
						while(lookingForEnd)
						{
							if(!InBounds(currentPosition) || theTiles[currentPosition] == IMPASSABLE)
							{
								lookingForEnd = false;
							}
							else if(theTiles[currentPosition] == INVALID)
							{
								hitInvalid = true;
								lookingForEnd = false;
							}
							else
							{
								wallIndexes.Add(currentPosition);
								currentPosition.x += movingX;
								currentPosition.y += movingY;

							}
						}
						//If we hit a position not in the list of positions remove a random position from the list of wall positions
						//And set the positions adjacent to it to invalid
						if(!hitInvalid)
						{
							int removedIndex = Random.Range(0, wallIndexes.Count);

							Vector2 removedTile = wallIndexes[removedIndex];
							Vector2 removedTileUpLeft;
							Vector2 removedTileDownRight;
							if(movingX != 0)
							{
								removedTileUpLeft= new Vector2
									(removedTile.x, removedTile.y + 1);
								removedTileDownRight = new Vector2
									(removedTile.x, removedTile.y - 1);
							}
							else
							{
								removedTileUpLeft = new Vector2
									(removedTile.x - 1, removedTile.y);
								removedTileDownRight = new Vector2
									(removedTile.x + 1, removedTile.y);
							}

							theTiles[removedTile] = INVALID;
							theTiles[removedTileUpLeft] = INVALID;
							theTiles[removedTileDownRight] = INVALID;

							wallIndexes.RemoveAt(removedIndex);
						}

						for(int c = 0; c < wallIndexes.Count; c ++)
						{
							Instantiate(wallTiles, wallIndexes[c], Quaternion.identity);
							theTiles[wallIndexes[c]] = IMPASSABLE;
						}
						for(int c = 0; c < wallIndexes.Count; c ++)
						{
							validPositions.Remove (wallIndexes[c]);
						}
						currentWall++;
					}
				}
			}

		}
	}

	public bool InBounds(Vector2 position)
	{
		return position.x > -1 && position.x < columns && position.y > -1 && position.y < rows;
	}
	public bool IsPassable(Vector2 position)
	{
		return InBounds(position) && theTiles[position] != IMPASSABLE;
	}

	// Use this for initialization
	void Start () 
	{
		InitialiseList ();
		BoardSetup ();

	//	SetWalls (10);
		BackTracker();

		//Instantiate (key, RandomPosition (), Quaternion.identity);
		//Instantiate (key, RandomPosition (), Quaternion.identity);

		GameObject instance  = Instantiate (key, new Vector2(0, 20), Quaternion.identity);
		instance.transform.SetParent (level);
		instance = Instantiate (door, new Vector2(20, -1), Quaternion.identity);
		instance.transform.SetParent (level);

		instance = Instantiate (player, new Vector2 (columns / 2, rows / 2), Quaternion.identity);
		instance.transform.SetParent (level);
		Instantiate (tester, Vector2.zero, Quaternion.identity);

		camera.transform.position = new Vector3 (columns / 2, rows / 2, -10.0f);
		camera.GetComponent<Camera> ().orthographicSize = (columns/2) + 1.5f;
	}

	// Update is called once per frame
	void Update () 
	{

	}
}
