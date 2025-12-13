using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    // ===== SINGLETON =====
    public static MenuManager Instance { get; private set; }

    // ===== PAUSE MENU =====
    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;           // The pause menu panel
    public Button resumeButton;                 // Resume game button
    public Button restartButton;                // Restart game button
    public Button exitButton;                   // Exit game button
    public TMP_InputField roundsInputField;     // Input field for changing max rounds
    public Button applyRoundsButton;            // Apply rounds change button

    // ===== END GAME MENUS (Game Over / Victory) =====
    [Header("End Game Buttons")]
    public Button gameOverRestartButton;        // Restart button on Game Over screen
    public Button gameOverExitButton;           // Exit button on Game Over screen
    public Button victoryRestartButton;         // Restart button on Victory screen
    public Button victoryExitButton;            // Exit button on Victory screen

    // ===== CURSOR MANAGEMENT =====
    private bool cursorWasLocked = true;        // Track if cursor was locked before pause

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Hide pause menu at start
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Set up button listeners
        SetupButtonListeners();

        // Initialize rounds input field with current max waves (will show saved value if it exists)
        UpdateRoundsInputField();
    }

    void UpdateRoundsInputField()
    {
        if (roundsInputField != null && GameManager.Instance != null)
        {
            roundsInputField.text = GameManager.Instance.maxWaves.ToString();
        }
    }

    void SetupButtonListeners()
    {
        // Pause Menu Buttons
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        if (applyRoundsButton != null)
            applyRoundsButton.onClick.AddListener(ApplyRoundsChange);

        // Game Over Buttons
        if (gameOverRestartButton != null)
            gameOverRestartButton.onClick.AddListener(RestartGame);

        if (gameOverExitButton != null)
            gameOverExitButton.onClick.AddListener(ExitGame);

        // Victory Buttons
        if (victoryRestartButton != null)
            victoryRestartButton.onClick.AddListener(RestartGame);

        if (victoryExitButton != null)
            victoryExitButton.onClick.AddListener(ExitGame);
    }

    // ===== PAUSE MENU MANAGEMENT =====

    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // Update the rounds input field to show current value
        UpdateRoundsInputField();

        // Show and unlock cursor when paused
        cursorWasLocked = Cursor.lockState == CursorLockMode.Locked;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Restore cursor state when unpaused
        if (cursorWasLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // ===== BUTTON ACTIONS =====

    public void ResumeGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Resume();
        }
    }

    public void RestartGame()
    {
        // Reset time scale in case it was paused
        Time.timeScale = 1f;


        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");

        // If in editor, stop play mode
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // If in build, quit application
        Application.Quit();
        #endif
    }

    public void ApplyRoundsChange()
    {
        if (roundsInputField != null && GameManager.Instance != null)
        {
            // Try to parse the input
            if (int.TryParse(roundsInputField.text, out int newMaxWaves))
            {
                // Validate the input (between 1 and 100)
                newMaxWaves = Mathf.Clamp(newMaxWaves, 1, 100);

                // Update the GameManager
                GameManager.Instance.maxWaves = newMaxWaves;

                // Save to PlayerPrefs so it persists across restarts
                PlayerPrefs.SetInt("MaxWaves", newMaxWaves);
                PlayerPrefs.Save();

                // Update the input field to show the clamped value
                roundsInputField.text = newMaxWaves.ToString();

                Debug.Log("Max waves changed to: " + newMaxWaves + " (saved for future games)");
            }
            else
            {
                // Invalid input, reset to current value
                roundsInputField.text = GameManager.Instance.maxWaves.ToString();
                Debug.LogWarning("Invalid rounds input. Please enter a number.");
            }
        }
    }

    // ===== END GAME CURSOR MANAGEMENT =====

    public void ShowEndGameCursor()
    {
        // Show cursor for end game menus
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
