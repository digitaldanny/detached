using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class UserController : MonoBehaviour
{
    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    // Configs

    // State
    [SerializeField] bool isControllable = false; // Serialized for debugging

    // Cache
    [SerializeField] Entity entity; // entity to control

    // **********************************************************************
    //                 MONO BEHAVIOUR CLASS OVERLOAD METHODS
    // **********************************************************************

    private void Start()
    {
        entity = GetComponent<Entity>(); // get the first component derived from Entity
    }

    private void Update()
    {
        // Player movement that can only happen when the player is not frozen.
        if (this.isControllable)
        {
            HandleRun(); // check if player is running or if sprite should be flipped
            HandleJump(); // check if player jumped
            HandleRanged(); // check if ranged attack was used
            HandleSpecial(); // check if special ability was used
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
        float controlThrow = CrossPlatformInputManager.GetAxis(GlobalConfigs.CONTROLLER_HORIZONTAL);
        entity.HandleRun(controlThrow);
    }

    private void HandleJump()
    {
        // Check if player has pressed the jump button
        if (CrossPlatformInputManager.GetButtonDown(GlobalConfigs.CONTROLLER_JUMP))
        {
            entity.HandleJump();
        }
    }

    // Fire1 reaction (left mouse click)
    private void HandleRanged()
    {
        // Check if the player clicked the fire button
        if (CrossPlatformInputManager.GetButtonDown(GlobalConfigs.CONTROLLER_FIRE1))
        {
            Vector2 cursorPos = Camera.main.ScreenToWorldPoint(CrossPlatformInputManager.mousePosition);
            entity.HandleRanged(cursorPos);
        }
    }

    // Fire2 reaction (right mouse click)
    private void HandleSpecial()
    {
        // Check if player presses teleport button
        if (CrossPlatformInputManager.GetButtonDown(GlobalConfigs.CONTROLLER_FIRE2))
        {
            Vector2 cursorPos = Camera.main.ScreenToWorldPoint(CrossPlatformInputManager.mousePosition);
            entity.HandleSpecial(cursorPos);
        }
    }

    // **********************************************************************
    //                          PUBLIC METHODS
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: SetControllable
     * Allows user to control Entity game object if set true.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public bool SetControllable(bool enabled)
    {
        this.isControllable = enabled;
        return true;
    }
}
