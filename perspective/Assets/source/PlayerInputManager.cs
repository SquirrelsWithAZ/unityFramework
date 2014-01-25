using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;

[ExecuteInEditMode]
public class PlayerInputManager : MonoBehaviour, iInputManager
{
  [HideInInspector]
  public InputManager _parentManager;

  // Specifies which gamepad to check input on
  public bool _RunInEditorMode;
  public string _PlayerNumberName;
  public InputHardwareTypes _GamepadType;
  public int _InputControllerNumber;
  public InputActions[] _NewActionsList;
  public bool _IncludeAllMissingActions;
  public bool _InsertSpecifiedActions;

  // Stores Mappings
  public InputAction[] _InputActions = new InputAction[System.Enum.GetValues(typeof(InputActions)).Length];
  private Dictionary<int, InputAction> _actionMappings = new Dictionary<int, InputAction>();
  private Dictionary<int, List<InputActions>> _spaceToActions = new Dictionary<int, List<InputActions>>();
  public Dictionary<int, iInputSource> _hashedInputs = new Dictionary<int, iInputSource>();

  public Dictionary<int, bool> _InputSpaceStates = new Dictionary<int, bool>();
  public Dictionary<int, int> _InputSpaceActivationFrames = new Dictionary<int, int>();
  public Dictionary<int, int> _InputSpaceFlushFrames = new Dictionary<int, int>();
  private List<InputSpaces> _InputSpaceStack = new List<InputSpaces>();
  private List<InputActions> _disabledActions = new List<InputActions>();

  private List<UnityCustomAxisInputSource> _directionalKeyboardInputs = new List<UnityCustomAxisInputSource>();
  private List<WinGamepadCustomAxisInputSource> _directionalGamepadInputs = new List<WinGamepadCustomAxisInputSource>();

  private bool _initialized;

  // Input Override flags
  private bool _inputWitheld = false;
  private int _inputFlushFrame;
  protected int _inputSpaceFlushFrame;
  private iInputSource _wipedInputSource = new NullInputSource();

  public void SetInputActionEnabled(InputActions action, bool state)
  {
    if (state)
    {
      if (_disabledActions.Exists(c => c == action))
      {
        _disabledActions.Remove(action);
      }
    }
    else
    {
      if (!_disabledActions.Exists(c => c == action))
      {
        _disabledActions.Add(action);
      }
    }
  }

  public iInputSource CheckInput(InputActions action)
  {
    // If input witheld or flushed, return a null Input Source
    if (_inputWitheld || _inputFlushFrame == Time.frameCount || !_actionMappings.ContainsKey((int)action) || _disabledActions.Exists(c => c == action))
    {
      return _wipedInputSource;
    }
    else
    {
      return _actionMappings[(int)action];
    }
  }

  void Awake()
  {
    if (!Application.isPlaying)
      return;

    _parentManager = transform.parent.GetComponent<InputManager>();

    TryDetermineControllerType();

    foreach (InputSpaces possibleSpace in System.Enum.GetValues(typeof(InputSpaces)))
    {
      _InputSpaceStates.Add((int)possibleSpace, false);
      _InputSpaceActivationFrames.Add((int)possibleSpace, -1);
      _InputSpaceFlushFrames.Add((int)possibleSpace, -1);
      _spaceToActions.Add((int)possibleSpace, new List<InputActions>());
    }
    foreach (PlayerInputManager.InputAction possibleAction in _InputActions)
    {
      possibleAction.Initialize(this, _InputControllerNumber);
      _actionMappings.Add((int)possibleAction._Action, possibleAction);
    }
    PushInputSpace(InputSpaces.GameSpace);
  }

  private void TryDetermineControllerType()
  {
#if UNITY_STANDALONE_WIN
    if (GamePad.GetState((PlayerIndex)(_InputControllerNumber - 1)).IsConnected)
      _GamepadType = InputHardwareTypes.Gamepad360;
    else
      _GamepadType = InputHardwareTypes.GamepadDS4;
#endif
#if UNITY_STANDALONE_OSX
    _GamepadType = InputHardwareTypes.Gamepad360;
#endif
#if UNITY_ANDROID
    _GamepadType = InputHardwareTypes.GamepadMoga;
#endif
  }

  void Update()
  {
    if (!Application.isPlaying)
    {
      if (_RunInEditorMode)
      {
        if (_InsertSpecifiedActions)
        {
          _initialized = false;
          _InsertSpecifiedActions = false;
        }
        else if (_NewActionsList != null)
        {
          // Remove existing and duplicate entries from _NewActionList
          HashSet<InputActions> existingList = new HashSet<InputActions>();

          // Copy existing entries into the HashSet
          for (int index = 0; index < _InputActions.Length; index++)
          {
            existingList.Add((InputActions)System.Enum.Parse(typeof(InputActions), _InputActions[index]._ActionName));
          }

          HashSet<InputActions> newList = new HashSet<InputActions>();
          List<InputActions> replacementList = new List<InputActions>();
          for (int i = 0; i < _NewActionsList.Length; i++)
          {
            if (existingList.Contains(_NewActionsList[i]))
            {
              Debug.LogError(_NewActionsList[i] + " is already in the Input Actions list");
            }
            else if (newList.Contains(_NewActionsList[i]))
            {
              if (_NewActionsList[i] != InputActions.NullAction)
                Debug.LogError(_NewActionsList[i] + " is already marked to be inserted");
            }
            else
            {
              newList.Add(_NewActionsList[i]);
              replacementList.Add(_NewActionsList[i]);
            }
          }
          // If inserting all, determine missing entries
          if (_IncludeAllMissingActions)
          {
            // Foreach possible action, add to _NewActionsList those that are missing
            foreach (InputActions possibleAction in System.Enum.GetValues(typeof(InputActions)))
            {
              if (existingList.Contains(possibleAction) || newList.Contains(possibleAction) || possibleAction == InputActions.NullAction)
              {
                continue;
              }
              else
              {
                newList.Add(possibleAction);
                replacementList.Add(possibleAction);
              }
            }

            _IncludeAllMissingActions = false;
          }
          _NewActionsList = replacementList.ToArray();
        }
        if (!_initialized)
        {
          _initialized = true;
          Dictionary<InputActions, PlayerInputManager.InputAction> existingList = new Dictionary<InputActions, PlayerInputManager.InputAction>();

          bool nullActionPresent = false;
          for (int index = 0; index < _NewActionsList.Length; index++)
          {
            if (_NewActionsList[index] == InputActions.NullAction)
              nullActionPresent = true;
          }

          int totalActionCount = _NewActionsList.Length;
          if (nullActionPresent)
            totalActionCount -= 1;

          if (_InputActions != null)
          {
            // Copy existing entries into the dictionary
            for (int index = 0; index < _InputActions.Length; index++)
            {
              existingList.Add((InputActions)System.Enum.Parse(typeof(InputActions), _InputActions[index]._ActionName), _InputActions[index]);
            }
            totalActionCount += _InputActions.Length;
          }

          // Generate entries for the new actions
          for (int index = 0; index < _NewActionsList.Length; index++)
          {
            if (existingList.ContainsKey(_NewActionsList[index]) || _NewActionsList[index] == InputActions.NullAction)
              continue;

            PlayerInputManager.InputAction newAction = new PlayerInputManager.InputAction();
            newAction._InputSpaces = new InputSpaces[1];
            InputActions action = _NewActionsList[index];
            newAction._ActionName = action.ToString();
            newAction._BinaryInputBindings = new BinaryInputs[1];
            newAction._AnalogInputBindings = new AnalogInputs[1];
            newAction._DirectionalInputBindings = new DirectionalInputs[1];
            existingList.Add(action, newAction);
          }
          _NewActionsList = null;

          // Clear the old array and set its size to include new entries
          _InputActions = new PlayerInputManager.InputAction[totalActionCount];
          int i = 0;
          foreach (InputActions possibleAction in System.Enum.GetValues(typeof(InputActions)))
          {
            if (existingList.ContainsKey(possibleAction))
            {
              _InputActions[i] = existingList[possibleAction];
              i++;
            }
          }
        }

        return;
      }
    }
  }
  void LateUpdate()
  {
    for (int i = 0; i < _directionalKeyboardInputs.Count; i++)
    {
      _directionalKeyboardInputs[i].UpdateKeyboardJoystickState();
      _directionalKeyboardInputs[i]._FrameUpdated = false;
    }
    for (int i = 0; i < _directionalGamepadInputs.Count; i++)
    {
      _directionalGamepadInputs[i].UpdateGamepadJoystickState();
      _directionalGamepadInputs[i]._FrameUpdated = false;
    }
  }

  public void FlushInput()
  {
    _inputFlushFrame = Time.frameCount;
  }

  public void WitholdInput(bool withold)
  {
    _inputWitheld = withold;
  }

  public void RegisterDirectionalKeyboardInput(UnityCustomAxisInputSource newInput)
  {
    _directionalKeyboardInputs.Add(newInput);
  }

  public void RegisterDirectionalGamepadInput(WinGamepadCustomAxisInputSource newInput)
  {
    _directionalGamepadInputs.Add(newInput);
  }

  public void PushInputSpace(InputSpaces space)
  {
    if (!_InputSpaceStack.Contains(space))
    {
      if (_InputSpaceStack.Count > 0)
        SetInputSpaceActive(_InputSpaceStack[_InputSpaceStack.Count - 1], false);

      _InputSpaceStack.Add(space);
      SetInputSpaceActive(space, true, true);
    }
  }

  public void PopInputSpace(InputSpaces space)
  {
    if (_InputSpaceStack.Contains(space))
    {
      int spaceIndex = _InputSpaceStack.IndexOf(space);
      _InputSpaceStack.Remove(space);
      SetInputSpaceActive(space, false);

      if (spaceIndex > 0 && spaceIndex == _InputSpaceStack.Count)
        SetInputSpaceActive(_InputSpaceStack[spaceIndex - 1], true);
    }
  }

  private void SetInputSpaceActive(InputSpaces space, bool active, bool pushedState = false)
  {
    if (_InputSpaceStates[(int)space] == active)
      return;

    _InputSpaceStates[(int)space] = active;
    if (active)
      _InputSpaceActivationFrames[(int)space] = Time.frameCount;

    List<InputActions> actionsInSpace = _spaceToActions[(int)space];
    for (int i = 0; i < actionsInSpace.Count; i++)
    {
      _actionMappings[(int)actionsInSpace[i]].SpaceActiveStateChanged(active, pushedState);
    }
  }

  public void FlushLowerInputSpaces(InputSpaces space)
  {
    if (!_InputSpaceStack.Contains(space))
      return;

    int spaceIndex = _InputSpaceStack.IndexOf(space);
    for (int i = 0; i < spaceIndex; i++)
    {
      _InputSpaceFlushFrames[(int)_InputSpaceStack[i]] = Time.frameCount;
    }

    _inputSpaceFlushFrame = Time.frameCount;
  }

  /// <summary>
  /// Stores key-binding and space info for a given Input Action
  /// </summary>
  [System.Serializable]
  public class InputAction : iInputSource
  {
    public string _ActionName;
    [HideInInspector]
    public InputActions _Action;
    public InputSpaces[] _InputSpaces;
    public BinaryInputs[] _BinaryInputBindings;
    public AnalogInputs[] _AnalogInputBindings;
    public DirectionalInputs[] _DirectionalInputBindings;

    private bool _inputActive;
    private int _activeSpaceCount;
    private int _lastFrameInactive;

    private Dictionary<int, List<iInputSource>> _bindingsByInputType = new Dictionary<int, List<iInputSource>>();

    private PlayerInputManager _inputManager;

    public void Initialize(PlayerInputManager inputManager, int playerNum)
    {
      _Action = (InputActions)System.Enum.Parse(typeof(InputActions), _ActionName);
      _inputManager = inputManager;
      foreach (InputSpaces controllingSpace in _InputSpaces)
        _inputManager._spaceToActions[(int)controllingSpace].Add(_Action);
      foreach (InputTypes type in System.Enum.GetValues(typeof(InputTypes)))
      {
        _bindingsByInputType.Add((int)type, new List<iInputSource>());
      }
      #if UNITY_STANDALONE_OSX
      foreach (BinaryInputs binaryInput in _BinaryInputBindings)
      {
        InputHardwareTypes hardwareType = InputManager.HardwareTypeOfDiscreteInput(InputManager.DiscreteInputFromBinary(binaryInput));
              if (hardwareType != inputManager._GamepadType && hardwareType != InputHardwareTypes.Keyboard)
                continue;
        if (binaryInput != BinaryInputs.noInput && hardwareType != InputHardwareTypes.GamepadDS4)
        {
          DiscreteInputs discreteInput = InputManager.DiscreteInputFromBinary(binaryInput);
          if (!_inputManager._hashedInputs.ContainsKey((int)discreteInput))
          {
            _inputManager._hashedInputs.Add((int)discreteInput, new UnityInputSource(InputTypes.Button, binaryInput.ToString(),
                                                                                     InputManager.HardwareTypeOfDiscreteInput(discreteInput) == InputHardwareTypes.Keyboard ? 0 : playerNum,
                                                                                     discreteInput));
          }
          _bindingsByInputType[(int)InputTypes.Button].Add(_inputManager._hashedInputs[(int)discreteInput]);
        }
      }
      foreach (AnalogInputs analogInput in _AnalogInputBindings)
      {
        InputHardwareTypes hardwareType = InputManager.HardwareTypeOfDiscreteInput(InputManager.DiscreteInputFromAnalog(analogInput));
              if (hardwareType != inputManager._GamepadType && hardwareType != InputHardwareTypes.Keyboard)
                continue;
        if (analogInput != AnalogInputs.noInput && hardwareType != InputHardwareTypes.GamepadDS4)
        {
          DiscreteInputs discreteInput = InputManager.DiscreteInputFromAnalog(analogInput);
          if (!_inputManager._hashedInputs.ContainsKey((int)discreteInput))
          {
            if (hardwareType == InputHardwareTypes.Gamepad360)
            {
              _inputManager._hashedInputs.Add((int)discreteInput, new UnityInputSource(InputTypes.Axis, analogInput.ToString(), playerNum, discreteInput));
            }
            else if (hardwareType == InputHardwareTypes.Keyboard)
            {
              Dictionary<int, DiscreteInputs> joystickInputs = new Dictionary<int, DiscreteInputs>();
              foreach (JoystickDirections direction in InputManager.GetDirectionsOfAxis(analogInput))
              {
                DiscreteInputs subInput = InputManager.GetKeyboardAxisSubInput(discreteInput, direction);
                if (subInput != DiscreteInputs.noInput)
                  joystickInputs.Add((int)direction, subInput);
              }
              UnityCustomAxisInputSource newKeyboardInput = new UnityCustomAxisInputSource(joystickInputs);
              _inputManager.RegisterDirectionalKeyboardInput(newKeyboardInput);
              _inputManager._hashedInputs.Add((int)discreteInput, newKeyboardInput);
            }
          }
          _bindingsByInputType[(int)InputTypes.Axis].Add(_inputManager._hashedInputs[(int)discreteInput]);
        }
      }
      foreach (DirectionalInputs directionalInput in _DirectionalInputBindings)
      {
        InputHardwareTypes hardwareType = InputManager.HardwareTypeOfDiscreteInput(InputManager.DiscreteInputFromDirectional(directionalInput));
              if (hardwareType != inputManager._GamepadType && hardwareType != InputHardwareTypes.Keyboard)
                continue;
        if (directionalInput != DirectionalInputs.noInput && hardwareType != InputHardwareTypes.GamepadDS4)
        {
          DiscreteInputs discreteInput = InputManager.DiscreteInputFromDirectional(directionalInput);
          if (!_inputManager._hashedInputs.ContainsKey((int)discreteInput))
          {
            if (hardwareType == InputHardwareTypes.Keyboard || InputManager.IsAnAggregateInput(discreteInput))
            {
              Dictionary<int, DiscreteInputs> joystickInputs = new Dictionary<int, DiscreteInputs>();
              foreach (JoystickDirections direction in System.Enum.GetValues(typeof(JoystickDirections)))
              {
                DiscreteInputs subInput = InputManager.GetKeyboardAxisSubInput(discreteInput, direction);
                if (subInput != DiscreteInputs.noInput)
                  joystickInputs.Add((int)direction, subInput);
              }
              UnityCustomAxisInputSource newKeyboardInput = new UnityCustomAxisInputSource(joystickInputs);
              _inputManager.RegisterDirectionalKeyboardInput(newKeyboardInput);
              _inputManager._hashedInputs.Add((int)discreteInput, newKeyboardInput);
            }
            else if (hardwareType == InputHardwareTypes.Gamepad360)
            {
              if (hardwareType != inputManager._GamepadType)
                continue;

              _inputManager._hashedInputs.Add((int)discreteInput, new UnityInputSource(InputTypes.Axis2D, directionalInput.ToString(), playerNum, discreteInput));
            }
          }
          _bindingsByInputType[(int)InputTypes.Axis2D].Add(_inputManager._hashedInputs[(int)discreteInput]);
        }
      }
      #endif

#if UNITY_STANDALONE_WIN
      foreach (BinaryInputs binaryInput in _BinaryInputBindings)
      {
        if (binaryInput != BinaryInputs.noInput)
        {
          DiscreteInputs binaryAsDiscrete = InputManager.DiscreteInputFromBinary(binaryInput);
          if (!_inputManager._hashedInputs.ContainsKey((int)binaryAsDiscrete))
          {
            InputHardwareTypes hardwareType = InputManager.HardwareTypeOfDiscreteInput(binaryAsDiscrete);
              if (hardwareType != inputManager._GamepadType && hardwareType != InputHardwareTypes.Keyboard)
                continue;
            if (hardwareType == InputHardwareTypes.Gamepad360)
            {
              if (hardwareType != inputManager._GamepadType)
                continue;

              _inputManager._hashedInputs.Add((int)binaryAsDiscrete, new WinGamepadInputSource(_inputManager._parentManager, binaryAsDiscrete, playerNum - 1));
            }
            if (hardwareType == InputHardwareTypes.GamepadDS4)
            {
              if (hardwareType != inputManager._GamepadType)
                continue;

              _inputManager._hashedInputs.Add((int)binaryAsDiscrete, new UnityInputSource(InputTypes.Button, binaryAsDiscrete.ToString(), playerNum - 1, binaryAsDiscrete));
            }
            else if (hardwareType == InputHardwareTypes.Keyboard)
              _inputManager._hashedInputs.Add((int)binaryAsDiscrete, new UnityInputSource(InputTypes.Button, binaryInput.ToString(), 0));
          }
          _bindingsByInputType[(int)InputTypes.Button].Add(_inputManager._hashedInputs[(int)binaryAsDiscrete]);
        }
      }
      foreach (AnalogInputs analogInput in _AnalogInputBindings)
      {
        if (analogInput != AnalogInputs.noInput)
        {
          DiscreteInputs analogAsDiscrete = InputManager.DiscreteInputFromAnalog(analogInput);
          if (!_inputManager._hashedInputs.ContainsKey((int)analogAsDiscrete))
          {
            InputHardwareTypes hardwareType = InputManager.HardwareTypeOfDiscreteInput(analogAsDiscrete);
              if (hardwareType != inputManager._GamepadType && hardwareType != InputHardwareTypes.Keyboard)
                continue;
            if (hardwareType == InputHardwareTypes.Gamepad360)
            {
              if (hardwareType != inputManager._GamepadType)
                continue;

              _inputManager._hashedInputs.Add((int)analogAsDiscrete, new WinGamepadInputSource(_inputManager._parentManager, analogAsDiscrete, playerNum - 1));
            }
            else if (hardwareType == InputHardwareTypes.GamepadDS4)
            {
              if (hardwareType != inputManager._GamepadType)
                continue;

              _inputManager._hashedInputs.Add((int)analogAsDiscrete, new UnityInputSource(InputTypes.Axis, analogAsDiscrete.ToString(), playerNum - 1, analogAsDiscrete));
            }
            else if (hardwareType == InputHardwareTypes.Keyboard)
            {
              Dictionary<int, DiscreteInputs> joystickInputs = new Dictionary<int, DiscreteInputs>();
              foreach (JoystickDirections direction in InputManager.GetDirectionsOfAxis(analogInput))
              {
                DiscreteInputs subInput = InputManager.GetKeyboardAxisSubInput(analogAsDiscrete, direction);
                if (subInput != DiscreteInputs.noInput)
                  joystickInputs.Add((int)direction, subInput);
              }
              UnityCustomAxisInputSource newKeyboardInput = new UnityCustomAxisInputSource(joystickInputs);
              _inputManager.RegisterDirectionalKeyboardInput(newKeyboardInput);
              _inputManager._hashedInputs.Add((int)analogAsDiscrete, newKeyboardInput);
            }
          }
          _bindingsByInputType[(int)InputTypes.Axis].Add(_inputManager._hashedInputs[(int)analogAsDiscrete]);
        }
      }
      foreach (DirectionalInputs directionalInput in _DirectionalInputBindings)
      {
        if (directionalInput != DirectionalInputs.noInput)
        {
          DiscreteInputs directionalAsDiscrete = InputManager.DiscreteInputFromDirectional(directionalInput);
          if (!_inputManager._hashedInputs.ContainsKey((int)directionalAsDiscrete))
          {
            InputHardwareTypes hardwareType = InputManager.HardwareTypeOfDiscreteInput(directionalAsDiscrete);
              if (hardwareType != inputManager._GamepadType && hardwareType != InputHardwareTypes.Keyboard)
                continue;
            if (hardwareType == InputHardwareTypes.Gamepad360)
            {
              if (hardwareType != inputManager._GamepadType)
                continue;

              if (!InputManager.IsAnAggregateInput(directionalAsDiscrete))
                _inputManager._hashedInputs.Add((int)directionalAsDiscrete, new WinGamepadInputSource(_inputManager._parentManager, directionalAsDiscrete, playerNum - 1));
              else
              {
                Dictionary<int, iInputSource> joystickInputs = new Dictionary<int, iInputSource>();
                foreach (JoystickDirections direction in System.Enum.GetValues(typeof(JoystickDirections)))
                {
                  DiscreteInputs subInput = InputManager.GetKeyboardAxisSubInput(directionalAsDiscrete, direction);
                  if (subInput != DiscreteInputs.noInput)
                  {

                    joystickInputs.Add((int)direction, new WinGamepadInputSource(_inputManager._parentManager, subInput, playerNum - 1) as iInputSource);
                  }
                }
                WinGamepadCustomAxisInputSource newGamepadInput = new WinGamepadCustomAxisInputSource(joystickInputs);
                _inputManager.RegisterDirectionalGamepadInput(newGamepadInput);
                _inputManager._hashedInputs.Add((int)directionalAsDiscrete, newGamepadInput);
              }
            }

            else if (hardwareType == InputHardwareTypes.GamepadDS4)
            {
              if (hardwareType != inputManager._GamepadType)
                continue;

              if (!InputManager.IsAnAggregateInput(directionalAsDiscrete))
                _inputManager._hashedInputs.Add((int)directionalAsDiscrete, new UnityInputSource(InputTypes.Axis2D, directionalAsDiscrete.ToString(), playerNum - 1));
              else
              {
                Dictionary<int, DiscreteInputs> joystickInputs = new Dictionary<int, DiscreteInputs>();
                foreach (JoystickDirections direction in System.Enum.GetValues(typeof(JoystickDirections)))
                {
                  DiscreteInputs subInput = InputManager.GetKeyboardAxisSubInput(directionalAsDiscrete, direction);
                  if (subInput != DiscreteInputs.noInput)
                    joystickInputs.Add((int)direction, subInput);
                }
                UnityCustomAxisInputSource newGameadInput = new UnityCustomAxisInputSource(joystickInputs);
                _inputManager.RegisterDirectionalKeyboardInput(newGameadInput);
                _inputManager._hashedInputs.Add((int)directionalAsDiscrete, newGameadInput);
              }
            }

            else if (hardwareType == InputHardwareTypes.Keyboard)
            {
              Dictionary<int, DiscreteInputs> joystickInputs = new Dictionary<int, DiscreteInputs>();
              foreach (JoystickDirections direction in System.Enum.GetValues(typeof(JoystickDirections)))
              {
                DiscreteInputs subInput = InputManager.GetKeyboardAxisSubInput(directionalAsDiscrete, direction);
                if (subInput != DiscreteInputs.noInput)
                  joystickInputs.Add((int)direction, subInput);
              }
              UnityCustomAxisInputSource newKeyboardInput = new UnityCustomAxisInputSource(joystickInputs);
              _inputManager.RegisterDirectionalKeyboardInput(newKeyboardInput);
              _inputManager._hashedInputs.Add((int)directionalAsDiscrete, newKeyboardInput);
            }
          }
          _bindingsByInputType[(int)InputTypes.Axis2D].Add(_inputManager._hashedInputs[(int)directionalAsDiscrete]);
        }
      }
#endif
#if UNITY_ANDROID
      foreach (BinaryInputs binaryInput in _BinaryInputBindings)
      {
        if (binaryInput != BinaryInputs.noInput)
        {
          DiscreteInputs discreteInput = InputManager.DiscreteInputFromBinary(binaryInput);
          InputHardwareTypes hardwareType = InputManager.HardwareTypeOfDiscreteInput(discreteInput);
          if ( (hardwareType != InputHardwareTypes.GamepadMoga) && (hardwareType != InputHardwareTypes.GamepadTouch))
            continue;
          if (!_inputManager._hashedInputs.ContainsKey((int)discreteInput))
          {
            if (hardwareType == InputHardwareTypes.GamepadMoga)
            {
              _inputManager._hashedInputs.Add((int)discreteInput, new MogaInputSource(InputTypes.Button, binaryInput.ToString(),
                                                                                       InputManager.HardwareTypeOfDiscreteInput(discreteInput) == InputHardwareTypes.Keyboard ? 0 : playerNum,
                                                                                       discreteInput));
            }
            if (hardwareType == InputHardwareTypes.GamepadTouch)
            {
              _inputManager._hashedInputs.Add((int)discreteInput, new TouchInputSource(InputTypes.Button, binaryInput.ToString(), 0, discreteInput));
            }
          }
          _bindingsByInputType[(int)InputTypes.Button].Add(_inputManager._hashedInputs[(int)discreteInput]);
        }
      }
      foreach (AnalogInputs analogInput in _AnalogInputBindings)
      {
        InputHardwareTypes hardwareType = InputManager.HardwareTypeOfDiscreteInput(InputManager.DiscreteInputFromAnalog(analogInput));
        if (analogInput != AnalogInputs.noInput)
        {
          if ( (hardwareType != InputHardwareTypes.GamepadMoga) && (hardwareType != InputHardwareTypes.GamepadTouch))
            continue;
          DiscreteInputs discreteInput = InputManager.DiscreteInputFromAnalog(analogInput);
          if (!_inputManager._hashedInputs.ContainsKey((int)discreteInput))
          {
            if (hardwareType == InputHardwareTypes.GamepadMoga)
            {
              _inputManager._hashedInputs.Add((int)discreteInput, new MogaInputSource(InputTypes.Axis, analogInput.ToString(), playerNum, discreteInput));
            }
            if (hardwareType == InputHardwareTypes.GamepadTouch)
            {
              _inputManager._hashedInputs.Add((int)discreteInput, new TouchInputSource(InputTypes.Axis, analogInput.ToString(), 0, discreteInput));
            }
            else if (hardwareType == InputHardwareTypes.Keyboard)
            {
//              Dictionary<int, DiscreteInputs> joystickInputs = new Dictionary<int, DiscreteInputs>();
//              foreach (JoystickDirections direction in InputManager.GetDirectionsOfAxis(analogInput))
//              {
//                DiscreteInputs subInput = InputManager.GetKeyboardAxisSubInput(discreteInput, direction);
//                if (subInput != DiscreteInputs.noInput)
//                  joystickInputs.Add((int)direction, subInput);
//              }
//              UnityCustomAxisInputSource newKeyboardInput = new MogaCustomAxisInputSource(joystickInputs);
//              _inputManager.RegisterDirectionalKeyboardInput(newKeyboardInput);
//              _inputManager._hashedInputs.Add((int)discreteInput, newKeyboardInput);
            }
          }
          _bindingsByInputType[(int)InputTypes.Axis].Add(_inputManager._hashedInputs[(int)discreteInput]);
        }
      }
      foreach (DirectionalInputs directionalInput in _DirectionalInputBindings)
      {
        InputHardwareTypes hardwareType = InputManager.HardwareTypeOfDiscreteInput(InputManager.DiscreteInputFromDirectional(directionalInput));
        if (directionalInput != DirectionalInputs.noInput)
        {
          if ( (hardwareType != InputHardwareTypes.GamepadMoga) && (hardwareType != InputHardwareTypes.GamepadTouch))
            continue;
          DiscreteInputs discreteInput = InputManager.DiscreteInputFromDirectional(directionalInput);
          if (!_inputManager._hashedInputs.ContainsKey((int)discreteInput))
          {
            if (hardwareType == InputHardwareTypes.Keyboard || InputManager.IsAnAggregateInput(discreteInput))
            {
//              Dictionary<int, DiscreteInputs> joystickInputs = new Dictionary<int, DiscreteInputs>();
//              foreach (JoystickDirections direction in System.Enum.GetValues(typeof(JoystickDirections)))
//              {
//                DiscreteInputs subInput = InputManager.GetKeyboardAxisSubInput(discreteInput, direction);
//                if (subInput != DiscreteInputs.noInput)
//                  joystickInputs.Add((int)direction, subInput);
//              }
//              UnityCustomAxisInputSource newKeyboardInput = new MogaCustomAxisInputSource(joystickInputs);
//              _inputManager.RegisterDirectionalKeyboardInput(newKeyboardInput);
//              _inputManager._hashedInputs.Add((int)discreteInput, newKeyboardInput);
            }
            else if (hardwareType == InputHardwareTypes.GamepadMoga)
            {
              if (hardwareType != inputManager._GamepadType)
                continue;

              _inputManager._hashedInputs.Add((int)discreteInput, new MogaInputSource(InputTypes.Axis2D, directionalInput.ToString(), playerNum, discreteInput));
            }
            else if (hardwareType == InputHardwareTypes.GamepadTouch)
            {
              _inputManager._hashedInputs.Add((int)discreteInput, new TouchInputSource(InputTypes.Axis2D, directionalInput.ToString(), 0, discreteInput));
            }
          }
          _bindingsByInputType[(int)InputTypes.Axis2D].Add(_inputManager._hashedInputs[(int)discreteInput]);
        }
      }
#endif

    }

    /// <summary>
    /// Notifies the action that one of its input spaces has been inactivated
    /// </summary>
    public void SpaceActiveStateChanged(bool stateActivated, bool pushedState = false)
    {
      int currCount = _activeSpaceCount + (stateActivated ? 1 : -1);
      if (currCount == 0)
      {
        _inputActive = isActive;
      }
      else if (!pushedState && stateActivated && currCount == 1)
      {
        _lastFrameInactive = Time.frameCount;
      }
      _activeSpaceCount = currCount;
    }

    private bool SpacesFlushed()
    {
      if (_inputManager._inputSpaceFlushFrame < Time.frameCount - 1)
        return false;

      for (int i = 0; i < _InputSpaces.Length; i++)
      {
        if (_inputManager._InputSpaceFlushFrames[(int)_InputSpaces[i]] >= Time.frameCount - 1)
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Checks the triggered state for all bindings.  Checks if trigger state was missed while spaces were inactive.
    /// </summary>
    /// <returns></returns>
    private bool AggregateIsTriggered()
    {
      bool spaceActiveLastFrame = !(_lastFrameInactive == Time.frameCount - 1);
      bool triggered = false;

      // Check all key bindings
      foreach (KeyValuePair<int, List<iInputSource>> inputList in _bindingsByInputType)
      {
        for (int i = 0; i < inputList.Value.Count; i++)
        {
          if (inputList.Value[i].isTriggered)
          {
            triggered = true;
          }

          // If active, but not triggered, return false if active last frame
          else if (inputList.Value[i].isActive)
          {
            if (spaceActiveLastFrame)
              return false;
            else if (!_inputActive)
              triggered = true;
          }
        }
      }

      // True if any key-binding was triggered
      return triggered;
    }

    /// <summary>
    /// Checks the triggered state for all bindings.  Checks if trigger state was missed while spaces were inactive.
    /// </summary>
    /// <returns></returns>
    private bool AggregateIsReleased()
    {
      bool spaceActiveLastFrame = !(_lastFrameInactive == Time.frameCount - 1);
      bool released = false;

      // Check all key bindings
      foreach (KeyValuePair<int, List<iInputSource>> inputList in _bindingsByInputType)
      {
        for (int i = 0; i < inputList.Value.Count; i++)
        {
          // If key is active, return false
          if (inputList.Value[i].isActive)
          {
            return false;
          }

          // If current key was released, either this frame or a previous, log it
          else if (inputList.Value[i].isReleased || (_inputActive && !spaceActiveLastFrame))
          {
            released = true;
          }
        }
      }

      // True if all key-bindings inactive and at least one was released
      return released;
    }

    /// <summary>
    /// Returns true if any key-binding is in an active state
    /// </summary>
    /// <returns></returns>
    private bool AggregateIsActive()
    {
      // Check all key bindings
      foreach (KeyValuePair<int, List<iInputSource>> inputList in _bindingsByInputType)
      {
        for (int i = 0; i < inputList.Value.Count; i++)
        {
          if (inputList.Value[i].isActive)
            return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Returns the maximal value of all key-bindings
    /// </summary>
    /// <returns></returns>
    private float AggregateValue()
    {
      float maximalValue = 0f;

      // Check all key bindings
      foreach (KeyValuePair<int, List<iInputSource>> inputList in _bindingsByInputType)
      {
        for (int i = 0; i < inputList.Value.Count; i++)
        {
          float currValue = inputList.Value[i].value;
          if (Mathf.Abs(currValue) > Mathf.Abs(maximalValue))
            maximalValue = currValue;
        }
      }

      return maximalValue;
    }

    /// <summary>
    /// Returns the maximal value of all key-bindings
    /// </summary>
    /// <returns></returns>
    private Vector2 AggregateVelocity()
    {
      Vector2 maximalVelocity = Vector2.zero;

      // Check all key bindings
      foreach (KeyValuePair<int, List<iInputSource>> inputList in _bindingsByInputType)
      {
        for (int i = 0; i < inputList.Value.Count; i++)
        {
          if (!inputList.Value[i].isActive)
          {
            continue;
          }

          Vector2 currVelocity = inputList.Value[i].velocity;
          if (currVelocity.sqrMagnitude > maximalVelocity.sqrMagnitude)
            maximalVelocity = currVelocity;
        }
      }

      return maximalVelocity;
    }

    public bool isTriggered
    {
      get { return (_activeSpaceCount > 0 && !SpacesFlushed()) ? AggregateIsTriggered() : false; }

    }

    public bool isReleased
    {
      get { return _activeSpaceCount > 0 ? AggregateIsReleased() : false; }
    }

    public bool isActive
    {
      get
      {
        return (_activeSpaceCount > 0 &&
                !_inputManager._disabledActions.Exists(c => c == _Action) &&
                !SpacesFlushed()) ? AggregateIsActive() : false;
      }
    }

    public float value
    {
      get { return (_activeSpaceCount > 0 && !SpacesFlushed()) ? AggregateValue() : 0f; }
    }

    public Vector2 velocity
    {
      get
      {
        //Debug.Log("Checking " + _ActionName + " velocity ");
        return (_activeSpaceCount > 0 && !SpacesFlushed()) ? AggregateVelocity() : Vector2.zero;
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
  }
}
