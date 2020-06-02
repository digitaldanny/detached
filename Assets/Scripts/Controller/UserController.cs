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
    [SerializeField] bool cusorEnabled = true;
    [SerializeField] GameObject cursor; // cursor prefab

    // State
    bool isControllable = true; // Serialized for debugging
    [SerializeField] Entity entity;

    // Cache
    GameObject cursorObj; // instantiated cursor game object
    Cursor cursorComponent; // cursor component of the game object

    // **********************************************************************
    //                 MONO BEHAVIOUR CLASS OVERLOAD METHODS
    // **********************************************************************

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

        UpdateCursor(); // update position of cursor based on mouse position.
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
        if (entity)
        {
            entity.HandleRun(controlThrow);
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

    // Fire1 reaction (left mouse click)
    private void HandleRanged()
    {
        // Check if the player clicked the fire button
        if (CrossPlatformInputManager.GetButtonDown(GlobalConfigs.CONTROLLER_FIRE1))
        {
            Vector2 cursorPos = Camera.main.ScreenToWorldPoint(CrossPlatformInputManager.mousePosition);
            if (entity)
            {
                entity.HandleRanged(cursorPos);
            }
        }
    }

    // Fire2 reaction (right mouse click)
    private void HandleSpecial()
    {
        // Check if player presses teleport button
        if (CrossPlatformInputManager.GetButtonDown(GlobalConfigs.CONTROLLER_FIRE2))
        {
            Vector2 cursorPos = Camera.main.ScreenToWorldPoint(CrossPlatformInputManager.mousePosition);
            if (entity)
            {
                entity.HandleSpecial(cursorPos);
            }
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: UpdateCursor
     * Update the position of the cursor based on position of the mouse
     * AND position of the selected entity.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void UpdateCursor()
    {
        if (this.cusorEnabled)
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
            // mouse position.
            cursorComponent.UpdatePosition(
                entity.GetPosition(),
                Camera.main.ScreenToWorldPoint(CrossPlatformInputManager.mousePosition)
            );
        }

        // Destroy cursor if it is not enabled
        else if (cursorObj)
        {
            Destroy(cursorObj);
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

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: SetEntity
     * Sets the entity that will be controlled.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public bool SetEntity(Entity entity)
    {
        this.entity = entity;
        return true;
    }    
}
