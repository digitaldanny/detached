using System.Collections.Generic;

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
