using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class SpiritMovement : MonoBehaviour
{
    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    // Configs
    CameraManager playerCamera;
    CameraManager SpiritCamera;

    // State
    bool isGrounded;

    // Cache
    Rigidbody2D myRigidbody;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFloorCollider;
    SpriteRenderer spriteRenderer;
    CameraManager cameraManager;

    // **********************************************************************
    //                           OVERLOAD METHODS
    // **********************************************************************

    void Start()
    {
        // set up states
        isGrounded = false;

        // set up cache
        myRigidbody = GetComponent<Rigidbody2D>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFloorCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        cameraManager = FindObjectOfType<CameraManager>();

        // switch to spirit camera
        cameraManager.SetSpiritToFollow(transform);
        cameraManager.SetSpiritCamera(true);
    }

    private void Update()
    {
        HandlePlayerTeleport();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // If this gameObject was hit in the floor collider..
            if (contact.otherCollider is BoxCollider2D)
                HandleTouchingGround(contact.point);
        }
    }

    // **********************************************************************
    //                         METHODS / COROUTINES
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
            isGrounded = true;

            // Force spirit to stick to ground at the contact point
            // Also, offset the y position by the sprite's y extent.
            float spriteExtentY = spriteRenderer.sprite.bounds.extents.y;
            transform.position = contactPoint + new Vector2(0f, spriteExtentY);

            // Stop all movement
            myRigidbody.velocity = new Vector2(0f, 0f);

            // Play hitting floor sound

            // Start animation
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandlePlayerTeleport
     * If the spirit has landed on the ground, the spirit teleports the
     * player to the spirit's current location and then destroys itself.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void HandlePlayerTeleport()
    {
        // Check if player presses teleport button
        if (CrossPlatformInputManager.GetButtonDown(GlobalConfigs.CONTROLLER_FIRE2))
        {
            // Teleport player to this location
            Player player = gameObject.GetComponentInParent<Player>();
            player.TeleportHere(transform.position);

            // Set camera to player before deleting spirit
            cameraManager.SetPlayerCamera(true);

            // Destroy spirit to show that player recombined with it
            Destroy(gameObject);
        }
    }
}
