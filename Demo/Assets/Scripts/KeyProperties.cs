using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyProperties : MonoBehaviour {


	public int type;

	public List<Color> colors = new List<Color>();
	public List<string> techniques = new List<string>(); //not sure how to handle this yet

	SpriteRenderer sRender;
	public string refactorName;

	// Use this for initialization
	void Start () 
	{
		sRender = GetComponent<SpriteRenderer> ();	
	}

	public void SetType(int t)
	{
		type = t;
		sRender.color = colors [t];
		refactorName = techniques [t];
	}
}
