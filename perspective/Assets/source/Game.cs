using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour
{
  public Grid grid;
  public InputManager inputManager;
  public ScoreManager scoreManager;

  public void Awake()
  {
    _instanceRef = this;
    inputManager.PushInputSpaceForAllPlayers(InputSpaces.GameSpace);
  }

  private static Game _instanceRef;
  public static Game instance
  {
    get
    {
      return _instanceRef;
    }
  }
}
