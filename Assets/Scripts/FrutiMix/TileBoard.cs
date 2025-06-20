using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls the board where all tiles are placed and moved in the 2048 game
public class TileBoard : MonoBehaviour
{
    [SerializeField] private Tile tilePrefab;       // Prefab used to instantiate new tiles
    [SerializeField] private TileState[] tileStates; // List of possible tile states (values, colors, sprites)

    private TileGrid grid;           // Reference to the grid that contains all cells
    private List<Tile> tiles;        // List of active tiles on the board
    private bool waiting;            // Prevents multiple moves while waiting for animations

    private void Awake()
    {
        // Initialize the grid and tile list
        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>(16); // Starting with a capacity for a 4x4 board
    }

    // Clears the board of all tiles and resets cell references
    public void ClearBoard()
    {
        foreach (var cell in grid.cells)
        {
            cell.tile = null;
        }

        foreach (var tile in tiles)
        {
            Destroy(tile.gameObject);
        }

        tiles.Clear();
    }

    // Creates a new tile at a random empty cell with the initial state
    public void CreateTile()
    {
        Tile tile = Instantiate(tilePrefab, grid.transform);
        tile.SetState(tileStates[0]); // Always start with the base tile (e.g. 2)
        tile.Spawn(grid.GetRandomEmptyCell());
        tiles.Add(tile);
    }

    // Handles input for movement (WASD or arrow keys)
    private void Update()
    {
        if (waiting) return; // Block input while waiting

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Move(Vector2Int.up, 0, 1, 1, 1); // Move Up
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(Vector2Int.left, 1, 1, 0, 1); // Move Left
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Move(Vector2Int.down, 0, 1, grid.Height - 2, -1); // Move Down
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(Vector2Int.right, grid.Width - 2, -1, 0, 1); // Move Right
        }
    }

    // Main movement logic for tiles
    private void Move(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
    {
        bool changed = false;

        // Traverse the grid based on direction (to handle proper merging order)
        for (int x = startX; x >= 0 && x < grid.Width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < grid.Height; y += incrementY)
            {
                TileCell cell = grid.GetCell(x, y);

                if (cell.Occupied)
                {
                    changed |= MoveTile(cell.tile, direction); // |= ensures any successful move sets changed = true
                }
            }
        }

        // If anything moved or merged, wait and spawn a new tile
        if (changed)
        {
            StartCoroutine(WaitForChanges());
        }
    }

    // Attempts to move a tile in a direction; handles merging if possible
    private bool MoveTile(Tile tile, Vector2Int direction)
    {
        TileCell newCell = null;
        TileCell adjacent = grid.GetAdjacentCell(tile.cell, direction);

        while (adjacent != null)
        {
            if (adjacent.Occupied)
            {
                if (CanMerge(tile, adjacent.tile))
                {
                    MergeTiles(tile, adjacent.tile);
                    return true;
                }

                break; // Can't move past another tile
            }

            newCell = adjacent;
            adjacent = grid.GetAdjacentCell(adjacent, direction);
        }

        if (newCell != null)
        {
            tile.MoveTo(newCell);
            return true;
        }

        return false;
    }

    // Checks if two tiles can be merged
    private bool CanMerge(Tile a, Tile b)
    {
        return a.state == b.state && !b.locked;
    }

    // Merges tile 'a' into tile 'b', and upgrades 'b' to the next tile state
    private void MergeTiles(Tile a, Tile b)
    {
        tiles.Remove(a); // Remove merged tile from list
        a.Merge(b.cell); // Animate and destroy tile 'a'

        int index = Mathf.Clamp(IndexOf(b.state) + 1, 0, tileStates.Length - 1);
        TileState newState = tileStates[index];

        b.SetState(newState); // Upgrade tile 'b'
        FrutiMixManager.Instance.IncreaseScore(newState.number); // Add score based on new tile value
    }

    // Finds the index of a given tile state in the tileStates array
    private int IndexOf(TileState state)
    {
        for (int i = 0; i < tileStates.Length; i++)
        {
            if (state == tileStates[i])
            {
                return i;
            }
        }

        return -1;
    }

    // Coroutine that delays between move input and next tile generation
    private IEnumerator WaitForChanges()
    {
        waiting = true;

        yield return new WaitForSeconds(0.1f); // Wait to let animations finish

        waiting = false;

        // Unlock all tiles for the next turn
        foreach (var tile in tiles)
        {
            tile.locked = false;
        }

        // Spawn a new tile if there is empty space
        if (tiles.Count != grid.Size)
        {
            CreateTile();
        }

        // Check if there are no moves left
        if (CheckForGameOver())
        {
            FrutiMixManager.Instance.GameOver();
        }
    }

    // Checks if the game is over (no moves left and no possible merges)
    public bool CheckForGameOver()
    {
        if (tiles.Count != grid.Size)
        {
            return false; // Still have empty space
        }

        foreach (var tile in tiles)
        {
            TileCell up = grid.GetAdjacentCell(tile.cell, Vector2Int.up);
            TileCell down = grid.GetAdjacentCell(tile.cell, Vector2Int.down);
            TileCell left = grid.GetAdjacentCell(tile.cell, Vector2Int.left);
            TileCell right = grid.GetAdjacentCell(tile.cell, Vector2Int.right);

            // If any adjacent tile can be merged, the game is not over
            if (up != null && CanMerge(tile, up.tile)) return false;
            if (down != null && CanMerge(tile, down.tile)) return false;
            if (left != null && CanMerge(tile, left.tile)) return false;
            if (right != null && CanMerge(tile, right.tile)) return false;
        }

        // No moves and no merges available: game over
        return true;
    }

    public void MoveUp() => Move(Vector2Int.up, 0, 1, 1, 1);
    public void MoveLeft() => Move(Vector2Int.left, 1, 1, 0, 1);
    public void MoveDown() => Move(Vector2Int.down, 0, 1, grid.Height - 2, -1);
    public void MoveRight() => Move(Vector2Int.right, grid.Width - 2, -1, 0, 1);

}
// Compare this snippet from Assets/Scripts/TileState.cs: