using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SpiritMovement : MonoBehaviour
{
    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    // Configs
    CameraManager playerCamera;
    CameraManager SpiritCamera;

    // State

    // Cache
    Rigidbody2D myRigidbody;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFloorCollider;
    SpriteRenderer spriteRenderer;

    // **********************************************************************
    //                           OVERLOAD METHODS
    // **********************************************************************

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFloorCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
}
