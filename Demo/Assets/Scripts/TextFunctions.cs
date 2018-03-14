using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Xml;


public static class TextFunctions
{
	//If this returns <0, something went wrong
	public static int getAttributeInt(XmlNode node, string name)
	{
		int type = -1;
		foreach (XmlAttribute attribute in node.Attributes)
		{
			if (attribute.Name == name) 
			{
				type = int.Parse (attribute.Value);
			}
		}
		return type;
	}

	public static void drawBox(Vector2 minBound, Vector2 maxBound, int offset, Transform boxHolder, Sprite borderSprite)
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

	public static void getTextBounds(ref Vector2 minBound, ref Vector2 maxBound, Text textComponent)
	{
		string text = textComponent.text;

		TextGenerator generator;

		generator = new TextGenerator (textComponent.text.Length);
		Vector2 extents = textComponent.gameObject.GetComponent<RectTransform> ().rect.size;
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
}
