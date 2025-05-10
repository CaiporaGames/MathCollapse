using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

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
                    matches.Add(new GridMatch(x, y, 1));
                    matches.Add(new GridMatch(x + 1, y, 1));
                    matches.Add(new GridMatch(x + 2, y, 1));

                    if(!indexes.Contains((x, y)))
                        indexes.Add((x, y));

                    // Continue checking beyond 3 (like 4 or 5 in a row)
                    int xExt = x + 3;
                    while (xExt < width && cells[xExt, y] == v)
                    {
                        matches.Add(new GridMatch(xExt, y, 1));
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
                    if(!matches.Contains(new GridMatch(x, y, 0)))
                        matches.Add(new GridMatch(x, y, 0));
                    if(!matches.Contains(new GridMatch(x, y + 1, 0)))
                        matches.Add(new GridMatch(x, y + 1, 0));
                    if(!matches.Contains(new GridMatch(x, y + 2, 0)))
                        matches.Add(new GridMatch(x, y + 2, 0));

                    int yExt = y + 3;
                    while (yExt < height && cells[x, yExt] == v)
                    {
                        if(!matches.Contains(new GridMatch(x, yExt, 0)))
                            matches.Add(new GridMatch(x, yExt, 0));
                            
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

    public int DeactivateCells(GridCell[,] cells)
    {
        foreach (var match in matches)
        {
            GridCell cell = cells[match.x, match.y];
            if (cell != null)
            {
                GridManager.OnCellMoveAction?.Invoke((cell.X, cell.Y, match.direction));

                Vector2 start = cell.RectTransform.anchoredPosition; // Start position
               // int magnitude = cell.X * 10 + 10 - cell.X * 15; // Calculate the difference in the y-axis
                Vector2 end = new Vector2(start.x, /* start.y + magnitude */ 35 - cell.X * 10);
                cell.RectTransform.anchoredPosition =  end;
                cell.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
        return matches.Count;
    }
}


public struct GridMatch
{
    public int x;
    public int y;
    public int direction;

    public GridMatch(int x, int y, int direction)
    {
        this.x = x;
        this.y = y;
        this.direction = direction;
    }
}
