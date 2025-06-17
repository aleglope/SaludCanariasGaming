using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Ensures GameManager executes before other scripts by default
[DefaultExecutionOrder(-1)]
public class FrutiMixManager : MonoBehaviour
{
    // Singleton pattern: allows global access to the GameManager instance
    public static FrutiMixManager Instance { get; private set; }

    [SerializeField] private TileBoard board;                 // Reference to the board that manages tile logic
    [SerializeField] private CanvasGroup gameOver;            // UI element shown when the game ends
    [SerializeField] private TextMeshProUGUI scoreText;       // UI text element showing the current score
    [SerializeField] private TextMeshProUGUI hiscoreText;     // UI text element showing the best score
    [SerializeField] private GameObject helpPanel; // Panel de ayuda mostrado/ocultado por animación

    public int score { get; private set; } = 0;               // Player's current score

    private void Awake()
    {
        // Singleton enforcement: destroy duplicates and persist the main instance
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        // Clear singleton reference when this instance is destroyed
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        // Start a new game on launch
        NewGame();
    }
    public void Help()
    {
        AnimatePopUp(helpPanel, true);
    }

    public void CloseHelp()
    {
        AnimatePopUp(helpPanel, false);
    }

    public void OnHelpClicked()
    {
        Invoke(nameof(Help), 0.1f);
    }

    public void OnCloseHelpClicked()
    {
        Invoke(nameof(CloseHelp), 0.1f);
    }
    public static void AnimatePopUp(GameObject panel, bool show)
    {
        if (panel == null) return;

        if (show)
        {
            panel.transform.localScale = Vector3.zero; // Escala inicial al abrir
            panel.SetActive(true); // Activa el panel
            LeanTween.scale(panel, Vector3.one, 0.7f).setEaseOutBack(); // Animación de entrada
        }
        else
        {
            // Reduce escala a 0 y desactiva el panel al finalizar
            LeanTween.scale(panel, Vector3.zero, 0.7f).setEaseInBack().setOnComplete(() =>
            {
                panel.SetActive(false);
            });
        }
    }
    // Resets game state and starts a new game
    public void NewGame()
    {
        SetScore(0); // Reset the score
        hiscoreText.text = LoadHiscore().ToString(); // Load and display the high score

        // Hide the game over screen
        gameOver.alpha = 0f;
        gameOver.interactable = false;

        // Clear and initialize the board with two tiles
        board.ClearBoard();
        StartCoroutine(SpawnTilesNextFrame()); // Wait for the next frame to spawn tiles
        board.enabled = true; // Enable player input
    }

    // Called when no moves are left and the game ends
    public void GameOver()
    {
        board.enabled = false; // Disable input
        gameOver.interactable = true; // Enable interaction with game over UI

        // Fade in the game over screen
        StartCoroutine(Fade(gameOver, 1f, 1f));
    }

    // Smoothly fades a CanvasGroup's alpha over time
    private IEnumerator Fade(CanvasGroup canvasGroup, float to, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        float duration = 0.5f;
        float from = canvasGroup.alpha;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    // Adds points to the score
    public void IncreaseScore(int points)
    {
        SetScore(score + points);
    }

    // Sets the score and updates UI
    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString();

        SaveHiscore(); // Check and update high score if needed
    }

    // Saves the current score as high score if it's greater
    private void SaveHiscore()
    {
        int hiscore = LoadHiscore();

        if (score > hiscore)
        {
            PlayerPrefs.SetInt("hiscore", score);
        }
    }

    // Loads the stored high score from PlayerPrefs
    private int LoadHiscore()
    {
        return PlayerPrefs.GetInt("hiscore", 0);
    }
    /// Creates two new tiles on the board at the start of the game
    private IEnumerator SpawnTilesNextFrame()
    {
        yield return null; // wait one frame
        board.CreateTile();
        board.CreateTile();
    }

}
// Compare this snippet from Assets/Scripts/Tile.cs:
// using System.Collections;