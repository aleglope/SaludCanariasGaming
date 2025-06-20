using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Representa una casilla individual del tablero donde se muestra una letra
public class TileSaludle : MonoBehaviour
{
    // Clase anidada que define el estado visual de una casilla
    [System.Serializable]
    public class State {
        public Color fillColor;     // Color de fondo de la casilla
        public Color outlineColor;  // Color del borde de la casilla
    }
    public Image FillImage => fill;
    public Outline OutlineComponent => outline;
    public Color CurrentFillColor => fill.color;
    public Color CurrentOutlineColor => outline.effectColor;

    // Estado actual de la casilla (vacía, correcta, incorrecta, etc.)
    public State state { get; private set; }

    // Letra actual mostrada en esta casilla
    public char letter { get; private set; }

    // Referencias a componentes UI necesarios para mostrar la letra y el estilo
    private TextMeshProUGUI text;  // Componente de texto (letra)
    private Image fill;            // Componente de fondo (color de relleno)
    private Outline outline;       // Componente de contorno (borde)

    // Se ejecuta al instanciarse el objeto o cargarse en escena
    private void Awake() {
        // Busca automáticamente el componente de texto dentro del hijo
        text = GetComponentInChildren<TextMeshProUGUI>();

        // Obtiene los componentes Image y Outline desde el mismo GameObject
        fill = GetComponent<Image>();
        outline = GetComponent<Outline>();
    }

    // Asigna una letra a la casilla y la muestra en pantalla
    public void SetLetter(char letter) {
        this.letter = letter;

        // Si la letra es nula ('\0'), muestra una cadena vacía
        text.text = letter == '\0' ? "" : letter.ToString();
        // Si hay una letra, aplicamos rebote visual
        if (letter != '\0') {
            UIAnimator.AnimateTileInput(gameObject);
        }
    }

    // Asigna un estado visual a la casilla (colores) y lo aplica
    public void SetState(State state) {
        this.state = state;
        // Voltear el tile en dos fases: escala X a 0 (oculta) → cambia color → escala X a 1 (muestra)
        UIAnimator.AnimateTileFlip(gameObject, fill, outline, state.fillColor, state.outlineColor);
        // Cambia el color de fondo y el borde de la casilla
        fill.color = state.fillColor;
        outline.effectColor = state.outlineColor;

    }
}
