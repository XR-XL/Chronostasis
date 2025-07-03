using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using JetBrains.Annotations;

public static class DataPresistenceManager
{
    public static void SaveGameTime(Timer timer)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/timer.track";
    }

    public static void SaveEnemiesEliminated() 
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/enemies.track";
    }
}
