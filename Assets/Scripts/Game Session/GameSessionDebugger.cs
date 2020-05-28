using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSessionDebugger : MonoBehaviour
{
    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * CLASS PARAMETER DESCRIPTIONS
     * -------------------------------------------------------------------
     * CONFIGS:
     * @debugModeEnabled
     *  If set (unlocked), changing the debug tools will affect the game.
     * @timeScale
     *  Scale of 1 is in real time. Scale of 2 doubles gameplay. 0.5 halves.
     * -------------------------------------------------------------------
     * STATES
     * -------------------------------------------------------------------
     * CACHE
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */

    // configs
    [Header("DEBUG TOOLS")]
    [SerializeField] bool debugModeEnabled;
    [SerializeField] [Range(0f, 100f)] float timeScale;

    // state

    // cache

    // **********************************************************************
    //                          OVERLOADED METHODS
    // **********************************************************************

    // Start is called before the first frame update
    void Start()
    {
        // set config and state variables to defaults
        SetConfigDefaults();

        // cache
    }

    // Update is called once per frame
    void Update()
    {
        if (debugModeEnabled)
        {
            HandleTimeScale();
        }
        else
        {
            SetConfigDefaults();
        }
    }

    // **********************************************************************
    //                         METHODS / COROUTINES
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: SetConfigDefaults
     * This function contains all the default configurations for state 
     * and configuration variables.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void SetConfigDefaults()
    {
        // configs
        debugModeEnabled = false;
        timeScale = 1.0f;

        // state
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: HandleTimeScale
     * This function scales time based on serialized config variable.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void HandleTimeScale()
    {
        Time.timeScale = this.timeScale;
    }
}
