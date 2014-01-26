using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;
using System;
using CustomExtensions;

public class Grid : MonoBehaviour
{
  public string levelDefinition;

  private int tileCountI;
  private int tileCountJ;
  private int tileWidth;
  private int tileHeight;

  private IDictionary<string, UnityEngine.Object> actors;
  private IDictionary<Type, List<Prop>> props;
  private GameObject[,] tiles; 

  void Awake() 
  {
    JSONNode jsonNode = JSON.Parse(this.levelDefinition);
    this.tileCountI = jsonNode["tileCountI"].AsInt;
    this.tileCountJ = jsonNode["tileCountJ"].AsInt;
    this.tileWidth = jsonNode["tileWidth"].AsInt;
    this.tileHeight = jsonNode["tileHeight"].AsInt;

    // Load actors
    this.actors = new Dictionary<string, UnityEngine.Object>();
    JSONArray actorJsonNodes = jsonNode["actors"].AsArray;
    foreach(JSONNode jsonData in actorJsonNodes)
    {
      string type = jsonData["type"];
      string prefab = jsonData["prefab"];
      this.actors[type] = Resources.Load(prefab);
    }

    // Load layout
    this.props = new Dictionary<Type, List<Prop>>();
    this.tiles = new GameObject[this.tileCountI, this.tileCountJ];
    JSONNode layoutJsonNodes = jsonNode["layout"];

    // Tiles
    JSONArray layoutTileNodes = layoutJsonNodes["tiles"].AsArray;
    for (int i = 0; i < this.tileCountI; i++)
    {
      for (int j = 0; j < this.tileCountJ; j++)
      {
        string type = layoutTileNodes[j * this.tileCountI + i]["type"];
        UnityEngine.Object linkage = this.actors[type];

        GameObject tileInstance = Instantiate(linkage) as GameObject;
        Tile tile = tileInstance.GetComponent<Tile>();
        if (tile != null)
        {
          tile.i = i;
          tile.j = j;

          tileInstance.transform.localScale = new Vector4(this.tileWidth, 1, this.tileHeight, 0);
          tileInstance.transform.position = new Vector4(i * this.tileWidth, 0, j * this.tileHeight, 1);
          tileInstance.transform.parent = this.transform;

          this.tiles[i, j] = tileInstance;

			Mesh mesh = tileInstance.transform.FindChild ("AnimationWrap").FindChild("Model").GetComponent<MeshFilter>().mesh;

			Vector2[] uvs = new Vector2[24];
			for(int v = 0; v < 24; v++)
			{
				uvs[v] = new Vector2();
			}

			Vector2 v0 = new Vector2(((float)i+1)/(float)this.tileCountI, ((float)j+1)/(float)this.tileCountJ); // .5, .5
			Vector2 v1 = new Vector2((float)i/(float)this.tileCountI, (float)(j+1)/(float)this.tileCountJ); // -.5, .5
			Vector2 v2 = new Vector2(((float)i+1)/(float)this.tileCountI, (float)j/(float)this.tileCountJ); // .5, -5
			Vector2 v3 = new Vector2((float)i/(float)this.tileCountI, (float)j/(float)this.tileCountJ); // -.5, -.5


			uvs[8] = v0;
			uvs[9] = v1;
			uvs[4] = v2;
			uvs[5] = v3;

			mesh.uv = uvs;
        }
        else
        {
          throw new MissingComponentException("Tile " + linkage + " has no tile component.");
        }
      }
    }

    // Props
    JSONArray layoutPropNodes = layoutJsonNodes["props"].AsArray;
    foreach (JSONNode propData in layoutPropNodes)
    {
      string type = propData["type"];
      UnityEngine.Object linkage = this.actors[type];
      GameObject propInstance = Instantiate(linkage) as GameObject;

      Prop prop = propInstance.GetComponent<Prop>();
      if (prop != null)
      {
        prop.i = propData["i"].AsInt;
        prop.j = propData["j"].AsInt;

        if (prop is Spawn)
        {
          Spawn spawnProp = prop as Spawn;
          spawnProp.player = propData["args"]["players"].AsArray[0];
        }
        else if (prop is Switch)
        {
          Switch switchProp = prop as Switch;
          switchProp.cooldown = propData["args"]["cooldown_ms"].AsDouble;
        }

        Tile tile = getTile(prop.i, prop.j);
       
        //Attach the prop to the AnimationWrap to enable animation
        prop.transform.parent = tile.transform.FindChild("AnimationWrap");
        //prop.transform.parent = tile.transform;

        prop.transform.localPosition = new Vector4(0.0f, tile.transform.localScale.y / 2.0f, 0.0f, 1.0f);

        if (!this.props.ContainsKey(prop.GetType()))
        {
          this.props[prop.GetType()] = new List<Prop>();
        }
        this.props[prop.GetType()].Add(prop);
      }
      else
      {
        throw new MissingComponentException("Prop " + linkage + " has no prop component");
      }
    }

    Vector3Extensions.gridRef = this;
    SetupCamera();
  }

  

  /// <summary>
  /// Sets camera position and orthographic size to encompass board
  /// </summary>
  private void SetupCamera()
  {
    float fullWidth = tileCountI * tileWidth;
    float fullHeight = tileCountJ * tileHeight;
    Camera.main.transform.position = new Vector3(fullWidth * .5f, 10f, fullHeight * .5f) - new Vector3(tileWidth * .5f, 0f, tileHeight * .5f);
    Camera.main.orthographicSize = Mathf.Max(fullWidth, fullHeight) * .5f;
    if (Camera.main.aspect < 1f)
      Camera.main.orthographicSize /= Camera.main.aspect;
  }

  void Update()
  {

  }

  public int getTileCountI()
  {
    return this.tileCountI;
  }

  public int getTileCountJ()
  {
    return this.tileCountJ;
  }

  public int getWidth()
  {
    return this.tileCountI * this.tileWidth;
  }

  public int getHeight()
  {
    return this.tileCountJ * this.tileHeight;
  }

  public int getTileWidth()
  {
    return this.tileWidth;
  }

  public int getTileHeight()
  {
    return this.tileHeight;
  }

  public Tile getTile(int i, int j)
  {
    if (i < 0 || i >= tileCountI || j < 0 || j >= tileCountJ)
      return null;
    else
      return this.tiles[i, j].GetComponent<Tile>();
  }

  public List<Prop> getProp(Type type)
  {
    return this.props[type];
  }
}

public enum TileTypes
{
  TypeA,
  TypeB,
  Neutral
}

namespace CustomExtensions
{
  public static class Vector3Extensions
  {
    public static Grid gridRef;

    public static GridPos GetGridPos(this Vector3 vec3)
    {
      Vector2 precisePos = vec3.GetTilePosPrecise();

      GridPos roundedPos = new GridPos(Mathf.RoundToInt(precisePos.x), Mathf.RoundToInt(precisePos.y));
      return roundedPos;
    }

    public static Vector2 GetTilePosPrecise(this Vector3 vec3)
    {
      float xFloat = (vec3.x / (float)gridRef.getTileWidth());
      float yFloat = (vec3.z / (float)gridRef.getTileHeight());

      return new Vector2(xFloat, yFloat);
    }

    public static Vector2 GetTilePosOffset(this Vector3 vec3)
    {
      Vector2 precisePos = vec3.GetTilePosPrecise();
      Vector2 roundedPos = new Vector2(Mathf.Round(precisePos.x), Mathf.Round(precisePos.y));
      Vector2 offsetPos = new Vector2(precisePos.x - roundedPos.x, precisePos.y - roundedPos.y);
      return offsetPos;
    }
  }
}
