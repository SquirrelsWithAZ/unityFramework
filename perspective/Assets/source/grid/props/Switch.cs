using UnityEngine;
using System.Collections;

public class Switch : Prop 
{
	public double cooldown;

  private double _cooldownCounter;

  public void StepOnSwitch()
  {
    if (_cooldownCounter == 0)
    {

      //Game.instance.grid.swapTileState();
      moveSphereCollider();
      _cooldownCounter = cooldown;
    }
  }

  private void Update()
  {
    if (_cooldownCounter > 0)
      _cooldownCounter -= Time.deltaTime;
    else if (_cooldownCounter < 0)
      _cooldownCounter = 0;
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
