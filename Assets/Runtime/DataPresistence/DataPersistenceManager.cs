using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DataPersistenceManager : MonoBehaviour
{
    private PlayerData playerData;
    private List<IDataPersistence> dataPersistenceObjects;
    public static DataPersistenceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple instances of singleton");
        }
        Instance = this;
    }

    private void Start()
    {
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadLevel();
    }

    public void NewLevel()
    {
        this.playerData = new PlayerData();
    }

    public void LoadLevel()
    {
        // load saved data
        // if no data, init new game

        if(this.playerData == null)
        {
            Debug.Log("No data, initialising to defaults");
            NewLevel();
        }
        // push state
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(playerData);
        }
        Debug.Log("Loaded time: " + playerData.timeElapsed);
    }

    public void SaveLevel() 
    {
        // pass data to scripts
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref playerData);
        }
        Debug.Log("Max time elapsed across saves: " + playerData.timeElapsed);
        // save data to data handler
    }

    private void OnDestroy()
    {
        SaveLevel();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }

}
