using UnityEngine;
using System.Collections;
using CustomExtensions;

public class ClampToGrid : MonoBehaviour {

    public float dy;

    // Update is called once per frame
    void Update () {

        GridPos currentGridPos = transform.position.GetGridPos();

        Tile t = Game.instance.grid.getTile(currentGridPos.x, currentGridPos.y);
        GameObject baseObject = t.ActiveModel;

        transform.position = new Vector3(
            transform.position.x,
            baseObject.transform.position.y + dy + 0.5f * t.transform.localScale.y,
            transform.position.z
        );
    }
}
