using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class EntityPlayerDefault : Entity
{
    protected enum ButtonState
    {
        DOWN,
        UP
    }

    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * CLASS PARAMETER DESCRIPTIONS
     * -------------------------------------------------------------------
     * CONFIGS:
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
     * STATES:
     * -------------------------------------------------------------------
     * CACHE:
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */

    // Configs
    [Header("SPIRIT")]
    [SerializeField] protected GameObject spirit;
    [SerializeField] protected float launchPower = 30f;
    [SerializeField] protected float verticalLaunchOffset = 0.5f;

    // State
    protected ButtonState _buttonStateRanged;

    // Cache
    protected Animator myAnimator;
    protected CheckpointManager checkpointManager;

    // **********************************************************************
    //                       ENTITY OVERLOAD METHODS
    // **********************************************************************

    private void Start()
    {
        DefaultGlobals();
        DefaultSettings();
    }

    // **********************************************************************
    //                           PROTECTED METHODS
    // **********************************************************************

    protected override void DefaultGlobals()
    {
        base.DefaultGlobals();

        // State
        _buttonStateRanged = ButtonState.UP;

        // Cache
        myAnimator = GetComponent<Animator>();
        checkpointManager = FindObjectOfType<CheckpointManager>();
        if (checkpointManager == null) { Debug.Log("ERROR (EntityPlayerDefault.Start): Could not find CheckpointManager object."); }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: DefaultSettings
     * Set up the following configurations.
     * - Player camera follows this object.
     * - Controller is enabled for this object and cursor is turned on.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    protected void DefaultSettings()
    {
        // Start the game with the cameras pointed at the player
        SetEntityForPlayerCameraToFollow(transform);

        // Players should start the game being able to control the player.
        SetControllerConfigs(new ControllerConfigs(true, this, false));
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: FreezePlayer
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    protected override void FreezePlayer()
    {
        base.FreezePlayer();
        myAnimator.SetBool(GlobalConfigs.ANIMATION_PLAYER_RUNNING, false);
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: Die
     * This function handles whether this Entity should take damage from
     * external damage dealers. if the health is less than or equal to 0,
     * the player respawns at the previous checkpoint.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    protected override void Die()
    {
        // Disable all components set up during instantiation
        SetControllerConfigs(new ControllerConfigs(false, null, false));
        SetEntityForPlayerCameraToFollow(null);

        // Play death animation, sound effect

        // reconfigure player to be the same as the last checkpoint
        checkpointManager.RespawnAtLastCheckpoint(this.gameObject);

        // Re-enable all components set up during instantiation
        DefaultSettings();
    }

    // **********************************************************************
    //                            PUBLIC METHODS
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleRange (Spirit Launch)
     * -------------------------------------------------------------------
     * Launches spirit towards the mouse and stops player from being able
     * to move until a signal is sent to allow the player to control the
     * character again.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public override void HandleRangedDown(Vector2 cursorDir)
    {
        if (_buttonStateRanged == ButtonState.UP)
        {
            // Do not allow the player to move after launching the spirit
            FreezePlayer();

            // Enable cursor for aiming spirit before launch while still allow controller.
            SetControllerConfigs(new ControllerConfigs(true, this, true));
            this._xAxisEnabled = false;

            _buttonStateRanged = ButtonState.DOWN;
        }
    }

    public override void HandleRangedUp(Vector2 cursorDir)
    {
        if (_buttonStateRanged == ButtonState.DOWN)
        {
            // Instantiate the detached spirit into the hierarchy as a child of
            // player GameObject.
            GameObject spiritLaunched = Instantiate(
                spirit,
                transform.position + new Vector3(0f, verticalLaunchOffset, 0f),
                Quaternion.identity,
                transform
            ) as GameObject;

            Rigidbody2D spiritRigidBody = spiritLaunched.gameObject.GetComponent<Rigidbody2D>();

            // Launch the spirit towards where the joystick was aiming
            spiritRigidBody.velocity += cursorDir * launchPower;

            // Disable cursor for spirit after launch. Continue to disable controller.
            SetControllerConfigs(new ControllerConfigs(false, this,  false));

            _buttonStateRanged = ButtonState.UP;
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleSpecial
     * 1. Teleports to the player's previous location if the sport is still
     *  valid.
     * 2. Set player camera.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public override void HandleSpecialDown(Vector2 cursorDir)
    {
        // If teleport point is still valid, teleport to location
        // and destroy the teleport point.
        if (_prevTeleportPoint)
        {
            transform.position = _prevTeleportPoint.GetComponent<TeleportPoint>().GetPosition();
            Destroy(_prevTeleportPoint);
        }
        else
        {
            Debug.Log("Teleport point isn't valid anymore");
        }
    }

    public override void HandleRun(float controlThrow)
    {
        base.HandleRun(controlThrow);

        // Tell animator when to play run animator
        bool playerHasHorizontalSpeed = Mathf.Abs(_myRigidbody.velocity.x) >= GlobalConfigs.ENTITY_RUN_VELOCITY_EPSILON;
        myAnimator.SetBool(GlobalConfigs.ANIMATION_PLAYER_RUNNING, playerHasHorizontalSpeed); 
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleDamage
     * This function handles whether this Entity should take damage from
     * external damage dealers. if the health is less than or equal to 0,
     * the player respawns at the previous checkpoint.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public override void HandleDamage(DamageUnit du) 
    {
        base.HandleDamage(du);

        if (health <= 0)
        {
            Die();
        }
    }
}
