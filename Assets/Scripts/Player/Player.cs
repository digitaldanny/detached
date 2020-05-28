using System;
using UnityEditor.U2D.Path.GUIFramework;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Player : MonoBehaviour
{
    // **********************************************************************
    //                          CLASS STRUCTURES
    // **********************************************************************

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: TeleportPoint
     * @valid
     *  Is the player allowed to teleport to the point?
     * @location
     *  Transform position to teleport to.
     * @velocity
     *  Velocity that the spirit was traveling at when player chose to 
     *  teleport.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    class TeleportPoint
    {
        public bool valid;
        public Vector2 location;
        public Vector2 velocity;

        // default constructor
        public TeleportPoint()
        {
            valid = false;
            location = new Vector2(0f, 0f);
            velocity = new Vector2(0f, 0f);
        }
    }

    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * CLASS PARAMETER DESCRIPTIONS
     * -------------------------------------------------------------------
     * CONFIGS:
     * @runSpeed
     *  .
     * @jumpPower
     *  Jump power for each jump allowed (useful for double jumping).
     * @secondsBetweenDoubleJump:
     *  .
     * @spirit
     *  .
     * @launchPower
     *  .
     * @verticalLaunchOffset
     *  Offset from player at launch so there isn't an instant floor collision.
     * @maxTimeInSpiritForm
     *  Time that user is allowed to stay in spirit form.
     * @spiritRechargeDelay
     *  Time between recharges in seconds.
     * @spiritRechargeAmount
     *  Amount of time to recharge spirit after every spiritRechargeDelay.
     * -------------------------------------------------------------------
     * STATES
     * @isAlive
     *  Is the player alive?
     * @isGrounded
     *  Is the player touching the ground?
     * @isFrozen
     *  Is the player able to move?
     * @currentNumberJumps
     *  How many times has the player jumped since leaving the ground?
     * @maxNumberJumps
     *  How many jumps is the player allowed to do before touching the ground?
     *  This is based on how many jumpPowers are defined in config variables.
     * @allowedJumpTime
     *  Allow the player to jump again after delay. Avoids spamming double jumps.
     * @previousTeleportPoint
     *  Keeps track of previous position that the player teleported from so the 
     *  player can return.
     * -------------------------------------------------------------------
     * CACHE
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */

    // Configs
    [Header("RUN")]
    [SerializeField] float maxRunSpeed = 5f;
    [SerializeField] float runAcceleration = 1f;

    [Header("JUMP")]
    [SerializeField] float[] jumpSpeed;
    [SerializeField] float fallSpeed = 5f;
    [SerializeField] float secondsBetweenDoubleJump = 0.1f;

    [Header("SPIRIT")]
    [SerializeField] GameObject spirit;
    [SerializeField] float launchPower = 10f;
    [SerializeField] float verticalLaunchOffset = 0.5f;
    [SerializeField] float maxTimeInSpiritForm = 5f; 
    [SerializeField] float spiritRechargeDelay = 2f; 
    [SerializeField] float spiritRechargeAmount = 0.5f;

    // State
    bool isAlive;
    bool isGrounded;
    bool isFrozen; 
    int currentNumberJumps; 
    int maxNumberJumps; 
    float allowedJumpTime;
    TeleportPoint prevTeleportPoint;

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
        maxNumberJumps = jumpSpeed.Length;
        allowedJumpTime = 0;
        prevTeleportPoint = new TeleportPoint();

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
            HandleTeleportUndo(); // check if player is trying to teleport to previous location
            HandleRun(); // check if player is running
            HandleJump(); // check if player jumped
            CheckFlipSprite(); // flip sprite depending on X axis move direction
            HandleSpiritLaunch(); // handles direction and strength of spirit launch
        }

        // Stuff that player should only do if frozen.
        else
        {

        }

        // Stuff player should do whether frozen or not.
        FallMultiplier(); // if player is falling, add more velocity toward ground
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
    //                      PRIVATE METHODS / COROUTINES
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleRun
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void HandleRun()
    {
        float controlThrow = CrossPlatformInputManager.GetAxis(GlobalConfigs.CONTROLLER_HORIZONTAL); // left to right = -1 to +1

        // Calculate new velocity using acceleration with a max run speed
        float xVelocity = myRigidBody.velocity.x + controlThrow * this.runAcceleration;
        float xVelocityClamped = Mathf.Clamp(xVelocity, -1f*this.maxRunSpeed, this.maxRunSpeed);
        Vector2 playerVelocity = new Vector2(xVelocityClamped, myRigidBody.velocity.y);

        myRigidBody.velocity = playerVelocity; // assign new run speed to player

        // Tell animator when to play run animator
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) >= GlobalConfigs.Epsilon;
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
        if (CrossPlatformInputManager.GetButtonDown(GlobalConfigs.CONTROLLER_JUMP))
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
                Jump(this.jumpSpeed[currentNumberJumps]);
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
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidBody.velocity.x) > GlobalConfigs.Epsilon;

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
        if (!CrossPlatformInputManager.GetButtonDown(GlobalConfigs.CONTROLLER_FIRE1))
            return;

        // Do not allow the player to move after launching the spirit
        FreezePlayer();

        // Instantiate the detached spirit into the hierarchy as a child of
        // player GameObject.
        GameObject spiritLaunched = Instantiate(
            spirit,
            transform.position + new Vector3(0f, verticalLaunchOffset, 0f),
            Quaternion.identity,
            transform
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

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: TeleportUndo
     * 1. Teleports to the player's previous location if the sport is still
     *  valid.
     * 2. Set player camera.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void HandleTeleportUndo()
    {
        // Check if player presses teleport button
        if (CrossPlatformInputManager.GetButtonDown(GlobalConfigs.CONTROLLER_FIRE2))
        {
            // check if teleport point is still valid
            if (this.prevTeleportPoint.valid)
            {
                this.prevTeleportPoint.valid = false; // make sure players can't teleport to same location twice
                transform.position = this.prevTeleportPoint.location; // teleport
            }
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: FallMultiplier
     * If the player begins falling, this function adds the fallSpeed 
     * multiplier
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void FallMultiplier()
    {
        // only apply falling multiplier if player is already falling
        if (myRigidBody.velocity.y < Mathf.Epsilon)
        {
            myRigidBody.velocity += Vector2.down * fallSpeed * Time.deltaTime;
        }
    }

    // **********************************************************************
    //                      PUBLIC METHODS / COROUTINES
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: TeleportHere
     * 1. Teleports the player to the specified location.
     * 2. Updates the previousTeleportLocation with valid and new location.
     * 3. Set player cameras.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public void TeleportHere(Vector2 location, Vector2 velocity)
    {
        // save current location as the previous teleport point in case player
        // tries to teleport back.
        this.prevTeleportPoint.valid = true;
        this.prevTeleportPoint.location = transform.position;

        // play teleportation animation

        // play teleportation sound

        // teleport to the requested location and add the requested velocity
        transform.position = location;
        myRigidBody.velocity = velocity;

        // enable player movement again
        this.isFrozen = false;
    }
}
