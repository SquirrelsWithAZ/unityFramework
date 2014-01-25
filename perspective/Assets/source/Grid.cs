using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;
using System;

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
