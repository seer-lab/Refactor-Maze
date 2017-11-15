using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tester : MonoBehaviour {

	public bool onOff;
	public GameObject level;
	public GameObject code;
	// Use this for initialization
	void Start () 
	{
		level = GameObject.Find ("Level");
		//code = GameObject.Find ("CodeLevel");
		//code.SetActive (false);
		onOff = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("p")) 
		{
			onOff = !onOff;
			level.SetActive (onOff);
		//	code.SetActive (!onOff);
		/*	if (onOff)
			{
				onOff = false;
				level.SetActive (false);
			} 
			else 
			{
				level.SetActive (true);
				onOff = true;
			}*/
		}
	}
}
