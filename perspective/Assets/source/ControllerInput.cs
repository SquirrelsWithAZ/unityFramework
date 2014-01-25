using UnityEngine;
using System.Collections;
using XInputDotNetPure;
using System.Collections.Generic;

// Class to hold 
public class ControllerState {
    // public variables for accessing the gamePad's state
    public float stickLX = 0.0f;
    public float stickLY = 0.0f;
     
    public float stickRX = 0.0f;
    public float stickRY = 0.0f;

    public bool stickLTriggered = false;
    public bool stickRTriggered = false;

    public bool stickLReleased = false;
    public bool stickRReleased = false;
       
    public bool buttonA = false;
    public bool buttonB = false;
    public bool buttonX = false;
    public bool buttonY = false;
     
    public bool buttonATriggered = false;
    public bool buttonBTriggered = false;
    public bool buttonXTriggered = false;
    public bool buttonYTriggered = false;
     
    public bool buttonAReleased = false;
    public bool buttonBReleased = false;
    public bool buttonXReleased = false;
    public bool buttonYReleased = false;
       
    public bool dpadUp    = false;
    public bool dpadDown  = false;
    public bool dpadLeft  = false;
    public bool dpadRight = false;
     
    public bool dpadUpTriggered    = false;
    public bool dpadDownTriggered  = false;
    public bool dpadLeftTriggered  = false;
    public bool dpadRightTriggered = false;
     
    public bool dpadUpReleased     = false;
    public bool dpadDownReleased   = false;
    public bool dpadLeftReleased   = false;
    public bool dpadRightReleased  = false;
     
    public bool buttonStart = false;
    public bool buttonBack  = false;
     
    public bool buttonStartTriggered = false;
    public bool buttonBackTriggered  = false;
     
    public bool buttonStartReleased = false;
    public bool buttonBackReleased  = false;
           
    public bool shoulderL = false;
    public bool shoulderR = false;
     
    public bool shoulderLTriggered = false;
    public bool shoulderRTriggered = false;
     
    public bool shoulderLReleased = false;
    public bool shoulderRReleased = false;
                   
    public bool stickLClick = false;
    public bool stickRClick = false;
     
    public bool stickLClickTriggered = false;
    public bool stickRClickTriggered = false;
     
    public bool stickLClickReleased = false;
    public bool stickRClickReleased = false;
     
    public float triggerL = 0.0f;
    public float triggerR = 0.0f;

    public bool triggerLTriggered = false;
    public bool triggerRTriggered = false;

    public bool triggerLReleased = false;
    public bool triggerRReleased = false;
}

public class ControllerInput : MonoBehaviour {
  public float deadZoneMagnitude = 0.2f;
  public int numPlayers = 1; // Set the number of players so we're not always polling for
  // more controllers than we have to (it's *super* slow to poll disconnected controllers)
  
  public float _updateInterval = 1f;

  // ControllerState is interpreted from GamePadState to have Triggered, Released, etc.
  public ControllerState [] curControllerState;
  // GamePadState is direct from XInput
  private GamePadState [] curGamePadState; // Space for numPlayers controllers
  private GamePadState [] prevGamePadState; // Hold last frame's info to test ButtonTriggered
  
  // Only check for connected controllers once in a while
  private float _updateCounter;
  public HashSet<int> _activeControllers = new HashSet<int>();
  private int _currentControllerToBeChecked;

  // Use this for initialization
  void Start () {
    // Initialize the controllerState fields
    curControllerState = new ControllerState[numPlayers];
    curGamePadState = new GamePadState[numPlayers];
    prevGamePadState = new GamePadState[numPlayers];

    for (int i = 0; i < numPlayers; i++)
    {
      curControllerState[i] = new ControllerState();
      curGamePadState[i] = new GamePadState();
      prevGamePadState[i] = new GamePadState();
    }

    // Poll controllers, assign numControllers
    for (int i = 0; i < numPlayers; ++i)
    {
      PlayerIndex testPlayerIndex = (PlayerIndex)i;
      curGamePadState[i] = GamePad.GetState(testPlayerIndex);
      if (curGamePadState[i].IsConnected)
        _activeControllers.Add(i);
    }

    // Fill out the initial state for active conrollers
    foreach(int i in _activeControllers)
    {
      FillControllerData(i, curGamePadState[i]);
      prevGamePadState[i] = curGamePadState[i];
    }
  }
  
  // Update is called once per frame
  // Building the update class like this may lead to an offset of a frame depending
  // on when ControllerInput is updated relative to other scripts
  void Update () 
  {
    // Every check controllers interval, check one of the possible controllers to see if it's been connected
    if (_updateCounter > _updateInterval)
    {
      if (!_activeControllers.Contains(_currentControllerToBeChecked))
      {
        PlayerIndex testPlayerIndex = (PlayerIndex)_currentControllerToBeChecked;
        curGamePadState[_currentControllerToBeChecked] = GamePad.GetState(testPlayerIndex);
        if (curGamePadState[_currentControllerToBeChecked].IsConnected)
        {
          _activeControllers.Add(_currentControllerToBeChecked);
        }
      }

      _currentControllerToBeChecked = (_currentControllerToBeChecked + 1) % numPlayers;
      _updateCounter = 0f;
    }
    else
      _updateCounter += Time.deltaTime;

    // Update active controllers each frame
    List<int> _controllersToBeRemoved = new List<int>();
    foreach(int i in _activeControllers)
    {
      // To apply Circular deadzone, pass "(GamePadDeadZone)2" as second parameter so it casts second enum in (some reason enum isn't in dll)
      curGamePadState[i] = GamePad.GetState((PlayerIndex)i, GamePadDeadZone.None); // We will apply deadzone in InputManager
      FillControllerData(i, curGamePadState[i]);
      prevGamePadState[i] = curGamePadState[i];

      // If current controller has been disconnected, remove it from the active controller list
      if (!prevGamePadState[i].IsConnected)
      {
        _controllersToBeRemoved.Add(i);
      }
    }
    foreach (int i in _controllersToBeRemoved)
    {
      _activeControllers.Remove(i);
    }
  }

  Vector2 ProcessAnalogStickDeadzone(float x, float y)
  {
    const float controllerMax = 1.0f;

    Vector2 stickDir = Vector2.zero;
    Vector2 stickValues = new Vector2(x, y);

    if (stickValues.sqrMagnitude > Mathf.Epsilon * Mathf.Epsilon)
    {
      stickDir = stickValues.normalized;
    }

    float stickNormalizedMagnitude = 0.0f;
    if ((stickValues.magnitude - deadZoneMagnitude) > Mathf.Epsilon)
    {
      float mag = (stickValues.magnitude - deadZoneMagnitude) / (controllerMax - deadZoneMagnitude);
      stickNormalizedMagnitude = Mathf.Clamp01(mag);
    }

    return stickDir * stickNormalizedMagnitude;
  }

  void FillControllerData( int controllerNum, GamePadState state )
  {
    Vector2 leftStick = ProcessAnalogStickDeadzone(state.ThumbSticks.Left.X, state.ThumbSticks.Left.Y);
    curControllerState[controllerNum].stickLX = leftStick.x;
    curControllerState[controllerNum].stickLY = leftStick.y;

    Vector2 rightStick = ProcessAnalogStickDeadzone(state.ThumbSticks.Right.X, state.ThumbSticks.Right.Y);
    curControllerState[controllerNum].stickRX = rightStick.x;
    curControllerState[controllerNum].stickRY = rightStick.y;

    Vector2 prevLeftStick = ProcessAnalogStickDeadzone(prevGamePadState[controllerNum].ThumbSticks.Left.X, prevGamePadState[controllerNum].ThumbSticks.Left.Y);
    Vector2 prevRightStick = ProcessAnalogStickDeadzone(prevGamePadState[controllerNum].ThumbSticks.Right.X, prevGamePadState[controllerNum].ThumbSticks.Right.Y);

    curControllerState[controllerNum].stickLTriggered = ((curControllerState[controllerNum].stickLX != 0 || curControllerState[controllerNum].stickLY != 0) &&
                                                         (prevLeftStick.x == 0 && prevLeftStick.y == 0));
    curControllerState[controllerNum].stickRTriggered = ((curControllerState[controllerNum].stickRX != 0 || curControllerState[controllerNum].stickRY != 0) &&
                                                         (prevRightStick.x == 0 && prevRightStick.y == 0));

    curControllerState[controllerNum].stickLReleased = ((curControllerState[controllerNum].stickLX == 0 && curControllerState[controllerNum].stickLY == 0) &&
                                                         (prevLeftStick.x != 0 || prevLeftStick.y != 0));
    curControllerState[controllerNum].stickRReleased = ((curControllerState[controllerNum].stickRX == 0 && curControllerState[controllerNum].stickRY == 0) &&
                                                         (prevRightStick.x != 0 || prevRightStick.y != 0));
   
    curControllerState[controllerNum].buttonA = ( state.Buttons.A == ButtonState.Pressed );
    curControllerState[controllerNum].buttonB = ( state.Buttons.B == ButtonState.Pressed );
    curControllerState[controllerNum].buttonX = ( state.Buttons.X == ButtonState.Pressed );
    curControllerState[controllerNum].buttonY = ( state.Buttons.Y == ButtonState.Pressed );
   
    curControllerState[controllerNum].buttonATriggered = ( curControllerState[controllerNum].buttonA &&
                                                           prevGamePadState[controllerNum].Buttons.A != ButtonState.Pressed );
    curControllerState[controllerNum].buttonBTriggered = ( curControllerState[controllerNum].buttonB &&
                                                           prevGamePadState[controllerNum].Buttons.B != ButtonState.Pressed);
    curControllerState[controllerNum].buttonXTriggered = ( curControllerState[controllerNum].buttonX &&
                                                           prevGamePadState[controllerNum].Buttons.X != ButtonState.Pressed);
    curControllerState[controllerNum].buttonYTriggered = ( curControllerState[controllerNum].buttonY &&
                                                           prevGamePadState[controllerNum].Buttons.Y != ButtonState.Pressed);

    curControllerState[controllerNum].buttonAReleased = (!curControllerState[controllerNum].buttonA &&
                                                         prevGamePadState[controllerNum].Buttons.A == ButtonState.Pressed);
    curControllerState[controllerNum].buttonBReleased = (!curControllerState[controllerNum].buttonB &&
                                                         prevGamePadState[controllerNum].Buttons.B == ButtonState.Pressed);
    curControllerState[controllerNum].buttonXReleased = (!curControllerState[controllerNum].buttonX &&
                                                         prevGamePadState[controllerNum].Buttons.X == ButtonState.Pressed);
    curControllerState[controllerNum].buttonYReleased = (!curControllerState[controllerNum].buttonY &&
                                                         prevGamePadState[controllerNum].Buttons.Y == ButtonState.Pressed);
   
    curControllerState[controllerNum].dpadUp = ( state.DPad.Up == ButtonState.Pressed );
    curControllerState[controllerNum].dpadDown = ( state.DPad.Down == ButtonState.Pressed );
    curControllerState[controllerNum].dpadLeft = ( state.DPad.Left == ButtonState.Pressed );
    curControllerState[controllerNum].dpadRight = ( state.DPad.Right == ButtonState.Pressed );

    curControllerState[controllerNum].dpadUpTriggered = (curControllerState[controllerNum].dpadUp &&
                                                         prevGamePadState[controllerNum].DPad.Up != ButtonState.Pressed);
    curControllerState[controllerNum].dpadDownTriggered = (curControllerState[controllerNum].dpadDown &&
                                                           prevGamePadState[controllerNum].DPad.Down != ButtonState.Pressed);
    curControllerState[controllerNum].dpadLeftTriggered = (curControllerState[controllerNum].dpadLeft &&
                                                           prevGamePadState[controllerNum].DPad.Left != ButtonState.Pressed);
    curControllerState[controllerNum].dpadRightTriggered = (curControllerState[controllerNum].dpadRight &&
                                                            prevGamePadState[controllerNum].DPad.Right != ButtonState.Pressed);

    curControllerState[controllerNum].dpadUpReleased = (!curControllerState[controllerNum].dpadUp &&
                                                        prevGamePadState[controllerNum].DPad.Up == ButtonState.Pressed);
    curControllerState[controllerNum].dpadDownReleased = (!curControllerState[controllerNum].dpadDown &&
                                                          prevGamePadState[controllerNum].DPad.Down == ButtonState.Pressed);
    curControllerState[controllerNum].dpadLeftReleased = (!curControllerState[controllerNum].dpadLeft &&
                                                          prevGamePadState[controllerNum].DPad.Left == ButtonState.Pressed);
    curControllerState[controllerNum].dpadRightReleased = (!curControllerState[controllerNum].dpadRight &&
                                                           prevGamePadState[controllerNum].DPad.Right == ButtonState.Pressed);
    
    curControllerState[controllerNum].buttonStart = ( state.Buttons.Start == ButtonState.Pressed );
    curControllerState[controllerNum].buttonBack = ( state.Buttons.Back == ButtonState.Pressed );

    curControllerState[controllerNum].buttonStartTriggered = (curControllerState[controllerNum].buttonStart &&
                                                              prevGamePadState[controllerNum].Buttons.Start != ButtonState.Pressed);
    curControllerState[controllerNum].buttonBackTriggered = (curControllerState[controllerNum].buttonBack &&
                                                             prevGamePadState[controllerNum].Buttons.Back != ButtonState.Pressed);

    curControllerState[controllerNum].buttonStartReleased = (!curControllerState[controllerNum].buttonStart &&
                                                             prevGamePadState[controllerNum].Buttons.Start == ButtonState.Pressed);
    curControllerState[controllerNum].buttonBackReleased = (!curControllerState[controllerNum].buttonBack &&
                                                            prevGamePadState[controllerNum].Buttons.Back == ButtonState.Pressed);

    curControllerState[controllerNum].shoulderL = ( state.Buttons.LeftShoulder == ButtonState.Pressed );
    curControllerState[controllerNum].shoulderR = ( state.Buttons.RightShoulder == ButtonState.Pressed );

    curControllerState[controllerNum].shoulderLTriggered = (curControllerState[controllerNum].shoulderL &&
                                                            prevGamePadState[controllerNum].Buttons.LeftShoulder != ButtonState.Pressed);
    curControllerState[controllerNum].shoulderRTriggered = (curControllerState[controllerNum].shoulderR && prevGamePadState[controllerNum].Buttons.RightShoulder != ButtonState.Pressed);

    curControllerState[controllerNum].shoulderLReleased = (!curControllerState[controllerNum].shoulderL && prevGamePadState[controllerNum].Buttons.LeftShoulder == ButtonState.Pressed);
    curControllerState[controllerNum].shoulderRReleased = (!curControllerState[controllerNum].shoulderR && prevGamePadState[controllerNum].Buttons.RightShoulder == ButtonState.Pressed);
   
    curControllerState[controllerNum].stickLClick = ( state.Buttons.LeftStick == ButtonState.Pressed );
    curControllerState[controllerNum].stickRClick = ( state.Buttons.RightStick == ButtonState.Pressed );

    curControllerState[controllerNum].stickLClickTriggered = (curControllerState[controllerNum].stickLClick && prevGamePadState[controllerNum].Buttons.LeftStick != ButtonState.Pressed);
    curControllerState[controllerNum].stickRClickTriggered = ( curControllerState[controllerNum].stickRClick && prevGamePadState[controllerNum].Buttons.RightStick != ButtonState.Pressed );

    curControllerState[controllerNum].stickLClickReleased = (!curControllerState[controllerNum].stickLClick && prevGamePadState[controllerNum].Buttons.LeftStick == ButtonState.Pressed);
    curControllerState[controllerNum].stickRClickReleased = (!curControllerState[controllerNum].stickRClick && prevGamePadState[controllerNum].Buttons.RightStick == ButtonState.Pressed);
   
    curControllerState[controllerNum].triggerL = state.Triggers.Left;
    curControllerState[controllerNum].triggerR = state.Triggers.Right;

    curControllerState[controllerNum].triggerLTriggered = (curControllerState[controllerNum].triggerL != 0 && prevGamePadState[controllerNum].Triggers.Left == 0);
    curControllerState[controllerNum].triggerRTriggered = (curControllerState[controllerNum].triggerR != 0 && prevGamePadState[controllerNum].Triggers.Right == 0);

    curControllerState[controllerNum].triggerLReleased = (curControllerState[controllerNum].triggerL == 0 && prevGamePadState[controllerNum].Triggers.Left != 0);
    curControllerState[controllerNum].triggerRReleased = (curControllerState[controllerNum].triggerR == 0 && prevGamePadState[controllerNum].Triggers.Right != 0);
  }
}
