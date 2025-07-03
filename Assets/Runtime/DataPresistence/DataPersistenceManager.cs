using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File storage config")]
    [SerializeField] private string fileName;

    private PlayerData playerData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;
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
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
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
        this.playerData = dataHandler.Load();

        // if no data, init new game
        if(this.playerData == null)
        {
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

        // save data to data handler
        dataHandler.Save(playerData);
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
