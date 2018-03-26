using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerController : MonoBehaviour {

	public float speed = 15.0f;
	private float moveSpeed; //speed * Time.deltaTime
	public Vector2 direction;

	private Rigidbody2D rb2d;
	public GameObject heldSmell;
	public GameObject nearSmell;

	public GameObject door;
	public GameObject keyDialog;
	private KeyProperties keyToAdd;


	public List<KeyProperties> keyList = new List<KeyProperties> (); //the int is the ID, representing the technique, of the key

	public LayerMask blockingLayer;

	private bool moving;
	public bool exiting;
	public bool keyLook;
	private bool recentKey; //So the player doesn't get when dropping keys.

	private Vector2 newPosition;


	// Use this for initialization
	void Start () 
	{
		rb2d = GetComponent<Rigidbody2D>();

		nearSmell = null;
		heldSmell = null;
		door = null;
		moving = false;
		exiting = false;
		keyLook = false;
		recentKey = false;

		keyDialog = GameObject.Find ("KeyDialog");
		keyDialog.SetActive (false);

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
	
	void Update () 
	{
		if(keyLook)
		{
			if (Input.GetKeyDown (KeyCode.Q)) 
			{
				PickUpKey ();
			}
			else if (Input.GetKeyDown (KeyCode.Return)) 
			{
				keyToAdd = null;
				keyDialog.SetActive (false);
				keyLook = false;
			}
		}
		else if (!moving) 
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
		else
		{
			move ();
			snapToPosition (newPosition);

		}
	}

	public void move()
	{
		moveSpeed = speed * Time.deltaTime;
		Vector2 velocity = direction * moveSpeed;

		rb2d.MovePosition (rb2d.position + velocity);
	}

	//need to rework this, it's currently possible to overshoot the position and never stop
	public void snapToPosition(Vector2 position)
	{
		if ((rb2d.position - position).magnitude <= speed * Time.deltaTime)  //try speed * Time.deltaTime, this doesn't work
		{
			transform.position = position;
			direction = Vector2.zero;
			moving = false;
		}
		/*if (direction.x > 0)
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
		}*/
	}

	void PickUpKey()
	{
		if (keyList.Count > 0) 
		{
			keyList [0].transform.position = keyToAdd.transform.position;
			keyList [0].gameObject.SetActive (true);
			keyList.Clear ();
			recentKey = true;
		}

		keyList.Add (keyToAdd);
		keyToAdd.gameObject.SetActive (false);

		keyToAdd = null;
		keyDialog.SetActive (false);
		keyLook = false;
	}

	void ShowDialog(GameObject keyObject)
	{
		if (!recentKey) 
		{
			keyLook = true;
			keyDialog.SetActive (true);
			keyToAdd = ((KeyProperties)keyObject.GetComponent (typeof(KeyProperties)));
			Text keyText = (keyDialog.GetComponentInChildren<Text> ());
			keyText.text = keyToAdd.refactorName + "\nPress the Q key to pick up\nPress enter continue";
			if (keyList.Count > 0) 
			{
				keyText.text += "\nPicking up this key will drop your current key.";
			}
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.CompareTag ("Smell"))
		{
			ShowDialog (other.gameObject);
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
		}
		else if(other.gameObject.CompareTag("Smell"))
		{
			if (other.gameObject.active) 
			{
				recentKey = false;
			}
		}	
	}
}
