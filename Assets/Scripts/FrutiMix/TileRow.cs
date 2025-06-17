using UnityEngine;

// Represents a row in the tile grid, used to organize cells in the editor
public class TileRow : MonoBehaviour
{
    // An array of TileCells that belong to this row
    public TileCell[] cells { get; private set; }

    // Called when the object is initialized
    private void Awake()
    {
        // Automatically finds all TileCell components in child objects
        cells = GetComponentsInChildren<TileCell>();
    }
}
