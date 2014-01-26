using UnityEngine;
using System.Collections;

public class CubeAnimation : MonoBehaviour {

	private bool moveUp = true;

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

    void OnTriggerEnter(Collider other)
    {
	  	if (other.gameObject.name == "SphereCollider")
    	{
    		if (moveUp)
        		animation.Play("cubeRise", PlayMode.StopAll);
        	else
        		animation.Play("cubeSink", PlayMode.StopAll);

        	moveUp = !moveUp;
    	}
	}
}
