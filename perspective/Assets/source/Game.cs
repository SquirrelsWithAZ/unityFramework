using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour
{
  public float maxTimeTwerking;
  public float timeDead;

  public Grid grid;
  public InputManager inputManager;
  public ScoreManager scoreManager;

  private List<Timeout> _timeouts = new List<Timeout>();

  public void Awake()
  {
    _instanceRef = this;
    inputManager.PushInputSpaceForAllPlayers(InputSpaces.GameSpace);
  }

  public void Update()
  {
      List<Timeout> toDelete = new List<Timeout>();
      foreach(Timeout t in _timeouts)
          if (t.Time <= Time.time)
          {
              t.Action();
              toDelete.Add(t);
          }

      foreach (Timeout t in toDelete) _timeouts.Remove(t);
  }

  public void setTimeout(float executeAfter, System.Action action)
  {
      _timeouts.Add(new Timeout() { Time = executeAfter, Action = action });
  }

  private static Game _instanceRef;
  public static Game instance
  {
    get
    {
      return _instanceRef;
    }
  }

  private class Timeout {
      public float Time;
      public System.Action Action;
  }
}