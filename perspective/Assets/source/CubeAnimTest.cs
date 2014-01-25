using UnityEngine;
using System.Collections;

public class CubeAnimTest : MonoBehaviour {

	public float moveSpeed = 5;
	public float speedDamp = 0.5f;

	private float speedDampV;
	private float currentSpeed;

	//public Animator cubeAnimator;

	// Use this for initialization
	void Start () {
	
	}
	
	void Update () {
 		if (Input.GetKeyUp ("1")) {
			animation.Play("cubeRise", PlayMode.StopAll);
		}
		if (Input.GetKeyUp ("2")) {
			animation.Play("cubeSink", PlayMode.StopAll);
		}
 
 		
    }
}
