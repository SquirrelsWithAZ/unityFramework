using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour 
{
	public Grid grid;
  public InputManager inputManager;

  public void Start()
  {
    inputManager.PushInputSpaceForAllPlayers(InputSpaces.GameSpace);
  }
}
