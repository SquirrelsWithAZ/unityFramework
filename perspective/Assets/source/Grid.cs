using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

public class Grid : MonoBehaviour
{
	public string levelDefinition;

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

				if(tileInstance.transform.GetComponent<Tile>() != null)
				{
					tileInstance.transform.localScale = new Vector4(this.tileWidth, 1, this.tileHeight, 0);
					tileInstance.transform.position = new Vector4(i * this.tileWidth, 0, j * this.tileHeight, 1);
					tileInstance.transform.parent = this.transform;

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
