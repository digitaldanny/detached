using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class UserController : MonoBehaviour
{
    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    // Configs
    [Header("CURSOR")]
    [SerializeField] bool cursorEnabled = true;
    [SerializeField] GameObject cursor; // cursor prefab
    [SerializeField] Vector2 defaultCursorDir;

    // State

    string controllerType; // is game being controlled by keyboard/mouse or xbox 360?
    Vector2 cursorDir; // direction that right joystick OR mouse is pointing at
    bool isControllable;
    Entity entity;

    // Cache
    GameObject cursorObj; // instantiated cursor game object
    Cursor cursorComponent; // cursor component of the game object

    // **********************************************************************
    //                MONO BEHAVIOUR CLASS OVERRIDE METHODS
    // **********************************************************************

    private void Start()
    {
        controllerType = GlobalConfigs.CONTROLLER_TYPE_XBOX360;
        cursorDir = defaultCursorDir;
    }

    private void Update()
    {
        UpdateCursorDir(); // update cursorDir state variable based on joystick OR mouse.
        UpdateCursorSprite(); // update joystick+mouse position AND position of cursor if enabled. 

        // Player movement that can only happen when the player is not frozen.
        if (this.isControllable)
        {
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
        if (entity) { entity.HandleRun(finalThrow); }
    }

    private void HandleVertical()
    {
        // left to right = -1 to +1
        float controlThrow = CrossPlatformInputManager.GetAxis(GlobalConfigs.CONTROLLER_LEFT_JOYSTICK_Y);
        if (entity)
        {
            entity.HandleVertical(controlThrow);
        }
    }

    private void HandleJump()
    {
        // Check if player has pressed the jump button
        if (CrossPlatformInputManager.GetButtonDown(GlobalConfigs.CONTROLLER_JUMP))
        {
            if (entity)
            {
                entity.HandleJump();
            }
        }
    }

    private void HandleRanged()
    {
        // Check if the player clicked the fire button
        if (CrossPlatformInputManager.GetButtonDown(GlobalConfigs.CONTROLLER_RANGED))
        {
            if (entity)
            {
                entity.HandleRanged(this.cursorDir);
            }
        }
    }

    private void HandleSpecial()
    {
        // Check if player presses teleport button
        if (CrossPlatformInputManager.GetButtonDown(GlobalConfigs.CONTROLLER_SPECIAL))
        {
            if (entity)
            {
                entity.HandleSpecial(this.cursorDir);
            }
        }
    }

    private void HandlePunch()
    {
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
        if (this.cursorEnabled)
        {
            if (!cursorObj)
            {
                // if the cursor hasn't been instantiated, instantiate as child here.
                cursorObj = Instantiate(
                    cursor,
                    entity.transform.position,
                    Quaternion.identity,
                    transform
                ) as GameObject;

                cursorComponent = cursorObj.GetComponent<Cursor>();
            }

            // update the cursor position based on player's center point and the 
            // mouse+joystick position.
            cursorComponent.UpdatePosition(
                entity.GetPosition(),
                this.cursorDir
            );
        }

        // Destroy cursor if it is not enabled
        else if (cursorObj)
        {
            Destroy(cursorObj);
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
        switch(controllerType)
        {
            case GlobalConfigs.CONTROLLER_TYPE_XBOX360:

                // Capture joystick values
                float joystickRightX = CrossPlatformInputManager.GetAxis(GlobalConfigs.CONTROLLER_RIGHT_JOYSTICK_X);
                float joystickRightY = CrossPlatformInputManager.GetAxis(GlobalConfigs.CONTROLLER_RIGHT_JOYSTICK_Y);

                // if being controlled by the joysticks, don't change the angle if
                // the joystick has recentered.
                if (Mathf.Abs(joystickRightX) <= GlobalConfigs.CONTROLLER_JOYSTICK_THRESHOLD &&
                    Mathf.Abs(joystickRightY) <= GlobalConfigs.CONTROLLER_JOYSTICK_THRESHOLD)
                {
                    break;
                }
                else
                {
                    // Joystick may not be at max value based on player input, so calculate
                    // the angle and produce the max vector based on the angle.
                    float theta = Mathf.Atan2(joystickRightY, joystickRightX);
                    this.cursorDir.x = Mathf.Cos(theta);
                    this.cursorDir.y = Mathf.Sin(theta);
                }
                break;

            case GlobalConfigs.CONTROLLER_TYPE_KEYBOARD:

                // Capture mouse position
                Vector2 cursorPos = Camera.main.ScreenToWorldPoint(CrossPlatformInputManager.mousePosition);

                // Calculate angle that cursor should be moved to if using the mouse
                // to aim.
                Vector2 entityPos = entity.GetPosition();
                float deltaX = cursorPos.x - entityPos.x;
                float deltaY = cursorPos.y - entityPos.y;
                float thetaRadians = Mathf.Atan2(deltaY, deltaX);

                // Calculate the new cursor position if using mouse.
                this.cursorDir.x = Mathf.Cos(thetaRadians);
                this.cursorDir.y = Mathf.Sin(thetaRadians);
                break;

            default:
                Debug.Log("ERROR (UserController.UpdateCursorDir): Unknown controller type.");
                break;
        }    
    }

    // **********************************************************************
    //                          PUBLIC METHODS
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: Set{Property}
     * Setter function for various controller attributes.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public bool SetControllable(bool enabled)
    {
        this.isControllable = enabled;
        return true;
    }
    public bool SetEntity(Entity entity)
    {
        this.entity = entity;
        return true;
    }    

    public bool SetCursorEnable(bool enabled)
    {
        this.cursorEnabled = enabled;
        return true;
    }
}
