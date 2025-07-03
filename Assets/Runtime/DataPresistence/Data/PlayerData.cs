using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    // Stage control
    public float timeElapsed;
    public float gameTimeElapsed;

    public SerializableDictionary<string, float> levelTimeTracker;

    public string sceneID;

    // enemies
    public int enemiesEliminated;

    // default values
    public PlayerData()
    {
        timeElapsed = 356459;
        enemiesEliminated = 0;
        sceneID = "";
        levelTimeTracker = new SerializableDictionary<string, float>();
    }
}
