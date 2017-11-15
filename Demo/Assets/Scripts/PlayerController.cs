using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public bool safe;
	public bool holding; //temporarily public

	public float speed = 15.0f;
	public Vector2 direction;

	private Rigidbody2D rb2d;
	public GameObject heldSmell;
	public GameObject nearSmell;

	public bool help;

	public LevelCreate level;
	public LayerMask blockingLayer;

	// Use this for initialization
	void Start () 
	{
		rb2d = GetComponent<Rigidbody2D>();

		nearSmell = null;
		heldSmell = null;
		safe = false;
		help = false;
	}

	void GetInput()
	{
		/*
		 * get input
		 * move with arrow keys
		 * pickup/drop with q
		 */
		//direction = Vector2.zero;
		if (direction.x == 0 && direction.y == 0)
		{
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

		if (Input.GetKeyDown ("q")) 
		{
			if (holding)
			{
				Drop ();
			} 
			else 
			{
				PickUp ();
			}
		}
	}
	
	void FixedUpdate () 
	{

		GetInput ();
		//check if direction == 0
		//if it does, check if the player is at an integer position
		//if not, figure out what direction they were moving in and 
		Vector2 halfSize = GetComponent<Renderer>().bounds.size * 0.5f;

		Vector2 velocity = direction * speed * Time.deltaTime;
		Vector2 newPosition = new Vector2(rb2d.position.x + direction.x * halfSize.x, rb2d.position.y + direction.y * halfSize.y) + velocity;

		RaycastHit2D hit = Physics2D.Linecast (rb2d.position, newPosition, blockingLayer);
		//RaycastHit2D hit = Physics2D.BoxCast(rb2d.position, GetComponent<Renderer>().bounds.size * 0.9f, 0, direction, velocity.magnitude, blockingLayer);


		//Can optimize this. currently checking every frame, but it's only nessacary to check once before moving

		if (hit.transform == null) 
		{
			rb2d.MovePosition (rb2d.position + velocity);
		} 
		else if (hit.transform.gameObject.CompareTag ("Door"))
		{
			if (holding)
			{
				holding = false;
				hit.transform.gameObject.SetActive (false);
			}
			else 
			{
				help = true;
				if (direction.x != 0) 
				{
					rb2d.MovePosition (new Vector2(Mathf.Round(rb2d.position.x), rb2d.position.y));
				}
				else if (direction.y != 0) 
				{
					rb2d.MovePosition (new Vector2 (rb2d.position.x, Mathf.Round (rb2d.position.y)));
				}
			}
		}

		else 
		{
			if (direction.x != 0) 
			{
				rb2d.MovePosition (new Vector2(Mathf.Round(rb2d.position.x), rb2d.position.y));
			}
			else if (direction.y != 0) 
			{
				rb2d.MovePosition (new Vector2 (rb2d.position.x, Mathf.Round (rb2d.position.y)));
			}
		}
	}

	void LateUpdate()
	{
		float snapDistance = 0.2f;
		if (direction.x > 0)
		{
			float fuckunity = Mathf.Ceil (rb2d.position.x);
			if (Mathf.Abs (rb2d.position.x - Mathf.Ceil (rb2d.position.x)) <= snapDistance) 
			{
				transform.position = new Vector3 (fuckunity, rb2d.position.y, 0.0f);
				direction = Vector2.zero;
			}
		} 
		else if (direction.x < 0) 
		{
			float fuckunity = Mathf.Floor (rb2d.position.x);
			if (Mathf.Abs (rb2d.position.x - Mathf.Floor (rb2d.position.x)) <= snapDistance) 
			{
				transform.position = new Vector3 (fuckunity, rb2d.position.y, 0.0f);
				direction = Vector2.zero;
			}
		}

		if (direction.y > 0)
		{
			float clampedPosition = Mathf.Ceil (rb2d.position.y);
			if (Mathf.Abs (rb2d.position.y - clampedPosition) <= snapDistance) 
			{
				transform.position = new Vector3 (rb2d.position.x, clampedPosition, 0.0f);
				direction = Vector2.zero;
			}
		} 
		else if (direction.y < 0) 
		{
			float clampedPosition = Mathf.Floor (rb2d.position.y);
			if (Mathf.Abs (rb2d.position.y - clampedPosition) <= snapDistance) 
			{
				transform.position = new Vector3 (rb2d.position.x, clampedPosition, 0.0f);
				direction = Vector2.zero;

			}
		}
	}
		
	void PickUp()
	{
		if (nearSmell != null) 
		{
			heldSmell = nearSmell;
			holding = true;

			heldSmell.transform.SetParent (this.gameObject.transform);
			((SmellScript)heldSmell.GetComponent (typeof(SmellScript))).PickUp();
			/*
			 * 
			 */
		}
	}

	void Drop()
	{
		((SmellScript)heldSmell.GetComponent (typeof(SmellScript))).Drop();
		holding = false;
		heldSmell.transform.parent = null;
		heldSmell = null;
	}
		

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.CompareTag ("Smell")) {
			nearSmell = other.gameObject;
		}
		/*else if (other.gameObject.CompareTag ("Wall")) 
		{

			if (direction.x != 0) 
			{
				help = true;

				transform.position = new Vector2(transform.position.x - other.transform.position.x, transform.position.y);
				//transform.position = (new Vector2 (Mathf.Floor (rb2d.position.x- direction.x), (rb2d.position.y)));
				//rb2d.MovePosition (new Vector2 (Mathf.Floor (rb2d.position.x- direction.x), (rb2d.position.y)));
			}
			if (direction.y != 0) 
			{
				rb2d.MovePosition (new Vector2 ((rb2d.position.x), Mathf.Floor(rb2d.position.y - direction.y)));
			}
		}*/
		else 
		{
			safe = true;
		}	
			
	}
	void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.CompareTag ("Smell")) 
		{
			if (nearSmell != null)
			{
				nearSmell = null;
			}
		} 
		else 
		{
			safe = false;
		}
	}
}
