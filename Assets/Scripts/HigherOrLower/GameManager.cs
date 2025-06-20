using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public PlatoUIManager platoUIManager;
    public GameObject panelRestart;
    private CanvasGroup canvasGroupRestart;
    public GameObject helpPanel;
    [Header("Panel Central")]
    public TextMeshProUGUI countScore;
    public TextMeshProUGUI countHighScore;
    private bool bloqueado = false;

    private bool isGameOver = false;
    private int score = 0;
    private int highScore = 0;
    private PlatoData platoIzquierdo;
    private PlatoData platoDerecho;
    private PlatoData[] todosLosPlatos;
    private int contadorDominante = 0;
    private bool ultimoGanadorDerecha = false;

    private void Start()
    {
        todosLosPlatos = Resources.LoadAll<PlatoData>("HigherOrLower/ScriptableObjects/Platos");


        if (todosLosPlatos.Length < 2)
        {
            Debug.LogError("Se requieren al menos 2 platos.");
            return;
        }

        platoIzquierdo = GetPlatoAleatorio(null);
        platoDerecho = GetPlatoAleatorio(platoIzquierdo);

        platoUIManager.SetPlatoLeft(platoIzquierdo);
        platoUIManager.SetPlatoRight(platoDerecho, false);

        SetScore(0);
        SetHighScore(PlayerPrefs.GetInt("Highscore", 0));
        if (panelRestart != null)
        {
            canvasGroupRestart = panelRestart.GetComponent<CanvasGroup>();
            panelRestart.SetActive(false);
        }
    }

    private PlatoData GetPlatoAleatorio(PlatoData evitar)
    {
        PlatoData candidato;
        do
        {
            candidato = todosLosPlatos[Random.Range(0, todosLosPlatos.Length)];
        } while (candidato == evitar);
        return candidato;
    }

    public void SetScore(int valor)
    {
        score = valor;
        countScore.text = $"{score}";
    }

    public void SetHighScore(int valor)
    {
        highScore = valor;
        countHighScore.text = $"{valor}";
    }

    public void GameOver()
    {
        isGameOver = true;

        if (score > PlayerPrefs.GetInt("Highscore", 0))
        {
            PlayerPrefs.SetInt("Highscore", score);
            countHighScore.text = $"{score}";
        }

        Debug.Log("Game Over");

        // Mostrar panel con animación
        if (panelRestart != null && canvasGroupRestart != null)
        {
            panelRestart.SetActive(true);
            canvasGroupRestart.alpha = 0f;
            canvasGroupRestart.interactable = false;
            canvasGroupRestart.blocksRaycasts = false;

            // Escala desde 0.8 a 1 y fade de 0 a 1
            panelRestart.transform.localScale = Vector3.one * 0.8f;

            LeanTween.alphaCanvas(canvasGroupRestart, 1f, 0.6f);
            LeanTween.scale(panelRestart, Vector3.one, 0.6f).setEaseOutBack().setOnComplete(() =>
            {
                canvasGroupRestart.interactable = true;
                canvasGroupRestart.blocksRaycasts = true;
            });
        }
    }

    public void ReiniciarPartida()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }

    public void CompararEleccion(bool eligeDerecha)
    {
        if (isGameOver || bloqueado) return;
        bloqueado = true; // Bloqueamos más clics
        StartCoroutine(CompararConDelay(eligeDerecha));
    }

    private IEnumerator CompararConDelay(bool eligeDerecha)
    {
        bool acierto = false;

        platoUIManager.SetPlatoLeft(platoIzquierdo, true);
        platoUIManager.SetPlatoRight(platoDerecho, true);

        int caloriasIzq = platoIzquierdo.caloriasPorRacion;
        int caloriasDer = platoDerecho.caloriasPorRacion;

        if (eligeDerecha && caloriasDer >= caloriasIzq) acierto = true;
        if (!eligeDerecha && caloriasIzq >= caloriasDer) acierto = true;

        if (acierto)
        {
            platoUIManager.SetPlatoCorrect(eligeDerecha);
            SetScore(score + 1);
        }
        else
        {
            platoUIManager.SetPlatoIncorrect(eligeDerecha);
        }

        // Esperar antes de permitir otro clic
        yield return new WaitForSeconds(2f);

        if (acierto)
        {
            if (contadorDominante > 0 && eligeDerecha == ultimoGanadorDerecha)
            {
                platoIzquierdo = GetPlatoAleatorio(null);
                platoDerecho = GetPlatoAleatorio(platoIzquierdo);
                contadorDominante = 0;
                platoUIManager.SetPlatoLeft(platoIzquierdo, false);
                platoUIManager.SetPlatoRight(platoDerecho, false);
            }
            else
            {
                if (eligeDerecha == ultimoGanadorDerecha)
                    contadorDominante++;
                else
                    contadorDominante = 1;

                ultimoGanadorDerecha = eligeDerecha;

                PlatoData nuevo = GetPlatoAleatorio(eligeDerecha ? platoIzquierdo : platoDerecho);
                if (eligeDerecha)
                    platoIzquierdo = nuevo;
                else
                    platoDerecho = nuevo;

                if (eligeDerecha)
                {
                    platoUIManager.SetPlatoLeft(platoIzquierdo, false);
                    platoUIManager.SetPlatoRight(platoDerecho, true);
                }
                else
                {
                    platoUIManager.SetPlatoLeft(platoIzquierdo, true);
                    platoUIManager.SetPlatoRight(platoDerecho, false);
                }
            }
        }
        else
        {
            GameOver();
            platoUIManager.SetPlatoRight(platoDerecho, true);
            platoUIManager.SetPlatoLeft(platoIzquierdo, true);
        }

        // ✅ Ahora sí desbloqueamos al final
        bloqueado = false;
    }
    public void Help()
    {
        UIAnimator.AnimatePopUp(helpPanel, true);
    }

    public void CloseHelp()
    {
        UIAnimator.AnimatePopUp(helpPanel, false);
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
