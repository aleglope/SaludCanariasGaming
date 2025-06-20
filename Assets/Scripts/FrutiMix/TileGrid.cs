using UnityEngine;

// Represents the entire grid that holds all tile cells and rows
public class TileGrid : MonoBehaviour
{
    // Array of rows in the grid (each row contains TileCells)
    public TileRow[] rows { get; private set; }

    // Flattened array of all cells in the grid
    public TileCell[] cells { get; private set; }

    // Total number of cells in the grid
    public int Size => cells.Length;

    // Number of rows (height of the grid)
    public int Height => rows.Length;

    // Number of columns (width of the grid), calculated from total size and height
    public int Width => Size / Height;

    private void Awake()
    {
        // Get all rows and cells present in children
        rows = GetComponentsInChildren<TileRow>();
        cells = GetComponentsInChildren<TileCell>();

        // Assign coordinates to each cell based on its index
        for (int i = 0; i < cells.Length; i++) {
            cells[i].coordinates = new Vector2Int(i % Width, i / Width);
        }
    }

    // Returns a cell using Vector2Int coordinates (x, y)
    public TileCell GetCell(Vector2Int coordinates)
    {
        return GetCell(coordinates.x, coordinates.y);
    }

    // Returns a cell at a specific (x, y) coordinate, or null if out of bounds
    public TileCell GetCell(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height) {
            return rows[y].cells[x];
        } else {
            return null;
        }
    }

    // Returns the cell adjacent to a given one in a specific direction (up/down/left/right)
    public TileCell GetAdjacentCell(TileCell cell, Vector2Int direction)
    {
        Vector2Int coordinates = cell.coordinates;

        // Note: Y axis is inverted (Unity UI coordinates)
        coordinates.x += direction.x;
        coordinates.y -= direction.y;

        return GetCell(coordinates);
    }

    // Returns a random empty cell in the grid, or null if the grid is full
    public TileCell GetRandomEmptyCell()
    {
        int index = Random.Range(0, cells.Length);
        int startingIndex = index;

        // Loop through the cells until an empty one is found
        while (cells[index].Occupied)
        {
            index++;

            if (index >= cells.Length) {
                index = 0;
            }

            // Full loop with no empty cells found
            if (index == startingIndex) {
                return null;
            }
        }

        return cells[index];
    }
}
// Compare this snippet from Assets/Scripts/TileRow.cs: