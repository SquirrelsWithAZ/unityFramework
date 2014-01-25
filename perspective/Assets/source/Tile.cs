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

public struct TilePos
{
  public TilePos(int xCoord, int yCoord)
  {
    x = xCoord;
    y = yCoord;
  }
  public int x;
  public int y;
}

namespace CustomExtensions
{
  public static class Vector3Extensions
  {
    public static TilePos GetTilePos(this Vector3 vec3)
    {
      float xFloat = vec3.x;
      float yFloat = vec3.z;

      return new TilePos(Mathf.RoundToInt(xFloat), Mathf.RoundToInt(yFloat));
    }
  }
}
