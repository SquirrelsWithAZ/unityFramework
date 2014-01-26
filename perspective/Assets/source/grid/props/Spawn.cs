using UnityEngine;
using System.Collections;

public class Spawn : Prop
{
    public string player;

    public TileTypes GoalForPlayerType
    {
        get { return player == "PlayerA" ? TileTypes.TypeB : TileTypes.TypeA; }
    }
}
