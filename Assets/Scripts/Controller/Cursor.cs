using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    // Configs
    [SerializeField] float radius = 5f;
    [SerializeField] float xOffset = 0f;
    [SerializeField] float yOffset = 0f;

    // State

    // Cache


    // **********************************************************************
    //                          PUBLIC METHODS
    // **********************************************************************
    public void UpdatePosition(Vector2 playerPos, Vector2 mousePos)
    {
        // Calculate angle that cursor should be moved to.
        float deltaX = mousePos.x - playerPos.x;
        float deltaY = mousePos.y - playerPos.y;
        float thetaRadians = Mathf.Atan2(deltaY, deltaX);

        // Calculate the new cursor position.
        Vector2 newPos = new Vector2(radius * Mathf.Cos(thetaRadians), radius * Mathf.Sin(thetaRadians));

        // Add configurable offsets from player.
        newPos += new Vector2(xOffset, yOffset);

        // Apply new position to cursor
        transform.position = playerPos + newPos;
    }
}
