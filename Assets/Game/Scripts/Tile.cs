using UnityEngine;

public class Tile : MonoBehaviour
{
    public int type; // Tile type ID
    public int x, y;

    private GridManager grid;
    private Vector2 startTouchPos, endTouchPos;

    void Start()
    {
        grid = FindFirstObjectByType<GridManager>();
    }

    public void SetPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    void OnMouseDown()
    {
        startTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    void OnMouseUp()
    {
        endTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = endTouchPos - startTouchPos;

        if (dir.magnitude < 0.2f) return;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x > 0) grid.TrySwap(x, y, x + 1, y);
            else grid.TrySwap(x, y, x - 1, y);
        }
        else
        {
            if (dir.y > 0) grid.TrySwap(x, y, x, y + 1);
            else grid.TrySwap(x, y, x, y - 1);
        }
    }
}
