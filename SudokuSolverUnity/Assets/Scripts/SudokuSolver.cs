using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum SolveStrategy
{
    LinePerLine,
    ColumnPerColumn,
    SquarePerSquare
}

public class SudokuSolver : MonoBehaviour
{
    public TMP_Text[,] gridText = new TMP_Text[9, 9];
    public GameObject cellPrefab;
    public Transform gridParent;

    public float speed = 0.1f;

    // Add an enum field for the solving strategy
    public SolveStrategy solveStrategy = SolveStrategy.LinePerLine;

    void Start()
    {
        InitializeGrid();
        SetInitialState(); // Set the initial state of the grid
        // GenerateSudoku();
    }

    void Update()
    {
        // Check if the space key is pressed
        if (Input.GetKeyUp(KeyCode.Space))
        {
            StartCoroutine(SolveSudokuWithDelay());
        }
    }

    void SetInitialState()
    {
        // Example: Set some initial values in the grid (replace this with your desired initial state)
        SetGridValue(0, 0, 5);
        SetGridValue(1, 1, 3);
        SetGridValue(2, 2, 8);
        // Add more initial values as needed
    }

    void SetGridValue(int row, int col, int value)
    {
        // Set the value in both the gridText and the actual grid
        gridText[row, col].text = value.ToString();
        // You might want to update the actual grid as well if you are using it for solving logic
        // grid[row, col] = value;
    }

    IEnumerator SolveSudokuWithDelay()
    {
        int[,] sudokuGrid = new int[9, 9];

        // Copy the initial state to the solving grid
        CopyInitialValues(sudokuGrid);

        // Generate a partially filled Sudoku grid using WFC
        WaveFunctionCollapse(sudokuGrid);

        // Copy the generated values to the gridText and gridInput with delay
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                gridText[row, col].text = sudokuGrid[row, col].ToString();
                yield return new WaitForSeconds(speed);
            }
        }
    }

    void CopyInitialValues(int[,] sudokuGrid)
    {
        // Copy the initial state to the solving grid
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                // Assuming gridText contains the initial values
                int.TryParse(gridText[row, col].text, out sudokuGrid[row, col]);
            }
        }
    }

    void InitializeGrid()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                GameObject cell = Instantiate(cellPrefab, new Vector3(col * 30, -row * 30, 0), Quaternion.identity, gridParent);
                gridText[row, col] = cell.transform.GetComponent<TMP_Text>();
            }
        }
    }

    void GenerateSudoku()
    {
        int[,] sudokuGrid = new int[9, 9];

        // Generate a partially filled Sudoku grid using WFC
        WaveFunctionCollapse(sudokuGrid);

        // Copy the generated values to the gridText and gridInput
        CopyGridValues(sudokuGrid);
    }

    void WaveFunctionCollapse(int[,] grid)
    {
        // Initialize all cells as having all possibilities (1-9)
        List<int>[,] possibilities = new List<int>[9, 9];
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                possibilities[row, col] = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            }
        }

        // Set up a random seed for demonstration purposes
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        while (true)
        {
            // Find the cell with the fewest possibilities based on the selected strategy
            int minPossibilities = int.MaxValue;
            int minRow = -1, minCol = -1;

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    int possibilitiesCount = possibilities[row, col].Count;

                    // Apply the selected strategy
                    switch (solveStrategy)
                    {
                        case SolveStrategy.LinePerLine:
                            possibilitiesCount = CountPossibilitiesInLine(possibilities, row);
                            break;
                        case SolveStrategy.ColumnPerColumn:
                            possibilitiesCount = CountPossibilitiesInColumn(possibilities, col);
                            break;
                        case SolveStrategy.SquarePerSquare:
                            possibilitiesCount = CountPossibilitiesInSquare(possibilities, row, col);
                            break;
                        // Add more strategies as needed
                    }

                    if (grid[row, col] == 0 && possibilitiesCount < minPossibilities)
                    {
                        minPossibilities = possibilitiesCount;
                        minRow = row;
                        minCol = col;
                    }
                }
            }

            // If all cells are filled or there's a contradiction, break the loop
            if (minRow == -1 || minPossibilities == 0)
                break;

            // Randomly choose a value from the possibilities
            int value;
            if (possibilities[minRow, minCol].Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, possibilities[minRow, minCol].Count);
                value = possibilities[minRow, minCol][randomIndex];
            }
            else
            {
                // Handle the case where the possibilities list is empty
                int randomValue = UnityEngine.Random.Range(1, 10);
                value = randomValue;
                // For example, set a default value or break out of the loop
                break;
            }


            // Set the chosen value to the grid
            grid[minRow, minCol] = value;

            // Remove the chosen value from the possibilities in the same row and column
            RemovePossibilitiesInRowAndColumn(possibilities, minRow, minCol, value);
        }
    }

    int CountPossibilitiesInLine(List<int>[,] possibilities, int row)
    {
        int count = 0;
        for (int col = 0; col < 9; col++)
        {
            count += possibilities[row, col].Count;
        }
        return count;
    }

    int CountPossibilitiesInColumn(List<int>[,] possibilities, int col)
    {
        int count = 0;
        for (int row = 0; row < 9; row++)
        {
            count += possibilities[row, col].Count;
        }
        return count;
    }

    int CountPossibilitiesInSquare(List<int>[,] possibilities, int row, int col)
    {
        int count = 0;
        int startRow = 3 * (row / 3);
        int startCol = 3 * (col / 3);

        for (int r = startRow; r < startRow + 3; r++)
        {
            for (int c = startCol; c < startCol + 3; c++)
            {
                count += possibilities[r, c].Count;
            }
        }

        return count;
    }

    void RemovePossibilitiesInRowAndColumn(List<int>[,] possibilities, int row, int col, int value)
    {
        // Remove the value from possibilities in the same row
        for (int c = 0; c < 9; c++)
        {
            possibilities[row, c].Remove(value);
        }

        // Remove the value from possibilities in the same column
        for (int r = 0; r < 9; r++)
        {
            possibilities[r, col].Remove(value);
        }
    }

    private void CopyGridValues(int[,] grid)
    {
        // Copy the grid values to the text components
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                gridText[row, col].text = grid[row, col].ToString();
            }
        }
    }
}