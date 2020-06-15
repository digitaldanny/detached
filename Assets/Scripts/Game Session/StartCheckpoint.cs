using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCheckpoint : Checkpoint
{
    void Start()
    {
        base.StartE();

        // Starting checkpoint should not be visible 
        spriteRenderer.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2DE(collision);
    }
}
