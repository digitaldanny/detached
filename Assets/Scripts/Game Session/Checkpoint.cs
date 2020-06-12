using System;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    // Configs
    [SerializeField] public int number;
    [SerializeField] public Color activationColor;
    [SerializeField] public Color cpNumberInvalidColor;

    // State
    public Vector2 position; // contains location of this checkpoint

    // Cache
    SpriteRenderer spriteRenderer;
    GameSession gameSession;

    // **********************************************************************
    //                MONO BEHAVIOUR CLASS OVERRIDE METHODS
    // **********************************************************************

    private void Start()
    {
        // State
        position = transform.position;

        // Cache
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameSession = FindObjectOfType<GameSession>();

        // Perform validity check on the checkpoint numbers
        SceneCheckpointValidityCheck();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerActivated(collision);
    }

    // **********************************************************************
    //                         PRIVATE METHODS
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: SceneCheckpointValidityCheck
     * This function checks if there are any other checkpoints with the
     * same checkpoint number in the scene. This is useful because multiple
     * checkpoints with the same number will break the level playability.
     * 
     * NOTE: This function could be bad for performance if there is a 
     * large number of checkpoints in a single scene.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void SceneCheckpointValidityCheck()
    {
        var allCheckpoints = FindObjectsOfType<Checkpoint>();
        foreach (Checkpoint cp in allCheckpoints)
        {
            if (cp.number == this.number && cp != this)
            {
                spriteRenderer.color = this.cpNumberInvalidColor;
                Debug.Log("ERROR (SceneCheckpointValidityCheck): Multiple checkpoints have the same checkpoint number!!");
            }
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: PlayerActivated
     * Check if the player collided with the checkpoint. If the checkpoint
     * number is higher than the GameSession's current checkpoint number,
     * overwrite it with this checkpoint so the player respawns at this
     * location.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void PlayerActivated(Collider2D collision)
    {
        if (collision.gameObject.tag == GlobalConfigs.TAG_PLAYER)
        {   
            // Update game session's checkpoint if player has located
            // a higher level location.
            if (gameSession.currentCheckpoint.number < this.number)
            {
                gameSession.currentCheckpoint = this;

                // Change to activation color
                spriteRenderer.color = this.activationColor;
            }
        }
    }
}
