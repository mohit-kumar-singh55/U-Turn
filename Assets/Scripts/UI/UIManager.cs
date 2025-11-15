using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("InGame UI")]
    [SerializeField] TMP_Text speedText;
    [SerializeField] TMP_Text timerText;
    [SerializeField] TMP_Text lapsCountText;

    [Header("Menu UI")]
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] GameObject loseUI;
    [SerializeField] GameObject winUI;
    [SerializeField] GameObject nextLevelButton;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetSpeedText(float speed) => speedText.text = speed.ToString();

    public void SetTimerText(float time) => timerText.text = time.ToString("f2");

    public void SetLapsText(int lapsCompleted, int totalLaps) => lapsCountText.text = $"{lapsCompleted}/{totalLaps}";

    public void ShowGameOverPanelUI(bool show) => gameOverPanel.SetActive(show);

    public void ShowLoseUI(bool show) => loseUI.SetActive(show);

    public void ShowWinUI(bool show) => winUI.SetActive(show);

    public void ShowNextLevelButton(bool show) => nextLevelButton.SetActive(show);
}
