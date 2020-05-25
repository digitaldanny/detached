using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Player : MonoBehaviour
{

    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    // Configs
    [Header("RUN")]
    [SerializeField] float runSpeed = 5f;

    [Header("JUMP")]
    [SerializeField] float[] jumpPower; // jump power for each jump allowed (useful for double jumping)
    [SerializeField] float secondsBetweenDoubleJump = 0.1f;

    [Header("SPIRIT")]
    [SerializeField] GameObject spirit;
    [SerializeField] float launchPower = 10f;
    [SerializeField] float verticalLaunchOffset = 0.5f; // Offset from player at launch so there isn't 
                                                        // an instant floor collision.

    // State
    bool isAlive; // is the player alive?
    bool isGrounded; // is the player touching the ground?
    bool isFrozen; // is the player able to move?
    
    int currentNumberJumps; // how many times has the player jumped since leaving the ground?
    int maxNumberJumps; // how many jumps is the player allowed to do before touching the ground?
    
    float allowedJumpTime; // allow the player to jump again after delay. Avoids spamming double jumps.

    // Cache
    Rigidbody2D myRigidBody;
    Animator myAnimator;
    Collider2D myCollider2D;

    // **********************************************************************
    //                           OVERLOAD METHODS
    // **********************************************************************

    void Start()
    {
        // Set up states
        currentNumberJumps = 0;
        isAlive = true;
        isGrounded = true;
        isFrozen = false;
        maxNumberJumps = jumpPower.Length;
        allowedJumpTime = 0;

        // Set up cache
        myRigidBody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myCollider2D = GetComponent<Collider2D>();
    }

    void Update()
    {
        // Player movement that can only happen when the player is not frozen.
        if (!isFrozen)
        {
            HandleRun(); // check if player is running
            HandleJump(); // check if player jumped
            CheckFlipSprite(); // flip sprite depending on X axis move direction
            HandleSpiritLaunch(); // handles direction and strength of spirit launch
        }

        // Stuff that player can do even if frozen.
        else
        {

        }    
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If landing on the ground:
        //  1. Reset current number of jumps so player can perform double jumps.
        //  2. Update the isGrounded state variable.
        if (myCollider2D.IsTouchingLayers(LayerMask.GetMask(GlobalConfigs.LAYER_COLLISION_GROUND)))
        {
            this.currentNumberJumps = 0;
            this.isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // If leaving the ground:
        //  1. Update the isGrounded state variable.
        if (!myCollider2D.IsTouchingLayers(LayerMask.GetMask(GlobalConfigs.LAYER_COLLISION_GROUND)))
        {
            this.isGrounded = false;
        }
    }

    // **********************************************************************
    //                         METHODS / COROUTINES
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleRun
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void HandleRun()
    {
        float controlThrow = CrossPlatformInputManager.GetAxis("Horizontal"); // left to right = -1 to +1

        // hardcoding y to 0 would make jump buggy
        Vector2 playerVelocity = new Vector2(controlThrow*runSpeed, myRigidBody.velocity.y); 

        myRigidBody.velocity = playerVelocity;

        // Tell animator when to play run animator
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;
        myAnimator.SetBool(GlobalConfigs.ANIMATION_RUNNING, playerHasHorizontalSpeed);
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleJump, Jump
     * -------------------------------------------------------------------
     * HandleJump:
     * This function checks if the player is trying to jump and calls
     * the jump function if the player is allowed to jump.
     * -------------------------------------------------------------------
     * Jump:
     * This function add the upward force to the player's physics body
     * to jump.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void HandleJump()
    {
        // Check if player has pressed the jump button
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            // Check if the player has met the delay requirement between jumps.
            // If he has, update the allowed jump time again for future jumps.
            // If he has not, return and wait for allowed jump time.
            if (Time.time >= this.allowedJumpTime)
                this.allowedJumpTime = Time.time + this.secondsBetweenDoubleJump;
            else
                return;

            // Player is always allowed to jump if touching the ground.
            // If already in the air, player can only jump if he hasn't met the
            // max number of jumps already.
            if (this.isGrounded || this.currentNumberJumps < this.maxNumberJumps)
            {
                Debug.Log(this.jumpPower[currentNumberJumps]);
                Jump(this.jumpPower[currentNumberJumps]);
            }
        }
    }

    private void Jump(float jumpPowerCustom)
    {
        // Add upward force to player so it jumps.
        Vector2 jumpVelocityToAdd = new Vector2(0f, jumpPowerCustom);
        myRigidBody.velocity += jumpVelocityToAdd;

        // Increment counter to make sure player doesn't make more jumps
        // in a row than allowed.
        this.currentNumberJumps++;
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: CheckFlipSprite
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void CheckFlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon;

        // if player is moving horizontally
        if (playerHasHorizontalSpeed)
        {
            // reverse the current scaling of the x axis
            transform.localScale = new Vector2(Mathf.Sign(myRigidBody.velocity.x), 1f);
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleSpiritLaunch, FreezePlayer
     * -------------------------------------------------------------------
     * HandleSpiritLaunch:
     * Launches spirit towards the mouse and stops player from being able
     * to move until a signal is sent to allow the player to control the
     * character again.
     * -------------------------------------------------------------------
     * FrezePlayer:
     * Sets the player's X velocity to 0 so player isn't stuck moving
     * left or right while frozen.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void HandleSpiritLaunch()
    {
        // Check if the player clicked the fire button
        if (!CrossPlatformInputManager.GetButtonDown("Fire1"))
            return;

        // Do not allow the player to move after launching the spirit
        FreezePlayer();

        // Instantiate the detached spirit into the hierarchy.
        GameObject spiritLaunched = Instantiate(
            spirit,
            transform.position + new Vector3(0f, verticalLaunchOffset, 0f),
            Quaternion.identity
        ) as GameObject;

        // Get the target vector
        Vector2 targetPos = Camera.main.ScreenToWorldPoint(CrossPlatformInputManager.mousePosition);
        Vector2 myPos = transform.position;
        Vector2 direction = (targetPos - myPos);
        direction.Normalize();

        // Launch the spirit towards where the cursor was aiming.
        Rigidbody2D spiritRigidBody = spiritLaunched.gameObject.GetComponent<Rigidbody2D>();
        spiritRigidBody.velocity += direction*launchPower;
    }

    private void FreezePlayer()
    {
        this.isFrozen = true;
        myRigidBody.velocity = new Vector2(0f, myRigidBody.velocity.y);
        myAnimator.SetBool(GlobalConfigs.ANIMATION_RUNNING, false);
    }
}
