using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TimeDisplay : MonoBehaviour, IDataPersistence
{
    [SerializeField] TextMeshProUGUI completedTimerText;
    [SerializeField] TextMeshProUGUI bestTimerText;
    [SerializeField] Timer timer;

    private float elapsedTime;
    private float bestTime;

    private void OnEnable()
    {
        UpdateText(this.bestTime);
    }

    private void UpdateText(float bestTime)
    {
        elapsedTime = timer.elapsedTime;


        int completedMinutes = Mathf.FloorToInt(elapsedTime / 60);
        int completedSeconds = Mathf.FloorToInt(elapsedTime % 60);
        completedTimerText.text = "Time: " + string.Format("{0:00}:{1:00}", completedMinutes, completedSeconds);

        int bestMinutes = Mathf.FloorToInt(bestTime / 60);
        int bestSeconds = Mathf.FloorToInt(bestTime % 60);
        bestTimerText.text = "Best time: " + string.Format("{0:00}:{1:00}", bestMinutes, bestSeconds);
    }

    public void LoadData(PlayerData data)
    {
        data.levelTimeTracker.TryGetValue(SceneManager.GetActiveScene().name, out float bestTimeSaved);
        this.bestTime = bestTimeSaved;
        UpdateText(this.bestTime);
    }

    public void SaveData(ref PlayerData data)
    {
        
    }

}
