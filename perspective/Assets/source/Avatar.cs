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

  private bool _moveToTileCenter;
  private int _forcedVerticalMovement;
  private int _forcedHorizontalMovement;
  private int _targetX;
  private int _targetY;

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
    TilePos currGridPos = transform.position.GetTilePos();
    Vector2 tilePosOffset = transform.position.GetTilePosOffset();

    transform.position += new Vector3(horizontalMovement, 0f, verticalMovement) * movementSpeed * Time.deltaTime;

    bool horizontalOverstep = false;
    bool verticalOverstep = false;

    currGridPos = transform.position.GetTilePos();
    Vector2 oldTilePosOffset = tilePosOffset;
    tilePosOffset = transform.position.GetTilePosOffset();

    if (_moveToTileCenter)
    {
      Vector3 tileCenter = new Vector3((float)(currGridPos.x * Game.instance.grid.getTileWidth()), 0f, 
                                       (float)(currGridPos.y * Game.instance.grid.getTileHeight()));

      if (Mathf.Sign(oldTilePosOffset.x) != Mathf.Sign(tilePosOffset.x))
      {
        transform.position = new Vector3(tileCenter.x, transform.position.y, transform.position.z);
      }
      if (Mathf.Sign(oldTilePosOffset.y) != Mathf.Sign(tilePosOffset.y))
      {
        transform.position = new Vector3(transform.position.x, transform.position.y, tileCenter.z);
      }

      _moveToTileCenter = false;

      currGridPos = transform.position.GetTilePos();
      tilePosOffset = transform.position.GetTilePosOffset();
    }

    CheckHorizontalMovement(ref horizontalMovement, ref verticalMovement, ref currGridPos, ref tilePosOffset, ref horizontalOverstep);
    CheckVerticalMovement(ref horizontalMovement, ref verticalMovement, ref currGridPos, ref tilePosOffset, ref verticalOverstep);

    if (horizontalOverstep)
    {
      transform.position = new Vector3((float)(currGridPos.x * Game.instance.grid.getTileWidth()), transform.position.y, transform.position.z);
    }
    if (verticalOverstep)
    {
      transform.position = new Vector3(transform.position.x, transform.position.y, (float)(currGridPos.y * Game.instance.grid.getTileHeight()));
    }
    currGridPos = transform.position.GetTilePos();
    Tile currTile = Game.instance.grid.getTile(currGridPos.x, currGridPos.y);

    if (currTile != _occupiedTile)
    {
      ChangeOccupiedTile(currTile);
    }
  }

  private void CheckVerticalMovement(ref int horizontalMovement, ref int verticalMovement, ref TilePos currGridPos, ref Vector2 tilePosOffset, ref bool verticalOverstep)
  {
    if (verticalMovement != 0 && ((verticalMovement == -1 && tilePosOffset.y < 0) || (verticalMovement == 1 && tilePosOffset.y > 0)))
    {
      // Check if direct vertical movement impossible
      Tile directNeighborTile = Game.instance.grid.getTile(currGridPos.x, currGridPos.y + verticalMovement);
      float currX = currGridPos.x;
      float signAdjustment = tilePosOffset.x == 0f ? 0f : Mathf.Sign(tilePosOffset.x);
      float adjustedX = (float)currGridPos.x + (1f * signAdjustment);
      int roundedX = Mathf.RoundToInt(adjustedX);
      Tile otherNeighborTile = Game.instance.grid.getTile(roundedX, currGridPos.y + verticalMovement);

      // direct neighbor accessible, but other not: move back to center
      if (TileWalkable(directNeighborTile) && !TileWalkable(otherNeighborTile))
      {
        verticalOverstep = true;
        if (tilePosOffset.y > 0f && verticalMovement >= 0)
        {
          _targetX = directNeighborTile.i;
        }
      }
      else if (!TileWalkable(directNeighborTile) || !TileWalkable(otherNeighborTile))
      {
        verticalMovement = 0;
        verticalOverstep = true;

        // Check diagonal tiles for accessibility (biasing toward velocity and then position)
        int nearestSide = Mathf.Abs(tilePosOffset.x) > smoothAroundCornerThreshold ? (int)Mathf.Sign(tilePosOffset.x) : 0;
        if (nearestSide != 0)
        {
          directNeighborTile = Game.instance.grid.getTile(currGridPos.x + nearestSide, currGridPos.y + verticalMovement);
          Tile connectionTile = Game.instance.grid.getTile(currGridPos.x + nearestSide, currGridPos.y);
          if (!TileWalkable(directNeighborTile) || !TileWalkable(connectionTile))
          {
            verticalMovement = 0;
          }
          else
          {
            if (nearestSide != (int)Mathf.Sign(tilePosOffset.x) * -1)
            {
              _targetX = directNeighborTile.i;
            }
          }
        }
      }
    }
  }

  private void CheckHorizontalMovement(ref int horizontalMovement, ref int verticalMovement, ref TilePos currGridPos, ref Vector2 tilePosOffset, ref bool horizontalOverstep)
  {
    if (horizontalMovement != 0 && ((horizontalMovement == -1 && tilePosOffset.x < 0) || (horizontalMovement == 1 && tilePosOffset.x > 0)))
    {
      // Check if direct horizontal movement impossible
      Tile directNeighborTile = Game.instance.grid.getTile(currGridPos.x + horizontalMovement, currGridPos.y);
      float currY = currGridPos.y;
      float signAdjustment = tilePosOffset.y == 0f ? 0f : Mathf.Sign(tilePosOffset.y);
      float adjustedY = (float)currGridPos.y + (1f * signAdjustment);
      int roundedY = Mathf.RoundToInt(adjustedY);
      Tile otherNeighborTile = Game.instance.grid.getTile(currGridPos.x + horizontalMovement, roundedY);

      // direct neighbor accessible, but other not: move back to center
      if (TileWalkable(directNeighborTile) && !TileWalkable(otherNeighborTile))
      {
        horizontalOverstep = true;
        if (tilePosOffset.y > 0f && verticalMovement >= 0)
        {
          _targetY = directNeighborTile.j;
        }
      }
      else if (!TileWalkable(directNeighborTile) || !TileWalkable(otherNeighborTile))
      {
        horizontalMovement = 0;
        horizontalOverstep = true;

        // Check diagonal tiles for accessibility (biasing toward velocity and then position)
        int nearestSide = Mathf.Abs(tilePosOffset.y) > smoothAroundCornerThreshold ? (int)Mathf.Sign(tilePosOffset.y) : 0;
        if (nearestSide != 0)
        {
          directNeighborTile = Game.instance.grid.getTile(currGridPos.x + horizontalMovement, currGridPos.y + nearestSide);
          Tile connectionTile = Game.instance.grid.getTile(currGridPos.x, currGridPos.y + nearestSide);
          if (!TileWalkable(directNeighborTile) || !TileWalkable(connectionTile))
          {
            horizontalMovement = 0;
          }
          else
          {
            if (nearestSide != (int)Mathf.Sign(tilePosOffset.y) * -1)
            {
              _targetY = directNeighborTile.j;
            }
          }
        }
      }
    }
  }

  private bool TileWalkable(Tile checkedTile, TileTypes walkableType = TileTypes.Neutral)
  {
    if (walkableType == TileTypes.Neutral)
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

    if (verticalMovement == 0)
    {
      verticalMovement = _forcedVerticalMovement;
      _forcedVerticalMovement = 0;
    }
    if (horizontalMovement == 0)
    {
      horizontalMovement = _forcedHorizontalMovement;
      _forcedHorizontalMovement = 0;
    }

    // Strategy: Check if direct movement possible.  Otherwise, look for indirect movement
    UpdatePosition(horizontalMovement, verticalMovement);
  }

  private void ChangeOccupiedTile(Tile currTile)
  {
    _occupiedTile = currTile;
  }
}