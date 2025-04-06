using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverScoreTotalDis;
    [SerializeField] private TextMeshProUGUI gameOverScoreTotalScore;
    [SerializeField] private GameObject[] hearts;

    public void UpdateScoreText(int score)
    {
        if (scoreText != null)
            scoreText.text = $" {score}";
    }

    public void UpdateDistanceText(float distance, bool stopDistance)
    {
        if (distanceText != null && !stopDistance)
            distanceText.text = $"    {Mathf.FloorToInt(distance)}m";
    }

    public void UpdateHighScoreText(int highScore)
    {
        if (highScoreText != null)
            highScoreText.text = $"    {highScore}";
    }

    public void ShowGameOverUI(float distance, int score)
    {
        SetGameOverPanelActive(true);
        // Total distance next line the distance
        gameOverScoreTotalDis.text = $"Total Distance : " +
                                     $"{Mathf.FloorToInt(distance)}m";
        gameOverScoreTotalScore.text = $"Total Score : " +
                                       $"{score}";
    }

    public void SetGameOverPanelActive(bool isActive)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(isActive);
    }

    public void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].SetActive(i < currentHealth);
        }
    }
}