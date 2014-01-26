using UnityEngine;
using System.Collections;

public class CubeAnimation : MonoBehaviour {

	private bool moveUp = true;

	public float rollSpeed;
	public float tiltX;
	public float tiltZ;

	// Use this for initialization
	void Start () {
	
	}
	
	void Update () {

		//IGNORE THIS.  NO UGLY CODE, NOTHING TO SEE HERE.
		//1/26/14: Elijah Tate
		
		//this.transform.eulerAngles = new Vector3(rollSpeed * tiltX, 0, rollSpeed * tiltZ);
		this.transform.Rotate(new Vector3(rollSpeed * tiltX, 0, rollSpeed * tiltZ));

 		if (Input.GetKeyUp ("4")) 
 		{
 			//Debug.Log("hit 4");
	
			//total grid size
			//Debug.Log(range.tileCountI + " , " + range.tileCountJ);
			//float tiltAngle = rollSpeed;

			float blockSize = 3f;

/*
			//grab the parent script
			Grid range = this.transform.parent.transform.parent.GetComponent<Grid>();

			float rangeX = (float)range.tileCountJ*blockSize - blockSize;
			float rangeZ = (float)range.tileCountI*blockSize - blockSize;
*/
			//DEBUG: getting exception on blue blocks
			float rangeX = 27f;
			float rangeZ = 27f;


			//roll out away from center
			tiltX =  (2f*(this.transform.position.z/rangeX)-1f);
			tiltZ = -(2f*(this.transform.position.x/rangeZ)-1f);
			
        	//this.transform.eulerAngles = new Vector3(tiltAngle * tiltX, 0, tiltAngle * tiltZ);
		
		}
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
