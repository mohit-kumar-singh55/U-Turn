using UnityEngine;
using UnityEngine.SceneManagement;

// Singleton Class
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private UIManager uiManager;
    // private AudioManager audioManager;
    private bool gameEnded = false;
    private bool menuActive = false;

    private readonly KeyCode cheatKeyNextLevel = KeyCode.L;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad(gameObject);

        ShowCursor(false);
    }

    void Start()
    {
        uiManager = UIManager.Instance;
    }

    void Update()
    {
        // cheat to go to next level
        if (Input.GetKeyDown(cheatKeyNextLevel)) GoToNextLevel();
    }

    public void ResumeGame() => SetShowMenu();

    public void SetShowMenu()
    {
        menuActive = !menuActive;
        // uiManager.ShowMenuUI(menuActive);
        Time.timeScale = menuActive ? 0 : 1;
        ShowCursor(menuActive);
    }

    public void TriggerLose()
    {
        if (gameEnded) return;

        gameEnded = true;

        GameOverSequence();
        uiManager.ShowLoseUI(true);
    }

    public void TriggerWin()
    {
        if (gameEnded) return;

        gameEnded = true;

        GameOverSequence();
        uiManager.ShowWinUI(true);
        uiManager.ShowNextLevelButton(true);
    }

    public void GameOverSequence()
    {
        ShowCursor(true);
        Timer.Instance.StopTimer(true);
        // AudioManager.Instance.StopAllAudios(true);
        uiManager.ShowGameOverPanelUI(true);
        Time.timeScale = 0.05f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    void ShowCursor(bool show = true)
    {
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = show;
    }

    public void ReloadGame()
    {
        Instance = null;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public void GoToNextLevel()
    {
        Instance = null;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // 0 -> intro scene, 1 -> main menu, 2 -> levels...
        int nextLevelInd = (SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings;
        nextLevelInd = nextLevelInd <= 1 ? 2 : nextLevelInd;
        SceneManager.LoadScene(nextLevelInd);
    }

    public void QuitGame() => Application.Quit();
}
