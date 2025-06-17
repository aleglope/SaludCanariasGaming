using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Representa una fila del tablero del juego, compuesta por varias celdas (tiles)
public class Row : MonoBehaviour
{
    // Arreglo de objetos Tile que representan las casillas de esta fila
    public TileSaludle[] tiles { get; private set; }

    // Propiedad que construye la palabra formada por las letras actuales de la fila
    public string word {
        get {
            string word = "";

            // Recorre cada tile de la fila y concatena su letra a la cadena final
            for (var i = 0; i < tiles.Length; i++) {
                word += tiles[i].letter;
            }

            return word;
        }
    }

    // Se ejecuta automáticamente cuando la escena se inicia o se instancia el objeto
    private void Awake()
    {
        // Obtiene todos los componentes Tile que sean hijos de este GameObject
        tiles = GetComponentsInChildren<TileSaludle>();
    }
}
