using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")] 
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private float initialGameSpeed = 5f;
    [SerializeField] private float maxGameSpeed = 20f;
    [SerializeField] private float gameSpeedIncreaseRate = 0.1f;
    [SerializeField] private float gameStartDelay = 1f;

    public static GameManager Instance { get; private set; }
    public float GameSpeed { get; private set; }

    public bool StopDistance { get; set; }
    public bool IsGameActive { get; private set; }

    private int score;
    private float distance;
    private int highScore;

    private UIManager uiManager;

    private void Awake()
    {
        InitializeSingleton();
        LoadHighScore();
        uiManager = FindObjectOfType<UIManager>();
    }
    private void Start()
    {
        InitializeGame();
        StartCoroutine(StartGameRoutine());
    }
    private void Update()
    {
        if (!IsGameActive) return;

        UpdateGameSpeed();
        UpdateDistance();
        UpdateUI();
    }
    private void InitializeSingleton()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }
    private void InitializeGame()
    {
        GameSpeed = initialGameSpeed;
        UpdateUI();
        uiManager.SetGameOverPanelActive(false);
    }
    private IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(gameStartDelay);
        IsGameActive = true;
    }
    private void UpdateGameSpeed()
    {
        GameSpeed += gameSpeedIncreaseRate * Time.deltaTime;
        GameSpeed = Mathf.Min(GameSpeed, maxGameSpeed);
    }
    private void UpdateDistance()
    {
        distance += GameSpeed * Time.deltaTime;
    }
    private void UpdateUI()
    {
        uiManager.UpdateScoreText(score);
        uiManager.UpdateDistanceText(distance, StopDistance);
        uiManager.UpdateHighScoreText(highScore);
    }
    public void AddScore(int points)
    {
        score += points;
        UpdateHighScore();
        UpdateUI();
    }
    private void UpdateHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }
    }
    public void GameOver()
    {
        if (!IsGameActive) return;

        IsGameActive = false;
        uiManager.ShowGameOverUI(distance, score);
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Adjust scene name as needed
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("EndlessRunner");
    }
}