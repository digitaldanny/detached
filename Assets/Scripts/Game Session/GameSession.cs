using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    // State
    [SerializeField] public Checkpoint currentCheckpoint; // current checkpoint can only be overwritten by higher number checkpoint

    // Start is called before the first frame update
    void Start()
    {
        FindStartingCheckpoint();
    }

    // **********************************************************************
    //                         PRIVATE METHODS
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: FindStartingCheckpoint
     * Assign the starting checkpoint to spawn the player at.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */

    private void FindStartingCheckpoint()
    {
        var allCheckpoints = FindObjectsOfType<Checkpoint>();
        foreach (Checkpoint cp in allCheckpoints)
        {
            if (cp.number == 0) // 0 is the first checkpoint on all levels
            {
                currentCheckpoint = cp;
                return;
            }
        }

        Debug.Log("ERROR (GameSession.FindStartingCheckpoint): Could not find starting checkpoint.");
    }
}
