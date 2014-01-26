using UnityEngine;
using System.Collections;

public class SphereColliderAnimator : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	if (Input.GetKeyUp ("space")) {
			animation.Play("sphereCollider", PlayMode.StopAll);
		}
	}
}
