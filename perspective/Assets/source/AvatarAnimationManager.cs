using UnityEngine;
using System.Collections;

public class AvatarAnimationManager : MonoBehaviour {
  public Avatar avatar;
  public Animation animationRoot;
  public AnimationClip idleClip;
  public AnimationClip idleToRunClip;
  public AnimationClip runClip;

  private bool _running;

  public void Start()
  {
    animationRoot.PlayQueued(idleClip.name, QueueMode.PlayNow);  
  }
  public void Update()
  {
    if(avatar._currentVelocity.sqrMagnitude != 0 && !_running)
    {
      _running = true;
      animationRoot.PlayQueued(idleToRunClip.name, QueueMode.PlayNow);
      animationRoot.PlayQueued(runClip.name, QueueMode.CompleteOthers);
    }
    else if(avatar._currentVelocity.sqrMagnitude == 0 && _running)
    {
      _running = false;
      animationRoot.PlayQueued(idleClip.name, QueueMode.PlayNow);
    }
  }
}
