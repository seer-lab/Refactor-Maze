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
	private float codeCameraHeight = 60;

	private int mazeWidth;
	private int mazeHeight;

	private int currentBlock;

	public int currentLevel = 0;
	private int numberOfLevels = 0;

	public enum State{PLAYING, LEVEL_TRANSITION, INSTRUCTIONS, ERROR};
	public State state;

	private List<int> codeBlocks = new List<int> ();
	private List<string> displayCodeBlocks = new List<string>();
	private List<GameObject> displayInstances = new List<GameObject> ();
	private List<string> correctCode = new List<string> ();
	private List<XmlNode> smellyCode = new List<XmlNode> ();

	public Sprite borderSprite;

	public Text legendText;

	private Transform boxHolder; // make the visual boxes easier to manage

	private GameObject textBox;

	private GameObject levelDisplay;

	public List<int> keyTypes = new List<int>();

	public List<int> levelKeys = new List<int> (); //temporarly public, going to move a lof ot this over to level create.

	private int currentKey;
	private int correctRefactor;

	private int firstLevel = -1;

    public string errorString = "";
    public Text errorText;

	// Use this for initialization
	void Start () 
	{
        string toLoad = "examples/example.xml";
        GameObjectSetUp();

        if (LoadXML(toLoad))
        {
            state = State.PLAYING;


            CameraStart();

            transitionTimer = 0.0f;
            levelTransitionTimer = 0.0f;
            code.SetActive(false);
            onOff = true;
            showInstructions = false;

            Cull(new Vector2(levelCameraPosition.x - (mazeWidth / 2 + 1), levelCameraPosition.y - (mazeHeight / 2 + 1)),
                new Vector2(levelCameraPosition.x + (mazeWidth / 2 + 1), levelCameraPosition.y + (mazeHeight / 2 + 1)));
   
            if (firstLevel >= 0)
            {
                currentBlock = firstLevel;
            }
            else
            {
                shuffleCodeBlocks();
                currentBlock = 0;
            }
            startY = display.transform.position.y;
            updateDisplay();

            //level create function here

            SetLevel();

            drawBoxes();
        }
        else
        {
            state = State.ERROR;
            errorText.transform.parent.gameObject.SetActive (true);
            currentGameObject.SetActive (false);
            display.SetActive (false);          
            
            errorText.text = errorString;

        }
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
		code.transform.position = new Vector3 (code.transform.position.x, 60, code.transform.position.z);

		codeObject = (TextTester)GameObject.Find("Code").GetComponent (typeof(TextTester));

		player = (PlayerController)GameObject.FindGameObjectWithTag("Player").GetComponent(typeof(PlayerController));

		display = GameObject.Find ("GreaterDisplay");

		levelDisplay = GameObject.Find ("LevelDisplay");

		levelDisplay.GetComponentInChildren<Text> ().text = "Level: 1";

		legendText = GameObject.Find ("Legend").GetComponentInChildren<Text>();

        errorText = GameObject.Find("ErrorScreen").GetComponentInChildren<Text>();
        errorText.transform.parent.gameObject.SetActive(false);
	}

	//Camera information
	void CameraStart()
	{
		mainCamera = Camera.main;
		levelCameraPosition = mainCamera.transform.position;
		mazeCameraSize = mainCamera.orthographicSize;
		codeCameraHeight = code.transform.position.y - 10;
		codeCameraPosition = new Vector3(levelCameraPosition.x, codeCameraHeight, levelCameraPosition.z);
	}

    bool LoadXML(string toLoad)
	{
        if (System.IO.File.Exists(toLoad))
        {
            keyTypes.Capacity = 3;

            for (int i = 0; i < 3; i++)
            {
                keyTypes.Add(i);
            }
			
            XmlDocument doc = new XmlDocument();
            doc.Load(toLoad);

            XmlNode levelnode = doc.DocumentElement.SelectSingleNode("/code");
            int refactorBlocks = 0;

            foreach (XmlNode node in levelnode.ChildNodes)
            {
                if (node.Name == "firstLevel")
                {
                    int first;
                    if (int.TryParse(node.InnerText, out first))
                    {
                        firstLevel = first;
                    }
                    else
                    {
                        print("Problem loading first level, randomizing levels");
                    }
                }
                else
                {
                    GameObject instance = Instantiate(displayInstance, display.transform);

                    displayInstances.Add(instance);
                    bool smell = false;
                    foreach (XmlNode blockNode in node.ChildNodes)
                    {
                        if (blockNode.Name == "smellcode")
                        {
                            displayCodeBlocks.Add(blockNode.InnerText);
                            smellyCode.Add(blockNode);
                            smell = true;
                            codeBlocks.Add(refactorBlocks);

                            numberOfLevels++;
                        }
                        else if (blockNode.Name == "correctcode")
                        {
                            correctCode.Add(blockNode.InnerText);
                            if (!smell)
                            {
                                smellyCode.Add(blockNode); //keeps the two lists parallel. Not crazy about this solution
                                displayCodeBlocks.Add(blockNode.InnerText);
                            }
                        }
                    }
                    refactorBlocks++;
                }
            }
            return true;
        }
        else
        {
            errorString = "Cannot find file " + toLoad;
            errorString += "\nPress the Escape key to exit";
            return false;        
        }
	}

	void SetLevel ()
	{		
		codeObject.getBoxes (smellyCode[codeBlocks[currentBlock]]);

		SetKeys (true);
	}

	void SetKeys(bool reset)
	{
	
		SetKeyTypes ();

		((LevelCreate)GameObject.Find ("LevelCreator").GetComponent (typeof(LevelCreate))).NewKeyPosition (currentLevel, levelKeys, reset);

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

		Vector2 bRight = new Vector2 (maxBound.x + 3, minBound.y - 1);
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
		if (firstLevel < 0 || (firstLevel >= 0 && currentLevel > 0)) 
		{
			currentBlock++;
			if (currentBlock >= codeBlocks.Count) 
			{
				currentBlock = 0;
				shuffleCodeBlocks ();
			}
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
                if (Input.GetKeyDown(KeyCode.Escape))
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
                        levelStart = new Vector2(door.transform.position.x + player.direction.x, 
                            door.transform.position.y + player.direction.y);
				
                        levelExit = player.transform.position;

                        cameraTarget = new Vector3(levelCameraPosition.x + (mazeWidth + 1) * player.direction.x,
                            levelCameraPosition.y + (mazeHeight + 1) * player.direction.y,
                            levelCameraPosition.z);

                        door.tag = "Wall";
                        door.GetComponent<SpriteRenderer>().color = Color.black;

                        if (cameraTarget.x > levelCameraPosition.x || cameraTarget.y > levelCameraPosition.y)
                        {
                            Cull(new Vector2(levelCameraPosition.x - (mazeWidth / 2 + 1), levelCameraPosition.y - (mazeHeight / 2 + 1)),
                                new Vector2(cameraTarget.x + (mazeWidth / 2 + 1), cameraTarget.y + (mazeHeight / 2 + 1)));
                        }
                        else
                        {
                            Cull(new Vector2(cameraTarget.x - (mazeWidth / 2 + 1), cameraTarget.y - (mazeHeight / 2 + 1)),
                                new Vector2(levelCameraPosition.x + (mazeWidth / 2 + 1), levelCameraPosition.y + (mazeHeight / 2 + 1)));
                        }
                        //hopefully temporary, 
                        door.SetActive(false);
                        display.SetActive(false);

                        state = State.LEVEL_TRANSITION;
                        transitionT = 0;
                    }
                    else
                    {
                        if (!player.keyLook)
                        {
                            if (player.keyList.Count > 0)
                            {
                                legendText.text = "Refactor: " + GetKeyName();
                                //maybe rethink this.
                                if (Input.GetKeyDown(KeyCode.Q))
                                {
                                    if (player.door != null)
                                    { 
                                        door = player.door;
                                        switchMode();
                                        currentKey = 0;
										
                                        codeObject.state = TextTester.State.SELECTING;
                                    } 
                                }
                            }
                            Vector3 displayPosition = display.transform.position;
                            if (Input.GetKey("w"))
                            {
                                display.transform.position = new Vector3(displayPosition.x, displayPosition.y + 1, displayPosition.z);
                            }
                            else if (Input.GetKey("s"))
                            {
                                display.transform.position = new Vector3(displayPosition.x, displayPosition.y - 1, displayPosition.z);
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

                            if (Input.GetKey(KeyCode.Q))
                            {
                                transitionTimer = 0.0f;

                                if (door != null)
                                { 
                                    door.SetActive(false);
                                }
                                legendText.transform.root.gameObject.SetActive(false);
                                switchMode();
                                removeBlock();
                                drawBoxes();
                            }

                            goto case TextTester.State.LOOKING;

                        case TextTester.State.INCORRECT:

                            if (Input.anyKeyDown)
                            {
                                switchMode();
                                legendText.text = "Refactor: ";
                                nextBlock();
                            }
                            break;

                        case TextTester.State.SELECTING:
                            if (Input.GetMouseButtonDown(0))
                            {
                                codeObject.CheckRefactor(player.keyList[currentKey].type, correctRefactor);
                            }
                            else if (Input.GetKeyDown(KeyCode.Q))
                            {
                                switchMode();
                            }				
                            goto case TextTester.State.LOOKING;

                        case TextTester.State.LOOKING:
                            codeObject.moveCamera();
                            break;
                    }
                }
                break;
            case State.LEVEL_TRANSITION:

                levelTransitionTimer += Time.deltaTime;
			//appropriate messages will be handled by code class
                if (levelTransitionTimer >= levelTransitionTime)
                {

                    currentLevel++;
                    if (currentLevel >= numberOfLevels)
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
                    }
                    else
                    {
                        nextLevel();
                    }
                }
                else
                {
                    transitionT += Time.deltaTime / levelTransitionTime;
                    mainCamera.transform.position = Vector3.Lerp(levelCameraPosition, cameraTarget, transitionT);
                    player.transform.position = Vector3.Lerp(levelExit, levelStart, transitionT);
                }
                break;
            case State.INSTRUCTIONS:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    switchInstructions();
                    state = State.PLAYING;
                }
                break;
            case State.ERROR:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                    #else
                    Application.Quit();
                    #endif
                }
                break;
        }
	}

	void nextLevel()
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

		SetKeys (false);

		display.SetActive (true);
		legendText.transform.root.gameObject.SetActive (true);

		levelDisplay.GetComponentInChildren<Text> ().text = "Level: "+ (currentLevel + 1);
		legendText.text = "Refactor: ";
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
		levelDisplay.SetActive (onOff);
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
