using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelComplete : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject playerController;
    [SerializeField] private GameObject playerHUD;
    public static bool GameIsPaused = false;
    public GameObject levelCompleteUI;


    void Update()
    {
        if (GameManager.Instance.levelCompleted)
        {
            Pause();
        }
    }

    public void Resume()
    {

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        levelCompleteUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        GameManager.Instance.UpdatePauseStatus(GameIsPaused); // updates singleton
        playerController.SetActive(true);
    }

    void Pause()
    {

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        levelCompleteUI.SetActive(true);
        Time.timeScale = 0f; // functionally pauses the game
        GameIsPaused = true;
        GameManager.Instance.UpdatePauseStatus(GameIsPaused);
        playerController.SetActive(false);
    }

    public void GoToScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Resume();
    }
}
