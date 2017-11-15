using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmellScript : MonoBehaviour {


	public float timeSinceDropped;
	public float dropTimer = 0.5f;

	public Vector2 startPosition; //temporarily public
	public Vector2 moveDirection;
	public bool dropped;
	public bool moved;
	public bool returning;
	public bool safe;

	private Rigidbody2D rb2d;


	void Start () 
	{
		rb2d = GetComponent<Rigidbody2D>();
	
		timeSinceDropped = 0.0f;
		dropped = false;
		moved = false;
		returning = false;
		safe = false;
		startPosition = this.transform.position;
	}
	
	// Update is called once per frame
	void Update ()
	{
		//if this smell has moved from it's starting position and isn't being held
		//wait breifly before calculating the direction back to the start
		//move back
		if (moved) 
		{
			if (dropped) 
			{
				timeSinceDropped += Time.deltaTime;
				if (timeSinceDropped >= dropTimer) 
				{
					timeSinceDropped = 0.0f;
					dropped = false;
					returning = true;
					CalculateDirection ();
				}
			} 
			if(returning)
			{
				MoveBack ();
			}
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.CompareTag ("SafeZone")) 
		{
			safe = true;
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.CompareTag ("SafeZone")) 
		{
			safe = false;
		}
	}

	void CalculateDirection()
	{
		moveDirection =  new Vector2 
			( startPosition.x - this.transform.position.x, startPosition.y - this.transform.position.y).normalized;
	}

	public void MoveBack()
	{
		float speed = 7.0f;
		Vector2 velocity = moveDirection * speed * Time.deltaTime;
		rb2d.MovePosition(rb2d.position + velocity);

		if (Mathf.Abs (this.transform.position.x - startPosition.x) <= 0.2f &&
			Mathf.Abs (this.transform.position.y - startPosition.y) <= 0.2f )
		{
			rb2d.MovePosition(startPosition);
			moved = false;
			returning = false;
			dropped = false;
		}
	}

	public void PickUp()
	{
		moved = true;
		dropped = false;
		returning = false;
	}

	public void Drop()
	{
		if (!safe) 
		{
			dropped = true;
			timeSinceDropped = 0.0f;
		}
	}
}
