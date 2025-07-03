using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour, IDataPersistence
{
    [SerializeField] TextMeshProUGUI timerText;
    [Space]
    public string id;
    public float elapsedTime;
    public float gameTime;

    private void Start()
    {
        id = SceneManager.GetActiveScene().name;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!GameManager.Instance.levelCompleted)
        {
            UpdateTime();
        }
        
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
        if (GameManager.Instance.levelCompleted && data.timeElapsed > this.elapsedTime)
        {
            data.timeElapsed = this.elapsedTime;
            data.gameTimeElapsed = this.gameTime;
            data.sceneID = SceneManager.GetActiveScene().name;
            if (data.levelTimeTracker.ContainsKey(SceneManager.GetActiveScene().name))
            {
                data.levelTimeTracker.Remove(SceneManager.GetActiveScene().name);
            }
            data.levelTimeTracker.Add(SceneManager.GetActiveScene().name, elapsedTime);
        }
    }
}
