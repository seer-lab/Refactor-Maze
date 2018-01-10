﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tester : MonoBehaviour {

	public bool onOff;
	public GameObject level;
	public GameObject code;
	public PlayerController player;

	GameObject door;

	public TextTester codeObject;

	public Camera mainCamera;
	public Vector3 levelCameraPosition;
	private Vector3 codeCameraPosition;

	public Vector2 levelStart;
	public Vector2 levelExit;

	private Vector3 cameraTarget;

	private float transitionTimer; //timer for transitioning back to the level screen after selecting something on the code screen
	private const int transitionTime = 2;

	private float levelTransitionTimer;
	private const float levelTransitionTime = 0.5f;

	private float transitionT = 0;

	private int mazeWidth;
	private int mazeHeight;

	public enum State{PLAYING, LEVEL_TRANSITION};
	public State state;

	// Use this for initialization
	void Start () 
	{
		state = State.PLAYING;
		level = GameObject.Find ("Level");

		mazeWidth = ((LevelCreate)GameObject.Find ("LevelCreator").GetComponent (typeof(LevelCreate))).columns;
		mazeHeight = ((LevelCreate)GameObject.Find ("LevelCreator").GetComponent (typeof(LevelCreate))).rows;

		code = GameObject.Find ("CodeLevel");

		codeObject = (TextTester)GameObject.Find("Code").GetComponent (typeof(TextTester));


		player = (PlayerController)GameObject.FindGameObjectWithTag("Player").GetComponent(typeof(PlayerController));

		mainCamera = Camera.main;
		levelCameraPosition = mainCamera.transform.position;
		codeCameraPosition = new Vector3(levelCameraPosition.x, 0, levelCameraPosition.z);

		transitionTimer = 0.0f;
		levelTransitionTimer = 0.0f;
		code.SetActive (false);
		onOff = true;

		Cull (new Vector2(levelCameraPosition.x - (mazeWidth / 2 + 1), levelCameraPosition.y - (mazeHeight / 2 + 1)),
			new Vector2(levelCameraPosition.x + (mazeWidth / 2 + 1), levelCameraPosition.y + (mazeHeight / 2 + 1)));

	}

	void Cull(Vector2 min, Vector2 max)
	{
		float xMin = min.x;
		float xMax = max.x;
		float yMin = min.y;
		float yMax = max.y;

		foreach(Transform child in level.transform)
		{
			/*float xMin = levelCameraPosition.x - (mazeWidth / 2 + 1);
			float xMax = levelCameraPosition.x + (mazeWidth / 2 + 1);
			float yMin = levelCameraPosition.y - (mazeHeight / 2 + 1);
			float yMax = levelCameraPosition.y + (mazeHeight / 2 + 1);*/

			//	if (child.position.x >= -1 && child.position.x < 22 && child.position.y >= -1 && child.position.y < 22) 
			if (child.position.x >= xMin
				&& child.position.x <= xMax
				&& child.position.y >= yMin
				&& child.position.y <= yMax)	
			{
				child.gameObject.SetActive (true);
			} 
			else 
			{
				child.gameObject.SetActive (false);
			}
		}
	}

	void FixedUpdate()
	{
		switch (state)
		{
		case State.PLAYING:
			break;
		case State.LEVEL_TRANSITION:
		//	player.move ();
			break;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		switch (state)
		{
		case State.PLAYING:
			if (Input.GetKeyDown ("p")) 
			{
				switchMode ();
				codeObject.state = TextTester.State.SELECTING; //temp, just for testing
			}

			if (level.active) 
			{
				if (player.exiting) 
				{
					codeObject.SetUp ("testlevel2");
					player.exiting = false;
					levelStart = new Vector2 (door.transform.position.x + player.direction.x, 
						door.transform.position.y + player.direction.y);
				
					//levelExit = new Vector2(Mathf.Floor(player.transform.position.x),
					//	Mathf.Floor(player.transform.position.y));
					levelExit = player.transform.position;

					cameraTarget = new Vector3 (levelCameraPosition.x + (mazeWidth + 1) * player.direction.x,
						levelCameraPosition.y + (mazeHeight + 1) * player.direction.y,
						levelCameraPosition.z);

					if (cameraTarget.x > levelCameraPosition.x || cameraTarget.y > levelCameraPosition.y) 
					{
						Cull (new Vector2(levelCameraPosition.x - (mazeWidth / 2 + 1), levelCameraPosition.y - (mazeHeight / 2 + 1)),
							new Vector2(cameraTarget.x + (mazeWidth / 2 + 1), cameraTarget.y + (mazeHeight / 2 + 1)));
					}
					else
					{
						Cull (new Vector2(cameraTarget.x - (mazeWidth / 2 + 1), cameraTarget.y - (mazeHeight / 2 + 1)),
							new Vector2(levelCameraPosition.x + (mazeWidth / 2 + 1), levelCameraPosition.y + (mazeHeight / 2 + 1)));
					}

				

					player.enabled = false;
					state = State.LEVEL_TRANSITION;
					transitionT = 0;
				}
				else
				{
					if (Input.GetKeyDown ("q")) 
					{
						if (player.door != null) 
						{ 

							door = player.door;
							switchMode ();
							if (player.hasKey)
							{
								codeObject.state = TextTester.State.SELECTING;
							} 
							else 
							{
								codeObject.state = TextTester.State.LOOKING;

							}
						}
					}
				}

			} 
			else 
			{
				if (codeObject.state != TextTester.State.SELECTING && codeObject.state != TextTester.State.LOOKING) 
				{
					transitionTimer += Time.deltaTime;
					//appropriate messages will be handled by code class
					if (transitionTimer >= transitionTime) 
					{
						transitionTimer = 0.0f;
						if (codeObject.state == TextTester.State.CORRECT) 
						{
							if (door != null) 
							{ 
								door.SetActive (false);

							}
						}
						else if (codeObject.state == TextTester.State.INCORRECT) 
						{
							codeObject.nextBlock ();	
						}
						switchMode ();
					}

				}
			}
			break;
		case State.LEVEL_TRANSITION:

			levelTransitionTimer += Time.deltaTime;
			//appropriate messages will be handled by code class
			if (levelTransitionTimer >= levelTransitionTime) 
			{
				levelTransitionTimer = 0.0f;
				state = State.PLAYING;
				player.enabled = true;
				//Snap to correct position
				player.transform.position = levelStart;

				player.direction = Vector2.zero;
				mainCamera.transform.position = cameraTarget;

				levelCameraPosition = cameraTarget;

				door.SetActive (true);
				door.tag = "Wall";
				Cull (new Vector2(levelCameraPosition.x - (mazeWidth / 2 + 1), levelCameraPosition.y - (mazeHeight / 2 + 1)),
					new Vector2(levelCameraPosition.x + (mazeWidth / 2 + 1), levelCameraPosition.y + (mazeHeight / 2 + 1)));
			} 
			else 
			{
				transitionT += Time.deltaTime/levelTransitionTime;
				mainCamera.transform.position = Vector3.Lerp(levelCameraPosition, cameraTarget, transitionT);
				player.transform.position = Vector3.Lerp (levelExit, levelStart, transitionT);
			}
			break;
		}
	}

	public void switchMode()
	{
		onOff = !onOff;
		level.SetActive (onOff);
		code.SetActive (!onOff);
		if (code.active) 
		{
			mainCamera.transform.position = codeCameraPosition;
		}
		else 
		{
			mainCamera.transform.position = levelCameraPosition;
		}
	}
}
