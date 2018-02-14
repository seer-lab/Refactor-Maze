using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using System.Xml;

public class tester : MonoBehaviour {

	public bool onOff;
	public GameObject level;
	public GameObject code;
	public GameObject display;
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

	//Starting y value for the display text
	private float startY;

	private int mazeWidth;
	private int mazeHeight;

	private int currentBlock;

	public enum State{PLAYING, LEVEL_TRANSITION};
	public State state;

	private List<int> codeBlocks = new List<int> ();
	private List<string> displayCodeBlocks = new List<string>();
	private List<string> correctCode = new List<string> ();
	private List<XmlNode> smellyCode = new List<XmlNode> ();

	public Sprite borderSprite;

	private Text displayText;
	private Text interactText;

	private Transform boxHolder; // make the visual boxes easier to manage

	private GameObject textBox;

	// Use this for initialization
	void Start () 
	{
		state = State.PLAYING;
		level = GameObject.Find ("Level");

		mazeWidth = ((LevelCreate)GameObject.Find ("LevelCreator").GetComponent (typeof(LevelCreate))).columns;
		mazeHeight = ((LevelCreate)GameObject.Find ("LevelCreator").GetComponent (typeof(LevelCreate))).rows;

		code = GameObject.Find ("CodeLevel");

		//Need to scale the transform down a bit to prevent the text from looking blurry on bigger screens
		code.transform.localScale = new Vector3 (0.5f, 0.5f, 1.0f);

		codeObject = (TextTester)GameObject.Find("Code").GetComponent (typeof(TextTester));

		interactText = (Text)codeObject.GetComponent (typeof(Text));

		player = (PlayerController)GameObject.FindGameObjectWithTag("Player").GetComponent(typeof(PlayerController));

		display = GameObject.Find ("Display");

		mainCamera = Camera.main;
		levelCameraPosition = mainCamera.transform.position;
		codeCameraPosition = new Vector3(levelCameraPosition.x, 0, levelCameraPosition.z);

		transitionTimer = 0.0f;
		levelTransitionTimer = 0.0f;
		code.SetActive (false);
		onOff = true;

		Cull (new Vector2(levelCameraPosition.x - (mazeWidth / 2 + 1), levelCameraPosition.y - (mazeHeight / 2 + 1)),
			new Vector2(levelCameraPosition.x + (mazeWidth / 2 + 1), levelCameraPosition.y + (mazeHeight / 2 + 1)));


		XmlDocument doc = new XmlDocument();
		doc.Load ("Assets/Scripts/test6.xml");

		XmlNode levelnode =  doc.DocumentElement.SelectSingleNode("/code");
		foreach (XmlNode node in levelnode.ChildNodes) 
		{
			foreach (XmlNode blockNode in node.ChildNodes) 
			{
				if (blockNode.Name == "smellcode")
				{
					displayCodeBlocks.Add (blockNode.InnerText);
					smellyCode.Add (blockNode);
				} 
				else if (blockNode.Name == "correctcode") 
				{
					correctCode.Add (blockNode.InnerText);
				}

			}
		}

		for (int i = 0; i < smellyCode.Count; i++) 
		{
			codeBlocks.Add (i);
		}
		shuffleCodeBlocks ();
		startY = display.transform.position.y;
		displayText = GameObject.Find ("DisplayText").GetComponent<Text>();
		displayText.text = "";
		currentBlock = 0;
		updateDisplay ();

		codeObject.getBoxes (smellyCode [codeBlocks[currentBlock]]);

		drawBoxes ();

	}
	private void shuffleCodeBlocks()
	{
		//Fisher-Yates shuffle
		int max = codeBlocks.Count;
		int theMax = max - 1;
		while (max > 0) 
		{
			int randomNumber = Random.Range (0, max);
			int temp = codeBlocks [theMax];
			codeBlocks[theMax] = codeBlocks[randomNumber];
			codeBlocks [randomNumber] = temp;
			max--;
		}		
	}

	private void updateDisplay()
	{
		displayText.text = "";
		display.transform.position = new Vector3 (display.transform.position.x, startY, display.transform.position.z);

		for (int i = 0; i < displayCodeBlocks.Count; i++) 
		{
			if (i == codeBlocks[currentBlock]) 
			{
				TextGenerator generator;

				generator = new TextGenerator (displayText.text.Length);
				Vector2 extents = displayText.gameObject.GetComponent<RectTransform>().rect.size;
				generator.Populate (displayText.text, displayText.GetGenerationSettings (extents));

				//minBound = new Vector2 (uleft.x, bright.y);
				//maxBound = new Vector2 (bright.x, uleft.y);

				int index = 0;
				if (displayText.text.Length > 0) 
				{
					index = displayText.text.Length - 1;
				}

				Vector2 bottomright = new Vector2 (generator.verts [(index) * 4 + 2].position.x, generator.verts [(index) * 4 + 2].position.y);

				float newY = displayText.transform.TransformPoint (bottomright).y;
				//float newY = bright.y;
				if (index > 0) 
				{
					//Need an offset for any block after the first one
					newY = startY + (startY - newY);
				}
				display.transform.position = new Vector3 (display.transform.position.x,
					newY, display.transform.position.z);

				displayText.text += "<b>";
				displayText.text += displayCodeBlocks [i];
				displayText.text += "</b>";
			}
			else 
			{
				displayText.text += displayCodeBlocks [i];
			}
		}
	}

	void drawBoxes()
	{
		Vector2 minBound = Vector2.zero;
		Vector2 maxBound = Vector2.zero;

		getTextBounds (ref minBound, ref maxBound, ref displayText);

		Vector2 bRight = new Vector2 (maxBound.x + 1, minBound.y - 1);
		Vector2 uLeft = new Vector2 (minBound.x - 1, maxBound.y + 1);

		Vector2 size = bRight - uLeft;
		Rect newRectangle = new Rect (uLeft, size);

		GameObject.Destroy (textBox);

		textBox = new GameObject("textBox");
		textBox.tag = "Exit";
		SpriteRenderer renderer = textBox.AddComponent <SpriteRenderer>();
		renderer.sprite = borderSprite;
		renderer.drawMode = SpriteDrawMode.Sliced;

		renderer.size = new Vector2 (newRectangle.width, newRectangle.height);;

		textBox.transform.position = new Vector2(uLeft.x + newRectangle.width * 0.5f, uLeft.y + newRectangle.height * 0.5f); //center the box

		textBox.transform.SetParent (displayText.transform);
	}

	private void getTextBounds(ref Vector2 minBound, ref Vector2 maxBound, ref Text textComponent )
	{
		string text = textComponent.text;

		TextGenerator generator;

		generator = new TextGenerator (textComponent.text.Length);
		Vector2 extents = textComponent.gameObject.GetComponent<RectTransform>().rect.size;
		generator.Populate (textComponent.text, textComponent.GetGenerationSettings (extents));

		//TODO need to take into account empty characters
		//Calculate bounds of the first character
		int startI = 0;
		Vector2 upperLeft = new Vector2 (generator.verts [startI * 4].position.x, generator.verts [startI * 4].position.y);
		Vector2 bottomright = new Vector2 (generator.verts [startI * 4 + 2].position.x, generator.verts [startI * 4 + 2].position.y);

		Vector3 uleft = textComponent.transform.TransformPoint (upperLeft);
		Vector3 bright = textComponent.transform.TransformPoint (bottomright);

		minBound = new Vector2 (uleft.x, bright.y);
		maxBound = new Vector2 (bright.x, uleft.y);

		//Loop through the rest of the characters to find the minimum and maximum bounds of the block
		//This could probably be optimized a bit(only check min and max y on the first and last line, check x max on new line)
		for (int i = startI; i < text.Length; i++) 
		{
			upperLeft = new Vector2 (generator.verts [i * 4].position.x, generator.verts [i * 4].position.y);
			bottomright = new Vector2 (generator.verts [i * 4 + 2].position.x, generator.verts [i * 4 + 2].position.y);

			uleft = textComponent.transform.TransformPoint (upperLeft);
			bright = textComponent.transform.TransformPoint (bottomright);

			if (uleft.y > maxBound.y) 
			{
				maxBound.y = uleft.y;
			}

			if (uleft.x < minBound.x) 
			{
				minBound.x = uleft.x;
			}

			if (bright.y < minBound.y) 
			{
				minBound.y = bright.y;
			}

			if (bright.x > maxBound.x) 
			{
				maxBound.x = bright.x;
			}	
		}
	}

	void Cull(Vector2 min, Vector2 max)
	{
		float xMin = min.x;
		float xMax = max.x;
		float yMin = min.y;
		float yMax = max.y;

		foreach(Transform child in level.transform)
		{
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

	private void removeBlock()
	{
		displayCodeBlocks [codeBlocks[currentBlock]] = correctCode [codeBlocks[currentBlock]];
		codeBlocks.RemoveAt (currentBlock);

		currentBlock = 0;
		shuffleCodeBlocks ();

		codeObject.getBoxes (smellyCode[codeBlocks[currentBlock]]);
		updateDisplay ();
	}
		
	public void nextBlock()
	{
		currentBlock++;
		if (currentBlock >= codeBlocks.Count)
		{
			currentBlock = 0;
			shuffleCodeBlocks ();
		}
		codeObject.getBoxes (smellyCode[codeBlocks[currentBlock]]);
		updateDisplay ();
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
					player.enabled = false;

					player.exiting = false;
					levelStart = new Vector2 (door.transform.position.x + player.direction.x, 
						door.transform.position.y + player.direction.y);
				
					levelExit = player.transform.position;

					cameraTarget = new Vector3 (levelCameraPosition.x + (mazeWidth + 1) * player.direction.x,
						levelCameraPosition.y + (mazeHeight + 1) * player.direction.y,
						levelCameraPosition.z);

					door.tag = "Wall";


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
					 //hopefully temporary, 
					door.SetActive (false);

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
					Vector3 displayPosition = display.transform.position;
					if (Input.GetKey ("w"))
					{
						display.transform.position = new Vector3 (displayPosition.x, displayPosition.y + 1, displayPosition.z);
					} 
					else if (Input.GetKey ("s"))
					{
						display.transform.position = new Vector3 (displayPosition.x, displayPosition.y - 1, displayPosition.z);
					} 
				}

			} 
			else 
			{
				if (codeObject.state == TextTester.State.CORRECT) 
				{
					interactText.text = correctCode [codeBlocks[currentBlock]]; //move this somewhere else I don't want it going every frame

					transitionTimer += Time.deltaTime;
					//appropriate messages will be handled by code class
					if (transitionTimer >= transitionTime) 
					{
						transitionTimer = 0.0f;

						if (door != null) 
						{ 
							door.SetActive (false);
						}


						switchMode ();
						removeBlock ();
//						nextBlock ();
						drawBoxes ();

					}

				}
				else if(codeObject.state == TextTester.State.INCORRECT)
				{
					if (Input.anyKeyDown)
					{

						switchMode ();
						nextBlock();

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
				//Snap to correct position
				player.transform.position = levelStart;

				player.direction = Vector2.zero;
				mainCamera.transform.position = cameraTarget;

				levelCameraPosition = cameraTarget;

			//	door.SetActive (true);

				Cull (new Vector2(levelCameraPosition.x - (mazeWidth / 2 + 1), levelCameraPosition.y - (mazeHeight / 2 + 1)),
					new Vector2(levelCameraPosition.x + (mazeWidth / 2 + 1), levelCameraPosition.y + (mazeHeight / 2 + 1)));
				player.enabled = true;

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
		display.SetActive (onOff);
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
