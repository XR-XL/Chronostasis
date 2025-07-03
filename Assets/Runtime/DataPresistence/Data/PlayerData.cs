using UnityEngine;

[System.Serializable]
public class PlayerData
{
    // Stage control
    public float timeElapsed;
    public float gameTimeElapsed;
    public int sceneID;

    // enemies
    public int enemiesEliminated;

    // default values
    public PlayerData()
    {
        timeElapsed = 0;
        enemiesEliminated = 0;
    }
}
