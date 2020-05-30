using System;
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
            GiveUserControl(collision.gameObject.GetComponent<Spirit>());
        }
    }

    // **********************************************************************
    //                       PRIVATE METHODS / COROUTINES
    // **********************************************************************
    private void GiveUserControl(Spirit spirit)
    {
        Debug.Log("Enemy gave user control!");

        // Destroy the spirit game object and play sounds/animation
        spirit.TakeControl();

        // Allow user input to control enemy
        myController.SetControllable(true);
    }
}
