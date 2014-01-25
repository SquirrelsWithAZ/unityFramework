using UnityEngine;
using System.Collections;

public class EventControl : MonoBehaviour {

    void Update () {
 
 		//Event e = Event.current;
 		if (Input.GetKeyUp ("space")) {
		//if (e.isKey) {
			Debug.Log("detected space");
			//Debug.Log("Detected key code: " + e.keyCode);
		}
 
    }
}

