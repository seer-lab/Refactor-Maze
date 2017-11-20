using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tester : MonoBehaviour {

	public bool onOff;
	public GameObject level;
	public GameObject code;
	public Camera mainCamera;
	private Vector3 cameraPosition;
	// Use this for initialization
	void Start () 
	{
		level = GameObject.Find ("Level");
		code = GameObject.Find ("CodeLevel");
		mainCamera = Camera.main;
		cameraPosition = mainCamera.transform.position;

		code.SetActive (false);
		onOff = true;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown ("p")) 
		{
			onOff = !onOff;
			level.SetActive (onOff);
			code.SetActive (!onOff);
			mainCamera.transform.position = cameraPosition;
		}
	}
}
