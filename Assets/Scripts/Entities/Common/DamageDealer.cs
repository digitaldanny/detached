using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DamageDealer : MonoBehaviour
{
    [SerializeField] private Vector2 knockback;
    [SerializeField] private int damage;
    [SerializeField] private float stunTime;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DamageEntity(collision);
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: DamageEntity
     * Create damage unit based on configured properties and apply it
     * to the collision object.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void DamageEntity(Collider2D collision)
    {
        // Only do damage if the collision is an entity type.
        Entity entity = collision.gameObject.GetComponent<Entity>();
        if (entity == null) return;

        // Create a damage packet to damage entity with.
        DamageUnit du = new DamageUnit(transform.position);
        du.damage = damage;
        du.knockback = knockback;
        du.stunTime = stunTime;

        // Apply damage unit to the specified entity
        entity.HandleDamage(du);
    }
}
