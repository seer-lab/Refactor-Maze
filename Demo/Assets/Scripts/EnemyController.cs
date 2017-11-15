using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

	public PlayerController player;

	private SpriteRenderer renderer;

	private bool awake;
	// Use this for initialization
	void Start () 
	{
		awake = false;
		renderer = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		//clean all of this up geez
		if (player.holding) 
		{
			awake = true;
		} 
		else 
		{
			awake = false;
		}

		if (awake) 
		{
			renderer.color = new Color (255, 0, 0);
		}
		else 
		{
			renderer.color = new Color (0, 0, 0);
		}
	}
}
