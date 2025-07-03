using UnityEngine;

[DefaultExecutionOrder(-5)]
public class GameManager : MonoBehaviour
{
    //initialise singleton

    public static GameManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {

    }

    // handling global variables
    // Timestop
    public bool timestopTriggered = false;

    public void UpdateTimestopStatus(bool _requestedTimestop)
    {
        timestopTriggered = _requestedTimestop;
        if (timestopTriggered)
        {
            Debug.Log("In timestop");
        }
    }

    // Game pause
    public bool gamePaused = false;

    public void UpdatePauseStatus(bool GameIsPaused)
    {
        gamePaused = GameIsPaused;
    }

    // Enemies eliminated 

    public int enemiesKilled = 0;

    public void UpdateEliminationCount(int count)
    {
        enemiesKilled += count;
    }

}
