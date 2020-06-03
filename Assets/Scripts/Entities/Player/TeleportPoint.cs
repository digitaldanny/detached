using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPoint : MonoBehaviour
{
    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    // Configs

    // State
    Vector2 location; // location 

    // Cache

    // **********************************************************************
    //                        MONO BEHAVIOUR METHODS
    // **********************************************************************

    private void Update()
    {
        transform.position = location;
    }

    // **********************************************************************
    //                           PUBLIC METHODS
    // **********************************************************************

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+ 
     * SUMMARY: {Set/Get}Position
     * Returns or sets teleport point's position in the world.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public void SetLocation(Vector2 location)
    {
        this.location = location;
    }

    public Vector2 GetPosition()
    {
        return location;
    }
}
