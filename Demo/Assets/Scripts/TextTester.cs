using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//using UnityEditor;
using System.Xml;

public class TextTester : MonoBehaviour 
{

	public Text textComponent; 
	public Camera mainCamera;

	public Sprite borderSprite;
	public Sprite highlightSprite;

	public List<string> ClickableWords = new List<string>(); 

	public enum State{LOOKING, SELECTING, CORRECT, INCORRECT};
	public State state;

	//For the top and bottom of a code fragment
	//wip
	float top = 0;
	float bottom = -50;

	public Dictionary<int, string> replacedWords = new Dictionary<int, string> ();
	public Dictionary<int, List<Rect>> rectangles = new Dictionary<int, List<Rect>>(); //wip use a list of dictionaries?

	private Transform boxHolder; // make the visual boxes easier to manage

	private XmlNode currentNode;

	private void setUpBoxes()
	{
		textComponent.text = "";
		rectangles.Clear ();
		destroyBoxes ();
	}

	private void destroyBoxes()
	{
		foreach (Transform child in boxHolder)
		{
			GameObject.Destroy(child.gameObject);
		}
	}

	private void addRectangles(int type, Vector2 minBound, Vector2 maxBound)
	{
		if (!rectangles.ContainsKey (type))
		{
			rectangles.Add (type, new List<Rect>());
		}

		float offset = 0.2f;

		Vector2 uLeft = new Vector2(minBound.x - offset, maxBound.y + offset);
		Vector2 bBight = new Vector2(maxBound.x + offset, minBound.y - offset);

		Vector2 size = bBight - uLeft;
		Rect newRectangle = new Rect (uLeft, size);

		rectangles [type].Add (newRectangle);
	}

	//Returns the minimum and maximum bounds of a text component's text starting at an xml node
	private void getTextBounds(XmlNode node, ref Vector2 minBound, ref Vector2 maxBound)
	{
		string text = textComponent.text;

		TextGenerator generator;

		generator = new TextGenerator (textComponent.text.Length);
		Vector2 extents = textComponent.gameObject.GetComponent<RectTransform>().rect.size;
		generator.Populate (textComponent.text, textComponent.GetGenerationSettings (extents));

		//TODO need to take into account empty characters
		//Calculate bounds of the first character
		int startI = text.Length - node.InnerText.Length;
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

	//Returns the minimum and maximum bounds of a text component's text
	private void getTextBounds(ref Vector2 minBound, ref Vector2 maxBound)
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

	private void getCodeBounds(XmlNode node)
	{

		Vector2 minBound = Vector2.zero;
		Vector2 maxBound = Vector2.zero;

		getTextBounds (node, ref minBound, ref maxBound);
			
		addRectangles (TextFunctions.getAttributeInt (node, "id"), minBound, maxBound);
	}

	public void getBoxes(XmlNode levelnode)
	{
		setUpBoxes ();
		currentNode = levelnode;
		foreach (XmlNode node in levelnode.ChildNodes) 
		{
			textComponent.text += node.InnerText;
			//print (node.InnerText);
			if (node.Name == "smell")
			{
				getCodeBounds (node);
			}
			else if (node.Name == "newline")
			{
				textComponent.text += "\n";
			}
		}

		Vector2 minBound = Vector2.zero;
		Vector2 maxBound = Vector2.zero;

		TextFunctions.getTextBounds (ref minBound, ref maxBound, textComponent);

		//Draws the border around the text
		TextFunctions.drawBox(minBound, maxBound, 1, boxHolder, borderSprite);

		//Draws the border around clickable text
		drawBoxes ();
		//using this for lower bounds it's gross
		TextGenerator g;

		g = new TextGenerator (textComponent.text.Length);
		Vector2 e = textComponent.gameObject.GetComponent<RectTransform>().rect.size;
		g.Populate (textComponent.text, textComponent.GetGenerationSettings (e));

		int what = textComponent.text.Length;
		Vector2 r = new Vector2 (g.verts [what * 4 + 2].position.x, g.verts [what * 4 + 2].position.y);

		bottom = textComponent.transform.TransformPoint (r).y;
	}

	void drawBox(Vector2 minBound, Vector2 maxBound, int offset)
	{
		Vector2 bRight = new Vector2 (maxBound.x + offset, minBound.y - offset);
		Vector2 uLeft = new Vector2 (minBound.x - offset, maxBound.y + offset);

		Vector2 size = bRight - uLeft;
		Rect newRectangle = new Rect (uLeft, size);

		GameObject hintBox = new GameObject("test");
		hintBox.tag = "Exit";
		SpriteRenderer renderer = hintBox.AddComponent <SpriteRenderer>();
		renderer.sprite = borderSprite;
		renderer.drawMode = SpriteDrawMode.Sliced;

		renderer.size = new Vector2 (newRectangle.width, newRectangle.height);;

		hintBox.transform.position = new Vector2(uLeft.x + newRectangle.width * 0.5f, uLeft.y + newRectangle.height * 0.5f); //center the box

		hintBox.transform.SetParent (boxHolder);
	}

	void drawBoxes()
	{
		foreach (KeyValuePair<int, List<Rect>> entry in rectangles) 
		{
			for (int i = 0; i < entry.Value.Count; i++) 
			{
				Rect rectangle = entry.Value [i];

				//TODO rename this
				Vector2 a = new Vector3 (rectangle.x, rectangle.y);

				GameObject smellBox = new GameObject("test");
				smellBox.tag = "Exit";
				SpriteRenderer renderer = smellBox.AddComponent <SpriteRenderer>();
				renderer.sprite = highlightSprite;
				renderer.drawMode = SpriteDrawMode.Sliced;

				renderer.size = new Vector2 (rectangle.width, rectangle.height);

				renderer.sortingLayerName = "BackGround";

				smellBox.transform.position = new Vector2(a.x + rectangle.width * 0.5f, a.y + rectangle.height * 0.5f); //center the box

				smellBox.transform.SetParent (boxHolder);

			}
		}
	}

	private void Start() 
	{
		boxHolder = new GameObject ("boxHolder").transform;
		boxHolder.SetParent (this.transform);
		textComponent = GetComponent<Text> ();
		mainCamera = Camera.main;

	}

	public void CheckRefactor(int type, int correctRefactor) 
	{
			
		Vector2 clickPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);

		foreach (KeyValuePair<int, List<Rect>> entry in rectangles) 
		{
			for (int i = 0; i < entry.Value.Count; i++) 
			{
				if (entry.Value [i].Contains (clickPosition, true)) 
				{
					if (correctRefactor != type) 
					{
						textComponent.text = "This is not the most appropriate refactor for this piece of code";

						destroyBoxes ();

						Vector2 minBound = Vector2.zero;
						Vector2 maxBound = Vector2.zero;

						TextFunctions.getTextBounds (ref minBound, ref maxBound, textComponent);

						//Draws the border around the text
						//drawBox (minBound, maxBound, 1);
						GameObject hintBox = TextFunctions.drawBox(minBound, maxBound, 1, boxHolder, borderSprite);

						mainCamera.transform.position = 
							new Vector3(hintBox.transform.position.x, hintBox.transform.position.y, mainCamera.transform .position.z);

						state = State.INCORRECT;

					}
					else if (entry.Key == 0)
					{ 
						XmlNode correctNode = currentNode.SelectSingleNode("../correctcode");
						textComponent.text = correctNode.InnerText;
						destroyBoxes ();

						Vector2 minBound = Vector2.zero;
						Vector2 maxBound = Vector2.zero;

						getTextBounds (ref minBound, ref maxBound);

						//Draws the border around the text
						drawBox (minBound, maxBound, 1);

						state = State.CORRECT;
						break;
					} 
					else 
					{
						XmlNode feedbackNode = currentNode.SelectSingleNode("../feedback");
						if (feedbackNode != null)
						{
							foreach (XmlNode node in feedbackNode.ChildNodes) 
							{
								if (node.Name == "hint") 
								{
									if (TextFunctions.getAttributeInt (node, "id") == entry.Key) 
									{
										//print (node.InnerText);
										destroyBoxes();
										textComponent.text = node.InnerText;
										textComponent.text += "\nPress any key to attempt another block";

										Vector2 minBound = Vector2.zero;
										Vector2 maxBound = Vector2.zero;

										TextFunctions.getTextBounds (ref minBound, ref maxBound, textComponent);

										//Draws the border around the text
										//drawBox (minBound, maxBound, 1);
										GameObject hintBox = TextFunctions.drawBox(minBound, maxBound, 1, boxHolder, borderSprite);

										mainCamera.transform.position = 
											new Vector3(hintBox.transform.position.x, hintBox.transform.position.y, mainCamera.transform .position.z);
									}
								}
							}
						}
						state = State.INCORRECT;
						break;
					}
				}
			}
		}
	}

	public void moveCamera()
	{
		float x = 0;
		float y = 0;

		if (Input.GetKey ("w") && mainCamera.transform.position.y < top) 
		{
			y = 1;
		}

		if (Input.GetKey ("s") && mainCamera.transform.position.y > bottom) 
		{
			y = -1;
		}

		mainCamera.transform.Translate (new Vector3 (x, y, 0.0f));
	}
}

