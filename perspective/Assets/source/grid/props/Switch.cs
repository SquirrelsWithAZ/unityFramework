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
}
