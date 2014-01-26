using UnityEngine;
using System.Collections;

public class SphereColliderAnimation : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	//debug mode
	if (Input.GetKeyUp ("space")) {
			animation.Play("sphereCollider", PlayMode.StopAll);
			
		}
	}

	public void Trigger(int i, int j)
	{
		//Debug.Log("sphere is listening");
		transform.position = new Vector3(0, 0, 0);
		animation.Play("sphereCollider", PlayMode.StopAll);
	}
}
