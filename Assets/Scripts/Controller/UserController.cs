using MyBox;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[CreateAssetMenu]
public class ControllerState : ScriptableObject
{
    public  Entity                   entityToControl;   // this controller connects directly to Entity objects.
    public  ControllerInputEnables   inputEn;           // are specific buttons or joysticks enabled?
    public  string                   controllerType;    // targeting XBOX controller or keyboard?
    public  bool                     cursorEnable;      // should the cursor be visible around the player?
    public  Vector2                  aimDir;            // what direction is the player aiming?

    public ControllerState()
    {
        entityToControl     = null;                                         // controller initially doesn't control anything
        inputEn             = new ControllerInputEnables();                 // holds button+axis enables (all disabled by default)
        controllerType      = GlobalConfigs.CONTROLLER_TYPE_XBOX360;
        cursorEnable        = false;                                        // if true, Cursor.UpdatePosition is called to visualize aimDir.
        aimDir              = new Vector2(0f, 0f);                          // direction that player is aiming cursor
    }
}

[System.Serializable]
public class ControllerInputEnables
{
    // default constructor
    public ControllerInputEnables() { all = false; }

    // -------- DATA --------- //

    // joysticks
    public bool jsLeftX;
    public bool jsLeftY;

    // buttons
    public bool bX;
    public bool bY;
    public bool bB;
    public bool bA;

    // setter for entire control system 
    public bool all
    {
        set
        {
            jsLeftX = value;
            jsLeftY = value;

            bX = value;
            bY = value;
            bB = value;
            bA = value;
        }
    }
}

public class UserController : MonoBehaviour
{
    // **********************************************************************
    //                       PRIVATE CLASS PARAMETERS
    // **********************************************************************

    // Configs
    [Header("CURSOR")]
    [MustBeAssigned]
    [SerializeField] 
    private GameObject _cursorPrefab;
    
    [SerializeField]  
    private Vector2 _defaultCursorDir;

    // State
    [MustBeAssigned]
    [SerializeField] 
    private ControllerState _state;

    // Cache
    private GameObject _cursorObj; // instantiated cursor game object
    private Cursor _cursorComponent; // cursor component of the game object

    // **********************************************************************
    //                      PUBLIC GETTERS / SETTERS
    // **********************************************************************

    public ControllerState state
    {
        get => _state;
        set { _state = value; }
    }

    // **********************************************************************
    //                MONO BEHAVIOUR CLASS OVERRIDE METHODS
    // **********************************************************************

    private void Update()
    {
        if (_state.entityToControl != null) // important check for when player is destroyed before a respawn
        {
            // Cursor
            UpdateCursorDir(); // update cursorDir state variable based on joystick OR mouse.
            UpdateCursorSprite(); // update joystick+mouse position AND position of cursor if enabled. 

            // Movement
            HandleRun(); // check if player is running or if sprite should be flipped
            HandleVertical(); // check if player is using left joystick's Y axis
            HandleJump(); // check if player jumped
            HandleRanged(); // check if ranged attack was used
            HandleSpecial(); // check if special ability was used
            HandlePunch(); // check if punch was used
        }
    }

    // **********************************************************************
    //                         PRIVATE METHODS
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: Handle<Input>
     * These functions call the appropriate Entity methods based on
     * user inputs. The user controls can be configured using the project
     * settings in "Edit>Project Settings>Input Manager>Axes."
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */

    private void HandleRun()
    {
        // Check if this input is enabled
        if (!_state.inputEn.jsLeftX) return;

        // left to right = -1 to +1
        float controlThrow = CrossPlatformInputManager.GetAxis(GlobalConfigs.CONTROLLER_LEFT_JOYSTICK_X);
        float keyboardThrow = CrossPlatformInputManager.GetAxisRaw(GlobalConfigs.CONTROLLER_HORIZONTAL);

        // Determine if input should come from keyboard or gamepad and which controller
        // threshold to use.
        bool usingGamepad = Mathf.Abs(controlThrow) > Mathf.Abs(keyboardThrow);
        float finalThrow = (usingGamepad) ? controlThrow : keyboardThrow;
        float finalThreshold = (usingGamepad) ? GlobalConfigs.CONTROLLER_JOYSTICK_THRESHOLD : GlobalConfigs.CONTROLLER_KEYBOARD_THRESHOLD;

        // Forcing gamepad and keyboard input to either -1, 0, or 1 will interpret well
        // to keyboard input.
        float maxThrow = 1.0f;
        float noMovement = 0.0f;

        // Set throw to max left or right values so that gameplay is identical for 
        // gamepad and keyboard controls.
        if (finalThrow >= finalThreshold)
            finalThrow = maxThrow;
        else if (finalThrow <= -1f * finalThreshold)
            finalThrow = -1f * maxThrow;
        else
            finalThrow = noMovement;

        // Actual run implementation varies between entities.
        if (_state.entityToControl) { _state.entityToControl.HandleRun(finalThrow); }
    }

    private void HandleVertical()
    {
        // Check if this input is enabled
        if (!_state.inputEn.jsLeftY) return;

        // left to right = -1 to +1
        float controlThrow = CrossPlatformInputManager.GetAxis(GlobalConfigs.CONTROLLER_LEFT_JOYSTICK_Y);
        if (_state.entityToControl)
        {
            _state.entityToControl.HandleVertical(controlThrow);
        }
    }

    private void HandleJump()
    {
        // Check if this input is enabled
        if (!_state.inputEn.bA) return;

        // Check if player has pressed the jump button
        if (CrossPlatformInputManager.GetButtonDown(GlobalConfigs.CONTROLLER_JUMP))
        {
            if (_state.entityToControl)
            {
                _state.entityToControl.HandleJump();
            }
        }
    }

    private void HandleRanged()
    {
        // Check if this input is enabled
        if (!_state.inputEn.bY) return;

        // Check if the player clicked the fire button
        if (CrossPlatformInputManager.GetButtonDown(GlobalConfigs.CONTROLLER_RANGED))
        {
            if (_state.entityToControl)
            {
                _state.entityToControl.HandleRangedDown(_state.aimDir);
            }
        }

        if (CrossPlatformInputManager.GetButtonUp(GlobalConfigs.CONTROLLER_RANGED))
        {
            if (_state.entityToControl)
            {
                _state.entityToControl.HandleRangedUp(_state.aimDir);
            }
        }
    }

    private void HandleSpecial()
    {
        // Check if this input is enabled
        if (!_state.inputEn.bB) return;

        // Check if player presses teleport button
        if (CrossPlatformInputManager.GetButtonDown(GlobalConfigs.CONTROLLER_SPECIAL))
        {
            if (_state.entityToControl)
            {
                _state.entityToControl.HandleSpecialDown(_state.aimDir);
            }
        }
    }

    private void HandlePunch()
    {
        // Check if this input is enabled
        if (!_state.inputEn.bX) return;

        // Check if player presses teleport button
        if (CrossPlatformInputManager.GetButtonDown(GlobalConfigs.CONTROLLER_PUNCH))
        {
            Debug.Log("ERROR (UserController.HandlePunch): Punch not implemented for controller.");
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: UpdateCursorSprite
     * Update the position of the cursor based on position of the mouse/joystick
     * AND position of the selected entity.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void UpdateCursorSprite()
    {
        // Update cursor sprite position if enabled.
        if (_state.cursorEnable)
        {
            if (!_cursorObj)
            {
                // if the cursor hasn't been instantiated, instantiate as child here.
                _cursorObj = Instantiate(
                    _cursorPrefab,
                    _state.entityToControl.transform.position,
                    Quaternion.identity,
                    transform
                ) as GameObject;

                _cursorComponent = _cursorObj.GetComponent<Cursor>();
            }

            // update the cursor position based on player's center point and the 
            // mouse+joystick position.
            _cursorComponent.UpdatePosition(
                _state.entityToControl.GetPosition(),
                _state.aimDir
            );
        }

        // Destroy cursor if it is not enabled
        else if (_cursorObj)
        {
            Destroy(_cursorObj);
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: UpdateCursorDir
     * Update cursorDir state variable based on whether the mouse or
     * right joystick is being used.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void UpdateCursorDir()
    {
        switch(_state.controllerType)
        {
            case GlobalConfigs.CONTROLLER_TYPE_XBOX360:

                // Capture joystick values
                float joystickLeftX = CrossPlatformInputManager.GetAxis(GlobalConfigs.CONTROLLER_LEFT_JOYSTICK_X);
                float joystickLeftY = CrossPlatformInputManager.GetAxis(GlobalConfigs.CONTROLLER_LEFT_JOYSTICK_Y);

                // if being controlled by the joysticks, don't change the angle if
                // the joystick has recentered.
                if (Mathf.Abs(joystickLeftX) <= GlobalConfigs.CONTROLLER_JOYSTICK_THRESHOLD &&
                    Mathf.Abs(joystickLeftY) <= GlobalConfigs.CONTROLLER_JOYSTICK_THRESHOLD)
                {
                    break;
                }
                else
                {
                    // Joystick may not be at max value based on player input, so calculate
                    // the angle and produce the max vector based on the angle.
                    float theta = Mathf.Atan2(joystickLeftY, joystickLeftX);
                    _state.aimDir.x = Mathf.Cos(theta);
                    _state.aimDir.y = Mathf.Sin(theta);
                }
                break;

            case GlobalConfigs.CONTROLLER_TYPE_KEYBOARD:

                // Capture mouse position
                Vector2 cursorPos = Camera.main.ScreenToWorldPoint(CrossPlatformInputManager.mousePosition);

                // Calculate angle that cursor should be moved to if using the mouse
                // to aim.
                Vector2 entityPos = _state.entityToControl.GetPosition();
                float deltaX = cursorPos.x - entityPos.x;
                float deltaY = cursorPos.y - entityPos.y;
                float thetaRadians = Mathf.Atan2(deltaY, deltaX);

                // Calculate the new cursor position if using mouse.
                _state.aimDir.x = Mathf.Cos(thetaRadians);
                _state.aimDir.y = Mathf.Sin(thetaRadians);
                break;

            default:
                Debug.Log("ERROR (UserController.UpdateCursorDir): Unknown controller type.");
                break;
        }    
    }
}
