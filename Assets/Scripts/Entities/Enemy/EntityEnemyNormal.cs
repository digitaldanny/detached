using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EntityEnemyNormal : Entity
{
    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    // Configs

    // State
    Transform player; // if the player controls the enemy, reference the player here.

    // Cache
    protected Animator myAnimator;

    // **********************************************************************
    //                      ENTITY CLASS OVERLOAD METHODS
    // **********************************************************************

    private void Start()
    {
        base.DefaultGlobals();

        // Cache
        myAnimator = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2DE(collision);

        // If enemy collided with spirit, give it control
        if (collision.gameObject.GetComponent<Spirit>() != null)
        {
            GiveUserControlOfEnemy(collision.gameObject.GetComponent<Spirit>());
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleRange (Spirit Return)
     * Launches spirit back to the player that originally launched the
     * spirit.
     * 
     * NOTE: This function will only be called when user controls the enemy.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public override void HandleRangedDown(Vector2 cursorPos)
    {
        GiveUserControlOfPlayer();
    }

    // **********************************************************************
    //                       PRIVATE METHODS / COROUTINES
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: GiveUserControlOfEnemy, GiveUserControlOfPlayer
     * These functions enable the UserController for either the player
     * or the enemy (this object).
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void GiveUserControlOfEnemy(Spirit spirit)
    {
        // Set enemy as entity for player cameras to follow
        SetEntityForPlayerCameraToFollow(transform);

        // Destroy the spirit game object and play sounds/animation
        player = spirit.GetPlayerReference();
        spirit.GiveUpControlToEntity(this);

        // Set the enemy as the entity to control
        _controllerState.entityToControl = this;
        _controllerState.cursorEnable = true;
        _controllerState.inputEn.all = true;
    }

    private void GiveUserControlOfPlayer()
    {
        // Set player as entity for camera to follow
        SetEntityForPlayerCameraToFollow(player);

        // Set player as the entity to control
        _controllerState.entityToControl = player.GetComponent<Entity>();
        _controllerState.cursorEnable = true;
        _controllerState.inputEn.all = true;
    }
}
