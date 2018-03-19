using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class EndScript : MonoBehaviour {


	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown (KeyCode.Return)) 
		{
			SceneManager.LoadScene (0);
		}
		else if (Input.GetKeyDown (KeyCode.Escape)) 
		{
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#else
			Application.Quit();
			#endif
		}
	}
}
