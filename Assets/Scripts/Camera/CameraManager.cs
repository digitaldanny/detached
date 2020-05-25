using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditorInternal;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // **********************************************************************
    //                           CLASS ENUMS
    // **********************************************************************
    public enum CameraLabel : int
    {
        CAM_PLAYER = 1,
        CAM_SPIRIT = 2,
        NONE = 100
    }

    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    // Configs
    [Header("VIRTUAL CAMERAS")]
    [SerializeField] GameObject playerVirtualCam;
    [SerializeField] int playerVirtualCamPriority = 10;

    [SerializeField] GameObject spiritsVirtualCam;
    [SerializeField] int spiritVirtualCamPriority = 9;

    [Header("OTHER")]
    [SerializeField] int maxCameraPriority = 100;

    [Header("DEBUG TOOLS")]
    [SerializeField] bool debugToolEnabled = false;
    [SerializeField] CameraLabel debugCamera = CameraLabel.CAM_PLAYER;

    // StateW
    CameraLabel debugCameraPrevious;

    // Cache

    // **********************************************************************
    //                          OVERLOADED METHODS
    // **********************************************************************

    private void Start()
    {
        debugCameraPrevious = CameraLabel.NONE;

        // Follow player by default
        SetPlayerCamera(true);
    }

    private void Update()
    {
        DebugCameraSwitcherTool();
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: Set{CameraSystem}
     * This function can be used by game objects to force camera system to
     * be in use by setting priority to max.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public bool SetSpiritCamera(bool enabled)
    {
        if (enabled == true)
        {
            spiritsVirtualCam.GetComponent<CinemachineVirtualCamera>().Priority = maxCameraPriority;
        }
        else
        {
            spiritsVirtualCam.GetComponent<CinemachineVirtualCamera>().Priority = spiritVirtualCamPriority;
        }

        // set the system to be on (only useful if using the priority system)
        // playerVirtualCam.SetActive(enabled);
        return true;
    }

    public bool SetPlayerCamera(bool enabled)
    {
        if (enabled == true)
        {
            playerVirtualCam.GetComponent<CinemachineStateDrivenCamera>().Priority = maxCameraPriority;
        }
        else
        {
            playerVirtualCam.GetComponent<CinemachineStateDrivenCamera>().Priority = spiritVirtualCamPriority;
        }

        // set the system to be on (only useful if using the priority system)
        // playerVirtualCam.SetActive(enabled);
        return true;
    }

    // **********************************************************************
    //                         METHODS / COROUTINES
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: Set{CameraSystem}
     * This function can be used by game objects to turn the camera system
     * on or off.
     * 
     * NOTE: The lowest priority camera CANNOT be turned off.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    public void SetSpiritToFollow(Transform spirit)
    {
        CinemachineVirtualCamera vcam = spiritsVirtualCam.GetComponent<CinemachineVirtualCamera>();
        vcam.Follow = spirit;
    }

    // **********************************************************************
    //                            DEBUG TOOLS
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: DebugCameraSwitcherTool
     * This function allows developers to switch between cameras.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void DebugCameraSwitcherTool()
    {
        // Only set priority if the camera has not been set already.
        bool switchable = (debugCamera != debugCameraPrevious) || debugCamera == CameraLabel.NONE;

        if (debugToolEnabled && switchable)
        {
            switch (debugCamera)
            {
                case CameraLabel.CAM_SPIRIT:
                    SetSpiritCamera(true);
                    SetPlayerCamera(false);
                    break;

                case CameraLabel.CAM_PLAYER:
                    SetPlayerCamera(true);
                    SetSpiritCamera(false);
                    break;

                default:
                    Debug.Log("ERROR (DebugCameraSwitcherTool): Default camera isn't set up.");
                    break;
            }
        }

        debugCameraPrevious = debugCamera;
    }
}
