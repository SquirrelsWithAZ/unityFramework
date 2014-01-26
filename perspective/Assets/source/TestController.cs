using UnityEngine;
using System.Collections;

public class TestController : MonoBehaviour 
{
  public PlayerInputManager inputManager;
	// Use this for initialization
	void Start () 
  {
	
	}
	
	// Update is called once per frame
	void Update () 
  {
    if (inputManager.CheckInput(InputActions.Down).isActive)
      transform.position -= new Vector3(0f, 0f, -5f) * Time.deltaTime;
	}
}
