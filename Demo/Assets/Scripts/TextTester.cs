using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using UnityEditor;
using System.Xml;

public class TextTester : MonoBehaviour 
{

	public Text textComponent; 
	public Camera mainCamera;

	public List<string> ClickableWords = new List<string>(); 
	List<List<int>> wordIndexes = new List<List<int>> ();

	public enum State{LOOKING, SELECTING, CORRECT, INCORRECT};
	public State state;

	//For the top and bottom of a code fragment
	//wip
	float top = 0;
	float bottom = -50;

	string correctCode = "";

	public Dictionary<int, string> replacedWords = new Dictionary<int, string> ();
	public Dictionary<int, List<Rect>> rectangles = new Dictionary<int, List<Rect>>(); //wip

	private void Start() 
	{
		//rectangles.Add (new List<Rect> ());
		textComponent = GetComponent<Text> ();
		mainCamera = Camera.main;

		XmlDocument doc = new XmlDocument();
		doc.Load ("Assets/Scripts/test.xml");

		correctCode = doc.DocumentElement.SelectSingleNode ("/stuff/correctcode").InnerText;
		XmlNode levelnode =  doc.DocumentElement.SelectSingleNode("/stuff/code");
		foreach (XmlNode node in levelnode.ChildNodes) 
		{
			textComponent.text += node.InnerText;
			string text = textComponent.text;

			if (node.Name == "r1")
			{
				TextGenerator generator;

				generator = new TextGenerator (textComponent.text.Length);
				Vector2 extents = textComponent.gameObject.GetComponent<RectTransform>().rect.size;
				generator.Populate (textComponent.text, textComponent.GetGenerationSettings (extents));

				int type = -1;
				foreach (XmlAttribute attribute in node.Attributes)
				{
					if (attribute.Name == "t") 
					{
						type = int.Parse (attribute.Value);
					}
				}

				//Calculate bounds of the first character
				int startI = text.Length - node.InnerText.Length;
				Vector2 upperLeft = new Vector2 (generator.verts [startI * 4].position.x, generator.verts [startI * 4].position.y);
				Vector2 bottomright = new Vector2 (generator.verts [startI * 4 + 2].position.x, generator.verts [startI * 4 + 2].position.y);

				Vector3 uleft = textComponent.transform.TransformPoint (upperLeft);
				Vector3 bright = textComponent.transform.TransformPoint (bottomright);

				float highestY = uleft.y;
				float lowestY = bright.y;
				float greatestX = bright.x;
				float lowestX = uleft.x;

				//Loop through the rest of the characters to find the minimum and maximum bounds of the block
				//This could probably be optimized a bit(only check min and max y on the first and last line, check x max on new line)
				for (int i = startI; i < text.Length; i++) 
				{
					upperLeft = new Vector2 (generator.verts [i * 4].position.x, generator.verts [i * 4].position.y);
					bottomright = new Vector2 (generator.verts [i * 4 + 2].position.x, generator.verts [i * 4 + 2].position.y);

					uleft = textComponent.transform.TransformPoint (upperLeft);
					bright = textComponent.transform.TransformPoint (bottomright);

					if (uleft.y > highestY) 
					{
						highestY = uleft.y;
					}

					if (uleft.x < lowestX) 
					{
						lowestX = uleft.x;
					}

					if (bright.y < lowestY) 
					{
						lowestY = bright.y;
					}

					if (bright.x > greatestX) 
					{
						greatestX = bright.x;
					}
				}
				if (!rectangles.ContainsKey (type))  //temp
				{
					rectangles.Add (type, new List<Rect>());
				}

				Vector2 uLeft = new Vector2(lowestX, highestY);
				Vector2 bBight = new Vector2(greatestX, lowestY);

				Vector2 size = bBight - uLeft;
				Rect newRectangle = new Rect (uLeft, size);

				rectangles [type].Add (newRectangle);


				/*	int indexOfTextQuad = text.Length - node.InnerText.Length;
				Vector2 upperLeft = new Vector2 (generator.verts [indexOfTextQuad * 4].position.x, generator.verts [indexOfTextQuad * 4].position.y);
				//indexOfTextQuad = text.Length;
				Vector2 bottomright = new Vector2 (generator.verts [indexOfTextQuad * 4 + 2].position.x, generator.verts [indexOfTextQuad * 4 + 2].position.y);

				Vector3 uleft = textComponent.transform.TransformPoint (upperLeft);
				Vector3 bright = textComponent.transform.TransformPoint (bottomright);
				Vector2 size = bright - uleft;
				Rect newRectangle = new Rect (uleft, size);
		
				int type = -1;
				foreach (XmlAttribute attribute in node.Attributes)
				{
					if (attribute.Name == "t") 
					{
						type = int.Parse (attribute.Value);
					}
				}
				if (!rectangles.ContainsKey (type))  //temp
				{
					rectangles.Add (type, new List<Rect>());
				}
				rectangles [type].Add (newRectangle);*/

			}
		}
		foreach (KeyValuePair<int, List<Rect>> entry in rectangles) 
		{
			for (int i = 0; i < entry.Value.Count; i++) 
			{
				Rect test = entry.Value [i];

				Vector2 a = new Vector3 (test.x, test.y);
				Vector2 b = new Vector3 (test.x + test.width, test.y);
				Vector2 c = new Vector3 (test.x, test.y + test.height);
				Vector2 d = new Vector3 (test.x + test.width, test.y + test.height);

				Debug.DrawLine (a, b, Color.blue, 50f);
				Debug.DrawLine (b, d, Color.blue, 50f);
				Debug.DrawLine (d, c, Color.blue, 50f);
				Debug.DrawLine (c, a, Color.blue, 50f);
			}
		}
	}

	void Update()
	{
		switch(state)
		{
		case State.SELECTING:

			if (Input.GetMouseButtonDown (0)) {
				Vector2 clickPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);

				foreach (KeyValuePair<int, List<Rect>> entry in rectangles) 
				{
					for (int i = 0; i < entry.Value.Count; i++) 
					{
						if (entry.Value [i].Contains (clickPosition, true)) 
						{
							if (entry.Key == 0)
							{ //temp
								textComponent.text = correctCode;

								state = State.CORRECT;
								break;
							} 
							else 
							{
								state = State.INCORRECT;
								break;
							}
						}
					}
				}
				print (state);

			}
			goto case State.LOOKING;

		case State.LOOKING:
			float x = 0;
			float y = 0;

			if (Input.GetKey ("up") && mainCamera.transform.position.y < top) 
			{
				y = 1;
			}

			if (Input.GetKey ("down") && mainCamera.transform.position.y > bottom) 
			{
				y = -1;
			}

			mainCamera.transform.Translate (new Vector3 (x, y, 0.0f));
			break;
		}
	}
}
