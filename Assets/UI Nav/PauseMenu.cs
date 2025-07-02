using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject playerController;
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
            playerController.SetActive(!GameIsPaused); // disbales the character from moving, flips based on the current status
        }
    }

    public void Resume()
    {
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        GameManager.Instance.UpdatePauseStatus(GameIsPaused); // updates singleton
        playerController.SetActive(true); // always allows the player to act after they press resume
    }
    void Pause()
    {
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // functionally pauses the game
        GameIsPaused = true;
        GameManager.Instance.UpdatePauseStatus(GameIsPaused);
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
