using UnityEngine;
using System.Collections;

public class CubeAnimation : MonoBehaviour {

	private bool moveUp = true;

	// Use this for initialization
	void Start () {
	
	}
	
	void Update () {
 		if (Input.GetKeyUp ("1")) 
 		{
			animation.Play("cubeRise", PlayMode.StopAll);
		}

		if (Input.GetKeyUp ("2")) 
		{
			animation.Play("cubeSink", PlayMode.StopAll);
		}

		if (Input.GetKeyUp ("3")) 
		{
			if (this.transform.parent.gameObject.name == "Tile_Type_A(Clone)")
			{
				if (moveUp)
					animation.Play("cubeSink", PlayMode.StopAll);
				else
					animation.Play("cubeRise", PlayMode.StopAll);	
			}

			if (this.transform.parent.gameObject.name == "Tile_Type_B(Clone)")
			{
				if (moveUp)
					animation.Play("cubeRise", PlayMode.StopAll);
				else
					animation.Play("cubeSink", PlayMode.StopAll);	
			}

			moveUp = !moveUp;
		}
    }

    void animationEventTop()
    {
    	Debug.Log("block at top");
    }

    void animationEventBottom()
    {
    	Debug.Log("block at bottom");
    }

    void OnTriggerEnter(Collider other)
    {
	  	if (other.gameObject.name == "SphereCollider")
    	{
    		if (this.transform.parent.gameObject.name == "Tile_Type_A(Clone)")
			{
				if (moveUp)
					animation.Play("cubeSink", PlayMode.StopAll);
				else
					animation.Play("cubeRise", PlayMode.StopAll);	
			}

			if (this.transform.parent.gameObject.name == "Tile_Type_B(Clone)")
			{
				if (moveUp)
					animation.Play("cubeRise", PlayMode.StopAll);
				else
					animation.Play("cubeSink", PlayMode.StopAll);	
			}

			moveUp = !moveUp;
    	}
	}
}
