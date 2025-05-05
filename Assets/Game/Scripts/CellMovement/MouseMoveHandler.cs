using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseMoveHandler : MonoBehaviour, IMoveHandler, IPointerDownHandler, IPointerUpHandler
{
    private GridManager gridManager;
    private GridCell startCell;
    private GridCell targetCell;
    private Vector2 startPos;
    private bool isDragging = false;

    public void Init(GridManager manager)
    {
        gridManager = manager;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startCell = GetComponent<GridCell>();
        startCell.HandleCellInteraction(false);

        if (startCell == null)
        {
            Debug.LogWarning("GridCell not found on this object.");
            return;
        }

        startPos = eventData.position;
        isDragging = true;
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging || startCell == null) return;

        isDragging = false;

        Vector2 endPos = eventData.position;
        Vector2 delta = endPos - startPos;

        Vector2Int direction = Vector2Int.zero;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            direction = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
        else
            direction = delta.y > 0 ? Vector2Int.up : Vector2Int.down;

        // Raycast at pointer position to find target cell
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = endPos
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        startCell.HandleCellInteraction(true);

        foreach (var result in results)
        {
            GridCell cell = result.gameObject.GetComponent<GridCell>();
            if (cell != null && cell != startCell)
            {
                targetCell = cell;
                break;
            }
        }

        if (targetCell == null)
        {
            Debug.Log("No valid target cell found.");
            return;
        }

        gridManager.SwapCells(startCell, targetCell);
        startCell = null;
        targetCell = null;
    }


    public void TryMove(GridCell fromCell, Vector2Int direction)
    {
        int targetX = fromCell.X + direction.x;
        int targetY = fromCell.Y + direction.y;

        if (!gridManager.IsInsideGrid(targetX, targetY)) return;

        gridManager.SwapCells(fromCell, targetCell);
    }
}
