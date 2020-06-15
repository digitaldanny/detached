using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class CheckpointManager : MonoBehaviour
{
    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    // Configs
    [SerializeField] private Color _invalidCheckpointNumberWarning;
    [MustBeAssigned][SerializeField] private List<Checkpoint> _checkpoints;

    // State
    [SerializeField] private Checkpoint _currentCheckpoint; // current checkpoint can only be overwritten by higher number checkpoint
    [SerializeField] private GameObject _checkpointedPlayer; // contains the state of the player at currentCheckpoint (always disabled)

    // Start is called before the first frame update
    void Start()
    {
        if (SceneCheckpointValidityCheck())
        {
            FindStartingCheckpoint();
        }
    }

    // **********************************************************************
    //                         GETTER / SETTERS
    // **********************************************************************

    public Checkpoint currentCheckpoint
    {
        get => _currentCheckpoint;
        set { _currentCheckpoint = value; }
    }

    public GameObject checkpointedPlayer
    {
        get { return _checkpointedPlayer; }

        set 
        {
            // If a player is already checkpointed, destroy it
            // so that multiple checkpointed players do not
            // exist in the scene.
            if (_checkpointedPlayer != null)
            {
                Destroy(_checkpointedPlayer);
            }

            // Make a copy of the player at the checkpoint location
            _checkpointedPlayer = Instantiate(
                value,
                _currentCheckpoint.transform.position,
                Quaternion.identity,
                transform
                ) as GameObject;

            // Player should be 
            _checkpointedPlayer.SetActive(false);
            _checkpointedPlayer.name = "checkpointedPlayerCopy";
        }
    }

    // **********************************************************************
    //                         PUBLIC METHODS
    // **********************************************************************

    public void RespawnAtLastCheckpoint(GameObject player)
    {
        Entity entity = player.GetComponent<Entity>();
        if (entity == null)
            Debug.Log("ERROR: Cannot find Entity component in " + player.name);

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (entity == null)
            Debug.Log("ERROR: Cannot find Rigidbody2D component in " + player.name);

        // Set the relevent checkpoint properties on passed player
        rb.velocity = new Vector2(0f, 0f); // player shouldn't move initially
        player.transform.position = _currentCheckpoint.transform.position; // player should respawn at current checkpoint location
        
        if (_checkpointedPlayer == null) // player never went into a checkpoint
        {
            entity.health = entity.maxHealth;
        }
        else
        {
            // health of player after passing through the checkpoint
            entity.health = _checkpointedPlayer.GetComponent<Entity>().health; 
        }
    }

    // **********************************************************************
    //                         PRIVATE METHODS
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: SceneCheckpointValidityCheck
     * This function checks if there are any two checkpoints with the 
     * same ID number. If there are, it will change the color of the 
     * two offending checkpoints in the scene. This can be easily checked
     * by looking at the scene view and checking for off-color checkpoints.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    protected bool SceneCheckpointValidityCheck()
    {
        bool valid = true;

        for (int i = 0; i < _checkpoints.Count; i++)
        {
            Checkpoint cp1 = _checkpoints[i];

            for (int j = i+1; j < _checkpoints.Count; j++)
            {
                Checkpoint cp2 = _checkpoints[j];
                
                // If the IDs match, flag both by changing the error color.
                if (cp1.number == cp2.number)
                {
                    valid = false;
                    cp1.GetComponentInParent<SpriteRenderer>().color = this._invalidCheckpointNumberWarning;
                    cp2.GetComponentInParent<SpriteRenderer>().color = this._invalidCheckpointNumberWarning;
                    Debug.Log("ERROR (SceneCheckpointValidityCheck): Multiple checkpoints have the same checkpoint number!!");
                }
            }
        }

        return valid;
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: FindStartingCheckpoint
     * Assign the starting checkpoint to spawn the player at.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */

    private void FindStartingCheckpoint()
    {
        StartCheckpoint startCheckpoint = FindObjectOfType<StartCheckpoint>();
        
        // Only assign the current checkpoint to the starting checkpoint if it is 
        // included in the list of checkpoints AND of type StartingCheckpoint.
        if (startCheckpoint != null)
        {
            if (_checkpoints.Contains(startCheckpoint))
            {
                this._currentCheckpoint = startCheckpoint;
                return;
            }
        }

        Debug.Log("ERROR (GameSession.FindStartingCheckpoint): Could not find starting checkpoint.");
    }
}
