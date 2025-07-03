using UnityEngine;
using System;
using System.IO;

public class FileDataHandler 
{
    private string dataDirPath = "";
    private string dataFileName = "";

    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    public PlayerData Load()
    {
        // OS compatibility with save path rather than just using a slash
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        PlayerData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                // load serialized data from file
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // deserialize data from JSON to C#
                loadedData = JsonUtility.FromJson<PlayerData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occored when loading from: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
    }

    public void Save(PlayerData data)
    {
        // OS compatibility with save path rather than just using a slash
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            // create dir - location of save
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // serialize C# game object into a JSON for recall
            string dataToStore = JsonUtility.ToJson(data, true);

            // write the serialized data to file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e) 
        {
            Debug.LogError("Error occored when saving to: " + fullPath + "\n" + e);
        }
    }
}
