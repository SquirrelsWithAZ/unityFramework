using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
  public int i;
  public int j;

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
