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
    private int[,] grid;

    private void Start()
    {
        GenerateGrid();
    }

    public bool IsInsideGrid(int x, int y)
    {
        float cellSize = cellPrefab.GetComponent<RectTransform>().sizeDelta.x;
        return x >= 0 && y >= 0 && x < width * cellSize && y < height * cellSize;
    }

    public int GetCellAt(int x, int y)
    {
        return grid[x, y];
    }

    public void SwapCells(GridCell a, GridCell b)
    {
        int aTempX = a.X;
        int aTempY = a.Y;

        a.SetGridPosition(b.X, b.Y);
        b.SetGridPosition(aTempX, aTempY);

        grid[aTempX, aTempY] = b.Value;
        grid[b.X, b.X] = a.Value;

        Vector2 posA = a.RectTransform.anchoredPosition;
        Vector2 posB = b.RectTransform.anchoredPosition;

        a.RectTransform.DoMove(posB, 0.2f);
        b.RectTransform.DoMove(posA, 0.2f);
    }

    private void GenerateGrid()
    {
        grid = new int[width, height];
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

                grid[x, y] = value;
            }
        }

        StartCoroutine(Timer(2));
    }

    private IEnumerator Timer(float time)
    {
        yield return new WaitForSeconds(time);
        gridLayoutGroup.enabled = false;
    }
}
