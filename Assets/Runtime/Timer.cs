using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour, IDataPersistence
{
    [SerializeField] TextMeshProUGUI timerText;
    [Space]
    public string id;
    public float elapsedTime;
    public float gameTime;

    private bool levelCompleted;

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateTime();
    }

    private void UpdateTime()
    {
        elapsedTime += Time.fixedDeltaTime;

        UpdateText(elapsedTime);

        if (!GameManager.Instance.timestopTriggered)
        {
            gameTime += Time.fixedDeltaTime;
        }
    }

    private void UpdateText(float elapsedTime)
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void LoadData(PlayerData data)
    {

    }

    public void SaveData(ref PlayerData data)
    {
        data.timeElapsed = Mathf.Min(data.timeElapsed, this.elapsedTime);
        data.gameTimeElapsed = Mathf.Min(data.gameTimeElapsed, this.gameTime);
        if (levelCompleted && data.timeElapsed > this.elapsedTime)
            data.levelTimeTracker.Add(id, elapsedTime);
    }
}
