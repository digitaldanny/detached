using UnityEngine;

/* 
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
 * SUMMARY: DamageUnit
 * Structure to contain all details that a single attack may apply 
 * to the target.
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
*/
public class DamageUnit
{
    public Vector2 origin; // position that attack came from
    public Vector2 knockback; // amount of velocity to add to the Entity's rigidbody
    public int damage; // amount to reduce from Entity's health
    public float stunTime; // amount of time that entity will be unable to move due to hit
    
    public DamageUnit(Vector2 origin)
    {
        this.origin = origin;
        knockback = new Vector2(0f, 0f);
        damage = 0;
        stunTime = 0.01f;
    }
}
