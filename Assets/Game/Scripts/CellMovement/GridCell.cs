using UnityEngine;
using TMPro;

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
}
