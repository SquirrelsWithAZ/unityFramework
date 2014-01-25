using UnityEngine;
using System.Collections;
using CustomExtensions;

public class Avatar : MonoBehaviour 
{
  public int playerNumber;
  public TileTypes initialLayer;

  public float movementSpeed;

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
    UpdatePosition();
  }

  private void UpdatePosition()
  {
    TilePos currGridPos = transform.position.GetTilePos();
    Tile currTile = Game.instance.grid.getTile(currGridPos.x, currGridPos.y);

    if (currTile != _occupiedTile)
    {
      ChangeOccupiedTile(currTile);
    }
  }

  private void ProcessInput()
  {
    // Movement
    int verticalMovement = 0;
    int horizontalMovement = 0;

    Debug.Log("Checking input on player " + playerNumber);
    if (_inputManager.CheckInput(InputActions.Up).isActive)
      verticalMovement += 1;
    else if (_inputManager.CheckInput(InputActions.Down).isActive)
      verticalMovement -= 1;

    if (_inputManager.CheckInput(InputActions.Right).isActive)
      horizontalMovement += 1;
    else if (_inputManager.CheckInput(InputActions.Left).isActive)
      horizontalMovement -= 1;

    transform.position += new Vector3(horizontalMovement, 0f, verticalMovement).normalized * movementSpeed;
  }

  private void ChangeOccupiedTile(Tile currTile)
  {
    _occupiedTile = currTile;
  }
}