using UnityEngine;

// Represents a single cell on the game grid that can hold a tile
public class TileCell : MonoBehaviour
{
    // The position of the cell within the grid (X = column, Y = row)
    public Vector2Int coordinates { get; set; }

    // The tile currently occupying this cell (null if empty)
    public Tile tile { get; set; }

    // Returns true if there is no tile in this cell
    public bool Empty => tile == null;

    // Returns true if the cell is occupied by a tile
    public bool Occupied => tile != null;
}
