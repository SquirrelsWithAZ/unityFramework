using UnityEngine;
using System.Collections;

public class Switch : Prop 
{
	public double cooldown;
  public Renderer switchRenderer;
  public AnimationCurve cooldownColorCurve;

  private Color _initialColor;
  private double _cooldownCounter;

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
      Game.instance.grid.swapTileState();
      Game.instance.grid.swapTileVisuals();
      _cooldownCounter = cooldown;
    }
  }

  private void Update()
  {
    if (_cooldownCounter > 0)
    {
      switchRenderer.material.color = Color.Lerp( _initialColor, Color.gray,cooldownColorCurve.Evaluate((float)(_cooldownCounter / cooldown)));
      _cooldownCounter -= Time.deltaTime;
    }
    else if (_cooldownCounter < 0)
    {
      switchRenderer.material.color = _initialColor;
      _cooldownCounter = 0;
    }
  }

  //snap the sphere collider over and trigger to kick off the state change 
  private void moveSphereCollider()
  {
      //Debug.Log("move sphere collider");

      int i = this.transform.GetComponent<Switch>().i;
      int j = this.transform.GetComponent<Switch>().j;

      Debug.Log("switch is at: " + i + " , " + j);

      GameObject sphere = GameObject.Find("SphereCollider");
      sphere.GetComponent<SphereColliderAnimation>().Trigger(i*3, j*3);
  }

}
