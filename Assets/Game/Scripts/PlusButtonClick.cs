using UnityEngine;

public class PlusButtonClick : MonoBehaviour
{
    private GridManager gridManager;
    private int x, y;
    private bool isInitialized = false;

    public void Initialize(GridManager manager, int posX, int posY)
    {
        gridManager = manager;
        x = posX;
        y = posY;
        isInitialized = true;

        // Add a collider if it doesn't exist (needed for mouse detection)
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one * 0.8f; // Adjust size as needed
        }
    }

    void OnMouseDown()
    {
        if (isInitialized && gridManager != null)
        {
            gridManager.OnPlusButtonClicked(x, y);
        }
    }

    // Alternative method for touch input on mobile
    void Update()
    {
        if (!isInitialized) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            // Check if mouse position is close to this plus button
            float distance = Vector3.Distance(transform.position, mousePos);
            if (distance < 0.5f) // Adjust threshold as needed
            {
                gridManager.OnPlusButtonClicked(x, y);
            }
        }
    }
}