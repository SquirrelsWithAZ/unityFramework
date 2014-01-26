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

  private Vector3 _currentVelocity;
  private Vector3 _targetVelocity;

  public void Start()
  {
    _inputManager = Game.instance.inputManager._PlayerInputManagers[playerNumber];
    gameObject.layer = Tile.GetPhysicsLayerFromType(initialLayer);
    _currentType = initialLayer;
  }

  public void Update()
  {
    ProcessInput();
    MoveByVelocity(GetCurrentVelocity());
  }

  private void MoveByVelocity(Vector3 currentVelocity, bool useRemainder = true)
  {
    Vector3 avatarPos = transform.position;
    GridPos currentGridPos = avatarPos.GetGridPos();
    Vector2 localTileOffset = avatarPos.GetTilePosOffset();
    float velocityRemainder = 0f;
    Tile pivotTile = null;

    // current velocity is horizontal
    if (currentVelocity.x != 0)
    {
      // movement right
      if (currentVelocity.x > 0)
      {
        // right of center (movement right)
        if (localTileOffset.x > 0)
        {
          pivotTile = Game.instance.grid.getTile(currentGridPos.x + 1, currentGridPos.y);
        }
        // at center or left of it (movement right)
        else
        {
          pivotTile = Game.instance.grid.getTile(currentGridPos.x, currentGridPos.y);
        }
      }
      // movement left
      else
      {
        // right of center (movement left)
        if (localTileOffset.x < 0)
        {
          pivotTile = Game.instance.grid.getTile(currentGridPos.x - 1, currentGridPos.y);
        }
        // at center or left of it (movement right)
        else
        {
          pivotTile = Game.instance.grid.getTile(currentGridPos.x, currentGridPos.y);
        }
      }

      // Get position diff between avatar to tile and avatar to moved-pos
      Vector3 movedPos = transform.position + currentVelocity;
      float movementDelta = (movedPos - transform.position).magnitude;
      float centerTileDelta = (pivotTile.GetWorldPosition() - transform.position).magnitude;

      // If moving past next tile center
      if (movementDelta > centerTileDelta)
      {
        transform.position = new Vector3(pivotTile.GetWorldPosition().x, transform.position.y, transform.position.z);
        velocityRemainder = movementDelta - centerTileDelta;

        // Check for desired movement
        if (useRemainder)
        {
          Vector3 desiredVelocity = GetTargetVelocity();
          if (desiredVelocity.y != 0)
          {
            _velocityDir = VelocityDir.Vertical;
            _targetVelocity = _currentVelocity;
            _currentVelocity = desiredVelocity.normalized;
            MoveByVelocity(desiredVelocity.normalized * velocityRemainder, false);
          }
        }
      }
      // If not, just move
      else
      {
        transform.position = movedPos;
      }
    }
    // current velocity is vertical
    else if (currentVelocity.y != 0)
    {
      // movement right
      if (currentVelocity.y > 0)
      {
        // right of center (movement right)
        if (localTileOffset.y > 0)
        {
          pivotTile = Game.instance.grid.getTile(currentGridPos.y, currentGridPos.y + 1);
        }
        // at center or left of it (movement right)
        else
        {
          pivotTile = Game.instance.grid.getTile(currentGridPos.x, currentGridPos.y);
        }
      }
      // movement left
      else
      {
        // right of center (movement left)
        if (localTileOffset.x < 0)
        {
          pivotTile = Game.instance.grid.getTile(currentGridPos.y, currentGridPos.y - 1);
        }
        // at center or left of it (movement right)
        else
        {
          pivotTile = Game.instance.grid.getTile(currentGridPos.x, currentGridPos.y);
        }
      }

      // Get position diff between avatar to tile and avatar to moved-pos
      Vector3 movedPos = transform.position + currentVelocity;
      float movementDelta = (movedPos - transform.position).magnitude;
      float centerTileDelta = (pivotTile.GetWorldPosition() - transform.position).magnitude;

      // If moving past next tile center
      if (movementDelta > centerTileDelta)
      {
        transform.position = new Vector3(transform.position.x, transform.position.y, pivotTile.GetWorldPosition().z);
        velocityRemainder = movementDelta - centerTileDelta;

        // Check for desired movement
        if (useRemainder)
        {
          Vector3 desiredVelocity = GetTargetVelocity();
          if (desiredVelocity.x != 0)
          {
            _velocityDir = VelocityDir.Horizontal;
            _targetVelocity = _currentVelocity;
            _currentVelocity = desiredVelocity.normalized;
            MoveByVelocity(desiredVelocity.normalized * velocityRemainder, false);
          }
        }
      }
      // If not, just move
      else
      {
        transform.position = movedPos;
      }
    }
  }

  private VelocityDir _velocityDir;

  private enum VelocityDir
  {
    Vertical,
    Horizontal,
    Static
  }

  private bool TileWalkable(Tile checkedTile, TileTypes walkableType = TileTypes.Neutral)
  {
    if (walkableType == TileTypes.Neutral)
      walkableType = _currentType;

    return checkedTile != null && (checkedTile.gameObject.layer == Tile.GetPhysicsLayerFromType(walkableType) || checkedTile.gameObject.layer == Tile.GetPhysicsLayerFromType(TileTypes.Neutral));
  }

  private void ProcessInput()
  {
    // Trigger Input
    if (_inputManager.CheckInput(InputActions.Up).isTriggered)
    {
      if (_velocityDir != VelocityDir.Horizontal)
      {
        _currentVelocity = new Vector3(0f, 0f, 1f);
        _velocityDir = VelocityDir.Vertical;
      }
      else
        _targetVelocity = new Vector3(0f, 0f, 1f);
    }
    else if (_inputManager.CheckInput(InputActions.Down).isTriggered)
    {
      if (_velocityDir != VelocityDir.Horizontal)
      {
        _currentVelocity = new Vector3(0f, 0f, -1f);
        _velocityDir = VelocityDir.Vertical;
      }
      else
        _targetVelocity = new Vector3(0f, 0f, -1f);
    }

    if (_inputManager.CheckInput(InputActions.Right).isTriggered)
    {
      if (_velocityDir != VelocityDir.Vertical)
      {
        _currentVelocity = new Vector3(0f, 0f, 1f);
        _velocityDir = VelocityDir.Horizontal;
      }
      else
        _targetVelocity = new Vector3(0f, 0f, 1f);
    }
    else if (_inputManager.CheckInput(InputActions.Left).isTriggered)
    {
      if (_velocityDir != VelocityDir.Vertical)
      {
        _currentVelocity = new Vector3(0f, 0f, -1f);
        _velocityDir = VelocityDir.Horizontal;
      }
      else
        _targetVelocity = new Vector3(0f, 0f, -1f);
    }

    // Release Input
    if (_inputManager.CheckInput(InputActions.Up).isReleased)
    {
      if (_velocityDir != VelocityDir.Horizontal)
      {
        _currentVelocity = new Vector3(0f, 0f, 0f);
        _velocityDir = VelocityDir.Static;
      }
      else
        _targetVelocity = new Vector3(0f, 0f, 0f);
    }
    else if (_inputManager.CheckInput(InputActions.Down).isReleased)
    {
      if (_velocityDir != VelocityDir.Horizontal)
      {
        _currentVelocity = new Vector3(0f, 0f, 0f);
        _velocityDir = VelocityDir.Static;
      }
      else
        _targetVelocity = new Vector3(0f, 0f, 0f);
    }

    if (_inputManager.CheckInput(InputActions.Right).isReleased)
    {
      if (_velocityDir != VelocityDir.Vertical)
      {
        _currentVelocity = new Vector3(0f, 0f, 0f);
        _velocityDir = VelocityDir.Static;
      }
      else
        _targetVelocity = new Vector3(0f, 0f, 0f);
    }
    else if (_inputManager.CheckInput(InputActions.Left).isReleased)
    {
      if (_velocityDir != VelocityDir.Vertical)
      {
        _currentVelocity = new Vector3(0f, 0f, 0f);
        _velocityDir = VelocityDir.Static;
      }
      else
        _targetVelocity = new Vector3(0f, 0f, 0f);
    }
  }

  Vector3 GetCurrentVelocity()
  {
    return _currentVelocity * movementSpeed * Time.deltaTime;
  }

  Vector3 GetTargetVelocity()
  {
    return _targetVelocity * movementSpeed * Time.deltaTime;
  }

  private void ChangeOccupiedTile(Tile currTile)
  {
    _occupiedTile = currTile;
  }
}