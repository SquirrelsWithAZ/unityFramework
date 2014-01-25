using UnityEngine;
using System.Collections;

public interface IGrid
{
	int getTileCountI();
	int getTileCountJ();
	int getWidth();
	int getHeight();
	Tile getTile(int i, int j);
}
