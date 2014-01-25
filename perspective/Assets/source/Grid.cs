using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;
using System;
using CustomExtensions;

public class Grid : MonoBehaviour
{
	public string levelDefinition;

	/* levelDefinition
	{
	    "tileCountI" : 2,
	    "tileCountJ" : 2,
	    "tileWidth" : 3,
	    "tileHeight" : 3,

	    "layout" : [
	        {"prefab" : "Tile_Type_A"},
	        {"prefab" : "Tile_Type_A"},
	        {"prefab" : "Tile_Type_A"},
	        {"prefab" : "Tile_Type_B"}
	    ]
	}
	*/

	private int tileCountI;
	private int tileCountJ;
	private int tileWidth;
	private int tileHeight;
	private GameObject[,] tiles; 

	void Awake() 
	{
		JSONNode jsonNode = JSON.Parse(this.levelDefinition);
		this.tileCountI = jsonNode["tileCountI"].AsInt;
		this.tileCountJ = jsonNode["tileCountJ"].AsInt;
		this.tileWidth = jsonNode["tileWidth"].AsInt;
		this.tileHeight = jsonNode["tileHeight"].AsInt;

		this.tiles = new GameObject[this.tileCountI, this.tileCountJ];

		JSONArray layoutJsonNodes = jsonNode["layout"].AsArray;
		for(int i = 0; i < this.tileCountI; i++)
		{
			for(int j = 0; j < this.tileCountJ; j++)
			{
				string linkage = layoutJsonNodes[j*this.tileCountI+i]["prefab"];
				

				GameObject tileInstance = Instantiate(Resources.Load(linkage)) as GameObject;
				Tile tile = tileInstance.GetComponent<Tile>();
				if(tile != null)
				{
					tile.i = i;
					tile.j = j;

					tileInstance.transform.localScale = new Vector4(this.tileWidth, 1, this.tileHeight, 0);
					tileInstance.transform.position = new Vector4(i * this.tileWidth, 0, j * this.tileHeight, 1);
					tileInstance.transform.parent = this.transform;

					//tag each tile with the type based on JSON parameter
					try
					{
						tileInstance.tag = linkage;
					}
					catch
					{
						Debug.LogError("Attempted to apply a non-existant tag. either check the JSON object or add this tag to inspector: " + linkage);
					}

					this.tiles[i,j] = tileInstance;
				}
				else
				{
					throw new MissingComponentException("Tile prefab " + linkage +  " has no tile component.");
				}
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

	public Tile getTile(int i, int j)
	{
		return this.tiles[i, j].GetComponent<Tile>();
	}
}

public enum TileTypes
{
  TypeA,
  TypeB,
  Neutral
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
    public static Grid gridRef;

    public static TilePos GetTilePos(this Vector3 vec3)
    {
      float xFloat = vec3.x / (float)gridRef.getWidth();
      float yFloat = vec3.z / (float)gridRef.getHeight();

      //Debug.Log("Getting tile coordinate for position " + xFloat + ", " + yFloat);
      TilePos newPos = new TilePos(Mathf.RoundToInt(xFloat + .25f), Mathf.RoundToInt(yFloat + .25f));
      //Debug.Log("Adjusted tile pos is (" + newPos.x + ", " + newPos.y + ")");
      return newPos;
    }
  }
}
