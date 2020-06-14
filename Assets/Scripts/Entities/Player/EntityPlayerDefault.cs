﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EntityPlayerDefault : Entity
{
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
    [SerializeField] protected float maxTimeInSpiritForm = 5f;
    [SerializeField] protected float spiritRechargeDelay = 2f;
    [SerializeField] protected float spiritRechargeAmount = 0.5f;

    // State

    // Cache
    protected Animator myAnimator;
    protected CheckpointManager gameSession;

    // **********************************************************************
    //                       ENTITY OVERLOAD METHODS
    // **********************************************************************

    private void Start()
    {
        base.StartE();

        // Cache
        myAnimator = GetComponent<Animator>();

        gameSession = FindObjectOfType<CheckpointManager>();
        if (gameSession == null) { Debug.Log("ERROR (EntityPlayerDefault.Start): Could not find GameSession object."); }

        // Start the game with the cameras pointed at the player
        SetEntityForPlayerCameraToFollow(transform);

        // Players should start the game being able to control the player.
        SetControllerConfigs(new ControllerConfigs(true, this, true));
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleRange (Spirit Launch)
     * -------------------------------------------------------------------
     * Launches spirit towards the mouse and stops player from being able
     * to move until a signal is sent to allow the player to control the
     * character again.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public override void HandleRanged(Vector2 cursorDir)
    {
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

        Rigidbody2D spiritRigidBody = spiritLaunched.gameObject.GetComponent<Rigidbody2D>();

        // Launch the spirit towards where the joystick was aiming
        spiritRigidBody.velocity += cursorDir * launchPower;
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleSpecial
     * 1. Teleports to the player's previous location if the sport is still
     *  valid.
     * 2. Set player camera.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public override void HandleSpecial(Vector2 cursorDir)
    {
        // If teleport point is still valid, teleport to location
        // and destroy the teleport point.
        if (prevTeleportPoint)
        {
            transform.position = prevTeleportPoint.GetComponent<TeleportPoint>().GetPosition();
            Destroy(prevTeleportPoint);
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
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) >= GlobalConfigs.ENTITY_RUN_VELOCITY_EPSILON;
        myAnimator.SetBool(GlobalConfigs.ANIMATION_PLAYER_RUNNING, playerHasHorizontalSpeed); 
    }

    protected override void FreezePlayer() 
    {
        base.FreezePlayer();
        myAnimator.SetBool(GlobalConfigs.ANIMATION_PLAYER_RUNNING, false);
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
            Debug.Log("ERROR (EntityPlayerDefault.HandleDamage): Player died, respawning at last checkpoint.");
            
            // Reset player health
            health = maxHealth;

            // Move player to previous checkpoint
            transform.position = gameSession.currentCheckpoint.position;

            // stop any movement caused by player getting hit
            myRigidbody.velocity = new Vector2(0f, 0f);
        }
    }
}
