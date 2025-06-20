using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    [System.Serializable]
    public class CardData
    {
        public Sprite normalFace;
        public Sprite matchedFace;
    }

    [Header("Prefabs y Layout")]
    public GameObject cardPrefab;
    public GameObject rowPrefab;
    public Transform panelCards;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public Image timeBarImage;

    [Header("Configuración")]
    public List<CardData> cardFaces;
    public int initialRows = 1;
    public int maxRows = 5;
    public int fixedColumns = 4;
    public float baseTime = 20f;
    public float timePerRow = 10f;
    public float maxTime = 40f;

    private int currentRowsCount;
    private float currentTime;
    private bool isGameOver = false;
    private bool isTransitioning = false;
    private bool isCheckingMatch = false;
    private int score = 0;

    private readonly List<GameObject> currentRows = new();
    private readonly List<Card> flippedCards = new();
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private GameObject restartPanel;

    void Start()
    {
        StartCoroutine(InitializeGame());
    }

    IEnumerator InitializeGame()
    {
        highScoreText.text = PlayerPrefs.GetInt("Highscore", 0).ToString();
        score = 0;
        currentRowsCount = initialRows;
        ResetTime();
        scoreText.text = score.ToString();
        AddNewBoard();
        yield return null;
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
    void Update()
    {
        if (isGameOver || isTransitioning) return;

        currentTime -= Time.deltaTime;
        float ratio = Mathf.Clamp01(currentTime / baseTime);
        timeBarImage.fillAmount = ratio;

        if (currentTime <= 0)
            StartCoroutine(HandleGameOver());
    }

    public void CardFlipped(Card card)
    {
        if (isTransitioning || isCheckingMatch || flippedCards.Contains(card)) return;

        flippedCards.Add(card);

        if (flippedCards.Count == 2)
        {
            isCheckingMatch = true;
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(0.5f);

        Card first = flippedCards[0];
        Card second = flippedCards[1];

        if (first.GetCardId() == second.GetCardId())
        {
            first.SetMatched(true);
            second.SetMatched(true);
            UpdateScore(11); // 10 base + 1 extra
            if (AllCardsMatched())
                StartCoroutine(WinAndNextRound());
        }
        else
        {
            first.FlipBack();
            second.FlipBack();
            UpdateScore(-1); // penalización
        }

        flippedCards.Clear();
        isCheckingMatch = false;
    }

    bool AllCardsMatched()
    {
        foreach (GameObject row in currentRows)
        {
            foreach (Transform card in row.transform)
            {
                Card c = card.GetComponent<Card>();
                if (!c.IsMatched()) return false;
            }
        }
        return true;
    }

    IEnumerator WinAndNextRound()
    {
        isTransitioning = true;

        foreach (GameObject row in currentRows)
        {
            foreach (Transform child in row.transform)
            {
                Card c = child.GetComponent<Card>();
                if (c != null && !c.IsMatched())
                {
                    c.Flip();
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }

        yield return new WaitForSeconds(1f);

        if (currentRowsCount < maxRows)
        {
            currentRowsCount++;
            float bonusTime = timePerRow;
            currentTime = Mathf.Min(currentTime + bonusTime, baseTime);
        }

        AddNewBoard();
        isTransitioning = false;
    }

    IEnumerator HandleGameOver()
    {
        isTransitioning = true;
        yield return new WaitForSeconds(0.5f);
        GameOver();
    }

    void AddNewBoard()
    {
        ClearBoard();

        if (cardFaces == null || cardFaces.Count == 0)
        {
            Debug.LogError("No hay imágenes disponibles en cardFaces.");
            return;
        }

        int totalCards = fixedColumns * currentRowsCount;
        int pairsNeeded = totalCards / 2;
        int maxPairs = Mathf.Min(pairsNeeded, cardFaces.Count);

        List<int> availableIndexes = new();
        for (int i = 0; i < cardFaces.Count; i++)
            availableIndexes.Add(i);

        Shuffle(availableIndexes);
        List<int> selectedIndexes = availableIndexes.GetRange(0, maxPairs);

        List<int> finalIds = new();
        foreach (int index in selectedIndexes)
        {
            finalIds.Add(index);
            finalIds.Add(index);
        }

        Shuffle(finalIds);

        int rowsToGenerate = Mathf.CeilToInt((float)finalIds.Count / fixedColumns);

        for (int row = 0; row < rowsToGenerate; row++)
        {
            GameObject newRow = Instantiate(rowPrefab, panelCards);
            currentRows.Add(newRow);

            for (int col = 0; col < fixedColumns; col++)
            {
                int index = row * fixedColumns + col;
                if (index >= finalIds.Count) break;

                int spriteIndex = finalIds[index];
                Sprite normal = cardFaces[spriteIndex].normalFace;
                Sprite matched = cardFaces[spriteIndex].matchedFace;

                GameObject cardGO = Instantiate(cardPrefab, newRow.transform);
                Card c = cardGO.GetComponent<Card>();

                if (c != null)
                {
                    c.SetUp(spriteIndex, normal, matched, this);
                    c.PlaySpawnEffect();
                }
                else
                {
                    Debug.LogError("El prefab de la carta no tiene el componente Card.cs");
                }
            }
        }
    }

    void ResetTime()
    {
        currentTime = Mathf.Min(baseTime + (initialRows - 1) * timePerRow, maxTime);
    }

    void ClearBoard()
    {
        foreach (GameObject row in currentRows)
            Destroy(row);

        currentRows.Clear();
        flippedCards.Clear();
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    void GameOver()
    {
        isGameOver = true;

        if (score > PlayerPrefs.GetInt("Highscore", 0))
        {
            PlayerPrefs.SetInt("Highscore", score);
            highScoreText.text = score.ToString();
        }

        Debug.Log("Game Over");

        if (restartPanel != null)
            AnimatePopUp(restartPanel, true);
    }
    public bool IsTransitioning() => isTransitioning || isCheckingMatch;

    void UpdateScore(int amount)
    {
        score = Mathf.Max(0, score + amount);
        scoreText.text = score.ToString();
    }
    public void RestartGame()
    {
    if (restartPanel != null)
        AnimatePopUp(restartPanel, false);

    StopAllCoroutines();
    ClearBoard();
    isGameOver = false;
    isTransitioning = false;
    isCheckingMatch = false;
    StartCoroutine(InitializeGame());
}

}
