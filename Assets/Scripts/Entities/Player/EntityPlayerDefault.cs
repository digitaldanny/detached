using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("SPIRIT")]
    [SerializeField] protected GameObject spirit;
    [SerializeField] protected float launchPower = 30f;
    [SerializeField] protected float verticalLaunchOffset = 0.5f;
    [SerializeField] protected float maxTimeInSpiritForm = 5f;
    [SerializeField] protected float spiritRechargeDelay = 2f;
    [SerializeField] protected float spiritRechargeAmount = 0.5f;

    // **********************************************************************
    //                       ENTITY OVERLOAD METHODS
    // **********************************************************************

    private void Start()
    {
        base.StartE();

        // Start the game with the cameras pointed at the player
        SetEntityForPlayerCameraToFollow(transform);

        // Players should start the game being able to control the player.
        SetEntityToControl(this);
        SetControllable(true);
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
    public override void HandleRanged(Vector2 cursorPos)
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

        // Get the target vector
        Vector2 myPos = transform.position;
        Vector2 direction = (cursorPos - myPos);
        direction.Normalize();

        // Launch the spirit towards where the cursor was aiming.
        Rigidbody2D spiritRigidBody = spiritLaunched.gameObject.GetComponent<Rigidbody2D>();
        spiritRigidBody.velocity += direction * launchPower;
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleSpecial
     * 1. Teleports to the player's previous location if the sport is still
     *  valid.
     * 2. Set player camera.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public override void HandleSpecial(Vector2 cursorPos)
    {
        // check if teleport point is still valid
        if (this.prevTeleportPoint.valid)
        {
            this.prevTeleportPoint.valid = false; // make sure players can't teleport to same location twice
            transform.position = this.prevTeleportPoint.location; // teleport
        }
    }
}
