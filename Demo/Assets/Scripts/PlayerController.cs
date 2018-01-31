using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float speed = 15.0f;
	public Vector2 direction;

	private Rigidbody2D rb2d;
	public GameObject heldSmell;
	public GameObject nearSmell;

	public GameObject door;

	public bool hasKey; //Probably want some type of list later

	public LayerMask blockingLayer;

	private bool moving;
	public bool exiting;

	private Vector2 newPosition;

	// Use this for initialization
	void Start () 
	{
		rb2d = GetComponent<Rigidbody2D>();

		nearSmell = null;
		heldSmell = null;
		door = null;
		hasKey = true;
		moving = false;
		exiting = false;

		newPosition = Vector2.zero;
	}

	void GetInput()
	{
		direction = Vector2.zero;
		if (Input.GetKey ("up"))
		{
			direction.y	= 1;
		} 
		else if (Input.GetKey ("down"))
		{
			direction.y	= -1;
		} 
		else if (Input.GetKey ("left"))
		{
			direction.x	= -1;
		}
		else if (Input.GetKey ("right")) 
		{
			direction.x = 1;
		}
	}

	void FixedUpdate()
	{
		if (moving) 
		{
			move ();
		}
	}
	
	void Update () 
	{
		if (!moving) 
		{
			GetInput ();
			if (direction.magnitude != 0)
			{
				newPosition = new Vector2 (rb2d.position.x + direction.x, rb2d.position.y + direction.y);

				RaycastHit2D hit = Physics2D.Linecast (rb2d.position, newPosition, blockingLayer);

				if (hit.transform == null) 
				{
					moving = true;
				}
			}
		}
	}

	void LateUpdate()
	{
		if (moving) 
		{
			snapToPosition (newPosition);
		}
	}

	public void move()
	{
		Vector2 velocity = direction * speed * Time.deltaTime;

		rb2d.MovePosition (rb2d.position + velocity);
	}

	public void snapToPosition(Vector2 position)
	{
		float snapDistance = 0.2f;
		if (direction.x > 0)
		{
			float clampedX = Mathf.Ceil (position.x);
			if (Mathf.Abs (rb2d.position.x - clampedX) <= snapDistance) 
			{
				transform.position = new Vector3 (clampedX, rb2d.position.y, 0.0f);
				direction = Vector2.zero;
				moving = false;
			}
		} 
		else if (direction.x < 0) 
		{
			float clampedX = Mathf.Floor (position.x);
			if (Mathf.Abs (rb2d.position.x - clampedX) <= snapDistance) 
			{
				transform.position = new Vector3 (clampedX, rb2d.position.y, 0.0f);
				direction = Vector2.zero;
				moving = false;
			}
		}
		else if (direction.y > 0)
		{
			float clampedY = Mathf.Ceil (position.y);
			if (Mathf.Abs (rb2d.position.y - clampedY) <= snapDistance) 
			{
				transform.position = new Vector3 (rb2d.position.x, clampedY, 0.0f);
				direction = Vector2.zero;
				moving = false;
			}
		} 
		else if (direction.y < 0) 
		{
			float clampedY = Mathf.Floor (position.y);
			if (Mathf.Abs (rb2d.position.y - clampedY) <= snapDistance) 
			{
				transform.position = new Vector3 (rb2d.position.x, clampedY, 0.0f);
				direction = Vector2.zero;
				moving = false;
			}
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.CompareTag ("Smell"))
		{
			//nearSmell = other.gameObject;
			hasKey = true;
			other.gameObject.SetActive (false);
		} 
		else if (other.gameObject.CompareTag ("Door")) 
		{
			door = other.gameObject;
		}
		else if (other.gameObject.CompareTag ("Exit")) 
		{
			exiting = true;
		}
			
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.CompareTag ("Door")) 
		{
			if (door != null)
			{
				door = null;
			}
		} 
		else if (other.gameObject.CompareTag ("Exit")) 
		{
			exiting = false;
			moving = false;
			//snapToPosition (rb2d.position);
		}
	}
}
