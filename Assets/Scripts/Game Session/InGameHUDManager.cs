using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
 * SUMMARY: EasyList
 * This class wraps the List<T> class to allow user to easily 
 * grow and shrink the size of the container for runtime 
 * user configuration.
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
*/
public class EasyList<T>
{
    private List<T> list;
    public T defaultValue;
    public int maxSize;

    // constructor
    public EasyList(int maxSize, T defaultValue)
    {
        list = new List<T>();

        // limit how large the list can get to avoid running out of memory
        this.maxSize = maxSize;

        // create a default value that new values are initialized to.
        this.defaultValue = defaultValue;
    }

    // User can request the size of the container during runtime.
    // ..
    // NOTE: This will return false if the requested size is larger
    // than the max size defined upon instantiation.
    public bool Resize(int size)
    {
        // Make sure the user is not requesting a size that is too large.
        if (size > this.maxSize)
            return false;

        // Add 0s to the end if requesting a larger size.
        if (list.Count < size)
        {
            while (list.Count < size) { list.Add(this.defaultValue); }
        }

        // Truncate the list if requesting a smaller size.
        else
        {
            while (list.Count > size) { list.RemoveAt(this.list.Count - 1); }
        }
        return true;
    }

    // User can read+write list values using brackets (eg. list[0])
    public object this[int i]
    {
        get { return this.list[i]; }
        set { this.list[i] = (T)value; }
    }

    // Gets the current size of the list
    public int GetCount() { return this.list.Count; }
}

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
            // heart.transform.position = heartSpawnStart + heartSpawnOffset*i;
            // heart.transform.localScale = heartScale;
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
            var list = heart.GetComponents(typeof(Component));
            for (int j = 0; j < list.Length; j++)
            {
                Debug.Log(list[j].name);
            }
            
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
