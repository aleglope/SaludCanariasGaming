using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Board : MonoBehaviour
{
    private static readonly KeyCode[] SUPPORTED_KEYS = new KeyCode[]{
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F,
        KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L,
        KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R,
        KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X,
        KeyCode.Y, KeyCode.Z
    };

    private Row[] rows;
    private string[] solutions;
    private string[] validWords;
    private string word;
    private int rowIndex;
    private int columnIndex;
    private bool isAnimating = false;

    [Header("States")]
    public TileSaludle.State emptyState;
    public TileSaludle.State occupateState;
    public TileSaludle.State correctState;
    public TileSaludle.State wrongSpotState;
    public TileSaludle.State incorrectState;

    [Header("UI")]
    public TextMeshProUGUI titleGameText;
    public GameObject invalidWordText;
    public Button newWordButton;
    public Button tryAgainButton;
    public Button helpButton;
    public Button closeHelpButton;
    public GameObject helpPanel;
    public GameObject finishBoard;

    [Header("Botones interactivos")]
    public Button[] interactableButtons;

    private void Awake()
    {
        rows = GetComponentsInChildren<Row>();
    }

    private void Start()
    {
        LoadData();
        NewGame();
    }

    public void NewGame()
    {
        // Asegúrate de restaurar el finishBoard
        if (finishBoard != null)
        {
            finishBoard.SetActive(false);
            foreach (var tile in finishBoard.GetComponentsInChildren<Transform>())
            {
                tile.localScale = Vector3.one;
                tile.localRotation = Quaternion.identity;
            }
        }
        UIAnimator.AnimateTextCharacters(titleGameText, this);
        ClearBoard();
        setRandomWord();
        newWordButton.gameObject.SetActive(false);
        tryAgainButton.gameObject.SetActive(false);
        enabled = true;
    }

    public void TryAgain()
    {
        // Asegúrate de restaurar el finishBoard
        UIAnimator.AnimateTextCharacters(titleGameText, this);
        ClearBoard();
        newWordButton.gameObject.SetActive(false);
        tryAgainButton.gameObject.SetActive(false);
        enabled = true;
    }

    public void Help()
    {
        UIAnimator.AnimatePopUp(helpPanel, true);
    }

    public void CloseHelp()
    {
        UIAnimator.AnimatePopUp(helpPanel, false);
    }

    private void LoadData()
    {
        TextAsset textFile = Resources.Load("Saludle/palabras_wordle_salud") as TextAsset;
        TextAsset textFileValid = Resources.Load("Saludle/palabras_validas") as TextAsset;

        if (textFile == null || textFileValid == null)
        {
            Debug.LogError("No se encontró uno de los archivos de palabras en Resources.");
            return;
        }

        validWords = textFileValid.text.Split('\n');
        solutions = textFile.text.Split('\n');
    }

    private void setRandomWord()
    {
        word = solutions[Random.Range(0, solutions.Length)];
        word = word.ToLower().Trim();
    }

    private void Update()
    {
        if (isAnimating) return;

        Row currentRow = rows[rowIndex];

        if (InputSimulator.GetKeyDown(KeyCode.Backspace))
        {
            invalidWordText.gameObject.SetActive(false);
            columnIndex = Mathf.Max(columnIndex - 1, 0);
            rows[rowIndex].tiles[columnIndex].SetState(emptyState);
            rows[rowIndex].tiles[columnIndex].SetLetter('\0');
        }
        else if (columnIndex >= currentRow.tiles.Length)
        {
            if (InputSimulator.GetKeyDown(KeyCode.Return))
            {
                SubmitRow(currentRow);
            }
        }
        else
        {
            for (int i = 0; i < SUPPORTED_KEYS.Length; i++)
            {
                if (InputSimulator.GetKeyDown(SUPPORTED_KEYS[i]))
                {
                    char letter = SUPPORTED_KEYS[i].ToString()[0];
                    rows[rowIndex].tiles[columnIndex].SetLetter(letter);
                    rows[rowIndex].tiles[columnIndex].SetState(occupateState);
                    columnIndex++;
                    break;
                }
            }

            if (Input.inputString == "ñ" || Input.inputString == "Ñ")
            {
                rows[rowIndex].tiles[columnIndex].SetLetter('Ñ');
                columnIndex++;
            }
        }
    }

    private void SubmitRow(Row row)
    {
        if (!isValidWord(row.word))
        {
            invalidWordText.gameObject.SetActive(true);
            return;
        }
        SetButtonsInteractable(false);
        isAnimating = true;
        StartCoroutine(AnimateRowReveal(row));
    }

    private IEnumerator AnimateRowReveal(Row row)
    {
        string tempSolution = word;

        for (int i = 0; i < row.tiles.Length; i++)
        {
            TileSaludle tile = row.tiles[i];
            char guessedLetter = char.ToLower(tile.letter);

            if (guessedLetter == word[i])
            {
                tile.SetState(correctState);
                tempSolution = ReplaceCharAt(tempSolution, i, '*');
            }
            yield return new WaitForSeconds(0.15f);
        }

        for (int i = 0; i < row.tiles.Length; i++)
        {
            TileSaludle tile = row.tiles[i];
            if (tile.state == correctState) continue;

            char guessedLetter = char.ToLower(tile.letter);
            int indexInWord = tempSolution.IndexOf(guessedLetter);

            if (indexInWord != -1)
            {
                tile.SetState(wrongSpotState);
                tempSolution = ReplaceCharAt(tempSolution, indexInWord, '*');
            }
            else
            {
                tile.SetState(incorrectState);
            }
            yield return new WaitForSeconds(0.15f);
        }

        if (hasWon(row))
        {
            enabled = false;
            if (finishBoard != null)
            {
                finishBoard.SetActive(true);
                UIAnimator.AnimateFinishBoardTiles(finishBoard, this);
            }
            UIAnimator.AnimateButtonAppear(newWordButton);
            newWordButton.gameObject.SetActive(true);
            tryAgainButton.gameObject.SetActive(false);

            yield break;
        }

        rowIndex++;
        columnIndex = 0;

        if (rowIndex == rows.Length)
        {
            enabled = false;
            newWordButton.gameObject.SetActive(true);
            tryAgainButton.gameObject.SetActive(true);
        }
        SetButtonsInteractable(true);
        isAnimating = false;
    }

    private void ClearBoard()
    {
        for (int row = 0; row < rows.Length; row++)
        {
            for (int col = 0; col < rows[row].tiles.Length; col++)
            {
                rows[row].tiles[col].SetLetter('\0');
                rows[row].tiles[col].SetState(emptyState);
            }
        }
        rowIndex = 0;
        columnIndex = 0;
        SetButtonsInteractable(true);
        isAnimating = false;
    }

    private string ReplaceCharAt(string str, int index, char newChar)
    {
        char[] chars = str.ToCharArray();
        chars[index] = newChar;
        return new string(chars);
    }

    private bool isValidWord(string word)
    {
        word = word.ToLower().Trim();
        for (int i = 0; i < validWords.Length; i++)
        {
            if (validWords[i].Trim().ToLower() == word)
            {
                return true;
            }
        }
        return false;
    }

    private bool hasWon(Row row)
    {
        for (int i = 0; i < row.tiles.Length; i++)
        {
            if (row.tiles[i].state != correctState)
            {
                return false;
            }
        }
        return true;
    }

    private void SetButtonsInteractable(bool interactable)
    {
        foreach (var button in interactableButtons)
        {
            button.interactable = interactable;
        }
    }

    public void OnNewGameClicked()
    {
        Invoke(nameof(NewGame), 0.1f);
    }

    public void OnTryAgainClicked()
    {
        Invoke(nameof(TryAgain), 0.1f);
    }

    public void OnHelpClicked()
    {
        Invoke(nameof(Help), 0.1f);
    }

    public void OnCloseHelpClicked()
    {
        Invoke(nameof(CloseHelp), 0.1f);
    }
}
