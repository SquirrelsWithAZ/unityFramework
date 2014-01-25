using UnityEngine;
using System.Collections;
using CustomExtensions;

public class Avatar : MonoBehaviour 
{
  public int playerNumber;
  public TileTypes initialLayer;

  public float movementSpeed;
  public float smoothAroundCornerThreshold = .3f;

  private iInputManager _inputManager;
  private Tile _occupiedTile;
  private TileTypes _currentType;

  public void Start()
  {
    _inputManager = Game.instance.inputManager._PlayerInputManagers[playerNumber];
    gameObject.layer = Tile.GetPhysicsLayerFromType(initialLayer);
    _currentType = initialLayer;
  }

  public void Update()
  {
    ProcessInput();
  }

  private void UpdatePosition(int horizontalMovement, int verticalMovement)
  {
    Vector3 newPos = transform.position + new Vector3(horizontalMovement, 0f, verticalMovement) * movementSpeed * Time.deltaTime;

    TilePos currGridPos = newPos.GetTilePos();
    Vector2 tilePosOffset = newPos.GetTilePosOffset();
    if (horizontalMovement != 0 && ((horizontalMovement == -1 && tilePosOffset.x < 0) || (horizontalMovement == 1 && tilePosOffset.x > 0)))
    {
      // Check if direct horizontal movement impossible
      Tile neighborTile = Game.instance.grid.getTile(currGridPos.x + horizontalMovement, currGridPos.y);
      if (!TileWalkable(neighborTile))
      {
        Debug.Log("Attempt to move to adjacent tile " + (currGridPos.x + horizontalMovement) + ", " + currGridPos.y + " could not be completed");
        // Check diagonal tiles for accessibility (biasing toward velocity and then position)
        int nearestSide = Mathf.Abs(tilePosOffset.x) > smoothAroundCornerThreshold ? (int)Mathf.Sign(tilePosOffset.x) : 0;
        if (nearestSide != 0)
        {
          neighborTile = Game.instance.grid.getTile(currGridPos.x + horizontalMovement, currGridPos.y + nearestSide);
          Tile connectionTile = Game.instance.grid.getTile(currGridPos.x, currGridPos.y + nearestSide);
          if (!TileWalkable(neighborTile) || !TileWalkable(connectionTile))
          {
            horizontalMovement = 0;
          }
        }
      }
    }

    if (verticalMovement != 0 && ((verticalMovement == -1 && tilePosOffset.y < 0) || (verticalMovement == 1 && tilePosOffset.y > 0)))
    {
      // Check if direct vertical movement impossible
      Tile neighborTile = Game.instance.grid.getTile(currGridPos.x, currGridPos.y + verticalMovement);
      if (!TileWalkable(neighborTile))
      {
        Debug.Log("Attempt to move to adjacent tile " + currGridPos.x + ", " + (currGridPos.y + verticalMovement) + " could not be completed");
        // Check diagonal tiles for accessibility (biasing toward velocity and then position)
        int nearestSide = Mathf.Abs(tilePosOffset.y) > smoothAroundCornerThreshold ? (int)Mathf.Sign(tilePosOffset.y) : 0;
        if (nearestSide != 0)
        {
          neighborTile = Game.instance.grid.getTile(currGridPos.x + nearestSide, currGridPos.y + verticalMovement);
          Tile connectionTile = Game.instance.grid.getTile(currGridPos.x + nearestSide, currGridPos.y);
          if (!TileWalkable(neighborTile) || !TileWalkable(connectionTile))
          {
            verticalMovement = 0;
          }
        }
      }
    }

    transform.position += new Vector3(horizontalMovement, 0f, verticalMovement) * movementSpeed * Time.deltaTime;
    currGridPos = transform.position.GetTilePos();
    Tile currTile = Game.instance.grid.getTile(currGridPos.x, currGridPos.y);

    if (currTile != _occupiedTile)
    {
      ChangeOccupiedTile(currTile);
    }
  }

  private bool TileWalkable(Tile checkedTile, TileTypes walkableType = TileTypes.Neutral)
  {
    if(walkableType == TileTypes.Neutral)
      walkableType = _currentType;

    return checkedTile != null && (checkedTile.gameObject.layer == Tile.GetPhysicsLayerFromType(walkableType) || checkedTile.gameObject.layer == Tile.GetPhysicsLayerFromType(TileTypes.Neutral));
  }

  private void ProcessInput()
  {
    // Movement
    int verticalMovement = 0;
    int horizontalMovement = 0;

    if (_inputManager.CheckInput(InputActions.Up).isActive)
      verticalMovement += 1;
    else if (_inputManager.CheckInput(InputActions.Down).isActive)
      verticalMovement -= 1;

    if (_inputManager.CheckInput(InputActions.Right).isActive)
      horizontalMovement += 1;
    else if (_inputManager.CheckInput(InputActions.Left).isActive)
      horizontalMovement -= 1;

    // Strategy: Check if direct movement possible.  Otherwise, look for indirect movement
    UpdatePosition(horizontalMovement, verticalMovement);
  }

  private void ChangeOccupiedTile(Tile currTile)
  {
    _occupiedTile = currTile;
  }
}