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
    //                           CLASS STRUCTS
    // **********************************************************************

    /*
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: vCam
     * @cam
     *  Cinemachine virtual camera.
     * @defaultPriority
     *  Default priority to set camera when returning from tempororary
     *  high priority.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */

    [System.Serializable]
    public class vCam
    {
        public GameObject cam;
        public int defaultPriority;
    }

    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    // Configs
    [Header("VIRTUAL CAMERAS")]
    [SerializeField] vCam playerCamSystem;
    [SerializeField] vCam spiritCamSystem;

    [Header("OTHER")]
    [SerializeField] int maxCameraPriority = 100;

    [Header("DEBUG TOOLS")] 
    [SerializeField] bool debugToolEnabled = false;
    [SerializeField] CameraLabel debugCamera = CameraLabel.CAM_PLAYER;

    // State
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
    public void SetCameraSystem(CameraLabel selectedSystem, bool setting)
    {
        switch (selectedSystem)
        {
            case CameraLabel.CAM_PLAYER:

                // If enabling, set player cam to max priority
                // Else, return player cam to default settings
                if (enabled == true)
                    playerCamSystem.cam.GetComponent<CinemachineStateDrivenCamera>().Priority = maxCameraPriority;
                else
                    playerCamSystem.cam.GetComponent<CinemachineStateDrivenCamera>().Priority = playerCamSystem.defaultPriority;
                break;


            case CameraLabel.CAM_SPIRIT:

                // If enabling, set spirit cam to max priority
                // Else, return spirit cam to default settings
                if (setting == true)
                    spiritCamSystem.cam.GetComponent<CinemachineVirtualCamera>().Priority = this.maxCameraPriority;
                else
                    spiritCamSystem.cam.GetComponent<CinemachineVirtualCamera>().Priority = spiritCamSystem.defaultPriority;
                break;


            default:
                Debug.Log("Selected camera is not handled.");
                break;
        }
    }

    public void SetSpiritCamera(bool setting)
    {
        SetCameraSystem(CameraLabel.CAM_SPIRIT, true);
        SetCameraSystem(CameraLabel.CAM_PLAYER, false);
    }

    public void SetPlayerCamera(bool enabled)
    {
        SetCameraSystem(CameraLabel.CAM_PLAYER, true);
        SetCameraSystem(CameraLabel.CAM_SPIRIT, false);
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
        CinemachineVirtualCamera vcam = spiritCamSystem.cam.GetComponent<CinemachineVirtualCamera>();
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
