using UnityEngine;
using System.Collections;

public class EventControl : MonoBehaviour {

	//global cooldown
	public double cooldown;
	public double _cooldownCounter;


	public  float 	movementSize = 1.0f;

	private float 	blockStartHeightA;
	private float 	blockStartHeightB;

	private bool 	moving 		= false;	//should the blocks be moving
	private float 	speed		= 0.1f;
	private float 	moveDistance= 1.0f;	
	private float 	direction 	= 1.0f;	
	private float 	startTime;
	private float 	journeyLength;


	void Start()
	{
		//offset by half of the normal move size
		moveDistance = movementSize / 2;
		moveBlockByTag(moveDistance);
		moving=true;
		//moveDistance = movementSize / 2;
		//moveBlockByTag(moveDistance);
		//moving=true;
		Debug.Log("Game Started");


	}


    void Update () {
 		if (Input.GetKeyUp ("space")) {
			//flip the direction of movement each time
			direction 			= - direction;
			moveDistance		= movementSize * direction;

			//setup the blocks to do thier thing
			moveBlockByTag(moveDistance);
			moving = true;	
		}
 
 		//wait for everything to get setup before we start moving to avoid copy exceptions
    	if (moving)
    	{
 			moveBlocks(moveDistance);
 		}	
    }

    //Do a linier interpolation between block initial position and the end calculated in moveBlockByTag()
    void moveBlocks(float _moveDistance)
    {
    	GameObject[] blocks;

    	//A Blocks
		blocks = GameObject.FindGameObjectsWithTag("Tile_Type_A");
		foreach (GameObject block in blocks)
		{
	    		float distCovered = (Time.time - startTime) * speed;
        		float fracJourney = distCovered / journeyLength;
	    		Vector3 endPosition   = new Vector3(block.transform.position.x, blockStartHeightA + _moveDistance, block.transform.position.z);
	    		block.transform.position = Vector3.Lerp(block.transform.position, endPosition, fracJourney);
    	}

    	//B Blocks
    	blocks = GameObject.FindGameObjectsWithTag("Tile_Type_B");
		foreach (GameObject block in blocks)
		{
	    		float distCovered = (Time.time - startTime) * speed;
        		float fracJourney = distCovered / journeyLength;
	    		Vector3 endPosition   = new Vector3(block.transform.position.x, blockStartHeightB - _moveDistance, block.transform.position.z);
	    		block.transform.position = Vector3.Lerp(block.transform.position, endPosition, fracJourney);
    	}
    }


    //Grab the blocks by the given tag, save starting position and calculate target position
    void moveBlockByTag(float _moveDistance)
    {
    	GameObject[] blocks;

    	blocks = GameObject.FindGameObjectsWithTag("Tile_Type_A");
    	//Debug.Log("found this many A blocks: " + blocks.Length);
		foreach (GameObject block in blocks)
		{
			blockStartHeightA 	= block.transform.position.y;
			startTime			= Time.time;
			Vector3 endPosition = new Vector3(block.transform.position.x, blockStartHeightA + _moveDistance, block.transform.position.z);
			journeyLength 		= Vector3.Distance(block.transform.position, endPosition);
			//Debug.Log("block S start height: " + blockStartHeightA);
		}

		blocks = GameObject.FindGameObjectsWithTag("Tile_Type_B");
    	//Debug.Log("found this many A blocks: " + blocks.Length);
		foreach (GameObject block in blocks)
		{
			blockStartHeightB 	= block.transform.position.y;
			startTime			= Time.time;
			Vector3 endPosition = new Vector3(block.transform.position.x, blockStartHeightB - _moveDistance, block.transform.position.z);
			journeyLength 		= Vector3.Distance(block.transform.position, endPosition);
			//Debug.Log("block S start height: " + blockStartHeightA);
		}

		
    }
}

