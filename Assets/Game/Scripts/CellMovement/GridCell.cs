using UnityEngine;
using TMPro;
using System.Collections;

public class GridCell : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Collider2D cellCollider = null;
    [SerializeField] private Color[] valueColors = new Color[9];
    [field: SerializeField] public int X { get; private set; }
    [field: SerializeField] public int Y { get; private set; }

    [field: SerializeField] public int Value { get; private set; }

    public RectTransform RectTransform => rectTransform;
    private RectTransform rectTransform;
    private int direction = -1;

    void Start()
    {
        GridManager.OnCellMoveAction += MoveCellDown;
        GridManager.OnMoveDeactivatedCellDownAction += MoveDeactivatedCellDown;
    }

    public void HandleCellInteraction(bool interaction)
    {
        cellCollider.enabled = interaction;
    }
    public void Init(int x, int y, int value)
    {
        X = x;
        Y = y;
        Value = value;
        SetColor();

        rectTransform = GetComponent<RectTransform>();

        UpdateDisplay();
    }

    public void SetValue(int value)
    {
        Value = value;
        SetColor();
        UpdateDisplay();
    }

    private void MoveCellDown((int x, int y, int direction) cellGridPosition)
    {
        if (cellGridPosition.y == Y && cellGridPosition.x > X)
        {
            Vector2 start = rectTransform.anchoredPosition; // Start position
            //int magnitude = cellGridPosition.x - X; // Calculate the difference in the y-axis
            Vector2 end = new Vector2(start.x, start.y - 10); // Move down by 10 units per magnitude
            StartCoroutine(Mover(start, end));
        }// x 
    }

    private void MoveDeactivatedCellDown()
    {
        if(!transform.GetChild(0).gameObject.activeSelf)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            SetValue(Random.Range(1, 10));
            Vector2 newEnd = new Vector2(rectTransform.anchoredPosition.x, -X * 10 + 5); 
            StartCoroutine(MoveDown(rectTransform.anchoredPosition, newEnd));
            direction = -1;
        }
    }//x * 10 - 60

    IEnumerator MoveDown(Vector2 start, Vector2 end)
    {
        float time = 0f; // Start with time at 0
        float duration = 0.2f; // Duration for the movement

        while (time < duration)
        {
            time += Time.deltaTime; // Increment time by the delta time
            Vector2 currentPosition = Vector2.Lerp(start, end, time / duration); // Calculate the current position based on normalized time
            rectTransform.anchoredPosition = currentPosition; // Move the UI element

            yield return null; // Wait until the next frame
        }

        // Ensure the final position is set
        rectTransform.anchoredPosition = end; // Final anchored position
    }

    IEnumerator Mover(Vector2 start, Vector2 end)
    {
        float time = 0f; // Start with time at 0
        float duration = 0.2f; // Duration for the movement

        while (time < duration)
        {
            time += Time.deltaTime; // Increment time by the delta time
            Vector2 currentPosition = Vector2.Lerp(start, end, time / duration); // Calculate the current position based on normalized time
            rectTransform.anchoredPosition = currentPosition; // Move the UI element

            yield return null; // Wait until the next frame
        }

        // Ensure the final position is set
        rectTransform.anchoredPosition = end; // Final anchored position
        GridManager.OnMoveDeactivatedCellDownAction?.Invoke();
    }


    private void SetColor()
    {
        text.color = valueColors[Value != 0 ? Value-1 : 0];
    }

    private void UpdateDisplay()
    {
        text.text = Value.ToString();
    }

    public void SetGridPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    void OnDestroy()
    {
        GridManager.OnCellMoveAction -= MoveCellDown;
        GridManager.OnMoveDeactivatedCellDownAction -= MoveDeactivatedCellDown;
    }
}
