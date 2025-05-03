using UnityEngine;

public interface IMoveHandler
{
    void TryMove(GridCell fromCell, Vector2Int direction);
}
