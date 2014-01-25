using UnityEngine;
using System.Collections;
using System.Reflection;
using System;
using System.Collections.Generic;

#region Input Definitions
public enum InputSpaces
{
  MenuSpace,
  GameSpace,
  PauseSpace
}

public enum InputActions
{
  NullAction,

  // Movement
  Up,
  Down,
  Left,
  Right,

  // Pause
  Pause,
  Unpause,

  // Menu
  MenuStick,
  MenuDown,
  MenuUp,
  MenuLeft,
  MenuRight,
  MenuConfirm,
  MenuBack
}
#endregion

// Maps specific controller and keyboard inputs to in-game actions and is polled
// for current input states
public class InputManager : MonoBehaviour
{
  // List of available players' input managers.
  public Dictionary<int, PlayerInputManager> _PlayerInputManagers = new Dictionary<int, PlayerInputManager>();
  private int[] _playerKeys;

  // Number of Players constant
  private Dictionary<int, string[]> _unityTriggerStrings = new Dictionary<int, string[]>();

  // Use this for initialization
  void Awake()
  {
#if UNITY_ANDROID
    // MOGA gamepad
    MogaInput.RegisterInputButton("moga_dpadUp", MogaController.KEYCODE_DPAD_UP);
    MogaInput.RegisterInputButton("moga_dpadDown", MogaController.KEYCODE_DPAD_DOWN);
    MogaInput.RegisterInputButton("moga_dpadLeft", MogaController.KEYCODE_DPAD_LEFT);
    MogaInput.RegisterInputButton("moga_dpadRight", MogaController.KEYCODE_DPAD_RIGHT);
    MogaInput.RegisterInputButton("moga_buttonA", MogaController.KEYCODE_BUTTON_A);
    MogaInput.RegisterInputButton("moga_buttonB", MogaController.KEYCODE_BUTTON_B);
    MogaInput.RegisterInputButton("moga_buttonX", MogaController.KEYCODE_BUTTON_X);
    MogaInput.RegisterInputButton("moga_buttonY", MogaController.KEYCODE_BUTTON_Y);
    MogaInput.RegisterInputButton("moga_L1", MogaController.KEYCODE_BUTTON_L1);
    MogaInput.RegisterInputButton("moga_R1", MogaController.KEYCODE_BUTTON_R1);
    MogaInput.RegisterInputButton("moga_L2", MogaController.KEYCODE_BUTTON_L2);
    MogaInput.RegisterInputButton("moga_R2", MogaController.KEYCODE_BUTTON_R2);
    MogaInput.RegisterInputButton("moga_thumbL", MogaController.KEYCODE_BUTTON_THUMBL);
    MogaInput.RegisterInputButton("moga_thumbR", MogaController.KEYCODE_BUTTON_THUMBR);
    MogaInput.RegisterInputButton("moga_start", MogaController.KEYCODE_BUTTON_START);
    MogaInput.RegisterInputButton("moga_select", MogaController.KEYCODE_BUTTON_SELECT);

    MogaInput.RegisterInputAxis("moga_stickLX", MogaController.AXIS_X);   // Left Nub Horizontal
    MogaInput.RegisterInputAxis("moga_stickLY", MogaController.AXIS_Y);     // Left Nub Vertical
    MogaInput.RegisterInputAxis("moga_stickRX", MogaController.AXIS_Z); // Right Nub Horizontal
    MogaInput.RegisterInputAxis("moga_stickRY", MogaController.AXIS_RZ);  // Right Nub Vertical
    MogaInput.RegisterInputAxis("moga_triggerL", MogaController.AXIS_LTRIGGER);  // L2 Trigger Axis
    MogaInput.RegisterInputAxis("moga_triggerR", MogaController.AXIS_RTRIGGER);  // R2 Trigger Axis
#endif

#if UNITY_ANDROID
    // touch gamepad
    GamepadTouchInput.Register("touch_dpadUp",    GamepadTouchController.AxisId.buttonUp);
    GamepadTouchInput.Register("touch_dpadDown",  GamepadTouchController.AxisId.buttonDown);
    GamepadTouchInput.Register("touch_dpadLeft",  GamepadTouchController.AxisId.buttonLeft);
    GamepadTouchInput.Register("touch_dpadRight", GamepadTouchController.AxisId.buttonRight);
    GamepadTouchInput.Register("touch_buttonA",   GamepadTouchController.AxisId.buttonA);
    GamepadTouchInput.Register("touch_buttonB",   GamepadTouchController.AxisId.buttonB);
    GamepadTouchInput.Register("touch_buttonX",   GamepadTouchController.AxisId.buttonX);
    GamepadTouchInput.Register("touch_buttonY",   GamepadTouchController.AxisId.buttonY);
    GamepadTouchInput.Register("touch_L1",        GamepadTouchController.AxisId.buttonL1);
    GamepadTouchInput.Register("touch_R1",        GamepadTouchController.AxisId.buttonR1);
    GamepadTouchInput.Register("touch_L2",        GamepadTouchController.AxisId.buttonL2);
    GamepadTouchInput.Register("touch_R2",        GamepadTouchController.AxisId.buttonR2);
    GamepadTouchInput.Register("touch_thumbL",    GamepadTouchController.AxisId.buttonThumbL);
    GamepadTouchInput.Register("touch_thumbR",    GamepadTouchController.AxisId.buttonThumbR);
    GamepadTouchInput.Register("touch_start",     GamepadTouchController.AxisId.buttonStart);
    GamepadTouchInput.Register("touch_select",    GamepadTouchController.AxisId.buttonSelect);

    GamepadTouchInput.Register("touch_stickLX",   GamepadTouchController.AxisId.axisLX);
    GamepadTouchInput.Register("touch_stickLY",   GamepadTouchController.AxisId.axisLY);
    GamepadTouchInput.Register("touch_stickRX",   GamepadTouchController.AxisId.axisRX);
    GamepadTouchInput.Register("touch_stickRY",   GamepadTouchController.AxisId.axisRY);
    GamepadTouchInput.Register("touch_triggerL",  GamepadTouchController.AxisId.axisTriggerL);
    GamepadTouchInput.Register("touch_triggerR",  GamepadTouchController.AxisId.axisTriggerR);
#endif

    PlayerInputManager[] playerManagers = transform.GetComponentsInChildren<PlayerInputManager>();
    if (playerManagers != null)
    {
      _playerKeys = new int[playerManagers.Length];
      for (int i = 0; i < playerManagers.Length; i++)
      {
        PlayerInputManager playerManager = playerManagers[i];
        int controllerNumber = playerManager._InputControllerNumber;
        _playerKeys[i] = controllerNumber;

        string playerNumString = (controllerNumber > 1 ? controllerNumber.ToString() : string.Empty);
        _unityTriggerStrings.Add(controllerNumber, new string[2]);
        _unityTriggerStrings[controllerNumber][0] = "triggerL" + playerNumString;
        _unityTriggerStrings[controllerNumber][1] = "triggerR" + playerNumString;

        _PlayerInputManagers.Add(playerManager._InputControllerNumber, playerManager);
      }
    }

    /*
    UNITY_EDITOR	Define for calling Unity Editor scripts from your game code.
    UNITY_STANDALONE_OSX	Platform define for compiling/executing code specifically for Mac OS (This includes Universal, PPC and Intel architectures).
    UNITY_DASHBOARD_WIDGET	Platform define when creating code for Mac OS dashboard widgets.
    UNITY_STANDALONE_WIN	Use this when you want to compile/execute code for Windows stand alone applications.
    UNITY_WEBPLAYER	Platform define for web player content (this includes Windows and Mac Web player executables).
    UNITY_WII	Platform define for compiling/executing code for the Wii console.
    UNITY_IPHONE	Platform define for compiling/executing code for the iPhone platform.
    UNITY_ANDROID	Platform define for the Android platform.
    UNITY_PS3	Platform define for running Play Station 3 code.
    UNITY_XBOX360	Platform define for executing XBbox 360 code.
    UNITY_NACL	Platform define when compiling code for Google native client (this will be set additionally to UNITY_WEBPLAYER).
    UNITY_FLASH	Platform define when compiling code for Adobe Flash.
     */
#if UNITY_STANDALONE_OSX
    // Remove the unnecessary ControllerInput component - it's for Windows and XInput.dll
    GameObject.Destroy(gameObject.GetComponent<ControllerInput>());
    for(int i = 0; i < _playerKeys.Length; i++)
      UnityInputSource.playerTriggerStates.Add(_playerKeys[i], new UnityInputSource.triggerStates());
#endif
#if UNITY_ANDROID
    // Remove the unnecessary ControllerInput component - it's for Windows and XInput.dll
    GameObject.Destroy(gameObject.GetComponent<ControllerInput>());
#endif
  }

  // Validates current input mappings
  void Update()
  {
#if UNITY_STANDALONE_OSX
    // Checks if gamepad triggers have been initialized
    for (int i = 0; i < _playerKeys.Length; i++)
    {
      if (!UnityInputSource.playerTriggerStates[_playerKeys[i]].triggerLInitialized)
      {
        float value = Input.GetAxis(_unityTriggerStrings[_playerKeys[i]][0]);
        if (value != 0)
          UnityInputSource.playerTriggerStates[_playerKeys[i]].triggerLInitialized = true;
      }
      if (!UnityInputSource.playerTriggerStates[_playerKeys[i]].triggerRInitialized)
      {
        float value = Input.GetAxis(_unityTriggerStrings[_playerKeys[i]][1]);
        if (value != 0)
          UnityInputSource.playerTriggerStates[_playerKeys[i]].triggerRInitialized = true;
      }
    }
#endif
  }

  public void PushInputSpaceForAllPlayers(InputSpaces space)
  {
    for (int i = 0; i < _playerKeys.Length; i++)
    {
      _PlayerInputManagers[_playerKeys[i]].PushInputSpace(space);
    }
  }
  public void PopInputSpaceForAllPlayers(InputSpaces space)
  {
    for (int i = 0; i < _playerKeys.Length; i++)
    {
      _PlayerInputManagers[_playerKeys[i]].PopInputSpace(space);
    }
  }
  public void FlushLowerInputSpacesForAllPlayers(InputSpaces space)
  {
    for (int i = 0; i < _playerKeys.Length; i++)
    {
      _PlayerInputManagers[_playerKeys[i]].FlushLowerInputSpaces(space);
    }
  }
  public void FlushAllPlayerInput()
  {
    for (int i = 0; i < _playerKeys.Length; i++)
    {
      _PlayerInputManagers[_playerKeys[i]].FlushInput();
    }
  }

  public static DiscreteInputs DiscreteInputFromBinary(BinaryInputs binaryInput)
  {
    return (DiscreteInputs)System.Enum.Parse(typeof(DiscreteInputs), binaryInput.ToString());
  }
  public static DiscreteInputs DiscreteInputFromAnalog(AnalogInputs analogInput)
  {
    return (DiscreteInputs)System.Enum.Parse(typeof(DiscreteInputs), analogInput.ToString());
  }
  public static DiscreteInputs DiscreteInputFromDirectional(DirectionalInputs directionalInput)
  {
    return (DiscreteInputs)System.Enum.Parse(typeof(DiscreteInputs), directionalInput.ToString());
  }
  public static InputHardwareTypes HardwareTypeOfDiscreteInput(DiscreteInputs input)
  {
    switch (input)
    {
      case DiscreteInputs.stickL:
      case DiscreteInputs.stickLClick:
      case DiscreteInputs.stickR:
      case DiscreteInputs.stickRClick:
      case DiscreteInputs.stickLX:
      case DiscreteInputs.stickLY:
      case DiscreteInputs.stickRX:
      case DiscreteInputs.stickRY:
      case DiscreteInputs.triggerL:
      case DiscreteInputs.triggerR:
      case DiscreteInputs.shoulderL:
      case DiscreteInputs.shoulderR:
      case DiscreteInputs.buttonStart:
      case DiscreteInputs.buttonBack:
      case DiscreteInputs.dpad:
      case DiscreteInputs.dpadX:
      case DiscreteInputs.dpadY:
      case DiscreteInputs.dpadUp:
      case DiscreteInputs.dpadDown:
      case DiscreteInputs.dpadLeft:
      case DiscreteInputs.dpadRight:
      case DiscreteInputs.buttonA:
      case DiscreteInputs.buttonB:
      case DiscreteInputs.buttonX:
      case DiscreteInputs.buttonY:
      case DiscreteInputs.buttonGuide:
        return InputHardwareTypes.Gamepad360;
      case DiscreteInputs.ds4_stickL:
      case DiscreteInputs.ds4_stickLX:
      case DiscreteInputs.ds4_stickLY:
      case DiscreteInputs.ds4_stickR:
      case DiscreteInputs.ds4_stickRX:
      case DiscreteInputs.ds4_stickRY:
      case DiscreteInputs.ds4_L1:
      case DiscreteInputs.ds4_R1:
      case DiscreteInputs.ds4_L2:
      case DiscreteInputs.ds4_R2:
      case DiscreteInputs.ds4_L3:
      case DiscreteInputs.ds4_R3:
      case DiscreteInputs.ds4_dpad:
      case DiscreteInputs.ds4_dpadX:
      case DiscreteInputs.ds4_dpadY:
      case DiscreteInputs.ds4_dpadUp:
      case DiscreteInputs.ds4_dpadDown:
      case DiscreteInputs.ds4_dpadLeft:
      case DiscreteInputs.ds4_dpadRight:
      case DiscreteInputs.ds4_buttonStart:
      case DiscreteInputs.ds4_buttonBack:
      case DiscreteInputs.ds4_PSButton:
      case DiscreteInputs.ds4_buttonX:
      case DiscreteInputs.ds4_buttonCircle:
      case DiscreteInputs.ds4_buttonSquare:
      case DiscreteInputs.ds4_buttonTriangle:
        return InputHardwareTypes.GamepadDS4;
      case DiscreteInputs.moga_dpad:
      case DiscreteInputs.moga_dpadX:
      case DiscreteInputs.moga_dpadY:
      case DiscreteInputs.moga_dpadUp:
      case DiscreteInputs.moga_dpadDown:
      case DiscreteInputs.moga_dpadLeft:
      case DiscreteInputs.moga_dpadRight:
      case DiscreteInputs.moga_buttonA:
      case DiscreteInputs.moga_buttonB:
      case DiscreteInputs.moga_buttonX:
      case DiscreteInputs.moga_buttonY:
      case DiscreteInputs.moga_L1:
      case DiscreteInputs.moga_R1:
      case DiscreteInputs.moga_L2:
      case DiscreteInputs.moga_R2:
      case DiscreteInputs.moga_thumbL:
      case DiscreteInputs.moga_thumbR:
      case DiscreteInputs.moga_start:
      case DiscreteInputs.moga_select:
      case DiscreteInputs.moga_stickL:
      case DiscreteInputs.moga_stickR:
      case DiscreteInputs.moga_stickLX:// AXIS_X
      case DiscreteInputs.moga_stickLY:// AXIS_Y
      case DiscreteInputs.moga_stickRX:// AXIS_Z
      case DiscreteInputs.moga_stickRY:// AXIS_RZ
      case DiscreteInputs.moga_triggerL:// AXIS_LTRIGGER
      case DiscreteInputs.moga_triggerR:// AXIS_RTRIGGER
        return InputHardwareTypes.GamepadMoga;
      case DiscreteInputs.touch_dpad:
      case DiscreteInputs.touch_dpadX:
      case DiscreteInputs.touch_dpadY:
      case DiscreteInputs.touch_dpadUp:
      case DiscreteInputs.touch_dpadDown:
      case DiscreteInputs.touch_dpadLeft:
      case DiscreteInputs.touch_dpadRight:
      case DiscreteInputs.touch_buttonA:
      case DiscreteInputs.touch_buttonB:
      case DiscreteInputs.touch_buttonX:
      case DiscreteInputs.touch_buttonY:
      case DiscreteInputs.touch_L1:
      case DiscreteInputs.touch_R1:
      case DiscreteInputs.touch_L2:
      case DiscreteInputs.touch_R2:
      case DiscreteInputs.touch_thumbL:
      case DiscreteInputs.touch_thumbR:
      case DiscreteInputs.touch_start:
      case DiscreteInputs.touch_select:
      case DiscreteInputs.touch_stickL:
      case DiscreteInputs.touch_stickR:
      case DiscreteInputs.touch_stickLX:
      case DiscreteInputs.touch_stickLY:
      case DiscreteInputs.touch_stickRX:
      case DiscreteInputs.touch_stickRY:
      case DiscreteInputs.touch_triggerL:
      case DiscreteInputs.touch_triggerR:
        return InputHardwareTypes.GamepadTouch;
      case DiscreteInputs.keyA:
      case DiscreteInputs.keyB:
      case DiscreteInputs.keyC:
      case DiscreteInputs.keyD:
      case DiscreteInputs.keyE:
      case DiscreteInputs.keyF:
      case DiscreteInputs.keyG:
      case DiscreteInputs.keyH:
      case DiscreteInputs.keyI:
      case DiscreteInputs.keyJ:
      case DiscreteInputs.keyK:
      case DiscreteInputs.keyL:
      case DiscreteInputs.keyM:
      case DiscreteInputs.keyN:
      case DiscreteInputs.keyO:
      case DiscreteInputs.keyP:
      case DiscreteInputs.keyQ:
      case DiscreteInputs.keyR:
      case DiscreteInputs.keyS:
      case DiscreteInputs.keyT:
      case DiscreteInputs.keyU:
      case DiscreteInputs.keyV:
      case DiscreteInputs.keyW:
      case DiscreteInputs.keyX:
      case DiscreteInputs.keyY:
      case DiscreteInputs.keyZ:
      case DiscreteInputs.keyEsc:
      case DiscreteInputs.keySpace:
      case DiscreteInputs.keyEnter:
      case DiscreteInputs.keyTab:
      case DiscreteInputs.keyBackspace:
      case DiscreteInputs.keyLeftShift:
      case DiscreteInputs.keyRightShift:
      case DiscreteInputs.keyLeftAlt:
      case DiscreteInputs.keyRightAlt:
      case DiscreteInputs.keyLeftCtrl:
      case DiscreteInputs.keyRightCtrl:
      case DiscreteInputs.key1:
      case DiscreteInputs.key2:
      case DiscreteInputs.key3:
      case DiscreteInputs.key4:
      case DiscreteInputs.key5:
      case DiscreteInputs.key6:
      case DiscreteInputs.key7:
      case DiscreteInputs.key8:
      case DiscreteInputs.key9:
      case DiscreteInputs.key0:
      case DiscreteInputs.keysWASD:
      case DiscreteInputs.keysArrows:
      case DiscreteInputs.keyArrowLeft:
      case DiscreteInputs.keyArrowRight:
      case DiscreteInputs.keyArrowUp:
      case DiscreteInputs.keyArrowDown:
      case DiscreteInputs.keysWASDX:
      case DiscreteInputs.keysWASDY:
      case DiscreteInputs.keysArrowsX:
      case DiscreteInputs.keysArrowsY:
        return InputHardwareTypes.Keyboard;
      default:
        return InputHardwareTypes.NoHardwareType;
    }
  }
  public static bool IsAnAggregateInput(DiscreteInputs input)
  {
    switch (input)
    {
      case DiscreteInputs.dpad:
      case DiscreteInputs.dpadX:
      case DiscreteInputs.dpadY:
        return true;
      default:
        return false;
    }
  }
  public static JoystickDirections[] GetDirectionsOfAxis(AnalogInputs axis)
  {
    JoystickDirections[] directions = new JoystickDirections[2];
    switch (axis)
    {
      case AnalogInputs.keysArrowsY:
      case AnalogInputs.keysWASDY:
      case AnalogInputs.dpadY:
        directions[0] = JoystickDirections.stickUp;
        directions[1] = JoystickDirections.stickDown;
        return directions;
      case AnalogInputs.keysArrowsX:
      case AnalogInputs.keysWASDX:
      case AnalogInputs.dpadX:
        directions[0] = JoystickDirections.stickRight;
        directions[1] = JoystickDirections.stickLeft;
        return directions;
    }
    return null;
  }
  public static DiscreteInputs GetKeyboardAxisSubInput(DiscreteInputs aggregateInput, JoystickDirections direction)
  {
    DiscreteInputs discreteInput = GetKeyboardAxisFromDirectional(aggregateInput, direction);
    if (discreteInput == DiscreteInputs.noInput)
      discreteInput = aggregateInput;

    // Get the specific input for given direction
    switch (direction)
    {
      case JoystickDirections.stickUp:
      case JoystickDirections.stickDown:
        {
          switch (discreteInput)
          {
            case DiscreteInputs.keysWASDY:
              if (direction == JoystickDirections.stickUp)
                return DiscreteInputs.keyW;
              else
                return DiscreteInputs.keyS;
            case DiscreteInputs.keysArrowsY:
              if (direction == JoystickDirections.stickUp)
                return DiscreteInputs.keyArrowUp;
              else
                return DiscreteInputs.keyArrowDown;
            case DiscreteInputs.dpadY:
              if (direction == JoystickDirections.stickUp)
                return DiscreteInputs.dpadUp;
              else
                return DiscreteInputs.dpadDown;
          }
        }
        break;
      case JoystickDirections.stickRight:
      case JoystickDirections.stickLeft:
        {
          switch (discreteInput)
          {
            case DiscreteInputs.keysWASDX:
              if (direction == JoystickDirections.stickRight)
                return DiscreteInputs.keyD;
              else
                return DiscreteInputs.keyA;
            case DiscreteInputs.keysArrowsX:
              if (direction == JoystickDirections.stickRight)
                return DiscreteInputs.keyArrowRight;
              else
                return DiscreteInputs.keyArrowLeft;
            case DiscreteInputs.dpadX:
              if (direction == JoystickDirections.stickRight)
                return DiscreteInputs.dpadRight;
              else
                return DiscreteInputs.dpadLeft;
          }
        }
        break;
    }

    return DiscreteInputs.noInput;
  }
  private static DiscreteInputs GetKeyboardAxisFromDirectional(DiscreteInputs directional, JoystickDirections direction)
  {
    // Convert to an axis input if needed
    switch (direction)
    {
      case JoystickDirections.stickUp:
      case JoystickDirections.stickDown:
        {
          switch (directional)
          {
            case DiscreteInputs.keysWASD:
              return DiscreteInputs.keysWASDY;
            case DiscreteInputs.keysArrows:
              return DiscreteInputs.keysArrowsY;
            case DiscreteInputs.dpad:
              return DiscreteInputs.dpadY;
          }
        }
        break;
      case JoystickDirections.stickRight:
      case JoystickDirections.stickLeft:
        {
          switch (directional)
          {
            case DiscreteInputs.keysWASD:
              return DiscreteInputs.keysWASDX;
            case DiscreteInputs.keysArrows:
              return DiscreteInputs.keysArrowsX;
            case DiscreteInputs.dpad:
              return DiscreteInputs.dpadX;
          }
        }
        break;
    }

    return DiscreteInputs.noInput;
  }

  // Input Helper Methods.  These are accessed by InputSources
  /// <summary>
  /// Provides the XZ oriented quaternion from an Axis2D velocity vector
  /// </summary>
  /// <param name="velocity"></param>
  /// <returns></returns>
  public static Quaternion VectorToXZQuaternion(Vector2 velocity)
  {
    Vector3 inputDir = new Vector3(velocity.x, 0.0f, velocity.y);
    Quaternion inputRotation = Quaternion.identity;
    inputRotation.SetLookRotation(inputDir);
    return inputRotation;
  }

  /// <summary>
  /// Provides the degree angle between an Axis2D velocity vector and the XZ up Vector (0,0,1)
  /// </summary>
  /// <param name="velocity"></param>
  /// <returns></returns>
  public static float VectorToXZAngle(Vector2 velocity)
  {
    return Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
  }
}

// All discrete input types (including aggregates like WASD and thumb-sticks)
public enum DiscreteInputs
{
  stickL,
  stickLClick,
  stickR,
  stickRClick,
  stickLX,
  stickLY,
  stickRX,
  stickRY,
  triggerL,
  triggerR,
  shoulderL,
  shoulderR,
  buttonStart,
  buttonBack,
  dpad,
  dpadX,
  dpadY,
  dpadUp,
  dpadDown,
  dpadLeft,
  dpadRight,
  buttonA,
  buttonB,
  buttonX,
  buttonY,
  buttonGuide,
  ds4_buttonX,
  ds4_buttonCircle,
  ds4_buttonSquare,
  ds4_buttonTriangle,
  ds4_stickL,
  ds4_stickLX,
  ds4_stickLY,
  ds4_stickR,
  ds4_stickRX,
  ds4_stickRY,
  ds4_L1,
  ds4_R1,
  ds4_L2,
  ds4_R2,
  ds4_L3,
  ds4_R3,
  ds4_dpad,
  ds4_dpadX,
  ds4_dpadY,
  ds4_dpadUp,
  ds4_dpadDown,
  ds4_dpadLeft,
  ds4_dpadRight,
  ds4_buttonStart,
  ds4_buttonBack,
  ds4_PSButton,
  keyA,
  keyB,
  keyC,
  keyD,
  keyE,
  keyF,
  keyG,
  keyH,
  keyI,
  keyJ,
  keyK,
  keyL,
  keyM,
  keyN,
  keyO,
  keyP,
  keyQ,
  keyR,
  keyS,
  keyT,
  keyU,
  keyV,
  keyW,
  keyX,
  keyY,
  keyZ,
  keyEsc,
  keySpace,
  keyEnter,
  keyTab,
  keyBackspace,
  keyLeftShift,
  keyRightShift,
  keyLeftAlt,
  keyRightAlt,
  keyLeftCtrl,
  keyRightCtrl,
  key1,
  key2,
  key3,
  key4,
  key5,
  key6,
  key7,
  key8,
  key9,
  key0,
  keysWASD,
  keysArrows,
  keyArrowLeft,
  keyArrowRight,
  keyArrowUp,
  keyArrowDown,
  keysWASDX,
  keysWASDY,
  keysArrowsX,
  keysArrowsY,
  moga_dpad,
  moga_dpadX,
  moga_dpadY,
  moga_dpadUp,
  moga_dpadDown,
  moga_dpadLeft,
  moga_dpadRight,
  moga_buttonA,
  moga_buttonB,
  moga_buttonX,
  moga_buttonY,
  moga_L1,
  moga_R1,
  moga_L2,
  moga_R2,
  moga_thumbL,
  moga_thumbR,
  moga_start,
  moga_select,
  moga_stickL,
  moga_stickR,
  moga_stickLX,// AXIS_X
  moga_stickLY,// AXIS_Y
  moga_stickRX,// AXIS_Z
  moga_stickRY,// AXIS_RZ
  moga_triggerL,// AXIS_LTRIGGER
  moga_triggerR,// AXIS_RTRIGGER
  touch_dpad,
  touch_dpadX,
  touch_dpadY,
  touch_dpadUp,
  touch_dpadDown,
  touch_dpadLeft,
  touch_dpadRight,
  touch_buttonA,
  touch_buttonB,
  touch_buttonX,
  touch_buttonY,
  touch_L1,
  touch_R1,
  touch_L2,
  touch_R2,
  touch_thumbL,
  touch_thumbR,
  touch_start,
  touch_select,
  touch_stickL,
  touch_stickR,
  touch_stickLX,
  touch_stickLY,
  touch_stickRX,
  touch_stickRY,
  touch_triggerL,
  touch_triggerR,
  noInput
}

public enum DirectionalInputs
{
  noInput,
  stickL,
  stickR,
  keysWASD,
  keysArrows,
  dpad,
  ds4_stickL,
  ds4_stickR,
  ds4_dpad,
  moga_dpad,
  moga_stickL,
  moga_stickR,
  touch_dpad,
  touch_stickL,
  touch_stickR
}
public enum AnalogInputs
{
  noInput,
  stickLX,
  stickLY,
  stickRX,
  stickRY,
  triggerL,
  triggerR,
  keysWASDX,
  keysWASDY,
  keysArrowsX,
  keysArrowsY,
  dpadX,
  dpadY,
  ds4_stickLX,
  ds4_stickLY,
  ds4_stickRX,
  ds4_stickRY,
  ds4_L2,
  ds4_R2,
  ds4_dpadX,
  ds4_dpadY,
  moga_dpadX,
  moga_dpadY,
  moga_stickLX,// AXIS_X
  moga_stickLY,// AXIS_Y
  moga_stickRX,// AXIS_Z
  moga_stickRY,// AXIS_RZ
  moga_triggerL,// AXIS_LTRIGGER
  moga_triggerR,// AXIS_RTRIGGER
  touch_dpadX,
  touch_dpadY,
  touch_stickLX,
  touch_stickLY,
  touch_stickRX,
  touch_stickRY,
  touch_triggerL,
  touch_triggerR
}
public enum BinaryInputs
{
  noInput,
  stickLClick,
  stickRClick,
  shoulderL,
  shoulderR,
  buttonStart,
  buttonBack,
  dpadUp,
  dpadDown,
  dpadLeft,
  dpadRight,
  buttonA,
  buttonB,
  buttonX,
  buttonY,
  buttonGuide,
  keyA,
  keyB,
  keyC,
  keyD,
  keyE,
  keyF,
  keyG,
  keyH,
  keyI,
  keyJ,
  keyK,
  keyL,
  keyM,
  keyN,
  keyO,
  keyP,
  keyQ,
  keyR,
  keyS,
  keyT,
  keyU,
  keyV,
  keyW,
  keyX,
  keyY,
  keyZ,
  keyEsc,
  keySpace,
  keyLeftShift,
  keyRightShift,
  keyLeftAlt,
  keyRightAlt,
  keyLeftCtrl,
  keyRightCtrl,
  key1,
  key2,
  key3,
  key4,
  key5,
  key6,
  key7,
  key8,
  key9,
  key0,
  keyArrowLeft,
  keyArrowRight,
  keyArrowUp,
  keyArrowDown,
  keyEnter,
  keyTab,
  keyBackspace,
  ds4_buttonX,
  ds4_buttonCircle,
  ds4_buttonSquare,
  ds4_buttonTriangle,
  ds4_L1,
  ds4_R1,
  ds4_L3,
  ds4_R3,
  ds4_dpad,
  ds4_dpadUp,
  ds4_dpadDown,
  ds4_dpadLeft,
  ds4_dpadRight,
  ds4_buttonStart,
  ds4_buttonBack,
  ds4_PSButton,
  moga_dpadUp,
  moga_dpadDown,
  moga_dpadLeft,
  moga_dpadRight,
  moga_buttonA,
  moga_buttonB,
  moga_buttonX,
  moga_buttonY,
  moga_L1,
  moga_R1,
  moga_L2,
  moga_R2,
  moga_thumbL,
  moga_thumbR,
  moga_start,
  moga_select,
  touch_dpadUp,
  touch_dpadDown,
  touch_dpadLeft,
  touch_dpadRight,
  touch_buttonA,
  touch_buttonB,
  touch_buttonX,
  touch_buttonY,
  touch_L1,
  touch_R1,
  touch_L2,
  touch_R2,
  touch_thumbL,
  touch_thumbR,
  touch_start,
  touch_select
}

public enum InputHardwareTypes
{
  NoHardwareType,
  Keyboard,
  Gamepad360,
  GamepadDS4,
  GamepadMoga,
  GamepadTouch
}

public interface iInputManager
{
  iInputSource CheckInput(InputActions action);
  void FlushInput();
  void WitholdInput(bool withold);

  void PushInputSpace(InputSpaces space);
  void PopInputSpace(InputSpaces space);
  void FlushLowerInputSpaces(InputSpaces space);
}
public interface iInputSource
{
  // Internal fields

  // Accessible fields
  bool isTriggered
  {
    get;
  }
  bool isReleased
  {
    get;
  }
  bool isActive
  {
    get;
  }
  float value
  {
    get;
  }
  Vector2 velocity
  {
    get;
  }
  Quaternion XZQuaternion
  {
    get;
  }
  float XZAngle
  {
    get;
  }
}
public enum InputTypes
{
  Button,
  Axis,
  Axis2D,
  NoInput
}
public enum JoystickDirections
{
  stickUp,
  stickDown,
  stickLeft,
  stickRight
}

/// <summary>
/// Special Null Input Source class which has un-modifiable zeroed out default values
/// </summary>
public class NullInputSource : iInputSource
{
  // Accessible fields
  public bool isTriggered
  {
    get { return false; }
  }
  public bool isReleased
  {
    get { return false; }
  }
  public bool isActive
  {
    get { return false; }
  }
  public float value
  {
    get { return 0f; }
  }
  public Vector2 velocity
  {
    get { return Vector2.zero; }
  }
  public Quaternion XZQuaternion
  {
    get { return Quaternion.identity; }
  }
  public float XZAngle
  {
    get { return 0f; }
  }
}

// Objects for tracking input states
public class UnityCustomAxisInputSource : iInputSource
{
  public UnityCustomAxisInputSource(Dictionary<int, DiscreteInputs> subInputs)
  {
    _subInputKeys = new int[subInputs.Count];
    int i = 0;
    foreach (KeyValuePair<int, DiscreteInputs> subInput in subInputs)
    {
      _subInputs.Add(subInput.Key, subInput.Value.ToString());
      _subInputKeys[i] = subInput.Key;
      i++;
    }
  }

  private Dictionary<int, string> _subInputs = new Dictionary<int, string>();
  private int[] _subInputKeys;
  private JoystickDirections _verticalDirection = JoystickDirections.stickUp;
  private JoystickDirections _horizontalDirection = JoystickDirections.stickRight;
  private bool _triggered;
  private bool _released;
  private float _value;
  private Vector2 _direction;
  public bool _FrameUpdated;

  public void UpdateKeyboardJoystickState()
  {
    if (!_FrameUpdated)
    {
      _FrameUpdated = true;
      SetDirections();
    }
  }

  public bool isTriggered
  {
    get
    {
      bool triggered = false;
      for (int i = 0; i < _subInputKeys.Length; i++)
      {
        if (Input.GetButtonDown(_subInputs[_subInputKeys[i]]))
          triggered = true;
        else if (Input.GetButton(_subInputs[_subInputKeys[i]]))
          return false;
      }
      return triggered;
    }
  }

  public bool isReleased
  {
    get
    {
      bool released = false;
      for (int i = 0; i < _subInputKeys.Length; i++)
      {
        if (Input.GetButtonUp(_subInputs[_subInputKeys[i]]))
          released = true;
        else if (Input.GetButton(_subInputs[_subInputKeys[i]]))
          return false;
      }
      return released;
    }
  }

  public bool isActive
  {
    get
    {
      for (int i = 0; i < _subInputKeys.Length; i++)
      {
        if (Input.GetButton(_subInputs[_subInputKeys[i]]))
          return true;
      }
      return false;
    }
  }

  public float value
  {
    get
    {
      UpdateKeyboardJoystickState();
      float xVal = GetHorizontalValue();
      float yVal = GetVerticalValue();

      if (xVal == 0)
        return yVal;
      else if (yVal == 0)
        return xVal;

      else return (new Vector2(xVal, yVal).normalized).magnitude;
    }
  }

  public Vector2 velocity
  {
    get
    {
      UpdateKeyboardJoystickState();
      float xVal = GetHorizontalValue();
      float yVal = GetVerticalValue();

      Vector2 velocity = new Vector2(xVal, yVal);
      if (xVal != 0 && yVal != 0)
        velocity.Normalize();

      return velocity;
    }
  }
  public Quaternion XZQuaternion
  {
    get { return InputManager.VectorToXZQuaternion(velocity); }
  }
  public float XZAngle
  {
    get { return InputManager.VectorToXZAngle(velocity); }
  }

  /// <summary>
  /// Determines which direction each axis of the joystick is pointed in (when both ends are pushed)
  /// </summary>
  private void SetDirections()
  {
    if (_subInputs.ContainsKey((int)JoystickDirections.stickUp) && _subInputs.ContainsKey((int)JoystickDirections.stickDown))
    {
      if (Input.GetButtonDown(_subInputs[(int)JoystickDirections.stickUp]) || !Input.GetButton(_subInputs[(int)JoystickDirections.stickDown]))
      {
        _verticalDirection = JoystickDirections.stickUp;
      }
      else if (Input.GetButtonDown(_subInputs[(int)JoystickDirections.stickDown]) || !Input.GetButton(_subInputs[(int)JoystickDirections.stickUp]))
      {
        _verticalDirection = JoystickDirections.stickDown;
      }
    }
    if (_subInputs.ContainsKey((int)JoystickDirections.stickRight) && _subInputs.ContainsKey((int)JoystickDirections.stickLeft))
    {
      if (Input.GetButtonDown(_subInputs[(int)JoystickDirections.stickRight]) || !Input.GetButton(_subInputs[(int)JoystickDirections.stickLeft]))
      {
        _horizontalDirection = JoystickDirections.stickRight;
      }
      else if (Input.GetButtonDown(_subInputs[(int)JoystickDirections.stickLeft]) || !Input.GetButton(_subInputs[(int)JoystickDirections.stickRight]))
      {
        _horizontalDirection = JoystickDirections.stickLeft;
      }
    }
  }

  /// <summary>
  /// Provides the current value of the vertical axis
  /// </summary>
  /// <returns></returns>
  private float GetVerticalValue()
  {
    float value = 0f;
    if (_verticalDirection == JoystickDirections.stickUp)
    {
      if (_subInputs.ContainsKey((int)JoystickDirections.stickUp))
        value = Input.GetButton(_subInputs[(int)JoystickDirections.stickUp]) ? 1f : 0f;
    }
    else
    {
      if (_subInputs.ContainsKey((int)JoystickDirections.stickDown))
        value = Input.GetButton(_subInputs[(int)JoystickDirections.stickDown]) ? -1f : 0f;
    }

    return value;
  }

  /// <summary>
  /// Provides the current value of the horizontal axis
  /// </summary>
  /// <returns></returns>
  private float GetHorizontalValue()
  {
    float value = 0f;
    if (_horizontalDirection == JoystickDirections.stickRight)
    {
      if (_subInputs.ContainsKey((int)JoystickDirections.stickRight))
        value = Input.GetButton(_subInputs[(int)JoystickDirections.stickRight]) ? 1f : 0f;
    }
    else
    {
      if (_subInputs.ContainsKey((int)JoystickDirections.stickLeft))
        value = Input.GetButton(_subInputs[(int)JoystickDirections.stickLeft]) ? -1f : 0f;
    }

    return value;
  }
}
public class WinGamepadCustomAxisInputSource : iInputSource
{
  public WinGamepadCustomAxisInputSource(Dictionary<int, iInputSource> subInputs)
  {
    _subInputKeys = new int[subInputs.Count];
    int i = 0;
    foreach (KeyValuePair<int, iInputSource> subInput in subInputs)
    {
      _subInputs.Add(subInput.Key, subInput.Value);
      _subInputKeys[i] = subInput.Key;
      i++;
    }
  }

  private Dictionary<int, iInputSource> _subInputs = new Dictionary<int, iInputSource>();
  private int[] _subInputKeys;
  private JoystickDirections _verticalDirection = JoystickDirections.stickUp;
  private JoystickDirections _horizontalDirection = JoystickDirections.stickRight;
  private bool _triggered;
  private bool _released;
  private float _value;
  private Vector2 _direction;
  public bool _FrameUpdated;

  public void UpdateGamepadJoystickState()
  {
    if (!_FrameUpdated)
    {
      _FrameUpdated = true;
      SetDirections();
    }
  }

  public bool isTriggered
  {
    get
    {
      bool triggered = false;
      for (int i = 0; i < _subInputKeys.Length; i++)
      {
        if (_subInputs[_subInputKeys[i]].isTriggered)
          triggered = true;
        else if (_subInputs[_subInputKeys[i]].isActive)
          return false;
      }
      return triggered;
    }
  }

  public bool isReleased
  {
    get
    {
      bool released = false;
      for (int i = 0; i < _subInputKeys.Length; i++)
      {
        if (_subInputs[_subInputKeys[i]].isReleased)
          released = true;
        else if (_subInputs[_subInputKeys[i]].isActive)
          return false;
      }
      return released;
    }
  }

  public bool isActive
  {
    get
    {
      for (int i = 0; i < _subInputKeys.Length; i++)
      {
        if (_subInputs[_subInputKeys[i]].isActive)
          return true;
      }
      return false;
    }
  }

  public float value
  {
    get
    {
      UpdateGamepadJoystickState();
      float xVal = GetHorizontalValue();
      float yVal = GetVerticalValue();

      if (xVal == 0)
        return yVal;
      else if (yVal == 0)
        return xVal;

      else return (new Vector2(xVal, yVal).normalized).magnitude;
    }
  }

  public Vector2 velocity
  {
    get
    {
      UpdateGamepadJoystickState();
      float xVal = GetHorizontalValue();
      float yVal = GetVerticalValue();

      Vector2 velocity = new Vector2(xVal, yVal);
      if (xVal != 0 && yVal != 0)
        velocity.Normalize();

      return velocity;
    }
  }
  public Quaternion XZQuaternion
  {
    get { return InputManager.VectorToXZQuaternion(velocity); }
  }
  public float XZAngle
  {
    get { return InputManager.VectorToXZAngle(velocity); }
  }

  /// <summary>
  /// Determines which direction each axis of the joystick is pointed in (when both ends are pushed)
  /// </summary>
  private void SetDirections()
  {
    if (_subInputs.ContainsKey((int)JoystickDirections.stickUp) && _subInputs.ContainsKey((int)JoystickDirections.stickDown))
    {
      if (_subInputs[(int)JoystickDirections.stickUp].isActive || !_subInputs[(int)JoystickDirections.stickDown].isActive)
      {
        _verticalDirection = JoystickDirections.stickUp;
      }
      else if (_subInputs[(int)JoystickDirections.stickDown].isActive || !_subInputs[(int)JoystickDirections.stickUp].isActive)
      {
        _verticalDirection = JoystickDirections.stickDown;
      }
    }
    if (_subInputs.ContainsKey((int)JoystickDirections.stickRight) && _subInputs.ContainsKey((int)JoystickDirections.stickLeft))
    {
      if (_subInputs[(int)JoystickDirections.stickRight].isActive || !_subInputs[(int)JoystickDirections.stickLeft].isActive)
      {
        _horizontalDirection = JoystickDirections.stickRight;
      }
      else if (_subInputs[(int)JoystickDirections.stickLeft].isActive || !_subInputs[(int)JoystickDirections.stickRight].isActive)
      {
        _horizontalDirection = JoystickDirections.stickLeft;
      }
    }
  }

  /// <summary>
  /// Provides the current value of the vertical axis
  /// </summary>
  /// <returns></returns>
  private float GetVerticalValue()
  {
    float value = 0f;
    if (_verticalDirection == JoystickDirections.stickUp)
    {
      if (_subInputs.ContainsKey((int)JoystickDirections.stickUp))
        value = _subInputs[(int)JoystickDirections.stickUp].isActive ? 1f : 0f;
    }
    else
    {
      if (_subInputs.ContainsKey((int)JoystickDirections.stickDown))
        value = _subInputs[(int)JoystickDirections.stickDown].isActive ? -1f : 0f;
    }

    return value;
  }

  /// <summary>
  /// Provides the current value of the horizontal axis
  /// </summary>
  /// <returns></returns>
  private float GetHorizontalValue()
  {
    float value = 0f;
    if (_horizontalDirection == JoystickDirections.stickRight)
    {
      if (_subInputs.ContainsKey((int)JoystickDirections.stickRight))
        value = _subInputs[(int)JoystickDirections.stickRight].isActive ? 1f : 0f;
    }
    else
    {
      if (_subInputs.ContainsKey((int)JoystickDirections.stickLeft))
        value = _subInputs[(int)JoystickDirections.stickLeft].isActive ? -1f : 0f;
    }

    return value;
  }
}
public class WinGamepadInputSource : iInputSource
{
  public WinGamepadInputSource(InputManager parentManager, DiscreteInputs input, int playerNum)
  {
    _parentManager = parentManager;
    _playerNumber = playerNum;
    _discreteInput = input;

    _controllerStateComponent = _parentManager.GetComponent<ControllerInput>();
  }

  // Internal fields
  private InputManager _parentManager;
  private DiscreteInputs _discreteInput;
  private int _playerNumber;
  private ControllerInput _controllerStateComponent;
  private bool _active;

  // Accessible fields
  public bool isTriggered
  {
    get { return _controllerStateComponent._activeControllers.Contains(_playerNumber) ? GetControllerStateTriggered(_discreteInput) : false; }
  }
  public bool isReleased
  {
    get
    {
      if (_controllerStateComponent._activeControllers.Contains(_playerNumber))
        return GetControllerStateReleased(_discreteInput);
      else
      {
        if (_active)
        {
          _active = false;
          return true;
        }
        else
          return false;
      }
    }
  }
  public bool isActive
  {
    get
    {
      if (_controllerStateComponent._activeControllers.Contains(_playerNumber))
      {
        _active = GetControllerStateValue(_discreteInput) != 0 ? true : false;
        return _active;
      }
      else
        return false;
    }
  }
  public float value
  {
    get { return _controllerStateComponent._activeControllers.Contains(_playerNumber) ? GetControllerStateValue(_discreteInput) : 0f; }
  }

  public Vector2 velocity
  {
    get { return _controllerStateComponent._activeControllers.Contains(_playerNumber) ? GetControllerStateVelocity(_discreteInput) : Vector2.zero; }
  }
  public Quaternion XZQuaternion
  {
    get { return InputManager.VectorToXZQuaternion(velocity); }
  }
  public float XZAngle
  {
    get { return InputManager.VectorToXZAngle(velocity); }
  }

  float GetControllerStateValue(DiscreteInputs input)
  {
    // Lookup player gamepadState
    ControllerState state = _controllerStateComponent.curControllerState[_playerNumber];

    switch (input)
    {
      case DiscreteInputs.stickLX:
        return state.stickLX;
      case DiscreteInputs.stickLY:
        return state.stickLY;

      case DiscreteInputs.stickRX:
        return state.stickRX;
      case DiscreteInputs.stickRY:
        return state.stickRY;

      case DiscreteInputs.stickL:
        return new Vector2(state.stickLX, state.stickLY).magnitude;
      case DiscreteInputs.stickR:
        return new Vector2(state.stickRX, state.stickRY).magnitude;

      case DiscreteInputs.buttonA:
        return state.buttonA ? 1f : 0f;
      case DiscreteInputs.buttonB:
        return state.buttonB ? 1f : 0f;
      case DiscreteInputs.buttonX:
        return state.buttonX ? 1f : 0f;
      case DiscreteInputs.buttonY:
        return state.buttonY ? 1f : 0f;

      case DiscreteInputs.dpadUp:
        return state.dpadUp ? 1f : 0f;
      case DiscreteInputs.dpadDown:
        return state.dpadDown ? 1f : 0f;
      case DiscreteInputs.dpadLeft:
        return state.dpadLeft ? 1f : 0f;
      case DiscreteInputs.dpadRight:
        return state.dpadRight ? 1f : 0f;

      case DiscreteInputs.buttonStart:
        return state.buttonStart ? 1f : 0f;
      case DiscreteInputs.buttonBack:
        return state.buttonBack ? 1f : 0f;

      case DiscreteInputs.shoulderL:
        return state.shoulderL ? 1f : 0f;
      case DiscreteInputs.shoulderR:
        return state.shoulderR ? 1f : 0f;

      case DiscreteInputs.stickLClick:
        return state.stickLClick ? 1f : 0f;
      case DiscreteInputs.stickRClick:
        return state.stickRClick ? 1f : 0f;

      case DiscreteInputs.triggerL:
        return state.triggerL;
      case DiscreteInputs.triggerR:
        return state.triggerR;

    default:
        return 0f;
    }
  }
  bool GetControllerStateTriggered(DiscreteInputs input)
  {
    // Lookup player gamepadState
    ControllerState state = _controllerStateComponent.curControllerState[_playerNumber];

    switch (input)
    {
      case DiscreteInputs.stickL:
        return state.stickLTriggered;
      case DiscreteInputs.stickR:
        return state.stickRTriggered;

      case DiscreteInputs.buttonA:
        return state.buttonATriggered;
      case DiscreteInputs.buttonB:
        return state.buttonBTriggered;
      case DiscreteInputs.buttonX:
        return state.buttonXTriggered;
      case DiscreteInputs.buttonY:
        return state.buttonYTriggered;

      case DiscreteInputs.dpadUp:
        return state.dpadUpTriggered;
      case DiscreteInputs.dpadDown:
        return state.dpadDownTriggered;
      case DiscreteInputs.dpadLeft:
        return state.dpadLeftTriggered;
      case DiscreteInputs.dpadRight:
        return state.dpadRightTriggered;

      case DiscreteInputs.buttonStart:
        return state.buttonStartTriggered;
      case DiscreteInputs.buttonBack:
        return state.buttonBackTriggered;

      case DiscreteInputs.shoulderL:
        return state.shoulderLTriggered;
      case DiscreteInputs.shoulderR:
        return state.shoulderRTriggered;

      case DiscreteInputs.stickLClick:
        return state.stickLClickTriggered;
      case DiscreteInputs.stickRClick:
        return state.stickRClickTriggered;

      case DiscreteInputs.triggerL:
        return state.triggerLTriggered;
      case DiscreteInputs.triggerR:
        return state.triggerRTriggered;

    default:
        return false;
    }
  }
  bool GetControllerStateReleased(DiscreteInputs input)
  {
    // Lookup player gamepadState
    ControllerState state = _controllerStateComponent.curControllerState[_playerNumber];

    switch (input)
    {
      case DiscreteInputs.stickL:
        return state.stickLReleased;
      case DiscreteInputs.stickR:
        return state.stickRReleased;

      case DiscreteInputs.buttonA:
        return state.buttonAReleased;
      case DiscreteInputs.buttonB:
        return state.buttonBReleased;
      case DiscreteInputs.buttonX:
        return state.buttonXReleased;
      case DiscreteInputs.buttonY:
        return state.buttonYReleased;

      case DiscreteInputs.dpadUp:
        return state.dpadUpReleased;
      case DiscreteInputs.dpadDown:
        return state.dpadDownReleased;
      case DiscreteInputs.dpadLeft:
        return state.dpadLeftReleased;
      case DiscreteInputs.dpadRight:
        return state.dpadRightReleased;

      case DiscreteInputs.buttonStart:
        return state.buttonStartReleased;
      case DiscreteInputs.buttonBack:
        return state.buttonBackReleased;

      case DiscreteInputs.shoulderL:
        return state.shoulderLReleased;
      case DiscreteInputs.shoulderR:
        return state.shoulderRReleased;

      case DiscreteInputs.stickLClick:
        return state.stickLClickReleased;
      case DiscreteInputs.stickRClick:
        return state.stickRClickReleased;

      case DiscreteInputs.triggerL:
        return state.triggerLReleased;
      case DiscreteInputs.triggerR:
        return state.triggerRReleased;

    default:
        return false;
    }
  }
  private Vector2 GetControllerStateVelocity(DiscreteInputs input)
  {
    // Lookup player gamepadState
    ControllerState state = _controllerStateComponent.curControllerState[_playerNumber];

    switch (input)
    {
      case DiscreteInputs.stickL:
        return new Vector2(state.stickLX, state.stickLY);
      case DiscreteInputs.stickR:
        return new Vector2(state.stickRX, state.stickRY);
      default:
        return Vector2.zero;
    }
  }
}
public class UnityInputSource : iInputSource
{
  public UnityInputSource(InputTypes inputType, string inputName, int playerNum, DiscreteInputs input = DiscreteInputs.noInput)
  {
    _inputType = inputType;
    _playerNumber = playerNum;
    _unityInputString = inputName;

    _discreteInput = input;

    if (_inputType == InputTypes.Axis2D)
    {
      _directionalSubStringX = _unityInputString + "X";
      _directionalSubStringY = _unityInputString + "Y";
    }

    if (input == DiscreteInputs.ds4_dpadUp || input == DiscreteInputs.ds4_dpadDown)
    {
      if (input == DiscreteInputs.ds4_dpadUp)
        _ds4_dpadSide = 1;
      else
        _ds4_dpadSide = -1;
      _unityInputString = DiscreteInputs.ds4_dpadY.ToString();
      _discreteInput = DiscreteInputs.ds4_dpadY;
      _inputType = InputTypes.Axis;
    }
    else if (input == DiscreteInputs.ds4_dpadLeft || input == DiscreteInputs.ds4_dpadRight)
    {
      if (input == DiscreteInputs.ds4_dpadRight)
        _ds4_dpadSide = 1;
      else
        _ds4_dpadSide = -1;
      _unityInputString = DiscreteInputs.ds4_dpadX.ToString();
      _discreteInput = DiscreteInputs.ds4_dpadX;
      _inputType = InputTypes.Axis;
    }

    if (_playerNumber > 1)
    {
      string playerNumString = _playerNumber.ToString();

      if (_inputType == InputTypes.Axis2D)
      {
        _directionalSubStringX += playerNumString;
        _directionalSubStringY += playerNumString;
      }
      else
        _unityInputString += playerNumString;
    }
  }

  // Internal fields
  private InputTypes _inputType;
  public InputHardwareTypes _hardwareType;
  private int _playerNumber;
  private string _unityInputString;
  private string _directionalSubStringX;
  private string _directionalSubStringY;
  private DiscreteInputs _discreteInput;
  private int _ds4_dpadSide;

  public class triggerStates
  {
    public bool triggerLInitialized = false;
    public bool triggerRInitialized = false;
  }
  static public Dictionary<int, triggerStates> playerTriggerStates = new Dictionary<int, triggerStates>();

  // Accessible fields
  public bool isTriggered
  {
    get
    {
      switch (_inputType)
      {
        case InputTypes.Button:
          return GetButtonTriggered();
        case InputTypes.Axis:
          return false;
        case InputTypes.Axis2D:
          return false;
        default:
          return false;
      }
    }
  }
  public bool isReleased
  {
    get
    {
      switch (_inputType)
      {
        case InputTypes.Button:
          return GetButtonReleased();
        case InputTypes.Axis:
          return false;
        case InputTypes.Axis2D:
          return false;
        default:
          return false;
      }
    }
  }
  public bool isActive
  {
    get
    {
      switch (_inputType)
      {
        case InputTypes.Button:
          return GetButtonActive();
        case InputTypes.Axis:
          return GetAxisActive();
        case InputTypes.Axis2D:
          return GetAxis2DActive();
        default:
          return false;
      }
    }
  }
  public float value
  {
    get
    {
      switch (_inputType)
      {
        case InputTypes.Button:
          return GetButtonValue();
        case InputTypes.Axis:
          return GetAxisIntensity();
        case InputTypes.Axis2D:
          return GetAxis2DMagnitude();
        default:
          return 0f;
      }
    }
  }

  public Vector2 velocity
  {
    get
    {
      switch (_inputType)
      {
        case InputTypes.Button:
          return new Vector2(0, 0);
        case InputTypes.Axis:
          return new Vector2(0, 0);
        case InputTypes.Axis2D:
          return GetAxis2DVelocity();
        default:
          return new Vector2(0, 0);
      }
    }
  }
  public Quaternion XZQuaternion
  {
    get { return InputManager.VectorToXZQuaternion(velocity); }
  }
  public float XZAngle
  {
    get { return InputManager.VectorToXZAngle(velocity); }
  }


  bool GetButtonTriggered()
  {
    return Input.GetButtonDown(_unityInputString);
  }
  bool GetButtonReleased()
  {
    return Input.GetButtonUp(_unityInputString);
  }
  bool GetButtonActive()
  {
    return Input.GetButton(_unityInputString);
  }
  float GetButtonValue()
  {
    return Input.GetAxis(_unityInputString);
  }

  float GetAxisIntensity()
  {
    float value = Input.GetAxis(_unityInputString);
    if (_discreteInput == DiscreteInputs.triggerL || _discreteInput == DiscreteInputs.triggerR)
    {
      if (value == -1f)
        value = 0;
      else
      {
        if (_discreteInput == DiscreteInputs.triggerL && (UnityInputSource.playerTriggerStates[_playerNumber].triggerLInitialized || value != 0))
          value = (value + 1f) / 2f;
        else if (_discreteInput == DiscreteInputs.triggerR && (UnityInputSource.playerTriggerStates[_playerNumber].triggerRInitialized || value != 0))
          value = (value + 1f) / 2f;
      }
    }
    else if (_discreteInput == DiscreteInputs.ds4_L2 || _discreteInput == DiscreteInputs.ds4_R2)
    {
      value = (value + 1f) / 2f;
    }
    else if (_discreteInput == DiscreteInputs.ds4_dpadY || _discreteInput == DiscreteInputs.ds4_dpadX)
    {
      if (_ds4_dpadSide > 0)
        value = Mathf.Max(value, 0f);
      else
        value = Mathf.Abs(Mathf.Min(value, 0f));
    }
    return value;
  }
  bool GetAxisActive()
  {
    float value = Input.GetAxis(_unityInputString);
    if (_discreteInput == DiscreteInputs.triggerL || _discreteInput == DiscreteInputs.triggerR)
    {
      if (value == -1f)
        value = 0;
      else
      {
        if (_discreteInput == DiscreteInputs.triggerL && (UnityInputSource.playerTriggerStates[_playerNumber].triggerLInitialized || value != 0))
          value = (value + 1f) / 2f;
        else if (_discreteInput == DiscreteInputs.triggerR && (UnityInputSource.playerTriggerStates[_playerNumber].triggerRInitialized || value != 0))
          value = (value + 1f) / 2f;
      }
    }
    else if (_discreteInput == DiscreteInputs.ds4_L2 || _discreteInput == DiscreteInputs.ds4_R2)
    {
      value = (value + 1f) / 2f;
    }
    else if (_discreteInput == DiscreteInputs.ds4_dpadY || _discreteInput == DiscreteInputs.ds4_dpadX)
    {
      if (_ds4_dpadSide > 0)
        value = Mathf.Max(value, 0f);
      else
        value = Mathf.Abs(Mathf.Min(value, 0f));
    }
    return value > 0 ? true : false;
  }

  bool GetAxis2DActive()
  {
    float xIntensity = Input.GetAxis(_directionalSubStringX);
    float yIntensity = Input.GetAxis(_directionalSubStringY);
    return (xIntensity != 0 || yIntensity != 0) ? true : false;
  }
  float GetAxis2DMagnitude()
  {
    float xIntensity = Input.GetAxis(_directionalSubStringX);
    float yIntensity = Input.GetAxis(_directionalSubStringY);
    Vector2 inputVelocity = new Vector2(xIntensity, yIntensity);
    if (inputVelocity.sqrMagnitude > 1)
      inputVelocity.Normalize();
    return inputVelocity.magnitude;
  }
  Vector2 GetAxis2DVelocity()
  {
    float xIntensity = Input.GetAxis(_directionalSubStringX);
    float yIntensity = Input.GetAxis(_directionalSubStringY);
    Vector2 inputVelocity = new Vector2(xIntensity, yIntensity);
    if (inputVelocity.sqrMagnitude > 1)
      inputVelocity.Normalize();
    return inputVelocity;
  }
}
