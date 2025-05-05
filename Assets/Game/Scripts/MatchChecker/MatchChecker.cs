using System.Collections.Generic;
public class MatchChecker
{
    private readonly List<GridMatch> matches = new();

    public List<GridMatch> GetMatches(int[,] grid)
    {
        matches.Clear();
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        // Horizontal matches
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 2; x++)
            {
                int v = grid[x, y];
                if (v == grid[x + 1, y] && v == grid[x + 2, y])
                {
                    matches.Add(new GridMatch(x, y));
                    matches.Add(new GridMatch(x + 1, y));
                    matches.Add(new GridMatch(x + 2, y));

                    // Optional: continue checking beyond 3 (like 4 or 5 in a row)
                    int xExt = x + 3;
                    while (xExt < width && grid[xExt, y] == v)
                    {
                        matches.Add(new GridMatch(xExt, y));
                        xExt++;
                    }

                    x = xExt - 1; // Skip ahead to avoid duplicate detection
                }
            }
        }

        // Vertical matches
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 2; y++)
            {
                int v = grid[x, y];
                if (v == grid[x, y + 1] && v == grid[x, y + 2])
                {
                    matches.Add(new GridMatch(x, y));
                    matches.Add(new GridMatch(x, y + 1));
                    matches.Add(new GridMatch(x, y + 2));

                    int yExt = y + 3;
                    while (yExt < height && grid[x, yExt] == v)
                    {
                        matches.Add(new GridMatch(x, yExt));
                        yExt++;
                    }

                    y = yExt - 1; // Skip ahead
                }
            }
        }

        return matches;
    }

    public void DestroyCells(GridCell[,] cells)
    {
        foreach (var match in matches)
        {
            GridCell cell = cells[match.x, match.y];
            if (cell != null)
            {
                UnityEngine.Object.Destroy(cell.gameObject);
                cells[match.x, match.y] = null;
            }
        }
    }
}


public struct GridMatch
{
    public int x;
    public int y;

    public GridMatch(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
