using UnityEngine;
using UnityEngine.EventSystems;

public class MouseMoveHandler : MonoBehaviour, IMoveHandler, IPointerDownHandler, IPointerUpHandler
{
    private GridManager gridManager;
    private GridCell startCell;
    private Vector2 startPos;
    private bool isDragging = false;

    public void Init(GridManager manager)
    {
        gridManager = manager;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startCell = GetComponent<GridCell>();
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

        Vector2 endPos = eventData.position;
        Vector2 delta = endPos - startPos;

        Vector2Int direction = Vector2Int.zero;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            direction = delta.x > 0 ? Vector2Int.up : Vector2Int.down;
        else
            direction = delta.y > 0 ? Vector2Int.right : Vector2Int.left;

        TryMove(startCell, direction);

        isDragging = false;
        startCell = null;
    }

    public void TryMove(GridCell fromCell, Vector2Int direction)
    {
        int targetX = fromCell.X + direction.x;
        int targetY = fromCell.Y + direction.y;

        if (!gridManager.IsInsideGrid(targetX, targetY)) return;

        GridCell targetCell = gridManager.GetCellAt(targetX, targetY).GetComponent<GridCell>();
        gridManager.SwapCells(fromCell, targetCell);
    }
}
