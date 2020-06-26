using System.Collections;
using UnityEngine;
using MyBox;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Entity : MonoBehaviour
{
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
    [Header("HEALTH")]
    [SerializeField] protected int _maxHealth = 1;

    [Header("RUN")]
    [SerializeField] protected float _maxRunSpeed = 15f;
    [SerializeField] protected float _runAcceleration = 2f;

    [Header("JUMP")]
    [SerializeField] protected float[] _jumpSpeed;
    [SerializeField] protected float _fallSpeed = 240f;
    [SerializeField] protected float _secondsBetweenDoubleJump = 0.1f;

    [Header("SPRITE")]
    [SerializeField] protected bool _spriteFacingRight = true;

    [Header("TELEPORT")]
    [SerializeField] protected GameObject _teleportPointPrefab;

    // State
    protected bool _isAlive;
    protected bool _isGrounded;
    protected int _currentNumberJumps;
    protected int _maxNumberJumps;
    protected float _allowedJumpTime;
    protected GameObject _prevTeleportPoint;
    [SerializeField] protected int _health;

    // Cache
    protected Rigidbody2D _myRigidbody;
    protected Collider2D _myCollider2D;
    protected SpriteRenderer _mySpriteRenderer;

    [Header("SCRIPTABLE OBJ ASSIGNMENTS")]
    [MustBeAssigned][SerializeField] protected ControllerState _controllerState;

    // **********************************************************************
    //                          GETTERS / SETTERS
    // **********************************************************************

    public int maxHealth
    {
        get => _maxHealth;
        set { _maxHealth = value; }
    }

    public int health
    {
        get => _health;
        set { _health = value; }
    }

    // **********************************************************************
    //                 MONO BEHAVIOUR CLASS OVERLOAD METHODS
    // **********************************************************************


    void Start() { DefaultGlobals(); }
    protected virtual void DefaultGlobals()
    {
        // Set up states
        _currentNumberJumps = 0;
        _isAlive = true;
        _isGrounded = true;
        _maxNumberJumps = _jumpSpeed.Length;
        _allowedJumpTime = 0;
        _health = _maxHealth;

        // Set up cache
        _myRigidbody = GetComponent<Rigidbody2D>();
        _myCollider2D = GetComponent<Collider2D>();
        _mySpriteRenderer = GetComponent<SpriteRenderer>();
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
        if (_myCollider2D.IsTouchingLayers(LayerMask.GetMask(GlobalConfigs.LAYER_COLLISION_GROUND)))
        {
            this._currentNumberJumps = 0;
            this._isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision) { OnCollisionEnter2DE(collision); }
    protected void OnCollisionExit2DE(Collision2D collision)
    {
        // If leaving the ground, update the isGrounded state variable.
        if (!_myCollider2D.IsTouchingLayers(LayerMask.GetMask(GlobalConfigs.LAYER_COLLISION_GROUND)))
        {
            this._isGrounded = false;
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
        if (_myRigidbody.velocity.y < Mathf.Epsilon)
        {
            _myRigidbody.velocity += Vector2.down * _fallSpeed * Time.deltaTime;
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: StopRunning
     * Sets the player's X velocity to 0 so player isn't stuck moving
     * left or right while frozen.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    protected virtual void StopRunning()
    {
        _myRigidbody.velocity = new Vector2(0f, _myRigidbody.velocity.y);
    }

    /* 
      * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
      * SUMMARY: HandleRun - left joystick horizontal axis
      * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     */
    public virtual void HandleRun(float controlThrow)
    {
        // Calculate new velocity using acceleration with a max run speed
        float xVelocity = _myRigidbody.velocity.x + controlThrow * this._runAcceleration;
        float xVelocityClamped = Mathf.Clamp(xVelocity, -1f * this._maxRunSpeed, this._maxRunSpeed);
        Vector2 playerVelocity = new Vector2(xVelocityClamped, _myRigidbody.velocity.y);

        _myRigidbody.velocity = playerVelocity; // assign new run speed to player

        bool playerHasHorizontalSpeed = Mathf.Abs(_myRigidbody.velocity.x) >= Mathf.Epsilon;

        // check if player's sprite should be flipped to face direction it is moving
        if (playerHasHorizontalSpeed)
        {
            // Sprite direction is implemented using XNOR boolean logic
            // XNOR Truth table reference: https://en.scratch-wiki.info/wiki/Truth_Table
            bool A = Mathf.Sign(_myRigidbody.velocity.x) <= 0.0f;
            bool B = this._spriteFacingRight;
            _mySpriteRenderer.flipX = A == B; // XNOR logic
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
        if (Time.time >= this._allowedJumpTime)
            this._allowedJumpTime = Time.time + this._secondsBetweenDoubleJump;
        else
            return;

        // Player is always allowed to jump if touching the ground.
        // If already in the air, player can only jump if he hasn't met the
        // max number of jumps already.
        if (this._currentNumberJumps < this._maxNumberJumps)
        {
            Jump(this._jumpSpeed[_currentNumberJumps]);

            // Increment counter to make sure player doesn't make more jumps
            // in a row than allowed.
            this._currentNumberJumps++;
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleDamage
     * This function handles basic KNOCKBACK and HEALTH REDUCTION; however,
     * it does not handle what to do with the entity once health goes 
     * below 0.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public virtual void HandleDamage(DamageUnit du)
    {
        // apply damage
        this._health -= du.damage;

        // determine direction to apply knockback to
        if (du.origin.x > transform.position.x) // to the left if entity is to the left of enemy
            du.knockback.x *= -1;
        
        // if (du.origin.y < transform.position.y) // downward if player is below enemy
        //     du.knockback.y *= -1;

        // stun the entity and apply the knockback
        StartCoroutine(DisableControlsForSeconds(du.stunTime));
        _myRigidbody.velocity = du.knockback;
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: DisableControlsForSeconds
     * Coroutine to disable controls for a short period of time.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    IEnumerator DisableControlsForSeconds(float stunTime)
    {
        // Disable entity controls
        _controllerState.inputEn.all = false;

        // wait for stun time
        yield return new WaitForSeconds(stunTime);

        // re-enable entity controls
        _controllerState.inputEn.all = true;
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
        if (!_prevTeleportPoint)
        {
            // save current location as the previous teleport point in case player
            // tries to teleport back.
            _prevTeleportPoint = Instantiate(
                _teleportPointPrefab,
                transform.position,
                Quaternion.identity
            ) as GameObject;
        }

        // modify teleport point's position after every teleport.
        _prevTeleportPoint.GetComponent<TeleportPoint>().SetLocation(transform.position);

        // play teleportation animation

        // play teleportation sound

        // teleport to the requested location and add the requested velocity
        transform.position = location;
        _myRigidbody.velocity = velocity;
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
    public virtual void HandleRangedDown(Vector2 cursorDir) { return; }
    public virtual void HandleRangedUp(Vector2 cursorDir) { return; }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleSpecial
     * Special ability varies from character to character.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public virtual void HandleSpecialDown(Vector2 cursorDir) { return; }
    public virtual void HandleSpecialUp(Vector2 cursorDir) { return; }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: SetToDefaults
     * Set player to default properties.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public virtual void SetToDefaults() { return; }


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
        _myRigidbody.velocity += jumpVelocityToAdd;
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: Die
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    protected virtual void Die() { return; }

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
