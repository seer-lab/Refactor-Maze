using System.Collections;
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
	private Vector3 cameraPosition;

	private float transitionTimer; //timer for transitioning back to the level screen after selecting something on the code screen
	private const int transitionTime = 2;

	// Use this for initialization
	void Start () 
	{
		level = GameObject.Find ("Level");
		code = GameObject.Find ("CodeLevel");

		codeObject = (TextTester)GameObject.Find("Code").GetComponent (typeof(TextTester));

		player = (PlayerController)GameObject.FindGameObjectWithTag("Player").GetComponent(typeof(PlayerController));

		mainCamera = Camera.main;
		cameraPosition = mainCamera.transform.position;

		transitionTimer = 0.0f;

		code.SetActive (false);
		onOff = true;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown ("p")) 
		{
			switchMode ();
			codeObject.state = TextTester.State.SELECTING; //temp, just for testing

		}

		if (level.active) 
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
						if (door != null) //temp
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
	}

	public void switchMode()
	{
		onOff = !onOff;
		level.SetActive (onOff);
		code.SetActive (!onOff);
		if (code.active) 
		{
			mainCamera.transform.position = new Vector3 (cameraPosition.x, 0, cameraPosition.z);
		}
		else 
		{
			mainCamera.transform.position = cameraPosition;
		}
	}
}
