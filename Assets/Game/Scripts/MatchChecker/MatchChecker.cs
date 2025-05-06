using System.Collections.Generic;
using System.Linq;
public class MatchChecker
{
    private readonly List<GridMatch> matches = new();
    public List<(int, int)> indexes = new List<(int, int)>();

    public List<GridMatch> GetMatches(GridCell[,] cells)
    {
        matches.Clear();
        indexes.Clear();

        int width = cells.GetLength(0);
        int height = cells.GetLength(1);

        // Vertical matches
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 2; x++)
            {
                GridCell v = cells[x, y];
                if (v.Value == cells[x + 1, y].Value && v.Value == cells[x + 2, y].Value)
                {
                    matches.Add(new GridMatch(x, y));
                    matches.Add(new GridMatch(x + 1, y));
                    matches.Add(new GridMatch(x + 2, y));

                    if(!indexes.Contains((x, y)))
                        indexes.Add((x, y));

                    // Continue checking beyond 3 (like 4 or 5 in a row)
                    int xExt = x + 3;
                    while (xExt < width && cells[xExt, y] == v)
                    {
                        matches.Add(new GridMatch(xExt, y));
                        xExt++;
                    }
                }
            }
        }

        // Horizontal matches
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 2; y++)
            {
                GridCell v = cells[x, y];
                if (v.Value == cells[x, y + 1].Value && v.Value == cells[x, y + 2].Value)
                {
                    matches.Add(new GridMatch(x, y));
                    matches.Add(new GridMatch(x, y + 1));
                    matches.Add(new GridMatch(x, y + 2));

                    if(!indexes.Contains((x, y)))
                        indexes.Add((x, y));
                    if(!indexes.Contains((x, y + 1)))
                        indexes.Add((x, y + 1));
                    if(!indexes.Contains((x, y + 2)))
                        indexes.Add((x, y + 2));

                    int yExt = y + 3;
                    while (yExt < height && cells[x, yExt] == v)
                    {
                        if(!indexes.Contains((x, yExt)))
                            indexes.Add((x, yExt));
                            
                        matches.Add(new GridMatch(x, yExt));
                        yExt++;
                    }
                }
            }
        }

        var ordered = indexes.OrderByDescending(pair => pair.Item2).ToList();
        indexes.Clear();
        indexes = ordered;

        return matches;
    }

    public void DestroyCells(GridCell[,] cells)
    {
        foreach (var match in matches)
        {
            GridCell cell = cells[match.x, match.y];
            if (cell != null)
            {
                cell.SetValue(0);
                cell.gameObject.SetActive(false);
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
