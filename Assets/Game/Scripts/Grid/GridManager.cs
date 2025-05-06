using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 6;
    [SerializeField] private GameObject cellPrefab; // Must have TMP and layout
    [SerializeField] private Transform gridParent;
    [SerializeField] private GridLayoutGroup gridLayoutGroup = null;
    private GridCell[,] cells;

    private MatchChecker matchChecker;

    private void Start()
    {
        matchChecker = new MatchChecker();
        GenerateGrid();
    }

    public bool IsInsideGrid(int x, int y)
    {
        float cellSize = cellPrefab.GetComponent<RectTransform>().sizeDelta.x;
        return x >= 0 && y >= 0 && x < width * cellSize && y < height * cellSize;
    }

    public void SwapCells(GridCell a, GridCell b, bool verifyBoard = true)
    {
        // Store original grid positions
        int aX = a.X;
        int aY = a.Y;
        int bX = b.X;
        int bY = b.Y;
        
        // Step 1: Update the positions inside the GridCell objects
        a.SetGridPosition(bX, bY);
        b.SetGridPosition(aX, aY);
        
        // Animate the UI movement
        Vector2 posA = a.RectTransform.anchoredPosition;
        Vector2 posB = b.RectTransform.anchoredPosition;

        // Step 2: Swap references in the cells array
        cells[bX, bY] = a;
        cells[aX, aY] = b;
        
        a.RectTransform.DoMove(posB, 0.2f);
        b.RectTransform.DoMove(posA, 0.2f);

        if(verifyBoard) StartCoroutine(Timer(0.5f));
    }

    private IEnumerator MoveCellsDown()
    {
        // Process columns from left to right
        for (int y = 0; y < width; y++)
        {
            // Start from the bottom row and work upwards
            for (int x = height - 1; x >= 0; x--)
            {
                // If this cell is empty (was part of a match)
                if (!cells[x, y].gameObject.activeSelf)
                {
                    // Look for the closest non-empty cell above this position
                    int emptyX = x;
                    int currentX = x - 1;
                    
                    while (currentX >= 0)
                    {
                        if (cells[currentX, y].gameObject.activeSelf)
                        {
                            // Found a non-empty cell, swap it with our empty one
                            SwapCells(cells[emptyX, y], cells[currentX, y], false);
                            yield return new WaitForSeconds(0.2f);
                            break;
                        }
                        currentX--;
                    }
                    x = currentX;
                    // If we couldn't find any active cells above, just activate this one with a new random value
                   /*  if (currentX < 0)
                    {
                        cells[x, y].gameObject.SetActive(true);
                        cells[x, y].SetValue(Random.Range(1, 10)); // Generate new random value
                    } */
                }
            }
        }
        
        // After all cells have moved down, check for new matches
        StartCoroutine(Timer(0.5f));
    }


    private void GenerateGrid()
    {
        cells = new GridCell[width, height];

        gridLayoutGroup.constraintCount = width;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject newCell = Instantiate(cellPrefab, gridParent);

                MouseMoveHandler mouseMoveHandler = newCell.GetComponent<MouseMoveHandler>();
                mouseMoveHandler.Init(this);

                GridCell cell = newCell.GetComponent<GridCell>();
                int value = Random.Range(1, 10);
                cell.Init(x, y, value);
                
                cells[x, y] = cell;
            }
        }

        StartCoroutine(Timer(0.5f));
    }

    private IEnumerator Timer(float time)
    {
        yield return new WaitForSeconds(time);
        gridLayoutGroup.enabled = false;

        matchChecker.GetMatches(cells);
        matchChecker.DestroyCells(cells);
        yield return new WaitForSeconds(time);
        StartCoroutine(MoveCellsDown());
    }
}
