using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameHUDManager : MonoBehaviour
{
    // **********************************************************************
    //                          CLASS PARAMETERS
    // **********************************************************************

    // Configs
    [SerializeField] GameObject heartPrefab;
    [SerializeField] Vector2 heartSpawnStart;
    [SerializeField] Vector2 heartSpawnOffset;
    [SerializeField] Vector2 heartScale;
    [SerializeField] Color heartAliveColor;
    [SerializeField] Color heartDeadColor;

    // State
    [SerializeField] Entity entity;
    EasyList<GameObject> heartList;
    int entityMaxHealth = 5;
    int entityCurrentHealth = 3;

    // Cache

    // **********************************************************************
    //                 MONO BEHAVIOUR CLASS OVERLOAD METHODS
    // **********************************************************************

    void Start()
    {
        heartList = new EasyList<GameObject>(GlobalConfigs.ENTITY_MAX_HEALTH, heartPrefab);
        heartList.Resize(entityMaxHealth);

        // Spawn hearts at full health
        for (int i = 0; i < entityMaxHealth; i++)
        {
            SpawnHeartAtOffset(i);
        }
    }

    void Update()
    {
        UpdateCurrentAndMaxHealth();
        CheckIfMaxHealthChanged();
        UpdateDeadHearts();
        UpdateHeartTransforms();
    }

    // **********************************************************************
    //                           PRIVATE METHODS
    // **********************************************************************

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: UpdatePositionAndOffset
     * This function updates heart position during runtime based on 
     * inspector configurations (heartSpawnStart, heartSpawnOffset).
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void UpdateHeartTransforms()
    {
        for (int i = 0; i < entityMaxHealth; i++)
        {
            GameObject heart = (GameObject)heartList[i];
            RectTransform rt = heart.GetComponent<RectTransform>();
            rt.localPosition = heartSpawnStart + heartSpawnOffset * i;
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: UpdateCurrentAndMaxHealth
     * This function gets the current and max health values from the 
     * currently selected entity.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void UpdateCurrentAndMaxHealth()
    {
        entityMaxHealth = entity.maxHealth;
        entityCurrentHealth = entity.health;
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: UpdateDeadHearts
     * If the currently selected entity's current health changed since
     * the previous update, this function will update the UI so that
     * the correct number of hearts are "greyed out"
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void UpdateDeadHearts()
    {
        // Get the sprite renderer for each heart game object.
        for (int i = 0; i < entityMaxHealth; i++)
        {
            GameObject heart = (GameObject)heartList[i];
            Image im = heart.GetComponent<Image>();

            if (im != null)
            {
                // Update sprite renderer for heartrs that are dead
                if (i < entityCurrentHealth)
                {
                    im.color = this.heartAliveColor;
                }

                // Update sprite renderer for heartrs that are alive
                else
                {
                    im.color = this.heartDeadColor;
                }
            }
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: CheckIfMaxHealthChanged
     * If the current entity's max health changes, this function appropriately
     * resizes the heart list, destroying or instantitating heart prefabs
     * as necessary.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private void CheckIfMaxHealthChanged()
    {
        int currentHeartCount = heartList.GetCount();

        // Only modify the heart list if the size has changed. 
        if (currentHeartCount != entityMaxHealth)
        {
            // If entity has new max number of hearts greater than current
            // number of hearts, resize and spawn new hearts.
            if (currentHeartCount < entityMaxHealth)
            {
                heartList.Resize(entityMaxHealth);

                int startIndex = (currentHeartCount == 0) ? 0 : currentHeartCount - 1; // NOTE: Handles case where current
                                                                                       // heartcount == 0 and changes to some 
                                                                                       // positive value.
                for (int i = startIndex; i < entityMaxHealth; i++)
                {
                    SpawnHeartAtOffset(i);
                }
            }

            // If entity has new max number of hearts less than current
            // number of hearts, delete those hearts, and resize the list.
            else if (currentHeartCount > entityMaxHealth)
            {
                for (int i = entityMaxHealth; i < currentHeartCount; i++)
                {
                    Destroy((GameObject)heartList[i]);
                }

                heartList.Resize(entityMaxHealth);
            }
        }
    }

    /* 
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
     * SUMMARY: SpawnHeartAtOffset
     * If the heart list can contain a new heart object at the provided
     * index, it will spawn the heart prefab at the starting position
     * + offset position * provided index. Otherwise, it will do nothing
     * and return false.
     * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
    */
    private bool SpawnHeartAtOffset(int heartIndex)
    {
        if (heartList.GetCount() >= heartIndex+1)
        {
            GameObject newHeart = Instantiate(
                heartPrefab,
                heartSpawnStart + heartSpawnOffset * heartIndex,
                Quaternion.identity,
                transform
                ) as GameObject;

            heartList[heartIndex] = newHeart;
            return true;
        }
        return true;
    }

}
