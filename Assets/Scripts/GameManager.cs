using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private float gameTimeLimit = 120f; // 2 minutes
    [SerializeField] private int totalCollectibles = 0;

    [Header("UI References")]
    [SerializeField] private UIManager uiManager;

    // Game state
    private float currentTime;
    private int collectedItems;
    private bool isGameOver;
    private bool isGameWon;

    public float CurrentTime => currentTime;
    public int CollectedItems => collectedItems;
    public int TotalCollectibles => totalCollectibles;
    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        InitializeGame();
    }

    private void Update()
    {
        if (isGameOver) return;

        UpdateTimer();
    }

    private void InitializeGame()
    {
        currentTime = gameTimeLimit;
        collectedItems = 0;
        isGameOver = false;
        isGameWon = false;

        // Count all collectibles in scene
        Collectible[] collectibles = FindObjectsOfType<Collectible>();
        totalCollectibles = collectibles.Length;

        if (uiManager != null)
        {
            uiManager.UpdateCollectibleUI(collectedItems, totalCollectibles);
            uiManager.UpdateTimerUI(currentTime);
        }

        Debug.Log($"Game initialized with {totalCollectibles} collectibles");
    }


    private void UpdateTimer()
    {
        currentTime -= Time.deltaTime;

        if (uiManager != null)
            uiManager.UpdateTimerUI(currentTime);

        if (currentTime <= 0)
        {
            TriggerGameOver("Time's up!");
        }
    }


    public void CollectItem()
    {
        collectedItems++;

        if (uiManager != null)
            uiManager.UpdateCollectibleUI(collectedItems, totalCollectibles);

        Debug.Log($"Collected {collectedItems}/{totalCollectibles}");

        // Check win condition
        if (collectedItems >= totalCollectibles)
        {
            TriggerGameWon();
        }
    }

   
    public void TriggerGameOver(string reason)
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log($"Game Over: {reason}");

        if (uiManager != null)
            uiManager.ShowGameOver(reason);

        //Time.timeScale = 0f;
    }

   
    private void TriggerGameWon()
    {
        if (isGameOver) return;

        isGameOver = true;
        isGameWon = true;
        Debug.Log("Game Won!");

        if (uiManager != null)
            uiManager.ShowGameWon(currentTime);

        //Time.timeScale = 0f;
    }

 
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

   
    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
