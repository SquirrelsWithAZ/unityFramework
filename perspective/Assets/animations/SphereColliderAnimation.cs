using UnityEngine;
using System.Collections;

public class SphereColliderAnimation : MonoBehaviour {

	// Use this for initialization
	void Start () {
		animation.Play("sphereCollider", PlayMode.StopAll);
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
		transform.position = new Vector3(i, 0, j);
		animation.Play("sphereCollider", PlayMode.StopAll);
	}
}
