using UnityEngine;
using System.Collections;

public class Switch : Prop
{
  public double cooldown;
  public Renderer switchRenderer;
  public AnimationCurve cooldownColorCurve;

  private Color _initialColor;
  private static double _globablCooldown;
  private static double _cooldownCounter;

  public void Start()
  {
    _initialColor = switchRenderer.material.color;
  }

  public void StepOnSwitch()
  {
    if (_cooldownCounter == 0)
    {


      //Game.instance.grid.swapTileState();
      moveSphereCollider();
      //Game.instance.grid.swapTileState();
      //Game.instance.grid.swapTileVisuals();
      StartCoroutine(CooldownCounterUpdate());
      //_cooldownCounter = cooldown;
    }
  }

  private void Update()
  {
    if (_cooldownCounter > 0)
    {
      switchRenderer.material.color = Color.Lerp(_initialColor, Color.gray, cooldownColorCurve.Evaluate((float)(_cooldownCounter / cooldown)));
    }
    else if (_cooldownCounter < 0)
    {
      switchRenderer.material.color = _initialColor;
    }
  }

  //snap the sphere collider over and trigger to kick off the state change 
  private void moveSphereCollider()
  {
    //Debug.Log("move sphere collider");

    int i = this.transform.GetComponent<Switch>().i;
    int j = this.transform.GetComponent<Switch>().j;

    //Debug.Log("switch is at: " + i + " , " + j);

    GameObject sphere = GameObject.Find("SphereCollider");
    SphereColliderAnimation animationManager = sphere.GetComponent<SphereColliderAnimation>();
    animationManager.Trigger(i * 3, j * 3);
    _cooldownCounter = animationManager.animation.clip.length;
    _globablCooldown = _cooldownCounter;
  }

  IEnumerator CooldownCounterUpdate()
  {
    while (_cooldownCounter > 0)
    {
      _cooldownCounter -= Time.deltaTime;
      yield return null;
    }

    if (_cooldownCounter < 0)
    {
      yield return null;
      _cooldownCounter = 0;
    }
  }
}
