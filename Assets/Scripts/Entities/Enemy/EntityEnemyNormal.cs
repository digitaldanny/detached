﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityEnemyNormal : Entity
{
    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    // Configs

    // State
    Entity player; // if the player controls the enemy, reference the player here.

    // Cache

    // **********************************************************************
    //                      ENTITY CLASS OVERLOAD METHODS
    // **********************************************************************

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
    public override void HandleRanged(Vector2 cursorPos)
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
        // Destroy the spirit game object and play sounds/animation
        player = spirit.GetPlayerReference();
        spirit.TakeControl();

        // Allow user input to control enemy
        myController.SetControllable(true);
    }

    private void GiveUserControlOfPlayer()
    {
        myController.SetControllable(false);
        player.SetControl(true);
    }
}