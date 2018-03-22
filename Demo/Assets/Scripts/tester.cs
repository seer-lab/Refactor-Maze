using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using System.Xml;



public class tester : MonoBehaviour {

	private bool onOff;
	private bool showInstructions;
	public GameObject level;
	public GameObject code;
	public GameObject displayInstance;
	public GameObject display;
	public GameObject instructions;

	private GameObject currentGameObject;

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
	//Current y value for the display text
	private float currentY;

	private float mazeCameraSize;
	//Found these numbers through experimentation
	private float codeCameraSize = 15;
	private float codeCameraHeight = -14;

	private int mazeWidth;
	private int mazeHeight;

	private int currentBlock;

	public int currentLevel = 0;
	private int numberOfLevels = 0;

	public enum State{PLAYING, LEVEL_TRANSITION, INSTRUCTIONS};
	public State state;

	private List<int> codeBlocks = new List<int> ();
	private List<string> displayCodeBlocks = new List<string>();
	private List<GameObject> displayInstances = new List<GameObject> ();
	private List<string> correctCode = new List<string> ();
	private List<XmlNode> smellyCode = new List<XmlNode> ();

	public Sprite borderSprite;

//	private Text displayText;
	private Text interactText;

	public Text legendText;

	private Transform boxHolder; // make the visual boxes easier to manage

	private GameObject textBox;


	public List<int> keyTypes = new List<int>();

	public List<int> levelKeys = new List<int> (); //temporarly public, going to move a lof ot this over to level create.

	private int currentKey;
	private int correctRefactor;

	//private Vector2 minBound = Vector2.zero;
	//private Vector2 maxBound = Vector2.zero;

	// Use this for initialization
	void Start () 
	{
		state = State.PLAYING;

		GameObjectSetUp ();

		CameraStart ();

		transitionTimer = 0.0f;
		levelTransitionTimer = 0.0f;
		code.SetActive (false);
		onOff = true;
		showInstructions = false;

		Cull (new Vector2(levelCameraPosition.x - (mazeWidth / 2 + 1), levelCameraPosition.y - (mazeHeight / 2 + 1)),
			new Vector2(levelCameraPosition.x + (mazeWidth / 2 + 1), levelCameraPosition.y + (mazeHeight / 2 + 1)));

		//SetInstructionText ();

		LoadXML ();

		shuffleCodeBlocks ();
		startY = display.transform.position.y;
		currentBlock = 0;
		updateDisplay ();

		//level create function here

		SetLevel ();

		drawBoxes ();
	}

	void GameObjectSetUp()
	{
		level = GameObject.Find ("Level");
		currentGameObject = level;

		instructions = GameObject.Find ("Instructions");
		instructions.SetActive (false); //find doesn't work if the object is not active, so find then deactivate it.

		LevelCreate leveCreate = ((LevelCreate)GameObject.Find ("LevelCreator").GetComponent (typeof(LevelCreate)));

		mazeWidth = leveCreate.columns;
		mazeHeight = leveCreate.rows;

		code = GameObject.Find ("CodeLevel");

		//Need to scale the transform down a bit to prevent the text from looking blurry on bigger screens
		code.transform.localScale = new Vector3 (0.5f, 0.5f, 1.0f);

		codeObject = (TextTester)GameObject.Find("Code").GetComponent (typeof(TextTester));

		interactText = (Text)codeObject.GetComponent (typeof(Text));

		player = (PlayerController)GameObject.FindGameObjectWithTag("Player").GetComponent(typeof(PlayerController));

		display = GameObject.Find ("GreaterDisplay");

		GameObject whatever = GameObject.Find ("Legend");

		legendText = whatever.GetComponentInChildren<Text>();
		whatever.gameObject.transform.SetParent (code.transform);
	}

	//Camera information
	void CameraStart()
	{
		mainCamera = Camera.main;
		levelCameraPosition = mainCamera.transform.position;
		mazeCameraSize = mainCamera.orthographicSize;
		codeCameraPosition = new Vector3(levelCameraPosition.x, codeCameraHeight, levelCameraPosition.z);
	}

	void LoadXML()
	{

		//assuming that correct key stuff will be in here somewhere

		//This is going to be every type of refactor technique, will all be listed somewhere in the xml file
		//At the top of a smell block(maybe?) a the correct technique will be listed.
		keyTypes.Capacity = 4;

		for (int i = 0; i < 4; i++)
		{
			keyTypes.Add (i);
		}
			
		XmlDocument doc = new XmlDocument();
		doc.Load ("Assets/Scripts/test7.xml");

		XmlNode levelnode =  doc.DocumentElement.SelectSingleNode("/code");
		int refactorBlocks = 0;

		foreach (XmlNode node in levelnode.ChildNodes) 
		{
			GameObject instance = Instantiate (displayInstance, display.transform);

			displayInstances.Add (instance);
			bool smell = false;
			foreach (XmlNode blockNode in node.ChildNodes) 
			{
				if (blockNode.Name == "smellcode")
				{
					displayCodeBlocks.Add (blockNode.InnerText);
					smellyCode.Add (blockNode);
					smell = true;
					codeBlocks.Add (refactorBlocks);

					numberOfLevels++;
				} 
				else if (blockNode.Name == "correctcode") 
				{
					correctCode.Add (blockNode.InnerText);
					if (!smell) 
					{
						smellyCode.Add (blockNode); //keeps the two lists parallel. Not crazy about this solution
						displayCodeBlocks.Add (blockNode.InnerText);
					}
				}
			}
			refactorBlocks++;
		}
	}

	void SetLevel ()
	{		
		codeObject.getBoxes (smellyCode[codeBlocks[currentBlock]]);

		SetKeys ();
	}

	void SetKeys()
	{
	
		SetKeyTypes ();

		((LevelCreate)GameObject.Find ("LevelCreator").GetComponent (typeof(LevelCreate))).NewKeyPosition (currentLevel, levelKeys);

		player.keyList.Clear ();
	}

	void SetKeyTypes()
	{

		levelKeys.Clear ();
		correctRefactor = TextFunctions.getAttributeInt (smellyCode[codeBlocks[currentBlock]], "refactorType");

		int numberOfKeys = 3; //vary with level

		List<int> randomKeys = new List<int> (keyTypes);

		levelKeys.Add (correctRefactor);

		randomKeys.Remove (correctRefactor);

		for (int i = 0; i < numberOfKeys - 1; i++) 
		{
			int index = Random.Range (0, randomKeys.Count);
			levelKeys.Add(randomKeys[index]);
			randomKeys.RemoveAt (index);
		}
	}

	private void SetInstructionText()
	{
		GameObject dI = Instantiate (displayInstance, mainCamera.transform);
		Text displayText = dI.GetComponentInChildren<Text> ();
		displayText.text = "";
		displayText.text += "<color=#000000ff>";
		displayText.text += "Move with the arrow keys\nOpen doors with the q key";
		displayText.text += "</color>";

		//Normally this is done in the parent display object, but as this is not a child of that object, it's done here
		dI.transform.localScale = new Vector3 (0.5f, 0.5f, 1.0f);
		//displayText.fontSize *= 2;

		Vector2 min = Vector2.zero;		
		Vector2 max = Vector2.zero;

		TextFunctions.getTextBounds (ref min, ref max, displayText);

		TextFunctions.drawBox (min, max, 1, dI.transform, borderSprite);

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
		display.transform.position = new Vector3 (display.transform.position.x, startY, display.transform.position.z);
		float newY = displayInstances [0].transform.position.y;
		float newY2 = startY;
		for (int i = 0; i < displayInstances.Count; i++) 
		{
			Text displayText = displayInstances[i].GetComponentInChildren<Text> ();
			displayText.text = "";
			//TODO set all text to black after completing last block.
			if (codeBlocks.Count > 0 && i == codeBlocks[currentBlock]) 
			{
				TextGenerator generator;

				generator = new TextGenerator (displayText.text.Length);
				Vector2 extents = displayText.gameObject.GetComponent<RectTransform>().rect.size;
				generator.Populate (displayText.text, displayText.GetGenerationSettings (extents));

				if (i > 0) 
				{
					newY2 = startY + (startY - newY);
				}
					
				displayText.text += "<color=#000000ff>";
				displayText.text += displayCodeBlocks [i];
				displayText.text += "</color>";
			}
			else 
			{
				displayText.text += displayCodeBlocks [i];
			}
			displayInstances [i].transform.position = new Vector3 (displayInstances [i].transform.position.x, 
				newY, displayInstances [i].transform.position.z );
			TextGenerator generator2;

			generator2 = new TextGenerator (displayText.text.Length);
			Vector2 extents2 = displayText.gameObject.GetComponent<RectTransform>().rect.size;
			generator2.Populate (displayText.text, displayText.GetGenerationSettings (extents2));

			int index2 = displayText.text.Length - 1;

			Vector2 bottomright2 = new Vector2 (generator2.verts [(index2) * 4 + 2].position.x, generator2.verts [(index2) * 4 + 2].position.y);
			Vector2 thePoint = displayText.transform.TransformPoint (bottomright2);
			newY =thePoint.y - 2;	
		}
		display.transform.position = new Vector3 (display.transform.position.x,
			newY2, display.transform.position.z);

		currentY = newY2;

	}

	void drawBoxes()
	{
		Vector2 minBound = Vector2.zero;
		Vector2 maxBound = Vector2.zero;

		getTextBounds (ref minBound, ref maxBound);

		//minBound = new Vector2 (0, -60);
		//maxBound = new Vector2 (40, 0);

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

		textBox.transform.SetParent (display.transform);
	}

	private void getTextBounds(ref Vector2 minBound, ref Vector2 maxBound )
	{
		minBound.x = 100;
		for (int c = 0; c < displayInstances.Count; c++) 
		{
			Text textComponent = displayInstances [c].GetComponentInChildren<Text> ();

			string text = textComponent.text;

			TextGenerator generator;

			generator = new TextGenerator (textComponent.text.Length);
			Vector2 extents = textComponent.gameObject.GetComponent<RectTransform> ().rect.size;
			generator.Populate (textComponent.text, textComponent.GetGenerationSettings (extents));

			//Calculate bounds of the first character
			int startI = 0;
			Vector2 upperLeft = new Vector2 (generator.verts [startI * 4].position.x, generator.verts [startI * 4].position.y);
			Vector2 bottomright = new Vector2 (generator.verts [startI * 4 + 2].position.x, generator.verts [startI * 4 + 2].position.y);

			Vector3 uleft = textComponent.transform.TransformPoint (upperLeft);
			Vector3 bright = textComponent.transform.TransformPoint (bottomright);

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
	}

	//currently turns keys back on when culling, don't want that.
	void Cull(Vector2 min, Vector2 max)
	{
		float xMin = min.x + level.transform.position.x;
		float xMax = max.x + level.transform.position.x;
		float yMin = min.y;
		float yMax = max.y;

		foreach(Transform child in level.transform)
		{
			if (child.position.x >= xMin
				&& child.position.x <= xMax
				&& child.position.y >= yMin
				&& child.position.y <= yMax)	
			{
				//ugh
				if (child.tag != "Smell") 
				{
					child.gameObject.SetActive (true);
				}
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
		if (codeBlocks.Count > 0) 
		{
			codeObject.getBoxes (smellyCode[codeBlocks[currentBlock]]);
		}

		//clear keys
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
		SetLevel ();
		updateDisplay ();
	}

	string GetKeyName()
	{
		return player.keyList [currentKey].refactorName;
	}

	// Update is called once per frame
	void Update () 
	{
		switch (state)
		{
		case State.PLAYING:
			if (Input.GetKeyDown (KeyCode.Escape)) 
			{
				switchInstructions();
				state = State.INSTRUCTIONS;
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
					display.SetActive (false);

					state = State.LEVEL_TRANSITION;
					transitionT = 0;
				}
				else
				{
					if(!player.keyLook)
					{
						//maybe rethink this.
						if (Input.GetKeyDown(KeyCode.Q)) 
						{
							if (player.door != null) 
							{ 
								if (player.keyList.Count > 0)
								{
									door = player.door;
									switchMode ();
									currentKey = 0;
									legendText.text = "Refactor: " + GetKeyName ();
										
									codeObject.state = TextTester.State.SELECTING;
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

			} 
			else 
			{
				switch (codeObject.state) 
				{
				case TextTester.State.CORRECT:

					transitionTimer += Time.deltaTime;

					if (Input.GetKey("e"))
					{
						transitionTimer = 0.0f;

						if (door != null) 
						{ 
							door.SetActive (false);
						}

						switchMode ();
						removeBlock ();
						drawBoxes ();
					}

					goto case TextTester.State.LOOKING;

				case TextTester.State.INCORRECT:

					if (Input.anyKeyDown)
					{
						switchMode ();
						nextBlock();
					}
					break;

				case TextTester.State.SELECTING:
					if (Input.GetMouseButtonDown (0))
					{
						codeObject.CheckRefactor (player.keyList [currentKey].type, correctRefactor);
					}

					else if (Input.GetKeyDown(KeyCode.Q)) 
					{
						switchMode ();
					}

					//TODO delete this
					if (Input.GetKeyDown ("up"))
					{
						currentKey++;
						if (currentKey >= player.keyList.Count) 
						{
							currentKey = 0;
						}
						legendText.text = "Refactor: " + GetKeyName ();
					} 
					else if (Input.GetKeyDown ("down")) 
					{
						currentKey--;
						if (currentKey < 0) 
						{
							currentKey = player.keyList.Count - 1;
						}
						legendText.text = "Refactor: " + GetKeyName ();
					} 					
					goto case TextTester.State.LOOKING;

				case TextTester.State.LOOKING:
					codeObject.moveCamera ();
					break;
				}
			}
			break;
		case State.LEVEL_TRANSITION:

			levelTransitionTimer += Time.deltaTime;
			//appropriate messages will be handled by code class
			if (levelTransitionTimer >= levelTransitionTime) {

				currentLevel++;
				if (currentLevel >= numberOfLevels) 
				{
					UnityEngine.SceneManagement.SceneManager.LoadScene (2);
				}
				else
				{

					levelTransitionTimer = 0.0f;
					state = State.PLAYING;
					//Snap to correct position
					player.transform.position = levelStart;

					player.direction = Vector2.zero;
					mainCamera.transform.position = cameraTarget;

					levelCameraPosition = cameraTarget;

					//	door.SetActive (true);

					Cull (new Vector2 (levelCameraPosition.x - (mazeWidth / 2 + 1), levelCameraPosition.y - (mazeHeight / 2 + 1)),
						new Vector2 (levelCameraPosition.x + (mazeWidth / 2 + 1), levelCameraPosition.y + (mazeHeight / 2 + 1)));
					player.enabled = true;

					SetKeys ();

					display.SetActive (true);
				}
			} 
			else 
			{
				transitionT += Time.deltaTime/levelTransitionTime;
				mainCamera.transform.position = Vector3.Lerp(levelCameraPosition, cameraTarget, transitionT);
				player.transform.position = Vector3.Lerp (levelExit, levelStart, transitionT);
			}
			break;
		case State.INSTRUCTIONS:
			if (Input.GetKeyDown (KeyCode.Escape)) 
			{
				switchInstructions ();
				state = State.PLAYING;
			}
			break;
		}
	}

	private void switchInstructions()
	{
		showInstructions = !showInstructions;
		instructions.SetActive (showInstructions);
		currentGameObject.SetActive (!showInstructions);
		if (currentGameObject == level) 
		{
			display.SetActive (!showInstructions);			
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
			currentGameObject = code;
			mainCamera.transform.position = codeCameraPosition;
			mainCamera.orthographicSize = codeCameraSize;
		}
		else 
		{
			currentGameObject = level;
			mainCamera.transform.position = levelCameraPosition;
			mainCamera.orthographicSize = mazeCameraSize;
			display.transform.position = new Vector3 (display.transform.position.x,
				currentY, display.transform.position.z);

		}
	}
}
