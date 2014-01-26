using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
  public int i;
  public int j;

  public Vector3 GetWorldPosition()
  {
    return transform.position;
  }

  public static int GetPhysicsLayerFromType(TileTypes tileType)
  {
    switch(tileType)
    {
      case TileTypes.Neutral:
        return LayerMask.NameToLayer("TileTypeNeutral");
      case TileTypes.TypeA:
        return LayerMask.NameToLayer("TileTypeA");
      case TileTypes.TypeB:
        return LayerMask.NameToLayer("TileTypeB");
      default:
        return LayerMask.NameToLayer("TileTypeNeutral");
    }
  }
}

[System.Serializable]
public class GridPos
{
  public GridPos(int xCoord, int yCoord)
  {
    x = xCoord;
    y = yCoord;
  }
  public int x;
  public int y;
}