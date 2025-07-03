using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    // Stage control
    public float timeElapsed;
    public float gameTimeElapsed;

    public Dictionary<string, float> levelTimeTracker;

    public int sceneID;

    // enemies
    public int enemiesEliminated;

    // default values
    public PlayerData()
    {
        timeElapsed = 0;
        enemiesEliminated = 0;
        levelTimeTracker = new Dictionary<string, float>();
    }
}
