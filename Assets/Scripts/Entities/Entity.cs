using UnityEngine;

public class Entity : MonoBehaviour
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
    protected class TeleportPoint
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
    [SerializeField] protected float maxRunSpeed = 15f;
    [SerializeField] protected float runAcceleration = 2f;

    [Header("JUMP")]
    [SerializeField] protected float[] jumpSpeed;
    [SerializeField] protected float fallSpeed = 240f;
    [SerializeField] protected float secondsBetweenDoubleJump = 0.1f;

    [Header("SPRITE")]
    [SerializeField] protected bool spriteFacingRight = true;

    // State
    protected bool isAlive;
    protected bool isGrounded;
    protected bool isFrozen;
    protected int currentNumberJumps;
    protected int maxNumberJumps;
    protected float allowedJumpTime;
    protected TeleportPoint prevTeleportPoint;

    // Cache
    protected Rigidbody2D myRigidbody;
    protected Animator myAnimator;
    protected Collider2D myCollider2D;
    protected UserController myController;
    protected SpriteRenderer mySpriteRenderer;

    // **********************************************************************
    //                 MONO BEHAVIOUR CLASS OVERLOAD METHODS
    // **********************************************************************

    void Start() { StartE(); }
    protected void StartE()
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
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myCollider2D = GetComponent<Collider2D>();
        myController = GetComponent<UserController>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update() { UpdateE(); }
    protected void UpdateE()
    {
        FallMultiplier(); // apply configurable fall speed so player falls to ground quicker
    }

    private void OnCollisionEnter2D(Collision2D collision) { OnCollisionEnter2DE(collision); }
    protected void OnCollisionEnter2DE(Collision2D collision)
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

    private void OnCollisionExit2D(Collision2D collision) { OnCollisionEnter2DE(collision); }
    protected void OnCollisionExit2DE(Collision2D collision)
    {
        // If leaving the ground:
        //  1. Update the isGrounded state variable.
        if (!myCollider2D.IsTouchingLayers(LayerMask.GetMask(GlobalConfigs.LAYER_COLLISION_GROUND)))
        {
            this.isGrounded = false;
        }
    }

    // **********************************************************************
    //                    PROTECTED METHODS / COROUTINES
    // **********************************************************************


    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: FallMultiplier
     * If the player begins falling, this function adds the fallSpeed 
     * multiplier
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    protected void FallMultiplier()
    {
        // only apply falling multiplier if player is already falling
        if (myRigidbody.velocity.y < Mathf.Epsilon)
        {
            myRigidbody.velocity += Vector2.down * fallSpeed * Time.deltaTime;
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: FreezePlayer
     * Sets the player's X velocity to 0 so player isn't stuck moving
     * left or right while frozen.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    protected void FreezePlayer()
    {
        this.isFrozen = true;
        myRigidbody.velocity = new Vector2(0f, myRigidbody.velocity.y);
        myAnimator.SetBool(GlobalConfigs.ANIMATION_PLAYER_RUNNING, false);
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: Jump
     * This function adds upward force to the rigidbody when called.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    protected void Jump(float jumpPowerCustom)
    {
        // Add upward force to player so it jumps.
        Vector2 jumpVelocityToAdd = new Vector2(0f, jumpPowerCustom);
        myRigidbody.velocity += jumpVelocityToAdd;
    }

    // **********************************************************************
    //                      PUBLIC METHODS / COROUTINES
    // **********************************************************************

    /* 
      * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
      * SUMMARY: HandleRun
      * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     */
    public virtual void HandleRun(float controlThrow)
    {
        if (isFrozen) { return; }

        // Calculate new velocity using acceleration with a max run speed
        float xVelocity = myRigidbody.velocity.x + controlThrow * this.runAcceleration;
        float xVelocityClamped = Mathf.Clamp(xVelocity, -1f * this.maxRunSpeed, this.maxRunSpeed);
        Vector2 playerVelocity = new Vector2(xVelocityClamped, myRigidbody.velocity.y);

        myRigidbody.velocity = playerVelocity; // assign new run speed to player

        // Tell animator when to play run animator
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) >= Mathf.Epsilon;
        myAnimator.SetBool(GlobalConfigs.ANIMATION_PLAYER_RUNNING, playerHasHorizontalSpeed);

        // check if player's sprite should be flipped to face direction it is moving
        if (playerHasHorizontalSpeed)
        {
            // Sprite direction is implemented using XNOR boolean logic
            // XNOR Truth table reference: https://en.scratch-wiki.info/wiki/Truth_Table
            bool A = Mathf.Sign(myRigidbody.velocity.x) <= 0.0f;
            bool B = this.spriteFacingRight;
            mySpriteRenderer.flipX = A == B; // XNOR logic
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleJump
     * This function checks if the player is trying to jump and calls
     * the jump function if the player is allowed to jump.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public virtual void HandleJump()
    {
        if (isFrozen) { return; }

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
        if (this.currentNumberJumps < this.maxNumberJumps)
        {
            Jump(this.jumpSpeed[currentNumberJumps]);

            // Increment counter to make sure player doesn't make more jumps
            // in a row than allowed.
            this.currentNumberJumps++;
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleRanged
     * Ranged attack varies from character to character.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public virtual void HandleRanged(Vector2 cursorPos)
    {
        Debug.Log("ERROR (Entity.HandleRanged): Method not implemented for base class.");
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleSpecial
     * Special ability varies from character to character.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public virtual void HandleSpecial(Vector2 cursorPos)
    {
        Debug.Log("ERROR (Entity.HandleSpecial): Method not implemented for base class.");
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: UpdateTeleportPoint
     * 1. Teleports the player to the specified location.
     * 2. Updates the previousTeleportLocation with valid and new location.
     * 3. Set player cameras.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public virtual void TeleportHere(Vector2 location, Vector2 velocity)
    {
        // save current location as the previous teleport point in case player
        // tries to teleport back.
        this.prevTeleportPoint.valid = true;
        this.prevTeleportPoint.location = transform.position;

        // play teleportation animation

        // play teleportation sound

        // teleport to the requested location and add the requested velocity
        transform.position = location;
        myRigidbody.velocity = velocity;

        // enable player movement again
        this.isFrozen = false;
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: RequestControl
     * After calling this function, the UserController will be enabled
     * and the player will be able to control this game object.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public virtual void SetControl(bool enabled)
    {
        myController.SetControllable(enabled);
    }
}
