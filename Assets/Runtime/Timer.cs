using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour, IDataPersistence
{
    [SerializeField] TextMeshProUGUI timerText;
    public float elapsedTime;
    public float gameTime;

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
        this.elapsedTime = data.timeElapsed;
        this.gameTime = data.gameTimeElapsed;
    }

    public void SaveData(ref PlayerData data) 
    {
        data.timeElapsed = Mathf.Max(data.timeElapsed, this.elapsedTime);
        data.gameTimeElapsed = Mathf.Max(data.gameTimeElapsed, this.gameTime);
    }

}
