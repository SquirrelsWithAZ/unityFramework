using UnityEngine;
using System.Collections;

public class CubeAnimation : MonoBehaviour {

	public float fadeScalar = 1.0f;

	private bool moveUp = true;

	public float rollSpeed;
	public float tiltX;
	public float tiltZ;
	public bool initialized = false;

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

    //fires when leaving or hitting the top
    void animationEventTop()
    {
    	//Debug.Log("block at top");
    }

	//fires when leaving or hitting the bottom
    void animationEventBottom()
    {
    	//Debug.Log("block at bottom");
    }

    //gets called from the animation to change blocks
    void changeState()
    {
    	//Debug.Log("trying to change game state");
    	
    	//DEBUG: Change this once swapTileVisuals() supports the param
		//Game.instance.grid.swapTileVisuals(fadeScalar);
		int i = this.transform.parent.GetComponent<Tile>().i;
		int j = this.transform.parent.GetComponent<Tile>().j;
		
		//don't let the state change on initial lift
		if (initialized)
		{
			Game.instance.grid.swapTileVisuals(i, j);
			Game.instance.grid.swapTileState(i, j);
		}	

		initialized = true;

		this.transform.gameObject.audio.Play();
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
