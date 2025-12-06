using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI collectibleText;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Game Won Panel")]
    [SerializeField] private GameObject gameWonPanel;
    [SerializeField] private TextMeshProUGUI gameWonText;
    [SerializeField] private TextMeshProUGUI finalTimeText;
    [SerializeField] private Button wonRestartButton;
    [SerializeField] private Button wonQuitButton;

    private void Start()
    {
        // Hide end game panels
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (gameWonPanel != null)
            gameWonPanel.SetActive(false);

        // Setup button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(() => GameManager.Instance?.RestartGame());

        if (quitButton != null)
            quitButton.onClick.AddListener(() => GameManager.Instance?.QuitGame());

        if (wonRestartButton != null)
            wonRestartButton.onClick.AddListener(() => GameManager.Instance?.RestartGame());

        if (wonQuitButton != null)
            wonQuitButton.onClick.AddListener(() => GameManager.Instance?.QuitGame());
    }

 
    public void UpdateTimerUI(float timeRemaining)
    {
        if (timerText == null) return;

        timeRemaining = Mathf.Max(0, timeRemaining);
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);

        timerText.text = $"Time: {minutes:00}:{seconds:00}";

        // Change color when time is running out
        if (timeRemaining < 30f)
            timerText.color = Color.red;
        else if (timeRemaining < 60f)
            timerText.color = Color.yellow;
        else
            timerText.color = Color.white;
    }


    public void UpdateCollectibleUI(int collected, int total)
    {
        if (collectibleText == null) return;

        collectibleText.text = $"Cubes: {collected}/{total}";
    }


    public void ShowGameOver(string reason)
    {
        if (gameOverPanel == null) return;

        gameOverPanel.SetActive(true);

        if (gameOverText != null)
            gameOverText.text = $"Game Over!\n{reason}";

        // Ensure cursor is visible and unlocked for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    public void ShowGameWon(float timeRemaining)
    {
        if (gameWonPanel == null) return;

        gameWonPanel.SetActive(true);

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);

        if (finalTimeText != null)
            finalTimeText.text = $"Time Remaining: {minutes:00}:{seconds:00}";

        // Ensure cursor is visible and unlocked for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
