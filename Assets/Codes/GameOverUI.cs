using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject gameOverPanel;
    public TMP_Text winnerText;
    public TMP_Text xpGainedText;
    public Button playAgainButton;
    public Button mainMenuButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Hide panel on start
        gameOverPanel.SetActive(false);

        // Add button listeners
        playAgainButton.onClick.AddListener(PlayAgain);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    public void ShowGameOver(bool playerWon, int xpGained)
    {
        gameOverPanel.SetActive(true);

        // Set winner text
        winnerText.text = playerWon ? "Victory!" : "Defeat!";
        
        // Set XP text
        xpGainedText.text = $"XP Gained: {xpGained}";

        // Optionally animate the panel appearing
        // You can add animations here later
    }

    private void PlayAgain()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ReturnToMainMenu()
    {
        // Load the main menu scene
        // Replace "MainMenu" with your actual main menu scene name
        SceneManager.LoadScene("MainMenu");
    }
}