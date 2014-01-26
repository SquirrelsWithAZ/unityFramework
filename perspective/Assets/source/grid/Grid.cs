using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;
using System;
using CustomExtensions;

public class Grid : MonoBehaviour
{
  public string levelDefinition;
  public GameObject playerAPrefab;
  public GameObject playerBPrefab;

  //grabbing these for animation calculations
  public int tileCountI;
  public int tileCountJ;
  private int tileWidth;
  private int tileHeight;

  private IDictionary<string, UnityEngine.Object> actors;
  private IDictionary<Type, List<Prop>> props;
  private GameObject[,] tiles;
  public List<Avatar> players { get; private set; }

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

          tileInstance.transform.localScale = new Vector4(this.tileWidth, this.tileWidth, this.tileHeight, 0);
          tileInstance.transform.position = new Vector4(i * this.tileWidth, 0, j * this.tileHeight, 1);
          tileInstance.transform.parent = this.transform;

          this.tiles[i, j] = tileInstance;

          // HACK: Tiles have either a single models (neutral tiles) or double models.
          bool singleModel = tileInstance.transform.FindChild("AnimationWrap").FindChild("Model") != null;
          if(singleModel)
          {
            Transform model = tileInstance.transform.FindChild("AnimationWrap").FindChild("Model");
            Mesh mesh = model.GetComponent<MeshFilter>().mesh;
            applyGlobalGridUVs(i, j, mesh);
          }
          else
          {
            for(int subModelIndex = 0; subModelIndex < 2; subModelIndex++)
            {
              string subModel = subModelIndex == 0 ? "A" : "B";
              
              Transform model = tileInstance.transform.FindChild("AnimationWrap").FindChild("Model_" + subModel);
              Mesh mesh = model.GetComponent<MeshFilter>().mesh;
              applyGlobalGridUVs(i, j, mesh);
            }
          }
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

        prop.transform.localPosition = new Vector4(0.0f, 1.0f / 2.0f, 0.0f, 1.0f);

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

    // Spawn players
    List<Prop> spawns = this.props[typeof(Spawn)];

    int playerNumber = 1;
    this.players = new List<Avatar>();

    if(spawns != null)
        foreach (Prop spawnProp in spawns)
        {
          Spawn spawn = spawnProp as Spawn;
          if (spawn == null) throw new System.InvalidCastException();

          Tile t = this.getTile(spawn.i, spawn.j);

          // New player
          GameObject spawnPrefab = null;
          switch (spawn.player)
          {
            case "PlayerA":
              spawnPrefab = playerAPrefab;
              break;
            case "PlayerB":
              spawnPrefab = playerBPrefab;
              break;
          }
          GameObject player = GameObject.Instantiate(
            spawnPrefab, 
            t.transform.position, 
            this.transform.rotation
          ) as GameObject;
          if (player == null) throw new System.InvalidOperationException();
          player.transform.localScale = new Vector3(
            this.getTileWidth() * player.transform.localScale.x,
            player.transform.localScale.y,
            this.getTileHeight() * player.transform.localScale.z
          );

          player.transform.parent = this.transform;
          playerNumber++;

          Avatar a = player.GetComponent<Avatar>();

          //a.playerNumber = playerNumber++;
          //a.initialLayer = (spawn.player == "a") ? TileTypes.TypeA : TileTypes.TypeB;

          if (a == null) throw new System.InvalidOperationException();
          this.players.Add(a);
        }

    // Finish by setting up the camera.
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

  private float CalculateCameraZoomFromBounds(Bounds targetBounds, Camera cam)
  {
    RadianFoV fov = new RadianFoV(cam.fieldOfView, cam.aspect);

    float xBound = targetBounds.extents.x / Mathf.Tan(fov._vertFoV);
    float zBound = targetBounds.extents.z / Mathf.Tan(fov._horizFoV);

    return Mathf.Max(xBound, zBound);
  }

  private struct RadianFoV
  {
    public float _horizFoV;
    public float _vertFoV;

    public RadianFoV(float fov, float aspect)
    {
      _horizFoV = fov * Mathf.Deg2Rad * 0.5f;
      _vertFoV = Mathf.Atan(Mathf.Tan(_horizFoV) * aspect);
    }
  }

  void Update()
  {
    
  }

  public void swapTileVisuals(int i, int j)
  {
    //for(int i = 0; i < this.tileCountI; i++)
    //{
      //for(int j = 0; j < this.tileCountJ; j++)
      //{
        GameObject tile = this.tiles[i, j];

        bool multiModel = tile.transform.FindChild("AnimationWrap").FindChild("Model") == null;
        if(multiModel)
        {
          GameObject modelA = tile.transform.FindChild ("AnimationWrap").FindChild("Model_A").gameObject;
          GameObject modelB = tile.transform.FindChild ("AnimationWrap").FindChild("Model_B").gameObject;
          modelA.SetActive(!modelA.activeSelf);
          modelB.SetActive(!modelB.activeSelf);
        }
      //}
    //}
  }

  public void swapTileState(int i, int j)
  {
    /*
    for(int i = 0; i < this.tileCountI; i++)
    {
      for(int j = 0; j < this.tileCountJ; j++)
      {
        */
        GameObject tile = this.tiles[i, j];

        bool multiModel = tile.transform.FindChild("AnimationWrap").FindChild("Model") == null;
		    if(multiModel)
        {
          if(tile.layer == Tile.GetPhysicsLayerFromType(TileTypes.TypeA))
          {
            tile.layer = Tile.GetPhysicsLayerFromType(TileTypes.TypeB);
          }
          else
          {
            tile.layer = Tile.GetPhysicsLayerFromType(TileTypes.TypeA);
          }
        }
      //}
    //}
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

  private void applyGlobalGridUVs(int i, int j, Mesh mesh)
  {
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

    uvs[12] = v0;
    uvs[14] = v1;
    uvs[15] = v2;
    uvs[13] = v3;

    mesh.uv = uvs;
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
    public static GridPos GetGridPos(this Vector3 vec3)
    {
      Vector2 precisePos = vec3.GetTilePosPrecise();

      GridPos roundedPos = new GridPos(Mathf.RoundToInt(precisePos.x), Mathf.RoundToInt(precisePos.y));
      return roundedPos;
    }

    public static Vector2 GetTilePosPrecise(this Vector3 vec3)
    {
      float xFloat = (vec3.x / (float)Game.instance.grid.getTileWidth());
      float yFloat = (vec3.z / (float)Game.instance.grid.getTileHeight());

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
