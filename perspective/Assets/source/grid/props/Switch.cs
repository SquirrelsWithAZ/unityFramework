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
}
