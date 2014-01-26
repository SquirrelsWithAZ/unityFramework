using UnityEngine;
using System.Collections;
using CustomExtensions;

public class Avatar : MonoBehaviour
{
  public GridPos currentGridPos;

  public int playerNumber;
  public TileTypes initialLayer;

  public float movementSpeed;
  public float smoothAroundCornerThreshold = .3f;

  private iInputManager _inputManager;
  private Tile _occupiedTile;
  private TileTypes _currentType;

  public Vector3 _currentVelocity;
  public Vector3 _targetVelocity;
  private bool _lockOutDirectionChange;
  private bool _compedVelocity;

  private float unstableDurationS;

  public void Start()
  {
    _inputManager = Game.instance.inputManager._PlayerInputManagers[playerNumber];
    gameObject.layer = Tile.GetPhysicsLayerFromType(initialLayer);
    _currentType = initialLayer;
    GridPos currentGridPos = transform.position.GetGridPos();
    ChangeOccupiedTile(Game.instance.grid.getTile(currentGridPos.x, currentGridPos.y));

    this.unstableDurationS = 0.0f;
  }

  public void FixedUpdate()
  {
    ProcessInput();
    if (_targetVelocity.sqrMagnitude == 0f)
    {
      if (_compedVelocity)
      {
        _currentVelocity = Vector3.zero;
        _compedVelocity = false;
        _velocityDir = VelocityDir.Static;
      }
    }
    MoveByVelocity(GetCurrentVelocity());
  }

  public void Update()
  {
    ProcessInput();

    Tile tile = Game.instance.grid.getTile(currentGridPos.x, currentGridPos.y);
    if(tile.gameObject.layer != this.gameObject.layer &&
      tile.gameObject.layer != Tile.GetPhysicsLayerFromType(TileTypes.Neutral))
    {
      HarlemShake(65, 0.05f);
      this.unstableDurationS += Time.deltaTime;
      MonoBehaviour.print(this.unstableDurationS);
    }
    else
    {
      Transform modelTransform = this.transform.FindChild("Player");
      modelTransform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

      this.unstableDurationS = 0.0f;
    }
  }

  public void HarlemShake(float intensity, float range)
  {
      Transform modelTransform = this.transform.FindChild("Player");
      modelTransform.localPosition = new Vector3(
          Mathf.Sin(Time.time*intensity) * range, 
          0.0f,
          Mathf.Cos(Time.time*(intensity-0.005f)) * range);
  }

  private void MoveByVelocity(Vector3 currentVelocity, bool useRemainder = true)
  {
    //Debug.Log("trying to move with velocity " + currentVelocity);
    Vector3 avatarPos = transform.position;
    currentGridPos = avatarPos.GetGridPos();
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
      float movementDelta = Mathf.Abs(currentVelocity.x);
      float centerTileDelta = Mathf.Abs(pivotTile.GetWorldPosition().x - transform.position.x);

      //Debug.Log("Movement delta is " + movementDelta + ", Center delta " + centerTileDelta);
      // If moving past next tile center
      if (movementDelta > centerTileDelta)
      {
        SetPosition(new Vector3(pivotTile.GetWorldPosition().x, transform.position.y, transform.position.z));
        velocityRemainder = movementDelta - centerTileDelta;

        // Check for desired movement
        if (useRemainder)
        {
          Vector3 desiredVelocity = GetTargetVelocity();
          if (desiredVelocity.z != 0 && !_lockOutDirectionChange)
          {
            Tile tileAhead = Game.instance.grid.getTile(pivotTile.i, pivotTile.j + (int)Mathf.Sign(desiredVelocity.z));
            if (TileWalkable(tileAhead))
            {
              _lockOutDirectionChange = true;
              _velocityDir = VelocityDir.Vertical;
              if (!_compedVelocity)
                _targetVelocity = _currentVelocity;
              else
                _targetVelocity = Vector3.zero;
              _compedVelocity = false;

              _currentVelocity = desiredVelocity.normalized;
              MoveByVelocity(desiredVelocity.normalized * velocityRemainder, false);
            }
            else
            {
              tileAhead = Game.instance.grid.getTile(pivotTile.i + (int)Mathf.Sign(currentVelocity.x), pivotTile.j);
              Tile otherTileAhead = Game.instance.grid.getTile(pivotTile.i + (int)Mathf.Sign(currentVelocity.x), pivotTile.j + (int)Mathf.Sign(localTileOffset.y));
              if (TileWalkable(tileAhead) && (localTileOffset.y == 0 || TileWalkable(otherTileAhead)))
              {
                SetPosition(movedPos);
              }
            }
          }
          else
          {
            Tile tileAhead = Game.instance.grid.getTile(pivotTile.i + (int)Mathf.Sign(currentVelocity.x), pivotTile.j);
            Tile otherTileAhead = Game.instance.grid.getTile(pivotTile.i + (int)Mathf.Sign(currentVelocity.x), pivotTile.j + (int)Mathf.Sign(localTileOffset.y));
            if (TileWalkable(tileAhead) && (localTileOffset.y == 0 || TileWalkable(otherTileAhead)))
            {
              SetPosition(movedPos);
            }
            else if(!_compedVelocity)
            {
              //Debug.Log("Cannot move forward without smoothing");
              localTileOffset = transform.position.GetTilePosOffset();

              // Can move forward from major tile
              if (TileWalkable(tileAhead))
              {
                //Debug.Log("Can move ahead if smoothed back to center");
                // determine if shift past smoothing threshold
                if (true)//Mathf.Abs(localTileOffset.y) < (.5f - smoothAroundCornerThreshold))
                {
                  //Debug.Log("Smoothing " + (localTileOffset.y > 0 ? "down" : "up"));
                  // Set velocity in direction of offset
                  _targetVelocity = _currentVelocity;
                  _velocityDir = VelocityDir.Vertical;
                  _currentVelocity = new Vector3(0f, 0f, -Mathf.Sign(localTileOffset.y));
                  _compedVelocity = true;
                }
              }
              // Can move forward from minor tile
              else if (TileWalkable(otherTileAhead))
              {
                //Debug.Log("Can move ahead if smoothed to next tile over");
                Tile connectionTile = Game.instance.grid.getTile(pivotTile.i, otherTileAhead.j);
                // determine if shift past smoothing threshold
                //if (TileWalkable(connectionTile))
                if (Mathf.Abs(localTileOffset.y) > smoothAroundCornerThreshold && TileWalkable(connectionTile))
                {
                  //Debug.Log("Smoothing " + (localTileOffset.y > 0 ? "up" : "down"));
                  // Set velocity in direction of offset
                  _targetVelocity = _currentVelocity;
                  _velocityDir = VelocityDir.Vertical;
                  _currentVelocity = new Vector3(0f, 0f, Mathf.Sign(localTileOffset.y));
                  _compedVelocity = true;
                }
              }
            }
          }
        }
        else
        {
          SetPosition(movedPos);
        }
      }
      // If not, just move
      else
      {
        Tile tileAhead = Game.instance.grid.getTile(pivotTile.i, pivotTile.j);
        if (TileWalkable(tileAhead))
        {
          SetPosition(movedPos);
        }
      }
    }
    // current velocity is vertical
    else if (currentVelocity.z != 0)
    {
      // movement north
      if (currentVelocity.z > 0)
      {
        // north of center (movement north)
        if (localTileOffset.y > 0)
        {
          pivotTile = Game.instance.grid.getTile(currentGridPos.x, currentGridPos.y + 1);
        }
        // at center or south of it (movement north)
        else
        {
          pivotTile = Game.instance.grid.getTile(currentGridPos.x, currentGridPos.y);
        }
      }
      // movement south
      else
      {
        // south of center (movement south)
        if (localTileOffset.y < 0)
        {
          pivotTile = Game.instance.grid.getTile(currentGridPos.x, currentGridPos.y - 1);
        }
        // at center or north of it (movement south)
        else
        {
          pivotTile = Game.instance.grid.getTile(currentGridPos.x, currentGridPos.y);
        }
      }
      // Get position diff between avatar to tile and avatar to moved-pos
      Vector3 movedPos = transform.position + currentVelocity;
      float movementDelta = Mathf.Abs(currentVelocity.z);
      float centerTileDelta = Mathf.Abs(pivotTile.GetWorldPosition().z - transform.position.z);

      // If moving past next tile center
      if (movementDelta > centerTileDelta)
      {
        SetPosition(new Vector3(transform.position.x, transform.position.y, pivotTile.GetWorldPosition().z));
        velocityRemainder = movementDelta - centerTileDelta;

        // use velocity remainder
        if (useRemainder)
        {
          Vector3 desiredVelocity = GetTargetVelocity();
          // Check for desired movement
          if (desiredVelocity.x != 0 && !_lockOutDirectionChange)
          {
            // Test walkable in direction of desired
            Tile tileAhead = Game.instance.grid.getTile(pivotTile.i + (int)Mathf.Sign(desiredVelocity.x), pivotTile.j);
            if (TileWalkable(tileAhead))
            {
              _lockOutDirectionChange = true;
              _velocityDir = VelocityDir.Horizontal;

              if (!_compedVelocity)
                _targetVelocity = _currentVelocity;
              else
                _targetVelocity = Vector3.zero;
              _compedVelocity = false;

              _currentVelocity = desiredVelocity.normalized;
              MoveByVelocity(desiredVelocity.normalized * velocityRemainder, false);
            }
            // if not, check if walkable in direction of current
            else
            {
              tileAhead = Game.instance.grid.getTile(pivotTile.i, pivotTile.j + (int)Mathf.Sign(currentVelocity.z));
              Tile otherTileAhead = Game.instance.grid.getTile(pivotTile.i + (int)Mathf.Sign(localTileOffset.x), pivotTile.j + (int)Mathf.Sign(currentVelocity.z));
              if (TileWalkable(tileAhead) && (localTileOffset.x == 0 || TileWalkable(otherTileAhead)))
              {
                SetPosition(movedPos);
              }
            }
          }
          // No desired, check if walkable in direction of current
          else
          {
            Tile tileAhead = Game.instance.grid.getTile(pivotTile.i, pivotTile.j + (int)Mathf.Sign(currentVelocity.z));
            Tile otherTileAhead = Game.instance.grid.getTile(pivotTile.i + (int)Mathf.Sign(localTileOffset.x), pivotTile.j + (int)Mathf.Sign(currentVelocity.z));
            // If walkable
            if (TileWalkable(tileAhead) && (localTileOffset.x == 0 || TileWalkable(otherTileAhead)))
            {
              SetPosition(movedPos);
            }
            // Check for corner smoothing
            else if(!_compedVelocity)
            {
              //Debug.Log("Cannot move forward without smoothing");
              localTileOffset = transform.position.GetTilePosOffset();

              // Can move forward from major tile
              if (TileWalkable(tileAhead))
              {
                //Debug.Log("Can move ahead if smoothed back to center");
                // determine if shift past smoothing threshold
                if (true)//Mathf.Abs(localTileOffset.x) < (.5f - smoothAroundCornerThreshold))
                {
                  //Debug.Log("Smoothing " + (localTileOffset.x > 0 ? "left" : "right"));
                  // Set velocity in direction of offset
                  _targetVelocity = _currentVelocity;
                  _velocityDir = VelocityDir.Horizontal;
                  _currentVelocity = new Vector3(-Mathf.Sign(localTileOffset.x), 0f, 0f);
                  _compedVelocity = true;
                }
              }
              // Can move forward from minor tile
              else if (TileWalkable(otherTileAhead))
              {
                //Debug.Log("Can move ahead if smoothed to next tile over");
                Tile connectionTile = Game.instance.grid.getTile(otherTileAhead.i, pivotTile.j);
                // determine if shift past smoothing threshold
                if (Mathf.Abs(localTileOffset.x) > smoothAroundCornerThreshold && TileWalkable(connectionTile))
                {
                  //Debug.Log("Smoothing " + (localTileOffset.x > 0 ? "right" : "left"));
                  // Set velocity in direction of offset
                  _targetVelocity = _currentVelocity;
                  _velocityDir = VelocityDir.Horizontal;
                  _currentVelocity = new Vector3(Mathf.Sign(localTileOffset.x), 0f, 0f);
                  _compedVelocity = true;
                }
              }
            }
          }
        }
      }
      // If not, just move
      else
      {
        Tile tileAhead = Game.instance.grid.getTile(pivotTile.i, pivotTile.j);

        // If walkable
        if (TileWalkable(tileAhead))
        {
          SetPosition(movedPos);
        }
      }
    }

    GridPos newGridPos = transform.position.GetGridPos();
    if (newGridPos.x != currentGridPos.x || newGridPos.y != currentGridPos.y)
    {
      ChangeOccupiedTile(Game.instance.grid.getTile(newGridPos.x, newGridPos.y));
    }
  }

  private void SetPosition(Vector3 newPos)
  {
    //Debug.LogError(string.Format("Setting position to {0:0.0000},{1:0.0000},{2:0.0000}", newPos.x, newPos.y, newPos.z));
    transform.position = newPos;
  }

  public VelocityDir _velocityDir;

  public enum VelocityDir
  {
    Static,
    Vertical,
    Horizontal
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
        _currentVelocity = new Vector3(1f, 0f, 0f);
        _velocityDir = VelocityDir.Horizontal;
      }
      else
        _targetVelocity = new Vector3(1f, 0f, 0f);
    }
    else if (_inputManager.CheckInput(InputActions.Left).isTriggered)
    {
      if (_velocityDir != VelocityDir.Vertical)
      {
        _currentVelocity = new Vector3(-1f, 0f, 0f);
        _velocityDir = VelocityDir.Horizontal;
      }
      else
        _targetVelocity = new Vector3(-1f, 0f, 0f);
    }

    // Release Input
    if (_inputManager.CheckInput(InputActions.Up).isReleased)
    {
      if (_velocityDir != VelocityDir.Horizontal)
      {
        if (_inputManager.CheckInput(InputActions.Down).isActive)
        {
          _currentVelocity = new Vector3(0f, 0f, -1f);
        }
        else if (_targetVelocity.x != 0f)
        {
          _currentVelocity = _targetVelocity;
          _velocityDir = VelocityDir.Horizontal;
        }
        else
        {
          _currentVelocity = new Vector3(0f, 0f, 0f);
          _velocityDir = VelocityDir.Static;
        }
      }
      else
        _targetVelocity = new Vector3(0f, 0f, 0f);
    }
    else if (_inputManager.CheckInput(InputActions.Down).isReleased)
    {
      if (_velocityDir != VelocityDir.Horizontal)
      {
        if (_inputManager.CheckInput(InputActions.Up).isActive)
        {
          _currentVelocity = new Vector3(0f, 0f, 1f);
        }
        else if (_targetVelocity.x != 0f)
        {
          _currentVelocity = _targetVelocity;
          _velocityDir = VelocityDir.Horizontal;
        }
        else
        {
          _currentVelocity = new Vector3(0f, 0f, 0f);
          _velocityDir = VelocityDir.Static;
        }
      }
      else
        _targetVelocity = new Vector3(0f, 0f, 0f);
    }

    if (_inputManager.CheckInput(InputActions.Right).isReleased)
    {
      if (_velocityDir != VelocityDir.Vertical)
      {
        if (_inputManager.CheckInput(InputActions.Left).isActive)
        {
          _currentVelocity = new Vector3(-1f, 0f, 0f);
        }
        else if (_targetVelocity.z != 0f)
        {
          _currentVelocity = _targetVelocity;
          _velocityDir = VelocityDir.Vertical;
        }
        else
        {
          _currentVelocity = new Vector3(0f, 0f, 0f);
          _velocityDir = VelocityDir.Static;
        }
      }
      else
        _targetVelocity = new Vector3(0f, 0f, 0f);
    }
    else if (_inputManager.CheckInput(InputActions.Left).isReleased)
    {
      if (_velocityDir != VelocityDir.Vertical)
      {
        if (_inputManager.CheckInput(InputActions.Right).isActive)
        {
          _currentVelocity = new Vector3(1f, 0f, 0f);
        }
        else if (_targetVelocity.z != 0f)
        {
          _currentVelocity = _targetVelocity;
          _velocityDir = VelocityDir.Vertical;
        }
        else
        {
          _currentVelocity = new Vector3(0f, 0f, 0f);
          _velocityDir = VelocityDir.Static;
        }
      }
      else
        _targetVelocity = new Vector3(0f, 0f, 0f);
    }
  }

  Vector3 GetCurrentVelocity()
  {
    return _currentVelocity * movementSpeed * Time.fixedDeltaTime;
  }

  Vector3 GetTargetVelocity()
  {
    return _targetVelocity * movementSpeed * Time.fixedDeltaTime;
  }

  private void ChangeOccupiedTile(Tile currTile)
  {
    _occupiedTile = currTile;
    _lockOutDirectionChange = false;

    foreach(Prop prop in Game.instance.grid.getProp(typeof(Switch)))
    {
      Switch switchProp = prop as Switch;
      if (switchProp.i == _occupiedTile.i && switchProp.j == _occupiedTile.j)
      {
        switchProp.StepOnSwitch();
      }
    }    
  }
}