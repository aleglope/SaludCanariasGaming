using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Represents an individual tile in the 2048 game
public class Tile : MonoBehaviour
{
    // Current state of the tile (e.g., number, colors, sprite)
    public TileState state { get; private set; }

    // The cell this tile currently occupies
    public TileCell cell { get; private set; }

    // Whether the tile is locked and cannot be moved or merged during this turn
    public bool locked { get; set; }

    // References to the tile's background and text components
    private Image background;
    private TextMeshProUGUI text;

    // Called when the object is first initialized
    private void Awake()
    {
        // Cache references to the Image and TextMeshPro components
        background = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Set the visual state of the tile (number, colors, sprite)
    public void SetState(TileState state)
    {
        this.state = state;

        // Apply the new visual settings from the given TileState
        background.color = state.backgroundColor;
        background.sprite = state.sprite;
        text.color = state.textColor;
        text.text = state.number.ToString();
    }

    // Spawn the tile in a specific cell and update its position
    public void Spawn(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = cell;
        this.cell.tile = this;
        transform.SetParent(cell.transform, false);
        transform.localPosition = Vector3.zero;

        // 🔥 Animación de aparición tipo pop
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one, 0.2f).setEaseOutBack();

        Debug.Log($"Tile {state.number} creado en {cell.name}, posición: {transform.localPosition}");
    }


    // Move the tile to a new cell with a smooth animation
    public void MoveTo(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = cell;
        this.cell.tile = this;

        transform.SetParent(cell.transform, false);

        // Animación visual con LeanTween
        LeanTween.moveLocal(gameObject, Vector3.zero, 0.1f).setEaseOutQuad();
    }


    // Merge this tile into another tile's cell
    public void Merge(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = null;

        // Lock the tile in the target cell to prevent further merging this turn
        cell.tile.locked = true;

        // Animate the merge movement and destroy this tile after animation
        transform.SetParent(cell.transform, false);
        StartCoroutine(Animate(Vector3.zero, true));
    }

    // Coroutine that smoothly moves the tile to a new position
    private IEnumerator Animate(Vector3 to, bool merging)
    {
        float elapsed = 0f;
        float duration = 0.05f;
        Vector3 from = transform.localPosition;

        while (elapsed < duration)
        {
            transform.localPosition = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = to;

        if (merging)
        {
            // 🔥 Escalado de rebote antes de destruir
            LeanTween.scale(gameObject, Vector3.one * 1.2f, 0.05f)
                .setEaseOutQuad()
                .setOnComplete(() =>
                {
                    LeanTween.scale(gameObject, Vector3.one, 0.05f)
                        .setEaseInQuad()
                        .setOnComplete(() =>
                        {
                            Destroy(gameObject);
                        });
                });
        }
    }

}
// Compare this snippet from Assets/Scripts/TileState.cs:
// using UnityEngine;