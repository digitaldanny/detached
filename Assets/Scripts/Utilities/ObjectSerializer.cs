using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

/* 
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
 * SUMMARY: ObjectSerializer
 * This class handles saving, overwriting, and loading objects into
 * files to be used at later times.
 * +-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+
*/
public class ObjectSerializer<T>
{
    private string _filename;

    public ObjectSerializer(string filename)
    {
        this._filename = filename;
    }

    public void Save(T itemToSave)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Path.Combine(Application.persistentDataPath, _filename));
        bf.Serialize(file, itemToSave);
        file.Close();
    }

    public T Load()
    {
        if (File.Exists(Path.Combine(Application.persistentDataPath, _filename)))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Path.Combine(Application.persistentDataPath, _filename), FileMode.Open);
            T itemToLoad = (T)bf.Deserialize(file);
            file.Close();
            return itemToLoad;
        }

        return default(T);
    }
}
