using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    // Configs
    [SerializeField] public int number;
    [SerializeField] public Color activationColor;

    // State

    // Cache
    protected SpriteRenderer spriteRenderer;
    protected CheckpointManager checkpointManager;

    // **********************************************************************
    //                MONO BEHAVIOUR CLASS OVERRIDE METHODS
    // **********************************************************************

    private void Start() { StartE(); }
    protected void StartE()
    {
        // Cache
        spriteRenderer = GetComponent<SpriteRenderer>();
        checkpointManager = FindObjectOfType<CheckpointManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision) { OnTriggerEnter2DE(collision); }
    protected void OnTriggerEnter2DE(Collider2D collision)
    {
        PlayerActivated(collision);
    }

    // **********************************************************************
    //                         PRIVATE METHODS
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: PlayerActivated
     * Check if the player collided with the checkpoint. If the checkpoint
     * number is higher than the GameSession's current checkpoint number,
     * overwrite it with this checkpoint so the player respawns at this
     * location.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    protected void PlayerActivated(Collider2D collision)
    {
        if (collision.gameObject.tag == GlobalConfigs.TAG_PLAYER)
        {   
            // Update game session's checkpoint if player has located
            // a higher level location.
            if (checkpointManager.currentCheckpoint.number <= this.number)
            {
                checkpointManager.currentCheckpoint = this;

                // Change to activation color
                spriteRenderer.color = this.activationColor;

                // Save the player state at the checkpoint
                checkpointManager.checkpointedPlayer = collision.gameObject;
            }
        }
    }
}
