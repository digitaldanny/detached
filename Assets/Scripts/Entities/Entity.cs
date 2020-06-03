using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Entity : MonoBehaviour
{

    // **********************************************************************
    //                          CLASS STRUCTURES
    // **********************************************************************

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: ControllerConfigs
     * @controllerEnabled
     *  
     * @entityToControl
     * @cursorEnabled
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    protected class ControllerConfigs
    {
        public bool controllerEnabled;
        public Entity entityToControl;
        public bool cursorEnabled;

        // default constructor
        public ControllerConfigs()
        {
            controllerEnabled = false;
            entityToControl = null;
            cursorEnabled = false;
        }

        // parameter constructor
        public ControllerConfigs(
            bool controllerEnabled,
            Entity entityToControl,
            bool cursorEnabled)
        {
            this.controllerEnabled = controllerEnabled;
            this.entityToControl = entityToControl;
            this.cursorEnabled = cursorEnabled;
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
     * @currentNumberJumps
     *  How many times has the player jumped since leaving the ground?
     * @maxNumberJumps
     *  How many jumps is the player allowed to do before touching the ground?
     *  This is based on how many jumpPowers are defined in config variables.
     * @allowedJumpTime
     *  Allow the player to jump again after delay. Avoids spamming double jumps.
      *@prevTeleportPoint
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

    [Header("TELEPORT")]
    [SerializeField] GameObject teleportPointPrefab;

    // State
    protected bool isAlive;
    protected bool isGrounded;
    protected int currentNumberJumps;
    protected int maxNumberJumps;
    protected float allowedJumpTime;
    protected GameObject prevTeleportPoint;

    // Cache
    protected Rigidbody2D myRigidbody;
    protected Collider2D myCollider2D;
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
        maxNumberJumps = jumpSpeed.Length;
        allowedJumpTime = 0;

        // Set up cache
        myRigidbody = GetComponent<Rigidbody2D>();
        myCollider2D = GetComponent<Collider2D>();
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
    //                 PUBLIC + VIRTUAL METHODS / COROUTINES
    // **********************************************************************


    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: FallMultiplier
     * If the player begins falling, this function adds the fallSpeed 
     * multiplier
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    protected virtual void FallMultiplier()
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
    protected virtual void FreezePlayer()
    {
        SetControllerConfigs(new ControllerConfigs(false, this, true));
        myRigidbody.velocity = new Vector2(0f, myRigidbody.velocity.y);
    }

    /* 
      * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
      * SUMMARY: HandleRun - left joystick horizontal axis
      * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     */
    public virtual void HandleRun(float controlThrow)
    {
        // Calculate new velocity using acceleration with a max run speed
        float xVelocity = myRigidbody.velocity.x + controlThrow * this.runAcceleration;
        float xVelocityClamped = Mathf.Clamp(xVelocity, -1f * this.maxRunSpeed, this.maxRunSpeed);
        Vector2 playerVelocity = new Vector2(xVelocityClamped, myRigidbody.velocity.y);

        myRigidbody.velocity = playerVelocity; // assign new run speed to player

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) >= Mathf.Epsilon;

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
     * SUMMARY: UpdateTeleportPoint
     * 1. Teleports the player to the specified location.
     * 2. Updates the previousTeleportLocation with valid and new location.
     * 3. Set player cameras.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public virtual void TeleportHere(Vector2 location, Vector2 velocity)
    {
        // If a teleport point doesn't exist yet, instantiate a new one.
        if (!prevTeleportPoint)
        {
            Debug.Log("Creating new teleport point");
            // save current location as the previous teleport point in case player
            // tries to teleport back.
            prevTeleportPoint = Instantiate(
                teleportPointPrefab,
                transform.position,
                Quaternion.identity
            ) as GameObject;
        }

        // modify teleport point's position after every teleport.
        prevTeleportPoint.GetComponent<TeleportPoint>().SetLocation(transform.position);

        // play teleportation animation

        // play teleportation sound

        // teleport to the requested location and add the requested velocity
        transform.position = location;
        myRigidbody.velocity = velocity;
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: GetPosition
     * Returns entity's position in the world.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public virtual Vector2 GetPosition()
    {
        return transform.position;
    }

    /* 
      * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
      * SUMMARY: HandleVertical - left joystick vertical axis
      * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     */
    public virtual void HandleVertical(float controlThrow) { return; }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleRanged
     * Ranged attack varies from character to character.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public virtual void HandleRanged(Vector2 cursorDir) { return; }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleSpecial
     * Special ability varies from character to character.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public virtual void HandleSpecial(Vector2 cursorDir) { return; }


    // **********************************************************************
    //                        PROTECTED METHODS
    // **********************************************************************


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
    //                        DEPENDENCY METHODS
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: SetControllerConfigs
     * Sets various user controller properties.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    protected void SetControllerConfigs(ControllerConfigs configs)
    {
        UserController controller = FindObjectOfType<UserController>();
        if (controller)
        {
            controller.SetControllable(configs.controllerEnabled);
            controller.SetEntity(configs.entityToControl);
            controller.SetCursorEnable(configs.cursorEnabled);
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: SetEntityForPlayerCameraToFollow
     * Set entity to follow using the player cameras.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    protected void SetEntityForPlayerCameraToFollow(Transform entityTransform)
    {
        CameraManager cameraManager = FindObjectOfType<CameraManager>();
        if (cameraManager) { cameraManager.SetEntityForPlayerCameraToFollow(entityTransform); }
    }
}
