using System;
using System.Runtime.CompilerServices;
using UnityEditor.U2D.Path.GUIFramework;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CameraManager))]
public class Spirit : Entity
{
    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    // Configs
    [Header("SPIRIT")]
    [SerializeField] float verticalEase = 1f;

    // State

    // Cache
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFloorCollider;
    SpriteRenderer spriteRenderer;
    CameraManager cameraManager;

    // **********************************************************************
    //                           OVERLOAD METHODS
    // **********************************************************************

    void Start()
    {
        base.DefaultGlobals();

        // Make player controls connect to spirit entity
        SetControllerConfigs(new ControllerConfigs(true, this, false));

        // set up states
        _isGrounded = false;

        // set up cache
        // myRigidbody = GetComponent<Rigidbody2D>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFloorCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        cameraManager = FindObjectOfType<CameraManager>();

        // switch to spirit camera
        cameraManager.SetSpiritToFollow(transform);
        cameraManager.SetSpiritCamera(true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Check if gameObject hit the floor by checking if hit
            // on the BoxCollider, which is located on the bottom of 
            // the spirit.
            if (contact.otherCollider is BoxCollider2D)
                HandleTouchingGround(contact.point);
        }
    }

    // **********************************************************************
    //                      PRIVATE METHODS / COROUTINES
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: CheckMyContactPoints
     * This function checks if the object has touched the ground. If it
     * has, the game object will stick to the ground.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void HandleTouchingGround(Vector2 contactPoint)
    {
        // Determine if the floor collider is touching the ground
        bool bottomColliderIsTouchingGround = myFloorCollider.IsTouchingLayers(
            LayerMask.GetMask(GlobalConfigs.LAYER_COLLISION_GROUND)
        );

        // If floor collider is touching the ground, stop moving and play
        // the appropriate sounds, animations.
        if (bottomColliderIsTouchingGround)
        {
            // Turn off all colliders to avoid bouncing
            myBodyCollider.enabled = false;
            myFloorCollider.enabled = false;

            // Change state to grounded so player will automatically teleport here.
            _isGrounded = true;

            // Force spirit to stick to ground at the contact point
            // Also, offset the y position by the sprite's y extent.
            float spriteExtentY = spriteRenderer.sprite.bounds.extents.y;
            transform.position = contactPoint + new Vector2(0f, spriteExtentY);

            // Stop all movement
            _myRigidbody.velocity = new Vector2(0f, 0f);

            // Play hitting floor sound

            // Start animation
        }
    }

    // **********************************************************************
    //                    ENTITY OVERRIDE METHODS / COROUTINES
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleVertical
     * Ease spirit towards direction that player towards vertical direction.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public override void HandleVertical(float controlThrow)
    {
        if (!_isGrounded)
        {
            _myRigidbody.velocity += new Vector2(0f, verticalEase * controlThrow);
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandlePlayerTeleport
     * If the spirit has landed on the ground, the spirit teleports the
     * player to the spirit's current location and then destroys itself.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public override void HandleRanged(Vector2 cursorDir)
    {
        // Teleport player to this location
        Entity player = GetPlayerReference().GetComponent<Entity>();
        player.TeleportHere(transform.position, _myRigidbody.velocity);
        this.GiveUpControlToEntity(player);
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: Handle{Action}
     * This actions should not do anything for the Spirit class.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public override void HandleRun(float controlThrow) { return; }
    public override void HandleJump() { return; }
    public override void HandleSpecial(Vector2 cursorDir) { return; }
    protected override void FallMultiplier() { return; }
    protected override void FreezePlayer() { return; }
    public override void HandleDamage(DamageUnit du) { return; }


    // **********************************************************************
    //                      PUBLIC METHODS / COROUTINES
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * 
     * SUMMARY: GiveUpControl
     * This function is called by the enemy that the spirit collided
     * with so the user can control the enemy.
     * ~ Destroy spirit game object.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public void GiveUpControlToEntity(Entity entityGainingControl)
    {
        // play animation

        // play sound

        // Set entity for spirit camera to follow == player to patch issue where
        // lookahead causes jerky re-centering when spirit spawns second time.
        cameraManager.SetSpiritToFollow(entityGainingControl.transform);

        // Give player camera control
        cameraManager.SetPlayerCamera(true);

        // Give new entity control of the user input
        SetControllerConfigs(new ControllerConfigs(true, entityGainingControl, true));

        Destroy(gameObject, 0f);
    }
    
    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: GetPlayerReference
     * Gets reference to player that launched the spirit out to control
     * the enemy.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public Transform GetPlayerReference()
    {
        return transform.parent;
    }
}
