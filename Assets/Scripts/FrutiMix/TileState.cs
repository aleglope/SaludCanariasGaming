using UnityEngine;

// Allows creating custom tile configurations in the Unity editor
[CreateAssetMenu(menuName = "Tile State")]
public class TileState : ScriptableObject
{
    // The number displayed on the tile (e.g. 2, 4, 8, ..., 2048)
    public int number;

    // Background color of the tile based on its number
    public Color backgroundColor;

    // Text color used to display the number
    public Color textColor;

    // Optional sprite used as a background or texture (can be null)
    public Sprite sprite;
}
//         this.cell = cell;
//         this.cell.tile = this;