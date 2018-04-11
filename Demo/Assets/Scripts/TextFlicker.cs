using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFlicker : MonoBehaviour {


	float flickerTimer = 0.0f;
	float flickerTime = 0.75f;

	Text textComponent;

	// Use this for initialization
	void Start () 
	{
		textComponent = GetComponent<Text> ();
		//Screen.SetResolution (800, 600, false);

	}
	
	// Update is called once per frame
	void Update () 
	{
		flickerTimer += Time.deltaTime;
		if (flickerTimer >= flickerTime) 
		{
			flickerTimer = 0.0f;
			textComponent.enabled = !textComponent.enabled;
		}
	}
}
